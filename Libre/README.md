
# Wee Hardware Stat Server (Gnat-Stats & Phat-Stats Compatible)
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



#  Note
This was primarily written to use OpenHardwareMonitor to output stats to a serial port for use with Gnat Stats written by Rupert Hirst and Colin Conway.
https://hackaday.io/project/19018-gnat-stats-tiny-oled-pc-performance-monitor

However, the windows application that comes with it uses .net framework along with  OpenHardwareMonitor which was forked by LibreHardwareMonitor.

I decided to write a completely new version using LibreHardwareMonitor and .Net 5.

Please note that this project is licensed under GPLv2 so even though it is compatible with the Arduino code for Gnat Stats it shares no code with the original HardwareSerialMonitor application and unfortunately is incompatible with the distribution license for that project.
