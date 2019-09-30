using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace System.Device.Gpio.Drivers
{
    public unsafe partial class SunxiDriver : GpioDriver
    {
        private volatile IntPtr gpioPointer0;
        private IntPtr gpioPointer1;
        private const int GpioRegisterOffset0 = 0x01C20800;
        private const int GpioRegisterOffset1 = 0x01F02C00;
        private static readonly object s_initializationLock = new object();
        private static readonly object s_sysFsInitializationLock = new object();
        private const string GpioMemoryFilePath = "/dev/mem";
        private UnixDriver _sysFSDriver = null;
        private readonly IDictionary<int, PinMode> _sysFSModes = new Dictionary<int, PinMode>();

        protected override void Dispose(bool disposing)
        {
            if (gpioPointer0 != default)
            {
                Interop.munmap(gpioPointer0, 0);
                gpioPointer0 = default;
            }

            if (gpioPointer1 != default)
            {
                Interop.munmap(gpioPointer1, 0);
                gpioPointer1 = default;
            }

            if (_sysFSDriver != null)
            {
                _sysFSDriver.Dispose();
                _sysFSDriver = null;
            }
        }

        /// <summary>
        /// Gets the mode of a pin for Unix.
        /// </summary>
        /// <param name="mode">The mode of a pin to get.</param>
        /// <returns>The mode of a pin for Unix.</returns>
        private PinMode GetModeForUnixDriver(PinMode mode)
        {
            switch (mode)
            {
                case PinMode.Input:
                case PinMode.InputPullUp:
                case PinMode.InputPullDown:
                    return PinMode.Input;
                case PinMode.Output:
                    return PinMode.Output;
                default:
                    throw new InvalidOperationException($"Can not parse pin mode {_sysFSModes}");
            }
        }

        /// <summary>
        /// Adds a handler for a pin value changed event.
        /// </summary>
        /// <param name="pinNumber">The pin number in the driver's logical numbering scheme.</param>
        /// <param name="eventTypes">The event types to wait for.</param>
        /// <param name="callback">Delegate that defines the structure for callbacks when a pin value changed event occurs.</param>
        protected internal override void AddCallbackForPinValueChangedEvent(int pinNumber, PinEventTypes eventTypes, PinChangeEventHandler callback)
        {
            ValidatePinNumber(pinNumber);
            InitializeSysFS();

            _sysFSDriver.OpenPin(pinNumber);
            _sysFSDriver.SetPinMode(pinNumber, GetModeForUnixDriver(_sysFSModes[pinNumber]));

            _sysFSDriver.AddCallbackForPinValueChangedEvent(pinNumber, eventTypes, callback);
        }

        /// <summary>
        /// Closes an open pin.
        /// </summary>
        /// <param name="pinNumber">The pin number in the driver's logical numbering scheme.</param>
        protected internal override void ClosePin(int pinNumber)
        {
            ValidatePinNumber(pinNumber);

            if (_sysFSModes.ContainsKey(pinNumber) && _sysFSModes[pinNumber] == PinMode.Output)
            {
                Write(pinNumber, PinValue.Low);
                SetPinMode(pinNumber, PinMode.Input);
            }
        }

        /// <summary>
        /// Checks if a pin supports a specific mode.
        /// </summary>
        /// <param name="pinNumber">The pin number in the driver's logical numbering scheme.</param>
        /// <param name="mode">The mode to check.</param>
        /// <returns>The status if the pin supports the mode.</returns>
        protected internal override bool IsPinModeSupported(int pinNumber, PinMode mode)
        {
            switch (mode)
            {
                case PinMode.Input:
                case PinMode.InputPullDown:
                case PinMode.InputPullUp:
                case PinMode.Output:
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Opens a pin in order for it to be ready to use.
        /// </summary>
        /// <param name="pinNumber">The pin number in the driver's logical numbering scheme.</param>
        protected internal override void OpenPin(int pinNumber)
        {
            ValidatePinNumber(pinNumber);
            Initialize();
            SetPinMode(pinNumber, PinMode.Input);
        }

        /// <summary>
        /// Reads the current value of a pin.
        /// </summary>
        /// <param name="pinNumber">The pin number in the driver's logical numbering scheme.</param>
        /// <returns>The value of the pin.</returns>
        protected internal unsafe override PinValue Read(int pinNumber)
        {
            ValidatePinNumber(pinNumber);

            var unmapped = UnmapPinNumber(pinNumber);

            int dataAddress;
            uint* dataPointer;
            if (unmapped.PortController < 10)
            {
                dataAddress = GpioRegisterOffset0 + unmapped.PortController * 0x24 + 0x10;

                dataPointer = (uint*)(gpioPointer0 + dataAddress);
            }
            else
            {
                dataAddress = GpioRegisterOffset1 + unmapped.PortController * 0x24 + 0x10;

                dataPointer = (uint*)(gpioPointer1 + dataAddress);
            }

            uint dataValue = *dataPointer;

            return Convert.ToBoolean((dataValue >> (unmapped.port - 1)) & 1) ? PinValue.High : PinValue.Low;
        }

        /// <summary>
        /// Removes a handler for a pin value changed event.
        /// </summary>
        /// <param name="pinNumber">The pin number in the driver's logical numbering scheme.</param>
        /// <param name="callback">Delegate that defines the structure for callbacks when a pin value changed event occurs.</param>
        protected internal override void RemoveCallbackForPinValueChangedEvent(int pinNumber, PinChangeEventHandler callback)
        {
            ValidatePinNumber(pinNumber);
            InitializeSysFS();

            _sysFSDriver.OpenPin(pinNumber);
            _sysFSDriver.SetPinMode(pinNumber, GetModeForUnixDriver(_sysFSModes[pinNumber]));

            _sysFSDriver.RemoveCallbackForPinValueChangedEvent(pinNumber, callback);
        }

        /// <summary>
        /// Sets the mode to a pin.
        /// </summary>
        /// <param name="pinNumber">The pin number in the driver's logical numbering scheme.</param>
        /// <param name="mode">The mode to be set.</param>
        protected internal override void SetPinMode(int pinNumber, PinMode mode)
        {
            ValidatePinNumber(pinNumber);

            if (!IsPinModeSupported(pinNumber, mode))
            {
                throw new InvalidOperationException($"The pin {pinNumber} does not support the selected mode {mode}.");
            }

            // Get port controller, port number and shift
            var unmapped = UnmapPinNumber(pinNumber);
            int cfgNum = unmapped.port / 8;
            int cfgShift = unmapped.port % 8;
            int pulNum = unmapped.port / 16;
            int pulShift = unmapped.port % 16;

            // Get register address, register pointer
            int cfgAddress, pulAddress;
            uint* cfgPointer, pulPointer;
            if (unmapped.PortController < 10)
            {
                cfgAddress = GpioRegisterOffset0 + unmapped.PortController * 0x24 + cfgNum * 0x04;
                pulAddress = GpioRegisterOffset0 + unmapped.PortController * 0x24 + (pulNum + 7) * 0x04;

                cfgPointer = (uint*)(gpioPointer0 + cfgAddress);
                pulPointer = (uint*)(gpioPointer0 + pulAddress);
            }
            else
            {
                cfgAddress = GpioRegisterOffset1 + unmapped.PortController * 0x24 + cfgNum * 0x04;
                pulAddress = GpioRegisterOffset1 + unmapped.PortController * 0x24 + (pulNum + 7) * 0x04;

                cfgPointer = (uint*)(gpioPointer1 + cfgAddress);
                pulPointer = (uint*)(gpioPointer1 + pulAddress);
            }

            uint cfgValue = *cfgPointer;
            uint pulValue = *pulPointer;

            // Clear register
            cfgValue &= ~(0xFU << (cfgShift * 4));
            pulValue &= ~(0b_11U << (pulShift * 2));

            switch (mode)
            {
                case PinMode.Output:
                    cfgValue |= (0b_001U << (cfgShift * 4));
                    break;
                case PinMode.Input:
                    // After clearing the register, the value is the input mode.
                    break;
                case PinMode.InputPullDown:
                    pulValue|= (0b_10U << (pulShift * 2));
                    break;
                case PinMode.InputPullUp:
                    pulValue |= (0b_01U << (pulShift * 2));
                    break;
                default:
                    throw new ArgumentException();
            }

            *cfgPointer = cfgValue;
            Thread.SpinWait(150);
            *pulPointer = pulValue;

            if (_sysFSModes.ContainsKey(pinNumber))
            {
                _sysFSModes[pinNumber] = mode;
            }
            else
            {
                _sysFSModes.Add(pinNumber, mode);
            }
        }

        /// <summary>
        /// Blocks execution until an event of type eventType is received or a cancellation is requested.
        /// </summary>
        /// <param name="pinNumber">The pin number in the driver's logical numbering scheme.</param>
        /// <param name="eventTypes">The event types to wait for.</param>
        /// <param name="cancellationToken">The cancellation token of when the operation should stop waiting for an event.</param>
        /// <returns>A structure that contains the result of the waiting operation.</returns>
        protected internal override WaitForEventResult WaitForEvent(int pinNumber, PinEventTypes eventTypes, CancellationToken cancellationToken)
        {
            ValidatePinNumber(pinNumber);
            InitializeSysFS();

            _sysFSDriver.OpenPin(pinNumber);
            _sysFSDriver.SetPinMode(pinNumber, GetModeForUnixDriver(_sysFSModes[pinNumber]));

            return _sysFSDriver.WaitForEvent(pinNumber, eventTypes, cancellationToken);
        }

        /// <summary>
        /// Async call until an event of type eventType is received or a cancellation is requested.
        /// </summary>
        /// <param name="pinNumber">The pin number in the driver's logical numbering scheme.</param>
        /// <param name="eventTypes">The event types to wait for.</param>
        /// <param name="cancellationToken">The cancellation token of when the operation should stop waiting for an event.</param>
        /// <returns>A task representing the operation of getting the structure that contains the result of the waiting operation</returns>
        protected internal override ValueTask<WaitForEventResult> WaitForEventAsync(int pinNumber, PinEventTypes eventTypes, CancellationToken cancellationToken)
        {
            ValidatePinNumber(pinNumber);
            InitializeSysFS();

            _sysFSDriver.OpenPin(pinNumber);
            _sysFSDriver.SetPinMode(pinNumber, GetModeForUnixDriver(_sysFSModes[pinNumber]));

            return _sysFSDriver.WaitForEventAsync(pinNumber, eventTypes, cancellationToken);
        }

        /// <summary>
        /// Writes a value to a pin.
        /// </summary>
        /// <param name="pinNumber">The pin number in the driver's logical numbering scheme.</param>
        /// <param name="value">The value to be written to the pin.</param>
        protected internal override void Write(int pinNumber, PinValue value)
        {
            ValidatePinNumber(pinNumber);

            var unmapped = UnmapPinNumber(pinNumber);

            int dataAddress;
            uint* dataPointer;
            if (unmapped.PortController < 10)
            {
                dataAddress = GpioRegisterOffset0 + unmapped.PortController * 0x24 + 0x10;

                dataPointer = (uint*)(gpioPointer0 + dataAddress);
            }
            else
            {
                dataAddress = GpioRegisterOffset1 + unmapped.PortController * 0x24 + 0x10;

                dataPointer = (uint*)(gpioPointer1 + dataAddress);
            }

            uint dataValue = *dataPointer;

            if (value == PinValue.High)
            {
                dataValue |= (uint)(1 << (unmapped.port - 1));
            }
            else
            {
                dataValue &= (uint)~(1 << (unmapped.port - 1));
            }

            *dataPointer = dataValue;
        }

        private void InitializeSysFS()
        {
            if (_sysFSDriver != null)
            {
                return;
            }
            lock (s_sysFsInitializationLock)
            {
                if (_sysFSDriver != null)
                {
                    return;
                }
                _sysFSDriver = new SysFsDriver();
            }
        }

        private void Initialize()
        {
            if (gpioPointer0 != null)
            {
                return;
            }

            lock (s_initializationLock)
            {
                if (gpioPointer0 != null)
                {
                    return;
                }

                int fileDescriptor = Interop.open(GpioMemoryFilePath, FileOpenFlags.O_RDWR | FileOpenFlags.O_SYNC);
                if (fileDescriptor == -1)
                {
                    throw new IOException($"Error {Marshal.GetLastWin32Error()} initializing the Gpio driver.");
                }

                IntPtr mapPointer0 = Interop.mmap(IntPtr.Zero, Environment.SystemPageSize, (MemoryMappedProtections.PROT_READ | MemoryMappedProtections.PROT_WRITE), MemoryMappedFlags.MAP_SHARED, fileDescriptor, GpioRegisterOffset0);
                IntPtr mapPointer1 = Interop.mmap(IntPtr.Zero, Environment.SystemPageSize, (MemoryMappedProtections.PROT_READ | MemoryMappedProtections.PROT_WRITE), MemoryMappedFlags.MAP_SHARED, fileDescriptor, GpioRegisterOffset1);
                if (mapPointer0.ToInt64() == -1 || mapPointer1.ToInt64() == -1)
                {
                    throw new IOException($"Error {Marshal.GetLastWin32Error()} initializing the Gpio driver.");
                }

                Interop.close(fileDescriptor);

                gpioPointer0 = mapPointer0;
                gpioPointer1 = mapPointer1;
            }
        }

        private int MapPinNumber(char portController, int port)
        {
            int alphabetPosition = MapPortController(portController);

            return alphabetPosition * 32 + port;
        }

        private (int PortController, int port) UnmapPinNumber(int pinNumber)
        {
            int port = pinNumber % 32;
            int portController = (pinNumber - port) / 32;

            return (portController, port);
        }

        private int MapPortController(char portController)
        {
            return portController switch
            {
                'A' => 0,
                'B' => 1,
                'C' => 2,
                'D' => 3,
                'E' => 4,
                'F' => 5,
                'G' => 6,
                'H' => 7,
                'I' => 8,
                'J' => 9,
                'K' => 10,
                'L' => 11,
                'M' => 12,
                _ => throw new Exception()
            };
        }

        private char UnmapPortController(int alphabetPosition)
        {
            return alphabetPosition switch
            {
                0 => 'A',
                1 => 'B',
                2 => 'C',
                3 => 'D',
                4 => 'E',
                5 => 'F',
                6 => 'G',
                7 => 'H',
                8 => 'I',
                9 => 'J',
                10 => 'K',
                11 => 'L',
                _ => throw new Exception()
            };
        }

        /// <summary>
        /// Gets the mode of a pin.
        /// </summary>
        /// <param name="pinNumber">The pin number in the driver's logical numbering scheme.</param>
        /// <returns>The mode of the pin.</returns>
        protected internal override PinMode GetPinMode(int pinNumber)
        {
            ValidatePinNumber(pinNumber);

            if (!_sysFSModes.ContainsKey(pinNumber))
            {
                throw new InvalidOperationException("Can not get a pin mode of a pin that is not open.");
            }
            return _sysFSModes[pinNumber];
        }
    }
}
