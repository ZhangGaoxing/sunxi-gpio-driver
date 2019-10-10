using System;
using System.Device.Gpio;
using System.Threading;

namespace AllwinnerGpioDriver.Samples
{
    class Program
    {
        static int number;
        static GpioController gpio;

        static void Main(string[] args)
        {
            using (gpio = new GpioController(PinNumberingScheme.Board))
            {
                try
                {
                    while (true)
                    {
                        Console.WriteLine("Please input pin number in the board pin header: ");
                        number = Convert.ToInt32(Console.ReadLine());

                        gpio.OpenPin(number);
                        gpio.SetPinMode(number, PinMode.Output);

                        gpio.Write(number, PinValue.High);
                        Thread.Sleep(1000);

                        gpio.ClosePin(number);
                    }
                }
                catch
                {
                    Console.WriteLine("Exit.");
                }
            }
        }
    }
}
