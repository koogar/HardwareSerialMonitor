using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.IO.Ports;
using System.Management;
using System.Reflection;
using System.Diagnostics;
using HardwareSerialMonitor.Properties;
using OpenHardwareMonitor;
using OpenHardwareMonitor.Hardware;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using IniParser;
using IniParser.Model;

namespace HardwareSerialMonitor
{
    public partial class Form1 : Form
    {
        private static Boolean isBT = Convert.ToBoolean(ReadINIData("isBT"));
        private static string BTDevice = Convert.ToString(ReadINIData("BTDevice"));
        private static Int32 dataCheckInterval = 3000;          // set the interval for the timers below to send data to the arduino
        private static string VID = Convert.ToString(ReadINIData("VendorID"));                     //set the device VID, this will be the default one to connect to (sparkfun pro micro)
        private static string PID = Convert.ToString(ReadINIData("ProductID"));                     //set the device PID, this will be the default one to connect to (sparkfun pro micro)        
        private static string devID = Convert.ToString(ReadINIData("DeviceID"));//string.Empty;             // this is a device ID string to contain the exact device ID
        OpenHardwareMonitor.Hardware.Computer thisComputer;     //set 'thisComputer' as the name of the instance for the dll
        private string Vid_Pid = "VID_" + VID + "&PID_" + PID;  //add the VID and PID together into string to check against the com port description later
        private string SPortVid_Pid = string.Empty;             //VID and PID string for the Com port in manual selection mode
        private bool isConnected = false;                //boolean to denote whether the default device is connected and port opened
        private bool isAttached = false;                    // boolean to denote whether the default device is attached to the computer
        private bool manualIsAttached = false;              // boolean to denote whether the manual device is attached to the computer
        private string portSelected = string.Empty;         // string to store the selected port in Manual mode
        private bool AutomaticPortSelect = true;            // boolean to denote whether the program has been set to Automatic connection mode
        private bool ManualPortSelect = false;           //boolean to denote whether the program has been set to manual connection mode
        private static SerialPort mySerialPort;                        //Port for the default device
        NotifyIcon ApplicationIcon;                             // notify icon for the notification bar
        Icon trayIcon;                                          // an icon instance to assign to the nofication bar
        private System.Windows.Forms.Timer connectionTimer1 = new System.Windows.Forms.Timer();// create a timer to check if the default device has been connected and send data to it
        private ContextMenuStrip menu = new ContextMenuStrip(); //create a menu strip for the notification icon

        public Form1()
        {
            thisComputer = new OpenHardwareMonitor.Hardware.Computer() { }; //initialise the dll instance as a new one 
            thisComputer.CPUEnabled = true;                                 //enable the datafield to be gathered
            thisComputer.GPUEnabled = true;
            thisComputer.HDDEnabled = true;
            thisComputer.MainboardEnabled = true;
            thisComputer.RAMEnabled = true;
            thisComputer.Open();                                            
            
            InitializeComponent();                              //Initialise the Form and its components

            trayIcon = Resources.TrayIcon1;         //set this file as the tray icon
            ApplicationIcon = new NotifyIcon();                 //initialise the Notification icon in the notify bar
            ApplicationIcon.Icon = trayIcon;                    //set the image icon as the icon set above
            ApplicationIcon.Visible = true;                     //make it visible
            ApplicationIcon.BalloonTipIcon = ToolTipIcon.Info;  //add icon bubble fields
            ApplicationIcon.BalloonTipText = "Hardware LCD Monitor";
            ApplicationIcon.BalloonTipTitle = "Hardware LCD Monitor";

            this.WindowState = FormWindowState.Minimized;  //start minimized
            this.ShowInTaskbar = false; // dont show icon on the taskbar
            this.Hide(); //Hide
            
            
            ApplicationIcon.ContextMenuStrip = menu;            //add the menu items to the menu strip
            ApplicationIcon.MouseUp += ApplicationIcon_MouseUp; //set an event manager for mouse right click 
            connectionTimer1.Interval = dataCheckInterval;       //sets the connectionTimer to "tick" in milliseconds to the int32 assigned
            connectionTimer1.Tick += connectionTimer1_Tick;     //event manager for each "tick"
            connectionTimer1.Start();                           //start the timer
            UsbDeviceNotifier.RegisterUsbDeviceNotification(this.Handle); // handle a notifier for usb devices
            checkDevice();                                      // check if the device is already attached (handler only looks after devices added or removed)
            CreateMenuItems();                                  //run the function to create the menu items for the notification icon
        }
        private void connectionTimer1_Tick(object sender, EventArgs e)
        {
            if (AutomaticPortSelect)
            {
                if (isAttached)
                    isConnected = ConnectToDevice(); //set the boolean to the result of the function
                if (isConnected)                 //if the boolean is true
                {
                    ApplicationIcon.Icon = Resources.TrayIconGreen;
                    if (trayIcon.Equals(Resources.TrayIcon1))
                        Debug.WriteLine("TrayIcon != TrayIcon1");
                    dataCheck();                 //run the datacheck function
                }
                else
                    ApplicationIcon.Icon = Resources.TrayIconRed;
            }

            if (ManualPortSelect) // if manualPortSelect is enabled
            {
                dataCheck(); // run the data check function
                if (!trayIcon.Equals(Resources.TrayIcon1))//if the tray icon is not the TrayIcon1
                {
                    ApplicationIcon.Icon = Resources.TrayIcon1; //make it TrayIcon1
                }
            }
        }
        private void ApplicationIcon_MouseUp(object sender, MouseEventArgs e)//the event manager for mouse right click on the notification icon
        {
            InvalidateMenu(menu);//run the function to wipe the menu and refresh it.
            if (e.Button == MouseButtons.Left) //if its a left click
            {
                MethodInfo mi = typeof(NotifyIcon).GetMethod("ShowContextMenu", BindingFlags.Instance | BindingFlags.NonPublic); // create and instantiate method to act as a right click
                mi.Invoke(ApplicationIcon, null);//invoke the method above
            }
        }
        void CreateMenuItems()
        {
            ToolStripMenuItem item;             // create an instance of a toolStripMenuItem called item
            ToolStripSeparator sep;             // create an instance of a toolStripMenuSeparator called sep
            String AutoText = String.Empty;
            item = new ToolStripMenuItem();     // instantiate item as a new toolstripmenu item
            if ((AutomaticPortSelect) && (mySerialPort != null))
            {
              //  Debug.WriteLine("PortName:" + mySerialPort);
                String AutoDeviceName = getDeviceName(mySerialPort.PortName.ToString());//change the menu item to the device name
                AutoText = "Automatic | " + AutoDeviceName + " (" + mySerialPort.PortName.ToString() +")";
            }
            else
                AutoText = "Automatic Mode";
            item.Text = AutoText;       // give it the text Automatic Mode
            if (AutomaticPortSelect)
                item.Checked = true;            // if the boolean is set, check the item
            else
                item.Checked = false;           // else set it as false
            item.Click += Item_Click;           // add a handler for clicking the menu item
            menu.Items.Add(item);               // add it to the menu

            item = new ToolStripMenuItem();     // instantiate item as a new toolstripmenuitem
            item.Text = "Serial Ports";         // set items text to "serial ports"
            menu.Items.Add(item);               // add item to the contextmenu menu

            string[] ports = SerialPort.GetPortNames(); // set up a string array called ports and set it to the list of port names
            
            foreach (string port in ports) // for each string in the array
            {
                string portName = port;
                string regexPattern = @"\D*(\d+)\D*";
                portName = "COM" + System.Text.RegularExpressions.Regex.Replace(portName, regexPattern, "$1");
                item = new ToolStripMenuItem();// instantiate a new toolstripmenuitem
                string deviceName = getDeviceName(portName);            // set the device's name to the function (passing the port name)
                item.Text = portName + " | " + deviceName;              // set the text to the string, which is the port name   
                if (portName == portSelected)                           
                    item.Checked = true;                            // if the port is the port that was selected, mark it as checked
                else
                    item.Checked = false;
                item.Click += new EventHandler((sender, e) => Selected_Serial(sender, e, portName)); //add a new event handler when clicking that item to run the function Selected_Serial with the port
                item.Image = Resources.Serial; //set the image for the item as the Serial image from the resources section
                menu.Items.Add(item);          //add the item to the contextmenu menu
            }
            sep = new ToolStripSeparator();   // instantiate a new toolstripseparator
            menu.Items.Add(sep);              // at the separator to the contextmenu
            item = new ToolStripMenuItem();   // instantiate a new toolstripmenuitem
            item.Text = "Refresh";            // set the text as refresh
            item.Click += refresh_Click;
            menu.Items.Add(item);             // add the item to the menu
            sep = new ToolStripSeparator();   // instantiate a new toolstripmenuseparator
            menu.Items.Add(sep);              // add the separator to the menu
            item = new ToolStripMenuItem();   // instantiate a new toolstripmenuitem
            item.Text = "About";
            item.Click += new System.EventHandler(About_Click); // add a handler to run the function About_Click()
            item.Image = Resources.info;      // set an image for the item
            menu.Items.Add(item);
            sep = new ToolStripSeparator();   // instantiate a new toolstripmenuseparator
            menu.Items.Add(sep);              // add the separator to the menu
            item = new ToolStripMenuItem();   // instantiate a new toolstripmenuitem
            item.Text = "Exit";               // give the text "exit"
            item.Click += new System.EventHandler(Exit_Click);  // add a handler to run the function Exit_Click()
            item.Image = Resources.Exit;      // set an image for the item
            menu.Items.Add(item);             // add it to the menu
        }
        private void refresh_Click(object sender, EventArgs e)
        {
            InvalidateMenu(menu);
            MethodInfo mi = typeof(NotifyIcon).GetMethod("ShowContextMenu", BindingFlags.Instance | BindingFlags.NonPublic); // create and instantiate method
            mi.Invoke(ApplicationIcon, null);//invoke the method above
        }
        private void About_Click(object sender, EventArgs e)
        {
            AboutBox1 aboutBox = new AboutBox1(); //create a new version of the AboutBox form (used as a template)
            aboutBox.Show(); //display it
        }
        private void Item_Click(object sender, EventArgs e)// automatic port selection handler
        {
            ManualPortSelect = false; // mark manualPortSelect as false
            AutomaticPortSelect = true;//mark automaticportselect as true
            portSelected = string.Empty;//set the portSelected as blank
            try
            {
                if (mySerialPort.IsOpen)//try close the port first
                {
                    mySerialPort.Close();
                }
                if (manualIsAttached)
                {
                    isAttached = true;
                    manualIsAttached = false;
                    Vid_Pid = SPortVid_Pid;//save the vid and pid from the manual selection
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error closing mySerialPort " + ex);
            }
        }
        void InvalidateMenu(ContextMenuStrip menu)
        {
            menu.Items.Clear();//clear the context menu 
            CreateMenuItems(); //run the function to repopulate it
        }
        void Selected_Serial(object sender, EventArgs e, string selected_port)
        {
            string regexPattern = @"\D*(\d+)\D*";
            selected_port = "COM" + System.Text.RegularExpressions.Regex.Replace(selected_port, regexPattern, "$1");
            manualIsAttached = true;//if the manual port device is attached
            SPortVid_Pid = getVidPid(selected_port); // save the device ID to a string using the function passing the port name
            devID = getDeviceID(selected_port);
            Int32 indexOfVID = SPortVid_Pid.IndexOf("_") + 1;
            Int32 indexOfPID = SPortVid_Pid.IndexOf("_", indexOfVID) + 1;
            Debug.WriteLine("SPortVid_Pid:"+SPortVid_Pid);
            Debug.WriteLine("SPortVid_Pid.Substring:" + SPortVid_Pid.Substring(indexOfVID));
            if (SPortVid_Pid.Contains("BTHENUM"))
            {
                VID = "";
                PID = "";
            }

            try
            {
                VID = SPortVid_Pid.Substring(indexOfVID, 4);
                PID = SPortVid_Pid.Substring(indexOfPID, 4);
                ModifyINIData("VendorID", VID.ToString());
                ModifyINIData("ProductID", PID.ToString());
            }
            catch (Exception f)
            {
                Debug.WriteLine("Exception setting VidPid in Selected_Serial():" + f);
            }
            ModifyINIData("DeviceID", devID.ToString());
            try
            {
                if (mySerialPort.IsOpen)// if the port is already open, close it
                {
                    mySerialPort.Close();
                }
            }
            catch { }
            if (AutomaticPortSelect)
            {
                AutomaticPortSelect = false;                                    //set the boolean to false this is checked when sending data to the device
            }
            if (!ManualPortSelect)
            {
                ManualPortSelect = true; // set the boolean to true, this is checked when sending data to the device
            }
            portSelected = selected_port;//save the port to a string
            Console.WriteLine("Selected port: " + selected_port);   // write the string to the console
            mySerialPort = new SerialPort(selected_port, 9600, Parity.None, 8, StopBits.One);// create a new port instance with values in the argument
            mySerialPort.ReadTimeout = 500;//set the timeouts
            mySerialPort.WriteTimeout = 500;
            mySerialPort.DtrEnable = true;
            mySerialPort.RtsEnable = true;
            try
            {
                if (!mySerialPort.IsOpen)//if the port created is not open
                {
                    mySerialPort.Open();//open it
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error opening mySerialPort " + ex);
            }
            //isConnected = true;
            //isAttached = true;
        }
        void Selected_Serial(string selected_port)//an overload of the function above, called when the manual device is reattached
        {
            string regexPattern = @"\D*(\d+)\D*";
            selected_port = "COM" + System.Text.RegularExpressions.Regex.Replace(selected_port, regexPattern, "$1");
            try
            {
                if (mySerialPort.IsOpen)
                {
                    mySerialPort.Close();
                }
            }
            catch { }
            if (AutomaticPortSelect)
            {
                AutomaticPortSelect = false;                                    //set the boolean to false this is checked when sending data to the device

            }
            if (!ManualPortSelect)
            {
                ManualPortSelect = true; // set the boolean to true, this is checked when sending data to the device
            }
            portSelected = selected_port;
            Console.WriteLine("Selected port: " + selected_port);   // write the string to the console
            mySerialPort = new SerialPort(selected_port, 9600, Parity.None, 8, StopBits.One);// create a new port instance with values in the argument
            mySerialPort.ReadTimeout = 500;//set the timeouts
            mySerialPort.WriteTimeout = 500;
            mySerialPort.DtrEnable = true;
            mySerialPort.RtsEnable = true;
            manualIsAttached = true;
            try
            {
                if (!mySerialPort.IsOpen)//if the port created is not open
                {
                    mySerialPort.Open();//open it
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error opening mySerialPort " + ex);
            }
        }
        private void dataCheck()//function overload with 0 arguments, called when a with the default serial port on a timer
        {
            string cpuTemp = "";
            string gpuTemp = "";
            string gpuLoad = "";
            string cpuLoad = "";
            string ramUsed = "";
            string cpuName = "";
            string gpuName = "";
            string gpuCoreClock = "";
            string gpuMemoryClock = "";
            string gpuShaderClock = "";
            string cpuClock = "";
            int highestCPUClock = 0;
            // enumerating all the hardware
            foreach (OpenHardwareMonitor.Hardware.IHardware hw in thisComputer.Hardware)// for each hardware item thisComputer
            {
                //Debug.WriteLine("Hardware Name="+hw.Name);
                //Debug.WriteLine("Checking: " + hw.HardwareType);
                if (hw.HardwareType.ToString().IndexOf("CPU") > -1)
                {
                    cpuName = "CPU:";
                    cpuName += hw.Name;
                }
                else if (hw.HardwareType.ToString().IndexOf("Gpu") > -1)
                {
                    gpuName = "GPU:";
                    gpuName += hw.Name;
                }
                hw.Update();                                                            //update it
                
                // searching for all sensors and adding data to listbox
                foreach (OpenHardwareMonitor.Hardware.ISensor s in hw.Sensors)          //for each sensor in the sensors part of the hardware
                {
                    //Console.WriteLine("Sensor: " + s.Name + " Type: " + s.SensorType + " Value: " + s.Value);
                    if (s.SensorType == OpenHardwareMonitor.Hardware.SensorType.Temperature)   // if the sensor type is a temperature sensor
                    {
                        //Debug.WriteLine("s.Name=" + s.Name);
                        if (s.Value != null)                                                  //if the value is not null
                        {
                            int curTemp = (int)s.Value;                                       //create a new int and set its value to the temperature value
                            switch (s.Name)                                                   // create a switch based on the sensor name
                            {
                                case "CPU Package":                                           // if the name is "CPU package"
                                    cpuTemp = curTemp.ToString();                             // set the string cpuTemp to the int value above converted to a string 
                                    break;                                                    // break from the switch so it doesnt run the case below
                                case "GPU Core":                                              //if the name is "GPU Core"
                                    gpuTemp = curTemp.ToString();
                                    break;
                            }
                        }
                    }
                    if (s.SensorType == OpenHardwareMonitor.Hardware.SensorType.Clock)   // if the sensor type is a temperature sensor
                    {
                        //Debug.WriteLine("s.Name=" + s.Name);
                        if (s.Value != null)                                                  //if the value is not null
                        {
                            int clockSpeed = (int)s.Value;                                       //create a new int and set its value to the temperature value
                            switch (s.Name)                                                   // create a switch based on the sensor name
                            {
                                // break from the switch so it doesnt run the case below
                                case "GPU Core":                                              //if the name is "GPU Core"
                                    gpuCoreClock = "|GCC"+clockSpeed.ToString();
                                    break;
                                case "GPU Memory":                                              //if the name is "GPU Memory"
                                    gpuMemoryClock = "|GMC"+clockSpeed.ToString();
                                    break;
                                case "GPU Shader":                                              //if the name is "GPU Shader"
                                    gpuShaderClock = "|GSC"+clockSpeed.ToString();
                                    break;
                            }
                            if (s.Name.IndexOf("CPU Core") > -1)
                            {
                                if (clockSpeed > highestCPUClock) // run through each iteration of CPU Core and if the speed is higher than the last save it
                                {
                                    highestCPUClock = clockSpeed;
                                    cpuClock = "|CHC"+highestCPUClock.ToString()+"|";
                                }
                            }
                        }
                    }
                    if (s.SensorType == OpenHardwareMonitor.Hardware.SensorType.Load)           // if the sensor type is a load value
                    {
                        if (s.Value != null)                                                    // if the value is not null
                        {
                            int curLoad = (int)s.Value;                                         // create a new int and set its value to the sensor value
                            switch (s.Name)                                                     //create a switch based on the name again
                            {
                                case "CPU Total":                                               //if the name is "CPU Total"
                                    cpuLoad = curLoad.ToString();                               //set the string cpuLoad to the int value converted to a string
                                    break;
                                case "GPU Core":
                                    gpuLoad = curLoad.ToString();
                                    break;
                            }
                        }
                    }
                    if (s.SensorType == OpenHardwareMonitor.Hardware.SensorType.Data)           // if the sensor is a data value etc.etc.
                    {
                        if (s.Value != null)
                        {
                            switch (s.Name)
                            {
                                case "Used Memory":                                             //if the name is "used memory"
                                    decimal decimalRam = Math.Round((decimal)s.Value, 1);       // create a new decimal and set the value to the sensor value a rounded to 1 decimal place
                                    ramUsed = decimalRam.ToString();                            // set the ramused string to the decimal converted to a string
                                    break;
                            }
                        }
                    }
                }
                if (cpuTemp == "") // if there is no cpuTemp assigned from earlier functions, get the average cpu temp
                {
                    foreach (OpenHardwareMonitor.Hardware.ISensor s in hw.Sensors)          //for each sensor in the sensors part of the hardware
                    {
                        int numTemps = 0;
                        int averageTemp = 0;
                        try
                        {
                            if (s.SensorType == OpenHardwareMonitor.Hardware.SensorType.Temperature)   // if the sensor type is a temperature sensor
                            {
                                if (s.Name.IndexOf("CPU Core") > -1)
                                {
                                    averageTemp = averageTemp + (int)s.Value;
                                    numTemps++;
                                }
                            }
                            if (numTemps > 0)
                            {
                                averageTemp = averageTemp / numTemps;
                                cpuTemp = averageTemp.ToString();
                            }
                        }
                        catch { }
                    }
                }
            }
            Debug.WriteLine("CPU Name:" + cpuName + " | GPU Name:" + gpuName);
            Debug.WriteLine(gpuCoreClock + gpuMemoryClock + gpuShaderClock + cpuClock);
            string stats = string.Empty;//create a new string and instantiate it as empty
            stats = "C" + cpuTemp + "c " + cpuLoad + "%|G" + gpuTemp + "c " + gpuLoad + "%|R" + ramUsed + "G|"; //write the strings to the new string along with separators and denotations the arduino can understand
            Debug.WriteLine(stats);//output the string to the debug console
            Debug.WriteLine(cpuName+gpuName);
            if (stats != string.Empty)//so long as its not empty
            {
                sendToArduino(stats+cpuName+gpuName+gpuCoreClock+gpuMemoryClock+gpuShaderClock+cpuClock);//send the string to the function
               // sendToArduino(cpuName+gpuName);
            }
        }
        private bool ConnectToDevice()//function to connect to the default device
        {
            //Debug.WriteLine("Attempting to Find Device");
            string[] portNames = SerialPort.GetPortNames(); //set a string array to the names of the ports
            string sInstanceName = string.Empty; // set an empty string to assign to the instance name of the serial port
            string sPortName = string.Empty;     // set an empty string to assign to the serial port name
            bool bFound = false;                // set a boolean to assign if the default port has been found
            for (int y = 0; y < portNames.Length; y++) // for every port that's available (a foreach would have also done here)
            {
                try //set a try to catch any exceptions accessing the management object searcher or opening the ports (if another program or instance of this program is running and is using that port it will cause an error)
                {
                    ManagementObjectSearcher searcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_PnPEntity WHERE ClassGuid='{4d36e978-e325-11ce-bfc1-08002be10318}'");//create a new ManagementObjectSearcher and instantiate it to the value results from the search function
                    foreach (ManagementObject queryObj in searcher.Get()) // for each result from the searcher above
                    {
                        sInstanceName = queryObj["PNPDeviceID"].ToString().ToUpper(); //set sInstanceName to the resulting instance name
                        if (devID != string.Empty)
                        {
                            //Debug.WriteLine("Checking DEV-ID");
                            if ((sInstanceName.IndexOf(Vid_Pid) > -1) && (sInstanceName.IndexOf(devID) > -1))//if the string Vid_Pid is present in the string
                            {
                                if ((isConnected == false) && (bFound == false))// if not already connected
                                {
                                    string name = queryObj["Name"].ToString();
                                    if (name.Contains("COM"))
                                    {
                                        int indexOfCOM = name.IndexOf("COM");
                                        int indexEndOfCOM = name.IndexOf(")", indexOfCOM);
                                        sPortName = name.Substring(indexOfCOM, (indexEndOfCOM - indexOfCOM));
                                        Debug.WriteLine("sPortName = " + sPortName);
                                    }
                                    //sPortName = queryObj["DeviceID"].ToString();// set the sPortName to the portname in the query
                                    mySerialPort = new SerialPort(sPortName, 9600, Parity.None, 8, StopBits.One);// create a new port instance with values in the argument
                                    mySerialPort.ReadTimeout = 500;//set the timeouts
                                    mySerialPort.WriteTimeout = 500;
                                    mySerialPort.DtrEnable = true;
                                    mySerialPort.RtsEnable = true;
                                    try
                                    {
                                        mySerialPort.Open();
                                        Debug.WriteLine(mySerialPort.PortName);
                                    }
                                    catch
                                    {
                                        Debug.WriteLine("Couldnt Open Serial Port");
                                    }
                                }
                                bFound = true;//set the boolean as true
                                Debug.WriteLine("DeviceFound1");
                            }
                            else // if the vid and pid are not found
                            {
                                //  bFound = false; // set the boolean as false
                            }
                        }
                        else
                        {
                            if (sInstanceName.IndexOf(Vid_Pid) > -1) //if the string Vid_Pid is present in the string
                            {
                                if (isConnected == false) // if not already connected
                                {
                                    sPortName = queryObj["PortName"].ToString();// set the sPortName to the portname in the query
                                    mySerialPort = new SerialPort(sPortName, 9600, Parity.None, 8, StopBits.One);// create a new port instance with values in the argument
                                    mySerialPort.ReadTimeout = 500;//set the timeouts
                                    mySerialPort.WriteTimeout = 500;
                                    mySerialPort.DtrEnable = true;
                                    mySerialPort.RtsEnable = true;
                                    //Debug.WriteLine("MySerial:" + mySerialPort.PortName);
                                }
                                bFound = true;//set the boolean as true
                                Debug.WriteLine("DeviceFound2");
                            }
                            else // if the vid and pid are not found
                            {
                                bFound = false; // set the boolean as false
                            }
                        }
                    }
                }
                catch (ManagementException e)
                {
                    System.Diagnostics.Debug.WriteLine("An error occurred while querying for WMI data: " + e.Message); //catch exceptions and output the error
                }
            }
            if (bFound) //if the boolean above is true
            {
                return true; // self explanitory
            }
            else
            {
                return false;
            }
        }
        private void sendToArduino(string arduinoData)//function to send the data to the arduino over the com port
        {
            if (AutomaticPortSelect)
            {
                if (isConnected)//if the default port is connected
                {
                    if (mySerialPort.IsOpen == false)//if the port is not open
                    {
                        try
                        {
                            mySerialPort.Open();//try open the port
                        }
                        catch (Exception e)
                        {
                            Debug.WriteLine("Error opening port: " + mySerialPort.PortName + Environment.NewLine + "Error: " + e.ToString());//catch any errors and output them
                        }
                    }
                    if (mySerialPort.IsOpen == true)// if the port is open
                    {
                        try
                        {
                            mySerialPort.WriteLine(arduinoData);//try to write the data to the port
                        }
                        catch (Exception e)
                        {
                            Debug.WriteLine("Error sending Serial Data: " + e.ToString());//catch any errors and output them
                            string errorString = e.ToString();
                            if (errorString.IndexOf("device is not connected") > -1)
                                Debug.WriteLine("Device Removed");
                            isConnected = false;
                        }
                    }
                }
            }
            else if (ManualPortSelect)//if a port has been manually selected
            {
                if (manualIsAttached)
                {
                    if (mySerialPort.IsOpen == false)//if the port is not open
                    {
                        try
                        {
                            mySerialPort.Open();//try open the port
                        }
                        catch (Exception e)
                        {
                            Debug.WriteLine("Error opening port: " + mySerialPort.PortName + Environment.NewLine + "Error: " + e.ToString());//catch any errors and output them
                        }
                    }
                    try
                    {
                        mySerialPort.WriteLine(arduinoData);//try write to the manual port
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine("Error sending Serial Data: " + e.ToString());//catch any errors
                        string errorString = e.ToString();
                        // ManualPortSelect = false;
                    }
                }
            }
        }
        private string getDeviceName(string port)
        {
            if (port == "COM1")// if the port is COM1 set the name to System port (as it generally is, might need to review this)
            {
                return "System Port";
            }
            string regexPattern = @"\D*(\d+)\D*";
            port = "COM" + System.Text.RegularExpressions.Regex.Replace(port, regexPattern, "$1");
            string deviceID = string.Empty;
            string deviceName = string.Empty;
            string sInstanceName = string.Empty; // set an empty string to assign to the instance name of the serial port
            string sPortName = string.Empty;     // set an empty string to assign to the serial port name
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_PnPEntity WHERE ClassGuid='{4d36e978-e325-11ce-bfc1-08002be10318}'");//create a new ManagementObjectSearcher and instantiate it to the value results from the search function
            try
            {
                foreach (ManagementObject queryObj in searcher.Get()) // for each result from the searcher above
                {
                    string queryObjDeviceName = queryObj["Name"].ToString();
                    if (queryObjDeviceName.Contains(port))
                    {
                        deviceName = queryObj["Description"].ToString();
                    }
                }
                if (deviceName == string.Empty)
                    deviceName = "(Name Not Available)";
            }
            catch
            {
                deviceName = "(Name Not Available)";
            }
            return deviceName;
        }
        private string getVidPid(string port)
        {
            if (port == "COM1")// if the port is COM1 set the name to System port (as it generally is, might need to review this)
            {
                return "VID_0000&PID_0000";
            }
            string regexPattern = @"\D*(\d+)\D*";
            port = "COM" + System.Text.RegularExpressions.Regex.Replace(port, regexPattern, "$1");
            string deviceID = string.Empty;
            string sPortDeviceID = string.Empty;
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_PnPEntity WHERE ClassGuid='{4d36e978-e325-11ce-bfc1-08002be10318}'");
            foreach (ManagementObject QueryObj in searcher.Get())
            {
                String deviceName = QueryObj["Name"].ToString();
                if (deviceName.Contains(port))
                {
                    sPortDeviceID = QueryObj["PNPDeviceID"].ToString();//save the device id to a string
                    Debug.WriteLine("SPortDeviceID from getVidPid:" + sPortDeviceID);
                    if (sPortDeviceID.Contains("BTHENUM"))
                    {
                        Int32 indexOfBT = sPortDeviceID.IndexOf("_");
                        sPortDeviceID = sPortDeviceID.Substring(0, indexOfBT);
                        System.Diagnostics.Debug.WriteLine("GetDeviceName - sPortDeviceID=" + sPortDeviceID);
                    }
                    else
                    {
                        Int32 indexOfVIDPID = sPortDeviceID.IndexOf("VID");//get the index position of "VID" in the string
                        sPortDeviceID = sPortDeviceID.Substring(indexOfVIDPID, 17);// get a substring of VID and PID numbers
                        System.Diagnostics.Debug.WriteLine("GetDeviceName - sPortDeviceID=" + sPortDeviceID);
                    }
                }
            }
            return sPortDeviceID;
        }
        private string getDeviceID(string port)
        {
            if (port == "COM1")// if the port is COM1 set the name to System port (as it generally is, might need to review this)
            {
                return "SystemPort";
            }
            string regexPattern = @"\D*(\d+)\D*";
            port = "COM" + System.Text.RegularExpressions.Regex.Replace(port, regexPattern, "$1");
            string deviceID = string.Empty;
            string sPortDeviceID = string.Empty;
            string sInstanceName = string.Empty; // set an empty string to assign to the instance name of the serial port
            string sPortName = string.Empty;     // set an empty string to assign to the serial port name
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_PnPEntity WHERE ClassGuid='{4d36e978-e325-11ce-bfc1-08002be10318}'");
            foreach (ManagementObject QueryObj in searcher.Get())
            {
                String deviceName = QueryObj["Name"].ToString();
                if (deviceName.Contains(port))
                {
                    deviceID = QueryObj["PNPDeviceID"].ToString();//save the device id to a string
                    if (deviceID.Contains("BTHENUM"))
                    {
                        Int32 indexOfBT = deviceID.IndexOf("_");
                        sPortDeviceID = deviceID.Substring(0, indexOfBT);
                    }
                    else
                    {
                        Int32 indexOfVIDPID = deviceID.IndexOf("VID");//get the index position of "VID" in the string
                        Int32 indexOfDevID = deviceID.IndexOf("\\", indexOfVIDPID);
                        Int32 indexOfDevIDEnd = deviceID.IndexOf("_", indexOfDevID);
                        Debug.WriteLine("deviceID:" + deviceID);
                        sPortDeviceID = deviceID.Substring(indexOfDevID + 1);
                        Debug.WriteLine("sPortDeviceID:" + sPortDeviceID);
                    }
                }
            }
            return sPortDeviceID;
        }
        public void Usb_DeviceRemoved(string deviceNameID)
        {
            if (deviceNameID.IndexOf(Vid_Pid) > -1)
            {
                Debug.WriteLine(Vid_Pid);
                System.Diagnostics.Debug.WriteLine("Default Device Removed");
                isConnected = false;
                isAttached = false;
                if (AutomaticPortSelect)
                {
                    try
                    {
                        mySerialPort.Dispose();
                        mySerialPort.Close();//try close the port
                    }
                    catch { }//catch any errors but dont bother outputting them
                }
            }
            if (deviceNameID.IndexOf(SPortVid_Pid) > -1)
            {
                System.Diagnostics.Debug.WriteLine("Manual Port Device Removed");
                manualIsAttached = false;
                if (ManualPortSelect)
                {
                    try
                    {
                        mySerialPort.Dispose();
                        mySerialPort.Close();//try close the port

                    }
                    catch { }//catch any errors but dont bother outputting them
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("Device:" + deviceNameID);
                System.Diagnostics.Debug.WriteLine("device:" + SPortVid_Pid);
            }
        }
        public void Usb_DeviceAdded(string deviceNameID)
        {
            if (deviceNameID.IndexOf(Vid_Pid) > -1)
            {
                System.Diagnostics.Debug.WriteLine("Default Device Attached");
                System.Threading.Thread.Sleep(1000);//wait a second for the device's com port to come online
                isAttached = true;//set the automatic device as attached
            }
            if (deviceNameID.IndexOf(SPortVid_Pid) > -1)
            {
                System.Diagnostics.Debug.WriteLine("Manual Port Device Attached");
                System.Threading.Thread.Sleep(1000);//wait a second for the device's com port to come online
                if (ManualPortSelect == true)//if its in manual mode
                    Selected_Serial(portSelected);//re assign the com port and open it
            }
        }
        protected override void WndProc(ref Message m)//function to handle the USB device notifications
        {
            base.WndProc(ref m);
            //System.Diagnostics.Debug.WriteLine(m.ToString());
            if (m.Msg == UsbDeviceNotifier.WmDevicechange)
            {
               // System.Diagnostics.Debug.WriteLine(m.ToString());
                switch ((int)m.WParam)
                {
                    case UsbDeviceNotifier.DbtDeviceremovecomplete:
                        DEV_BROADCAST_DEVICEINTERFACE hdrOut = (DEV_BROADCAST_DEVICEINTERFACE)m.GetLParam(typeof(DEV_BROADCAST_DEVICEINTERFACE));
                       // System.Diagnostics.Debug.WriteLine("HDROut:" + hdrOut.dbcc_name);
                        Usb_DeviceRemoved(hdrOut.dbcc_name); // this is where you do your magic
                        break;
                    case UsbDeviceNotifier.DbtDevicearrival:
                        DEV_BROADCAST_DEVICEINTERFACE hdrIn = (DEV_BROADCAST_DEVICEINTERFACE)m.GetLParam(typeof(DEV_BROADCAST_DEVICEINTERFACE));
                        //System.Diagnostics.Debug.WriteLine("HDRIn:" + hdrIn.dbcc_name);
                        Usb_DeviceAdded(hdrIn.dbcc_name); // this is where you do your magic
                        break;
                }
            }
        }
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]//sets the layout structure for the function below
        internal struct DEV_BROADCAST_DEVICEINTERFACE
        {
            // Data size.
            public int dbcc_size;
            // Device type.
            public int dbcc_devicetype;
            // Reserved data.
            public int dbcc_reserved;
            // Class GUID.
            public Guid dbcc_classguid;
            // Device name data.
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 255)]//manage the data in the next line
            public string dbcc_name;
        }
        public void checkDevice()//function to check if the default device is already attached once the program has started
        {
            string sInstanceName = string.Empty;

            ManagementObjectSearcher searcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_PnPEntity WHERE ClassGuid='{4d36e978-e325-11ce-bfc1-08002be10318}'");//create a new ManagementObjectSearcher and instantiate it to the value results from the search function
            foreach (ManagementObject queryObj in searcher.Get()) // for each result from the searcher above
            {
                //string sPortName = queryObj["DeviceID"].ToString(); //set sPortName to the port name
                sInstanceName = queryObj["PNPDeviceID"].ToString().ToUpper(); //set sInstanceName to the DeviceID
                if (sInstanceName.IndexOf(Vid_Pid) > -1)
                {
                    if (sInstanceName.IndexOf(devID) > -1)
                        isAttached = true;
                    else
                        isAttached = true;
                }
            }
            if (isAttached)
                isConnected = ConnectToDevice();
        }
        void Exit_Click(object sender, EventArgs e)
        {
            try
            {
                if (mySerialPort.IsOpen == true)//if the port is open close it
                    mySerialPort.Close();
            }
            catch { }

            this.Dispose();
            
            Application.Exit();//if the application has been closed
        }
        public static void CreateINIData()//create INI data file data
        {
            var data = new IniData();
            IniData createData = new IniData();
            FileIniDataParser iniParser = new FileIniDataParser();

            createData.Sections.AddSection("DeviceConfig");
            createData.Sections.GetSectionData("DeviceConfig").LeadingComments.Add("This is the configuration file for the Application");
            createData.Sections.GetSectionData("DeviceConfig").Keys.AddKey("VendorID", "0000");
            createData.Sections.GetSectionData("DeviceConfig").Keys.AddKey("ProductID", "0000");
            createData.Sections.GetSectionData("DeviceConfig").Keys.AddKey("DeviceID", "0");
            createData.Sections.GetSectionData("DeviceConfig").Keys.AddKey("isBT", "false");
            createData.Sections.GetSectionData("DeviceConfig").Keys.AddKey("BTDevice", "");
            iniParser.WriteFile("Config.ini", createData);
        }
        public static void ModifyINIData(String name, String value) // Modify INI data file data
        {
            int RetryTimes = 0;
            RetryIniModify:
            if (File.Exists("Config.ini"))
            {
                FileIniDataParser iniParser = new FileIniDataParser();
                IniData modifiedData = iniParser.ReadFile("Config.ini");
                modifiedData["DeviceConfig"][name] = value;
                iniParser.WriteFile("Config.ini", modifiedData);
            }
            else
            {
                if (RetryTimes == 0)
                {
                    CreateINIData();
                    RetryTimes = 1;
                    goto RetryIniModify;
                }
            }
        }
        public static String ReadINIData(String name) //Read INI data
        {
            string readIniData = null;
            int RetryTimes = 0;
            RetryIniRead:
            if (File.Exists("Config.ini"))
            {
                FileIniDataParser iniParser = new FileIniDataParser();
                IniData readData = iniParser.ReadFile("Config.ini");
                readIniData = readData["DeviceConfig"][name];
            }
            else
            {
                if (RetryTimes == 0)
                {
                    CreateINIData();
                    RetryTimes = 1;
                    goto RetryIniRead;
                }
            }
            return readIniData;
        }
    }
}
