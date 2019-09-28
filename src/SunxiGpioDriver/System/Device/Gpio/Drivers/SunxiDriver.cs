using System;
using System.Collections.Generic;
using System.Text;

namespace System.Device.Gpio.Drivers
{
    public partial class SunxiDriver
    {
        protected internal override int PinCount => 28;

        private void ValidatePinNumber(int pinNumber)
        {
            if (pinNumber < 0 || pinNumber > 27)
            {
                throw new ArgumentException("The specified pin number is invalid.", nameof(pinNumber));
            }
        }

        /// <summary>
        /// Converts a board pin number to the driver's logical numbering scheme.
        /// </summary>
        /// <param name="pinNumber">The board pin number to convert.</param>
        /// <returns>The pin number in the driver's logical numbering scheme.</returns>
        protected internal override int ConvertPinNumberToLogicalNumberingScheme(int pinNumber)
        {
            return pinNumber switch
            {                
                _ => throw new ArgumentException($"Board (header) pin {pinNumber} is not a GPIO pin on the {GetType().Name} device.", nameof(pinNumber))
            };
        }
    }
}
