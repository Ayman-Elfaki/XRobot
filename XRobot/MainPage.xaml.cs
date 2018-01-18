using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace XRobot
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private Arduino _arduino;
        private DispatcherTimer _timer;
        private byte _speed;
        private byte _dirction;
        private Color _color;

        public MainPage()
        {
            this.InitializeComponent();

            _timer = new DispatcherTimer();
            _timer.Interval = new TimeSpan(0, 0,2);
            _timer.Tick += _timer_Tick;
           
            _arduino = new Arduino();
            _color = new Color();
            _color.R = byte.MaxValue;
            _color.B = 0;
            _color.G = 0;
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            var devices = await _arduino.GetAvailableDevices();
            portsList.ItemsSource = devices.Select(a => a.Name);
            portsList.SelectedIndex = 0;

        }

        private async void connectButton_Click(object sender, RoutedEventArgs e)
        {
            if (!_arduino.IsOpen)
            {
                var devices = await _arduino.GetAvailableDevices();
                var device = devices.ToList().Find(d => d.Name == portsList.SelectedValue.ToString());

                await _arduino.Open(device);
                portsList.IsEnabled = false;
                connectButton.Content = "Disconnect";
                portsList.IsEnabled = false;
                speedSlider.IsEnabled = true;
                Window.Current.CoreWindow.KeyDown += CoreWindow_KeyDown;
                Window.Current.CoreWindow.KeyUp += CoreWindow_KeyUp;
                _timer.Start();
            }
            else
            {
                _arduino.Close();
                portsList.IsEnabled = true;
                connectButton.Content = "Connect";
                speedSlider.IsEnabled = false;
                _timer.Stop();
                Window.Current.CoreWindow.KeyDown -= CoreWindow_KeyDown;
                Window.Current.CoreWindow.KeyUp -= CoreWindow_KeyUp;
            }
        }


        private async void _timer_Tick(object sender, object e)
        {
            if (_arduino.IsOpen)
            {
                await _arduino.WriteByteAsync(240);
                var lightValue = await _arduino.ReadByteAsync();
                _color.A = lightValue;
                lightIntensityCircle.Fill = new SolidColorBrush(_color);
                lightIntensity.Text = lightValue.ToString();
            }
        }

        private async void CoreWindow_KeyUp(Windows.UI.Core.CoreWindow sender, Windows.UI.Core.KeyEventArgs args)
        {
            _dirction = 0;
            await _arduino.WriteByteAsync(Convert.ToByte(_speed + _dirction));
        }

        private async void CoreWindow_KeyDown(Windows.UI.Core.CoreWindow sender, Windows.UI.Core.KeyEventArgs args)
        {
            switch (args.VirtualKey)
            {
                case Windows.System.VirtualKey.Left:
                    _dirction = 128;
                    await _arduino.WriteByteAsync(Convert.ToByte(_speed + _dirction));
                    break;
                case Windows.System.VirtualKey.Up:
                    _dirction = 16;
                    await _arduino.WriteByteAsync(Convert.ToByte(_speed + _dirction));
                    break;
                case Windows.System.VirtualKey.Right:
                    _dirction = 64;
                    await _arduino.WriteByteAsync(Convert.ToByte(_speed + _dirction));
                    break;
                case Windows.System.VirtualKey.Down:
                    _dirction = 32;
                    await _arduino.WriteByteAsync(Convert.ToByte(_speed + _dirction));
                    break;
                case Windows.System.VirtualKey.Space:
                    _dirction = 0;
                    await _arduino.WriteByteAsync(Convert.ToByte(_speed + _dirction));
                    break;
                case Windows.System.VirtualKey.Add:
                    speedSlider.Value += 1;
                    break;
                case Windows.System.VirtualKey.Subtract:
                    speedSlider.Value -= 1;
                    break;
                default:
                    break;
            }
        }

        private async void speedSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {

            _speed = Convert.ToByte(e.NewValue);
            await _arduino.WriteByteAsync(Convert.ToByte(_speed + _dirction));

        }

        private async void forwardBtn_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            if (_arduino.IsOpen)
            {
                _dirction = 16;
                await _arduino.WriteByteAsync(Convert.ToByte(_speed + _dirction));
            }
        }

        private async void backwardBtn_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            if (_arduino.IsOpen)
            {
                _dirction = 32;
                await _arduino.WriteByteAsync(Convert.ToByte(_speed + _dirction));
            }
        }

        private async void rightBtn_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            if (_arduino.IsOpen)
            {
                _dirction = 64;
                await _arduino.WriteByteAsync(Convert.ToByte(_speed + _dirction));
            }
        }

        private async void leftBtn_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            if (_arduino.IsOpen)
            {
                _dirction = 128;
                await _arduino.WriteByteAsync(Convert.ToByte(_speed + _dirction));
            }
        }

        private async void allBtn_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            if (_arduino.IsOpen)
            {
                _dirction = 0;
                await _arduino.WriteByteAsync(Convert.ToByte(_speed + _dirction));
            }
        }

    }
}
