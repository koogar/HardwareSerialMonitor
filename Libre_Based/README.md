

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

Please make sure you have the latest .net 9 runtime installed https://dotnet.microsoft.com/download/dotnet/9.0

Change the COM port number to match your Arduino in appsettings.json (open with notepad)
Note: your Arduino COM port can change if you plug it in a different USB port so you may have to update the appsettings.json in the future

Always run HardwareSerialMonitor_v2.exe as administrator by changing the file properties!!!



To allow HardwareSerialMonitor_v2 to run on Windows startup…

1) the install location must be C:\Program Files (x86)\HardwareSerialMonitor_v2

2) Goto the Windows "Startup" folder here "%AppData%\Microsoft\Windows\Start Menu\Programs\Startup"


3) Place the .vbs file from the AutoRunOnWindowsStartup folder in the HardwareSerialMonitor_v2 directory, eg. "HardwareSerialMonitor_v2.vbs" (command window)  or "HardwareSerialMonitor_v2Silent.vbs" (Silent operation, no command window) 
in the windows startup folder




Next time Windows runs, HardwareSerialMonitor_v2 will autostart on the last know USB port

Note: 

If you changed/Moved the default installation directory you will need to edit the "HardwareSerialMonitor_v2.vbs" or "HardwareSerialMonitor_v2Silent.vbs" in notepad etc, to reflect those changes



