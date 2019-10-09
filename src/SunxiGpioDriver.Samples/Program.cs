using System;
using System.Device.Gpio;

namespace AllwinnerGpioDriver.Samples
{
    class Program
    {
        static void Main(string[] args)
        {
            using GpioController gpio = new GpioController(PinNumberingScheme.Board);
            //using GpioController gpio = new GpioController(PinNumberingScheme.Board, new OrangePiZeroDriver());

            gpio.OpenPin(10);
            gpio.SetPinMode(10, PinMode.InputPullUp);

            gpio.RegisterCallbackForPinValueChangedEvent(10, PinEventTypes.Rising, Switch_Pressed_Handler);

            Console.ReadKey();
        }

        private static void Switch_Pressed_Handler(object sender, PinValueChangedEventArgs pinValueChangedEventArgs)
        {
            Console.WriteLine("The switch is pressed.");
        }
    }
}
