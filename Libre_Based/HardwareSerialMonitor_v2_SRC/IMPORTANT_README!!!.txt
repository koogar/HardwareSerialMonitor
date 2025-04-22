HardwareSerialMonitor2 is based on Wee Hardware Stat Server  Copyright (C) 2021  Vinod Mishra

Modified for PhatStats/GnatStats by R.Hirst

https://tallmanlabs.com

----------------------------------------------------------------------------


Important Note: 

dotnet6 is required to run!!!

https://dotnet.microsoft.com/en-us/download/dotnet/9.0


After Installation got to the install directory and change the "properties" 
of the WeeHardwareStatServer.exe in "Compatibility" to 

"Run this program as administrator" 

before launch!!!

----------------------------------------------------------------------------

Edit the appsettings.json 


  "SerialPortSettings": {
    "Port": "COM3",  //Change to your Specific Arduino port

Run the HardwareSerialMonitor_v2.exe as Admin



To run on Windows Startup
--------------------------

To allow HardwareSerialMonitor_v2 to run on Windows startup…

the install location must be C:\Program Files (x86)\HardwareSerialMonitor_v2

Goto the Windows "Startup" folder here "%AppData%\Microsoft\Windows\Start Menu\Programs\Startup"

Place the .vbs file from the AutoRunOnWindowsStartup folder in the HardwareSerialMonitor_v2 directory, eg. "HardwareSerialMonitor_v2.vbs" (command window) or "HardwareSerialMonitor_v2Silent.vbs" (Silent operation, no command window) in the windows startup folder

Next time Windows runs, HardwareSerialMonitor_v2 will autostart on the last know USB port

Note:

If you changed/Moved the default installation directory you will need to edit the "HardwareSerialMonitor_v2.vbs" or "HardwareSerialMonitor_v2Silent.vbs" in notepad etc, to reflect those changes


