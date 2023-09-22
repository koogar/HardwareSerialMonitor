Set WshShell = CreateObject("WScript.Shell" ) 
  WshShell.Run chr(34) & "C:\Program Files (x86)\HardwareSerialMonitor\HardwareSerialMonitor.exe" & Chr(34), 0 
  Set WshShell = Nothing