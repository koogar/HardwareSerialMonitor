
# Wee Hardware Stat Server (Gnat-Stats & Phat-Stats Compatible)
Copyright (C) 2021  Vinod Mishra
-----------------------------------
A tiny server that uses LibreHardwareMonitor to send data to a serial port (for use with Arduino). Planning to add more features soon.

This project is licensed under GPL v2.
LibreHardwareMonitory library is licensed under Mozilla Public License 2.0.


Following fields are supported for custom format
- CpuName
- CpuTemperature
- CpuLoad
- CpuFanSpeedLoad
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


## Installation
This is a simple console application and has built in support to be run as a Windows Service as well which would be the easiest way to get it working.

Please make sure you have the latest .net 5 runtime installed https://dotnet.microsoft.com/download/dotnet/5.0

Please change settings to fit your use case in appsetttings.json

Run 'InstallService.bat' as an administrator.


## Uninstallation
Run "DeleteService.bat" as an administrator

Warning!!!:
 
      You Must have a fixed folder location before you install the service. 
      If you want to move the install folder you have to delete the service first, then move the folder.
      You can then install service again from the new folder location.

## Future plan
- Release the counterpart to this for Aruduino that uses Json to communicate rather than a string
- Linux support 

#  Note
This was primarily written to use OpenHardwareMonitor to output stats to a serial port for use with Gnat Stats written by Rupert Hirst and Colin Conway.
https://hackaday.io/project/19018-gnat-stats-tiny-oled-pc-performance-monitor

However, the windows application that comes with it uses .net framework along with  OpenHardwareMonitor which was forked by LibreHardwareMonitor.

I decided to write a completely new version using LibreHardwareMonitor and .Net 5.

Please note that this project is licensed under GPLv2 so even though it is compatible with the Arduino code for Gnat Stats it shares no code with the original HardwareSerialMonitor application and unfortunately is incompatible with the distribution license for that project.
