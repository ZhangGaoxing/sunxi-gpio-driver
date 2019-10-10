FROM mcr.microsoft.com/dotnet/core/sdk:2.1-stretch-arm32v7 AS build
# FROM mcr.microsoft.com/dotnet/core/sdk:2.1 AS build
WORKDIR /app

# publish app
COPY src .
WORKDIR /app/SunxiGpioDriver.Samples
RUN dotnet restore
RUN dotnet publish -c release -r linux-arm -o out

## run app
FROM mcr.microsoft.com/dotnet/core/runtime:2.1-stretch-slim-arm32v7 AS runtime
# FROM mcr.microsoft.com/dotnet/core/runtime:2.1 AS runtime
WORKDIR /app
COPY --from=build /app/SunxiGpioDriver.Samples/out ./

ENTRYPOINT ["dotnet", "SunxiGpioDriver.Samples.dll"]