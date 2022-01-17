using DynamicData;
using Plugin.BLE;
using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.EventArgs;
using Plugin.BLE.Abstractions.Extensions;
using ReactiveUI;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using XRobot.Controls;
using XRobot.Helpers;

#pragma warning disable CA1416

namespace XRobot.Services;

public class BluetoothController
{
    // HM-10 Ble LE Service and Characteristic Id

    private static readonly Guid serviceGuid = Guid.Parse("0000FFE0-0000-1000-8000-00805F9B34FB");

    private static readonly Guid characteristicGuid = Guid.Parse("0000FFE1-0000-1000-8000-00805F9B34FB");

    // Protocol Opreations

    private static readonly byte MOVE = 0xF0;

    private static readonly byte SNSE = 0x0F;

    // Protocol Prefix - Postfix Indicator 

    private static readonly byte BGN = 0xAA;

    private static readonly byte FIN = 0x55;


    private readonly IBluetoothLE _ble;

    private readonly IAdapter _adapter;


    private ICharacteristic _characteristic;


    private readonly ReadOnlyObservableCollection<IDevice> _devices;
    public ReadOnlyObservableCollection<IDevice> Devices => _devices;

    private CompositeDisposable _disposables = new();

    private IDevice _device;


    private Subject<float> _sensorValue = new();
    public IObservable<float> SensorValue => _sensorValue;



    private Subject<string> _sensorValueString = new();
    public IObservable<string> SensorValueString => _sensorValueString;

    public IObservable<bool> IsConnected { get; }

    public IObservable<bool> IsEnabled { get; }


    public BluetoothController()
    {
        _devices = new ReadOnlyObservableCollection<IDevice>(new());

        _ble = CrossBluetoothLE.Current;

        _adapter = CrossBluetoothLE.Current.Adapter;

        _adapter.ScanMode = ScanMode.LowLatency;

        Observable.FromEventPattern<DeviceEventArgs>(_adapter, nameof(IAdapter.DeviceDiscovered))
                  .Select(s => s.EventArgs.Device)
                  .ToObservableChangeSet(x => x.Id)
                  .Bind(out _devices)
                  .Subscribe()
                  .DisposeWith(_disposables);

        var deviceConnectedObs = Observable.FromEventPattern<DeviceEventArgs>(_adapter, nameof(IAdapter.DeviceConnected))
                                     .Select(s => true);

        var deviceDisconnectedObs = Observable.FromEventPattern<DeviceEventArgs>(_adapter, nameof(IAdapter.DeviceDisconnected))
                                        .Select(s => false);

        var deviceConnectionLostObs = Observable.FromEventPattern<DeviceEventArgs>(_adapter, nameof(IAdapter.DeviceConnectionLost))
                                       .Select(s => false);

        IsConnected = deviceConnectedObs.Merge(deviceDisconnectedObs).Merge(deviceConnectionLostObs);


        IsEnabled = Observable.FromEventPattern<BluetoothStateChangedArgs>(_ble, nameof(IBluetoothLE.StateChanged))
                                        .Select(s => s.EventArgs.NewState == BluetoothState.On)
                                        .StartWith(_ble.IsOn);

    }

    public async Task StartScanAsync()
    {
        if (_ble.IsOn && _ble.IsAvailable)
        {
            await _adapter.StartScanningForDevicesAsync();
        }
    }

    public async Task StopScanAsync()
    {
        if (_ble.IsOn && _ble.IsAvailable)
        {
            await _adapter.StopScanningForDevicesAsync();
        }
    }

    public async Task ConnectAsync(IDevice device)
    {
        if (_ble.IsOn && _ble.IsAvailable && device.State != DeviceState.Connected)
        {

            try
            {
                await _adapter.ConnectToDeviceAsync(device);

                _device = device;

                var service = await _device.GetServiceAsync(serviceGuid);

                _characteristic = await service.GetCharacteristicAsync(characteristicGuid);

                _characteristic.WriteType = CharacteristicWriteType.WithoutResponse;

                var valueUpdatedObs = Observable.FromEventPattern<CharacteristicUpdatedEventArgs>(_characteristic, nameof(ICharacteristic.ValueUpdated))
                          .Select(s => s.EventArgs.Characteristic.Value);

                valueUpdatedObs.Where(s => s.Length == 1)
                               .Select(s => (float)s[0])
                               .Select(s => s.Map(0, 255, 0, 100))
                               .ObserveOn(RxApp.MainThreadScheduler)
                               .Subscribe(_sensorValue)
                               .DisposeWith(_disposables);

                await _characteristic.StartUpdatesAsync();

                Observable.Interval(TimeSpan.FromSeconds(2))
                          .Select(s => Unit.Default)
                          .Subscribe(async x => await ReadSensorAsync())
                          .DisposeWith(_disposables);

            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message, "Exception");
            }
        }
    }

    public async Task DisconnectAsync()
    {
        try
        {
            if (_ble.IsOn && _ble.IsAvailable)
            {
                if (!_disposables.IsDisposed)
                {
                    _disposables.Dispose();
                    _disposables = new CompositeDisposable();
                }

                if (_characteristic is not null)
                {
                    await _characteristic.StopUpdatesAsync();
                    _characteristic = null;
                }

                if (_device is not null && _device.State == DeviceState.Connected)
                {
                    await _adapter.DisconnectDeviceAsync(_device);

                    _device.Dispose();
                    _device = null;
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message, "Exception");
        }
    }

    private async Task WriteAsync(byte[] values)
    {
        if (_device is not null && _characteristic is not null)
        {
            if (_device.State == DeviceState.Connected)
            {
                await _characteristic.WriteAsync(values);
            }
        }
    }

    public async Task DigitalMoveAsync(JoystickDirection direction, float speed)
    {
        var dircationCommand = (byte)direction;

        var newSpeed = (byte)speed.Map(0, 100, 0, 255);

        var data = new byte[] { BGN, MOVE, dircationCommand, newSpeed, FIN };

        await WriteAsync(data);
    }

    public async Task ReadSensorAsync()
    {
        var data = new byte[] { BGN, SNSE, 0, 0, FIN };
        await WriteAsync(data);
    }
}
