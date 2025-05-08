## [HardwareSerialMonitor GitHub Repository](https://www.google.com/maps/search/HardwareSerialMonitor+GitHub+Repository)

[HardwareSerialMonitor](https://github.com/koogar/HardwareSerialMonitor) is a Windows-based application developed by Rupert Hirst and Colin Conway (Tallman Labs) designed to transmit PC hardware performance statistics over a serial connection. This tool is particularly useful for displaying real-time system metrics on external microcontroller-driven displays such as those used in Gnat-Stats, Phat-Stats, or Tacho-Stats projects. 

https://github.com/koogar/Phat-Stats - https://github.com/koogar/Gnat-Stats

### Key Features

* **Real-Time Hardware Monitoring**: Utilizes the OpenHardwareMonitorLib.dll to collect data on CPU, GPU, RAM, and other system components.

* **Serial Communication**: Sends collected data over USB or Bluetooth serial ports, facilitating integration with microcontroller-based displays.

* **Broad CPU Support**: Includes experimental support for various Intel architectures, including Jasper Lake, Rocket Lake, Alder Lake (desktop and mobile), Raptor Lake, Meteor Lake, Arrow Lake, and Panther Lake.

* **Auto-Start Configuration**: Can be configured to launch automatically on Windows startup using a provided VBScript or through Windows Task Scheduler.

### Compatibility

* **Operating Systems**: Compatible with Windows 7, 10, and 11 (64-bit).

* **.NET Framework**: Requires .NET Framework 4.8.

* **Microcontroller Integration**: Designed to work seamlessly with Arduino-based projects like Gnat-Stats and Phat-Stats, which display system metrics on OLED or TFT screens.


### Installation and Setup

* **Installer**: Available as a Windows installer executable.([GitHub][5])

* **Auto-Start Setup**: Instructions are provided for setting up the application to run at startup using either a VBScript shortcut in the Startup folder or by configuring a task in Windows Task Scheduler.

For more detailed information, source code, and downloads, visit the [HardwareSerialMonitor GitHub repository](https://github.com/koogar/HardwareSerialMonitor).



  GnatStats / PhatStats PC Performance Monitor / HardwareSerialMonitor Windows Client  
  Rupert Hirst & Colin Conway © 2016-2025
  
  http://tallmanlabs.com  & http://runawaybrainz.blogspot.com/
  
 
  ![]( https://github.com/koogar/HardwareSerialMonitor/blob/main/images/HardwareSerialMonitor_App.jpg)



---

✅ Run HardwareSerialMonitor as Administrator (always):
Locate HardwareSerialMonitor.exe

Right-click → Properties

Go to Compatibility tab

Check [✔] Run this program as administrator

Click OK

---

## 🖥️ **Auto-start HardwareSerialMonitor via VBS Script (Startup Folder Method):**

1. Open **File Explorer** and navigate to:

   ```
   %AppData%\Microsoft\Windows\Start Menu\Programs\Startup
   ```

   *(You can paste this into the address bar and press Enter.)*

2. **Right-click** inside the **Startup** folder → select **New → Shortcut**

3. In the **location field**, browse to:

   ```
   C:\Program Files (x86)\HardwareSerialMonitor\HardwareSerialMonitor.vbs
   ```

   *(or adjust the path if you installed it elsewhere)*

4. Name the shortcut:

   ```
   HardwareSerialMonitor-Shortcut
   ```

✅ Done!
→ Now, every time Windows starts, `HardwareSerialMonitor.vbs` will run, which in turn launches `HardwareSerialMonitor.exe` on the **last known USB port**.

---

### ⚠️ **Note:**

If you moved or installed the app in a different folder, you must **edit `HardwareSerialMonitor.vbs`** in Notepad (or another editor) to update the file path to match your installation directory.

---

Alternatively Auto-Start via Task Scheduler.

### 🚀 **Enable Auto-Start at System Login (via Task Scheduler):**

1. Open **Start Menu** → Search for **Task Scheduler** → Open it
2. Click **Create Task**

#### **General Tab:**

* **Name**: *(anything you like)*
* Check **\[✔] Run only when user is logged on**
* Check **\[✔] Run with highest privileges**
* **Configure for**: Windows 10

#### **Triggers Tab:**

* Click **New**
* **Begin the task**: *At log on*
* **\[✔] Specific user**: *(select your user account)*

#### **Actions Tab:**

* Click **New**
* **Action**: *Start a program*
* **Program/script**: *Browse to `HardwareSerialMonitor.exe`*

#### **Conditions Tab:**

* *(Optional)* Uncheck **\[ ] Start the task only if the computer is on AC power**

#### **Settings Tab:**

* **\[✔] Allow task to be run on demand**
* *(Optional)* **\[✔] Stop the task if it runs longer than...** *(set as needed)*

---

✅ Done! Now **HardwareSerialMonitor** will auto-run at login with administrator privileges.


  Licence
  -------
  
  GPL v2
  
Gnat-Stats, Phat-Stats, Tacho-Stats, uVolume & HardwareSerialMonitor 
Copyright (C) 2016  Colin Conway, Rupert Hirst and contributors
 
This program is free software; you can redistribute it and/or
modify it under the terms of the GNU General Public License
as published by the Free Software Foundation; either version 2
of the License, or (at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program; If not, see <http://www.gnu.org/licenses/>.

---

