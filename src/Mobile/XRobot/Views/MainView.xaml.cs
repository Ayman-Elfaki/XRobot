

using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Disposables;

using ReactiveUI;
using ReactiveUI.Maui;

using XRobot.Controls;
using XRobot.ViewModels;

namespace XRobot.Views;

public partial class MainView : ReactiveContentPage<MainViewModel>
{
    public MainView(MainViewModel mainViewModel)
    {
        InitializeComponent();

        ViewModel = mainViewModel;

        this.WhenActivated(d =>
        {

            Observable.FromEventPattern<ToggleButtonEventArgs>(bluetoothButton, nameof(ToggleButton.Checked))
                      .Select(b => b.EventArgs.IsChecked)
                      .InvokeCommand(ViewModel, x => x.ScanCommand)
                      .DisposeWith(d);

            Observable.FromEventPattern<EventArgs>(minusButton, nameof(RepeatButton.Pressed))
                      .Select(b => Unit.Default)
                      .InvokeCommand(ViewModel, x => x.DecreaseSpeedCommand)
                      .DisposeWith(d);

            Observable.FromEventPattern<EventArgs>(plusButton, nameof(RepeatButton.Pressed))
                      .Select(b => Unit.Default)
                      .InvokeCommand(ViewModel, x => x.IncreaseSpeedCommand)
                      .DisposeWith(d);

            Observable.FromEventPattern<EventArgs>(joystickControl, nameof(Joystick.DigitalMoved))
                      .Select(b => joystickControl.Direction)
                      .InvokeCommand(ViewModel, x => x.DigitalMoveCommand)
                      .DisposeWith(d);


            this.OneWayBind(ViewModel, vm => vm.IsConnected, v => v.bluetoothButton.IsChecked)
                .DisposeWith(d);

            //this.OneWayBind(ViewModel, vm => vm.IsEnabled, v => v.joystickControl.IsEnabled)
            //    .DisposeWith(d);

            this.OneWayBind(ViewModel, vm => vm.IsEnabled, v => v.bluetoothButton.IsEnabled)
                .DisposeWith(d);

            this.OneWayBind(ViewModel, vm => vm.IsEnabled, v => v.speed.IsEnabled)
                .DisposeWith(d);

            this.OneWayBind(ViewModel, vm => vm.IsEnabled, v => v.sensor.IsEnabled)
                .DisposeWith(d);

            this.OneWayBind(ViewModel, vm => vm.IsEnabled, v => v.plusButton.IsEnabled)
                .DisposeWith(d);

            this.OneWayBind(ViewModel, vm => vm.IsEnabled, v => v.minusButton.IsEnabled)
                .DisposeWith(d);


            this.OneWayBind(ViewModel, vm => vm.Speed, v => v.speed.Value)
                .DisposeWith(d);

            this.OneWayBind(ViewModel, vm => vm.SensorValue, v => v.sensor.Value)
                .DisposeWith(d);


            this.OneWayBind(ViewModel, vm => vm.IsBusy, v => v.busyActivityIndicator.IsRunning)
                .DisposeWith(d);

            this.OneWayBind(ViewModel, vm => vm.IsBusy, v => v.controlsContainer.IsVisible, v => !v)
                .DisposeWith(d);


            ViewModel.SelectDeviceInteraction.RegisterHandler(async x =>
            {
                var devices = x.Input.Select(s => s.Name).ToArray();

                var result = await DisplayActionSheet("Select a Device:", "Cancel", "Ok", devices);

                x.SetOutput(x.Input.FirstOrDefault(d => d.Name == result));

            }).DisposeWith(d);




        });

    }

    protected override async void OnAppearing()
    {
        await MainThread.InvokeOnMainThreadAsync(async () =>
        {
            var startAnimation = new Animation()
            {
                {0, 1, new Animation(v => speed.Value = (float)v, 0, 100, Easing.SpringIn) },
                {0, 1, new Animation(v => sensor.Value = (float)v, 0, 100, Easing.SpringIn) }
            };

            startAnimation.Commit(this, "StartAnimation", length: 2000);

            await Task.Delay(2000);

            var endAnimation = new Animation()
            {
                {0, 1, new Animation(v => speed.Value = (float)v, 100, 0, Easing.Linear) },
                {0, 1, new Animation(v => sensor.Value = (float)v, 100, 0, Easing.Linear) }
            };

            endAnimation.Commit(this, "EndAnimation", length: 2000);

        });


#if ANDROID

        await Permissions.RequestAsync<Platforms.BluetoothPermission>();

#endif


    }
}
