# Wee Hardware Stat Server

A tiny server that uses LibreHardwareMonitor to send data to a serial port (for use with Arduino). Planning to add more features soon.

This project is licensed under GPL v2.
LibreHardwareMonitory library is licensed under Mozilla Public License 2.0.

This application supports sends hardware stats in a string output to a configured serial port in appsettings file. 
The string format can be changed from the default of Key1:Value1#Key2:Value2#|
to any custom format by changing appsettings file. 

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
- EthernetUploadSpeed
- EthernetDownloadSpeed

## Installation
This is a simple console application and has built in support to be run as a Windows Service as well which would be the easiest way to get it working.

Please make sure you have the latest .net 5 runtime installed https://dotnet.microsoft.com/download/dotnet/5.0

Please change settings to fit your use case in appsetttings.json

Run 'InstallService.bat' as an administrator.


## Uninstallation
Run "DeleteService.bat" as an administrator

## Future plan
- Release the counterpart to this for Aruduino that uses Json to communicate rather than a string
- Linux support 

#  Note
This was primarily written to use LibreHardwareMonitor to output stats to a serial port for use with Gnat Stats written by Rupert Hirst and Colin Conway.
https://hackaday.io/project/19018-gnat-stats-tiny-oled-pc-performance-monitor

However, the windows application that comes with it uses .net framework along with  OpenHardwareMonitor which was forked by LibreHardwareMonitor.

I decided to write a completely new version using LibreHardwareMonitor and .Net 5.

Please note that this project is licensed under GPLv2 so even though it is compatible with the Arduino code for Gnat Stats it shares no code with the original HardwareSerialMonitor application and unfortunately is incompatible with the distribution license for that project.
