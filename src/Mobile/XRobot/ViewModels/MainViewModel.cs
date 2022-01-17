

using System.Reactive;
using System.Reactive.Linq;

using ReactiveUI;
using ReactiveUI.Fody.Helpers;

using Plugin.BLE.Abstractions.Contracts;

using XRobot.Services;
using System.Diagnostics;
using DynamicData.Binding;
using XRobot.Controls;
using System.Reactive.Disposables;

namespace XRobot.ViewModels;

public class MainViewModel : ReactiveObject
{
    private readonly BluetoothController _bluetoothController;

    private readonly IAlertMessage _message;


    private readonly Interaction<IDevice[], IDevice> _selectDeviceInteraction;
    public Interaction<IDevice[], IDevice> SelectDeviceInteraction => _selectDeviceInteraction;


    [Reactive] public float Speed { get; set; }
    [Reactive] public bool IsBusy { get; private set; }

    public bool IsEnabled { [ObservableAsProperty] get; }
    public bool IsConnected { [ObservableAsProperty] get; }
    public float SensorValue { [ObservableAsProperty] get; }


    #region Commands

    public ReactiveCommand<Unit, float> IncreaseSpeedCommand { get; }
    public ReactiveCommand<Unit, float> DecreaseSpeedCommand { get; }
    public ReactiveCommand<bool, Unit> ScanCommand { get; }
    public ReactiveCommand<JoystickDirection, Unit> DigitalMoveCommand { get; }

    #endregion


    public MainViewModel(BluetoothController bluetoothController, IAlertMessage message)
    {
        _bluetoothController = bluetoothController;

        _message = message;

        _selectDeviceInteraction = new Interaction<IDevice[], IDevice>();

        IncreaseSpeedCommand = ReactiveCommand.Create(() =>
        {
            if (Speed == 100)
            {
                return 100;
            }

            return Speed += 10f;
        });

        DecreaseSpeedCommand = ReactiveCommand.Create(() =>
        {
            if (Speed == 0)
            {
                return 0;
            }

            return Speed -= 10f;
        });

        _bluetoothController.IsEnabled.ToPropertyEx(this, x => x.IsEnabled);

        _bluetoothController.IsConnected.ToPropertyEx(this, x => x.IsConnected);

        _bluetoothController.SensorValue.ToPropertyEx(this, x => x.SensorValue);



        ScanCommand = ReactiveCommand.CreateFromTask<bool>(OnScanAsync);

        DigitalMoveCommand = ReactiveCommand.CreateFromTask<JoystickDirection>(OnDigitalMoveAsync);

    }

    private async Task OnScanAsync(bool isChecked)
    {
        if (isChecked && !IsConnected)
        {

            IsBusy = true;

            await _bluetoothController.StartScanAsync();

            var devices = _bluetoothController.Devices.ToArray();

            var result = await _selectDeviceInteraction.Handle(devices);

            await _bluetoothController.StopScanAsync();

            if (result is not null)
            {
                await _bluetoothController.ConnectAsync(result);

                _message.ShortAlert($"Connected To { result} Successfully");
            }
            else
            {
                _message.ShortAlert($"Unable to Connected to {result}");
            }

            IsBusy = false;

        }
        else if (IsConnected)
        {
            await _bluetoothController.DisconnectAsync();

            _message.ShortAlert($"Disconnected Successfully");
        }
    }
    private async Task OnDigitalMoveAsync(JoystickDirection direction)
    {
        if (IsConnected)
        {
            await _bluetoothController.DigitalMoveAsync(direction, Speed);
        }
    }


}
