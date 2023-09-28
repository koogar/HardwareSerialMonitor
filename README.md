
  GnatStats / PhatStats PC Performance Monitor / HardwareSerialMonitor Windows Client  
  
  Rupert Hirst & Colin Conway © 2016-2021
  
  http://tallmanlabs.com  & http://runawaybrainz.blogspot.com/
  
 
  ![]( https://github.com/koogar/HardwareSerialMonitor/blob/main/images/HardwareSerialMonitor_App.jpg)


---------------------------------------------------------------------------------------------------------
Autostart HardwareSerialMonitor.exe using the HardwareSerialMonitor.VBS
---------------------------------------------------------------------------------------------------------
To allow HardwareSerialMonitor to run on Windows startup...

    1) Goto the Windows "Start-up" folder here "%AppData%\Microsoft\Windows\Start Menu\Programs\Startup"

    2) Right click in the "Start-up" folder and create a "New" shortcut


    3) Then browse for the "HardwareSerialMonitor.vbs" in the "C:\Program Files (x86)\HardwareSerialMonitor" folder.

    Name the shortcut to "HardwareSerialMonitor-Shortcut"

    Next time Windows runs, HardwareSerialMonitor will autostart on the last know USB port

    Note: 

    If you changed/Moved the defualt install directory you will need to edit the "HardwareSerialMonitor.vbs" 
    in notepad etc, to reflect those changes




Licence
-------
GPL v2

µVolume, Gnat-Stats, Phat-Stats & Hardware Serial Monitor Copyright (C) 2016 Colin Conway, Rupert Hirst and contributors

This program is free software; you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation; either version 2 of the License, or (at your option) any later version.

This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.

You should have received a copy of the GNU General Public License along with this program; If not, see http://www.gnu.org/licenses/.
