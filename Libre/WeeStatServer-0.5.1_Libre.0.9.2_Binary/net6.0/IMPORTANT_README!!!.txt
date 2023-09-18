Wee Hardware Stat Server 
Copyright (C) 2021  Vinod Mishra

Modified for PhatStats/GnatStats by R.Hirst

https://tallmanlabs.com

----------------------------------------------------------------------------


Important Note: 

dotnet6 is required to run!!!

https://dotnet.microsoft.com/en-us/download/dotnet/6.0


After Installation got to the install directory and change the "properties" 
of the WeeHardwareStatServer.exe in "Compatiability" to 

"Run this program as administrator" 

before launch!!!

----------------------------------------------------------------------------

Edit the appsettings.json 


  "SerialPortSettings": {
    "Port": "COM3",  //Change to your Specific Arduino port

Run the WeeHardwareStatServer.exe as Admin


