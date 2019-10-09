using System;
using System.Device.Gpio;
using System.Device.Gpio.Drivers;
using System.Threading;

namespace AllwinnerGpioDriver.Samples
{
    class Program
    {
        static GpioController gpio = new GpioController(PinNumberingScheme.Board);

        static void Main(string[] args)
        {
            gpio.OpenPin(10);
            gpio.SetPinMode(10, PinMode.InputPullUp);

            gpio.RegisterCallbackForPinValueChangedEvent(10, PinEventTypes.Rising, handler);

            Console.ReadKey();

            gpio.Dispose();
        }

        private static void handler(object sender, PinValueChangedEventArgs pinValueChangedEventArgs)
        {
            Console.WriteLine("switch pressed");
        }
    }
}
