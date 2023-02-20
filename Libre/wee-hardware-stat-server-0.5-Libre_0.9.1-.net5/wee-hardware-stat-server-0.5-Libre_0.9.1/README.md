# Wee Hardware Stat Server

A tiny server that uses LibreHardwareMonitor and HWInfo to send data to a serial port (for use with Arduino). Planning to add more features soon.

This project is licensed under GPL v2.
LibreHardwareMonitory library is licensed under Mozilla Public License 2.0.

This application supports sends hardware stats in a string output to a configured serial port in appsettings file. 

## Configuration
All the settings live in appsettings.json

Following can be configured
 - SerialPortSettings: All the settings for serial port for Arduino live under this node.
 - OutputSettings: Hardware stat source and output format can be configured in this node.
	- EnableHWiNFO: Enables HWiNFO integration. More information about this is under its own section.
	- OutputInterval: Output interval in milliseconds.
	- CustomOutput: (true/false) Disable the standard string output and choose another formatted output.
	- CustomOutputFormat: As explained above this can be any string with the fields mentioend below in {} to output stats in a custom format.
	- HWiNFOStats: Settings for the HWiNFO stats that need to be overriden .

### Output format
The default output format is of Key1:Value1#Key2:Value2#|
Key values can be found in Description attribute of HardwareInfo.cs.

This can be changed to any custom format and supports the following fields
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

### HWiNFO
To enable HWiNFO:
- Please install from https://www.hwinfo.com/ and make sure you install the driver from "Driver Management" tab in its settings.
- Enable "Auto Start" in "General / User Interface" section.
- Please note that you only need the sensors part of HWiNFO for this. 
- In Configure Sensors, goto "HWiNFO Gadget" and check "Enable reporting to Gadget" and "Report value in Gadget" for each reading
  you want to support.

Once this is done you can set "EnableHWInfo" to true in app settings and define a section called "HWiNFOStats" as shown below
```json
"HWiNFOStats": [
      {
        "StatName": "CpuTemperature",
        "SensorName": "ASUS ROG CROSSHAIR VIII DARK HERO (Nuvoton NCT6798D)",
        "ReadingName": "CPU"
      }
    ]
```
- StatName is the field you want to override from the above or a new key you want to add in your output that is not a standard field.
- SensorName is the the name of the overall sensor you can get form HWiNFO sensor screen.
- ReadingName is the exact name of the reading you want to ouptput.

## Installation
This is a simple console application and has built in support to be run as a Windows Service as well which would be the easiest way to get it working.

Please make sure you have the latest .net 5 runtime installed https://dotnet.microsoft.com/download/dotnet/5.0

Please change settings to fit your use case in appsetttings.json

Run 'InstallService.bat' as an administrator.


## Uninstallation
Run "DeleteService.bat" as an administrator.


#  Note
This was primarily written to use LibreHardwareMonitor to output stats to a serial port for use with Gnat Stats written by Rupert Hirst and Colin Conway.
https://hackaday.io/project/19018-gnat-stats-tiny-oled-pc-performance-monitor

However, the windows application that comes with it uses .net framework along with  OpenHardwareMonitor which was forked by LibreHardwareMonitor.

I decided to write a completely new version using LibreHardwareMonitor and .Net 5.

Please note that this project is licensed under GPLv2 so even though it is compatible with the Arduino code for Gnat Stats it shares no code with the original HardwareSerialMonitor application and unfortunately is incompatible with the distribution license for that project.
