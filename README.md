# Sunxi GPIO Driver for .NET

**sunxi** represents the family of ARM SoCs from Allwinner Technology. This project contains a **full function(PULL-UP, PULL-DOWN)** generic GPIO driver `SunxiDriver` for Allwinner SoCs and some special GPIO drivers like `OrangePiZeroDriver`, `OrangePiLite2Driver`.

[dotnet/iot #780](https://github.com/dotnet/iot/pull/780)

## Getting Started

### Generic GPIO driver: `SunxiDriver`
```C#
// Beacuse this is a generic driver, the pin scheme can only be Logical.
// The offset(base address) can be find in the corresponding SoC datasheet.
using GpioController gpio = new GpioController(PinNumberingScheme.Logical, new SunxiDriver(gpioRegisterOffset0: 0x01C20800, gpioRegisterOffset1: 0x01F02C00));

// Convert pin number to logical scheme.
int number = SunxiDriver.MapPinNumber('A', 10);
// Open the GPIO pin.
gpio.OpenPin(number);
// Set the pin mode.
gpio.SetPinMode(number, PinMode.InputPullUp);
// Read current value of the pin.
PinValue value = gpio.Read(number);
// Register a value changed callback.
gpio.RegisterCallbackForPinValueChangedEvent(10, PinEventTypes.Rising, Switch_Pressed_Handler);
```

### Special GPIO driver: `OrangePiZeroDriver`, `OrangePiLite2Driver`
```C#
// The programm get the best applicable driver automatically.
using GpioController gpio = new GpioController(PinNumberingScheme.Board);

gpio.OpenPin(10);
gpio.SetPinMode(10, PinMode.Output);
// Write a value to the pin.
gpio.Write(10, PinValue.High);
```

## Run the sample
```
cd SunxiGpioDriver.Samples
dotnet publish -c release -r linux-arm -o YOUR_FOLDER
sudo dotnet YOUR_FOLDER/SunxiGpioDriver.Samples.dll
```

## Run the sample with Docker
Before build docker image, you need to modify SDK, runtime and apt sources(in China) to adapt to the corresponding Linux platform.

```
docker build -t sunxi-sample -f Dockerfile .
docker run --rm -it --device /dev/mem sunxi-sample
```