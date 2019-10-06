using System;
using System.Collections.Generic;
using System.Text;

namespace System.Device.Gpio.Drivers
{
    public partial class SunxiDriver
    {
        protected internal override int PinCount => 28;

        /// <summary>
        /// Converts a board pin number to the driver's logical numbering scheme.
        /// </summary>
        /// <param name="pinNumber">The board pin number to convert.</param>
        /// <returns>The pin number in the driver's logical numbering scheme.</returns>
        protected internal override int ConvertPinNumberToLogicalNumberingScheme(int pinNumber)
        {
            return pinNumber;
        }
    }
}
