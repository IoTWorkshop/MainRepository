using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Client;
using System;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Gpio;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;


namespace Two_Way_Communication_RPi
{
    
    public sealed partial class MainPage : Page
    {
        private const int LED_PIN = 5;
        private GpioPin pin;
        private GpioPinValue pinValue;
        private SolidColorBrush redBrush = new SolidColorBrush(Windows.UI.Colors.Red);
        private SolidColorBrush grayBrush = new SolidColorBrush(Windows.UI.Colors.LightGray);
        static DeviceClient deviceClient;
        static string iotHubUri = "IoTWorkshop.azure-devices.net";
        static string deviceKey = "UGI5riqG4YTjwNxqo/RufcFdLQxflT76RHuQXjQi0zY=";
        static ServiceClient serviceClient;
        static string connectionString = "HostName=IoTWorkshop.azure-devices.net;SharedAccessKeyName=iothubowner;SharedAccessKey=jG8iydzTREG2v0U/iGVj1tsbr9yTrkkVYOg0gFZxCrw=";
        private static int i = 2;
        public MainPage()
        {
            deviceClient = DeviceClient.Create(iotHubUri, new DeviceAuthenticationWithRegistrySymmetricKey("IoTWorkshopRPi", deviceKey));
            serviceClient = ServiceClient.CreateFromConnectionString(connectionString);
            InitializeComponent();
            InitGPIO();
            ReceiveC2dAsync();
        }
        private void InitGPIO()
        {
            var gpio = GpioController.GetDefault();

            if (gpio == null)
            {
                pin = null;
                return;
            }

            pin = gpio.OpenPin(LED_PIN);
            pinValue = GpioPinValue.Low;
            pin.Write(pinValue);
            pin.SetDriveMode(GpioPinDriveMode.Output);

        }
        private async void ReceiveC2dAsync()
        {
            while (true)
            {

                Microsoft.Azure.Devices.Client.Message receivedMessage = await deviceClient.ReceiveAsync();
                if (receivedMessage == null) continue;
                string receivedData = Encoding.ASCII.GetString(receivedMessage.GetBytes());

                if (receivedData == "1")
                {
                    pinValue = GpioPinValue.High;
                    pin.Write(pinValue);
                    LED.Fill = redBrush;
                }
                else
                {
                    pinValue = GpioPinValue.Low;
                    pin.Write(pinValue);
                    LED.Fill = grayBrush;
                }
                await deviceClient.CompleteAsync(receivedMessage);
            }

        }

        private async void button_Click(object sender, RoutedEventArgs e)
        {
            if (i % 2 == 0)
                await SendCloudToDeviceMessageAsync("1");
            else
                await SendCloudToDeviceMessageAsync("0");
            i++;
        }

        private async static Task SendCloudToDeviceMessageAsync(string podaci)
        {
            var commandMessage = new Microsoft.Azure.Devices.Message(Encoding.ASCII.GetBytes(podaci));
            await serviceClient.SendAsync("IoTWorkshopAPP", commandMessage);
        }
    }
}
