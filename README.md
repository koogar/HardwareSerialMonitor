
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


    Alternatively

---------------------------------------------------------------------------------------------------------
Autostart HardwareSerialMonitor.exe using Windows Task Sheduler
---------------------------------------------------------------------------------------------------------
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
