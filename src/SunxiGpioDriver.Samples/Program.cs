using System;
using System.Device.Gpio;
using System.Device.Gpio.Drivers;
using System.Threading;

namespace AllwinnerGpioDriver.Samples
{
    class Program
    {
        static GpioController gpio = new GpioController(PinNumberingScheme.Logical, new OrangePiZeroDriver());

        static void Main(string[] args)
        {
            gpio.OpenPin(14);
            gpio.SetPinMode(14, PinMode.Output);

            while (true)
            {
                gpio.Write(14, PinValue.High);
                Thread.Sleep(500);
                gpio.Write(14, PinValue.Low);
                Thread.Sleep(500);
            }

            //gpio.RegisterCallbackForPinValueChangedEvent(14, PinEventTypes.Falling, handler);

            //Console.WriteLine(gpio.Read(14));

            Console.ReadKey();
        }

        private static void handler(object sender, PinValueChangedEventArgs pinValueChangedEventArgs)
        {
            Console.WriteLine(gpio.Read(14));
        }
    }
}
