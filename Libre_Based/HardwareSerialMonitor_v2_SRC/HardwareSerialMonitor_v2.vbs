REM : R.Hirst (tallmanLabs)
REM WshShell.Run chr(34) & "C:\Program Files (x86)\HardwareSerialMonitor_v2\HardwareSerialMonitor_v2.exe" & Chr(34), 0=SILENT , Run Till Told

Set WshShell = CreateObject("WScript.Shell" )
  WshShell.Run chr(34) & "C:\Program Files (x86)\HardwareSerialMonitor_v2\HardwareSerialMonitor_v2.exe" & Chr(34), 1 , TRUE
  Set WshShell = Nothing

