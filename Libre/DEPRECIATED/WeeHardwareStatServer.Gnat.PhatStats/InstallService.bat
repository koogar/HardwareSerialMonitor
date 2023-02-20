
set /P username=Enter username: 
set /P password=Enter password: 
sc create "Wee Hardware Stat Server" start=auto binPath=%~dp0WeeHardwareStatServer.exe type=own obj="%username%" password="%password%"
sc start "Wee Hardware Stat Server"