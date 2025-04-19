

# HardwareSerialMonitor_v2 based on Wee Hardware Stat Server (Gnat-Stats & Phat-Stats Compatible)
Copyright (C) 2021  Vinod Mishra
-----------------------------------
A tiny server that uses LibreHardwareMonitor to send data to a serial port (for use with Arduino). Planning to add more features soon.

This project is licensed under GPL v2.
LibreHardwareMonitor library is licensed under Mozilla Public License 2.0.


Following fields are supported for custom format
- CpuName
- CpuTemperature
- CpuLoad
- CpuClock
- GpuName
- GpuTemperature
- GpuLoad
- GpuCoreClock
- GpuMemoryClock
- GpuShaderClock
- GpuMemoryTotal
- GpuFanSpeedLoad
- GpuFanSpeedRpm
- GpuMemoryLoad
- GpuPower
- GpuMemoryUsed
- RamLoad
- RamUsed
- RamAvailable






#  Installation

Please make sure you have the latest .net 9 runtime installed https://dotnet.microsoft.com/download/dotnet/5.0

Change the COM port number to match your Arduino in appsetttings.json (open with notepad)
Note: your Arduino COM port can change if you plug it in a different USB port so you may have to update the appsetttings.json in the future

Run HardwareSerialMonitor_v2.exe as administrator.



# Or optionally Install as a service which will run silently at startup.


This is a simple console application and has built in support to be run as a Windows Service as well which would be the easiest way to get it working.

Please change settings to fit your use case in appsetttings.json


Run 'InstallService.bat' as an administrator.

Uninstallation
Run "DeleteService.bat" as an administrator.
