
  GnatStats / PhatStats PC Performance Monitor / HardwareSerialMonitor Windows Client  
  
  Rupert Hirst & Colin Conway © 2016-2021
  
  http://tallmanlabs.com  & http://runawaybrainz.blogspot.com/
  
 
  ![]( https://github.com/koogar/HardwareSerialMonitor/blob/main/images/HardwareSerialMonitor_App.jpg)
  
---------------------------------------------------------------------------------------------------------
Autostart HardwareSerialMonitor.exe
-----------------------------------
Locate Hardware Monitor executable> Right-Click> Properties> Compatibility> Run this program as Admin [X]

![](https://github.com/koogar/HardwareSerialMonitor/blob/main/images/you-need-to-be-admin-you-can-do-it-300x300.jpg)

Enable auto-start on system log-in
----------------------------------

Start Menu > Search for "Task Scheduler"

    Create Task

General Tab:

    Name: Whatever you like

    [X]: Run only when user is logged on

    [X]: Run with highest privileges

    Select: Configure for: Windows 10


Triggers Tab> New:

    Begin the task, Select: At log on

    [X]: Specific User:

Actions Tab> New:

    Action, Select: "Start a program"

    Program/script: > Browse the HardwareSerialMonitor.exe

Conditions Tab:

    [optional] Disable "Start the task only if the computer is on AC power"


Settings Tab:

    To Enable,  [X] : "Allow task to be run on demand"
    
    To Disable, [X] : "Stop the task if it runs longer than"

---------------------------------------------------------------------------------------------------------

Licence
-------
GPL v2

µVolume, Gnat-Stats, Phat-Stats & Hardware Serial Monitor Copyright (C) 2016 Colin Conway, Rupert Hirst and contributors

This program is free software; you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation; either version 2 of the License, or (at your option) any later version.

This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.

You should have received a copy of the GNU General Public License along with this program; If not, see http://www.gnu.org/licenses/.
