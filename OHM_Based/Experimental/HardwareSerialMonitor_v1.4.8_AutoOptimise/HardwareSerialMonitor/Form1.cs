using HardwareSerialMonitor.Properties;
using IniParser;
using IniParser.Model;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Ports;
using System.Management;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Microsoft.Win32;

namespace HardwareSerialMonitor
{
    public partial class Form1 : Form
    {
        private static Boolean isBT = Convert.ToBoolean(ReadINIData("isBT"));
        private static string BTDevice = Convert.ToString(ReadINIData("BTDevice"));
        private static Int32 dataCheckInterval = 3000;          // set the interval for the timers below to send data to the arduino
        private static string VID = Convert.ToString(ReadINIData("VendorID"));  //set the device VID, this will be the default one to connect to
        private static string PID = Convert.ToString(ReadINIData("ProductID"));   //set the device PID, this will be the default one to connect to        
        private static string devID = Convert.ToString(ReadINIData("DeviceID"));    // this is a device ID string to contain the exact device ID
        OpenHardwareMonitor.Hardware.Computer thisComputer;    // instance for the hardware monitoring library
        private string Vid_Pid = "VID_" + VID + "&PID_" + PID;     // used to check against the com port description
        private string SPortVid_Pid = string.Empty;                //VID and PID string for manual mode
        private bool isConnected = false;                          // denotes whether the default device is connected
        private bool isAttached = false;                           // denotes whether the default device is attached
        private bool manualIsAttached = false;                     // denotes whether the manual device is attached
        private string portSelected = string.Empty;                // the selected port in manual mode
        private bool AutomaticPortSelect = true;                   // whether Automatic mode is active
        private bool ManualPortSelect = false;                     // whether Manual mode is active
        private static SerialPort mySerialPort;                    // port for the default device
        NotifyIcon ApplicationIcon;                              // notification icon for the taskbar
        Icon trayIcon;                                           // tray icon
        private System.Windows.Forms.Timer connectionTimer1 = new System.Windows.Forms.Timer(); // timer to check device connection
        private ContextMenuStrip menu = new ContextMenuStrip();    // tray context menu

        public Form1()
        {
            thisComputer = new OpenHardwareMonitor.Hardware.Computer() { };
            thisComputer.CPUEnabled = true;
            thisComputer.GPUEnabled = true;
            thisComputer.HDDEnabled = true;
            thisComputer.MainboardEnabled = true;
            thisComputer.RAMEnabled = true;
            thisComputer.Open();

            InitializeComponent();

            trayIcon = Resources.TrayIcon1;
            ApplicationIcon = new NotifyIcon();
            ApplicationIcon.Icon = trayIcon;
            ApplicationIcon.Visible = true;
            ApplicationIcon.BalloonTipIcon = ToolTipIcon.Info;
            ApplicationIcon.BalloonTipText = "HSM";
            ApplicationIcon.BalloonTipTitle = "HSM";

            // Hide Form1 on startup
            this.WindowState = FormWindowState.Minimized;
            this.ShowInTaskbar = false;
            this.FormBorderStyle = FormBorderStyle.FixedToolWindow;
            this.ControlBox = false;
            this.Opacity = 0;
            this.Hide();

            ApplicationIcon.ContextMenuStrip = menu;
            ApplicationIcon.MouseUp += ApplicationIcon_MouseUp;
            connectionTimer1.Interval = dataCheckInterval;
            connectionTimer1.Tick += connectionTimer1_Tick;
            connectionTimer1.Start();
            UsbDeviceNotifier.RegisterUsbDeviceNotification(this.Handle);
            checkDevice();
            CreateMenuItems();
        }

        // Optimized connection tick method
        private void connectionTimer1_Tick(object sender, EventArgs e)
        {
            if (AutomaticPortSelect)
            {
                // Attempt connection only if not currently connected and device is attached
                if (!isConnected && isAttached)
                {
                    isConnected = ConnectToDevice();
                }

                // If connected and the serial port is open, update tray icon and check data
                if (isConnected && mySerialPort != null && mySerialPort.IsOpen)
                {
                    ApplicationIcon.Icon = Resources.TrayIconGreen;
                    dataCheck();
                }
                else
                {
                    ApplicationIcon.Icon = Resources.TrayIconRed;
                }
            }
            else if (ManualPortSelect) // Manual mode
            {
                dataCheck();
                if (!trayIcon.Equals(Resources.TrayIcon1))
                {
                    ApplicationIcon.Icon = Resources.TrayIcon1;
                }
            }
        }

        private void ApplicationIcon_MouseUp(object sender, MouseEventArgs e)
        {
            InvalidateMenu(menu);
            if (e.Button == MouseButtons.Left)
            {
                MethodInfo mi = typeof(NotifyIcon).GetMethod("ShowContextMenu", BindingFlags.Instance | BindingFlags.NonPublic);
                mi.Invoke(ApplicationIcon, null);
            }
        }

        void CreateMenuItems()
        {
            // "Run at Startup" menu item
            ToolStripMenuItem runAtStartupItem = new ToolStripMenuItem("Run at Startup")
            {
                CheckOnClick = true,
                Checked = IsAutorunEnabled()
            };
            runAtStartupItem.Click += (s, e) =>
            {
                SetAutorun(runAtStartupItem.Checked);
            };
            menu.Items.Add(runAtStartupItem);
            menu.Items.Add(new ToolStripSeparator());

            ToolStripMenuItem item;
            ToolStripSeparator sep;
            string AutoText = string.Empty;
            item = new ToolStripMenuItem();
            if ((AutomaticPortSelect) && (mySerialPort != null))
            {
                string AutoDeviceName = getDeviceName(mySerialPort.PortName.ToString());
                AutoText = "Automatic | " + AutoDeviceName + " (" + mySerialPort.PortName.ToString() + ")";
            }
            else
            {
                AutoText = "Automatic Mode";
            }
            item.Text = AutoText;
            item.Checked = AutomaticPortSelect;
            item.Click += Item_Click;
            menu.Items.Add(item);

            item = new ToolStripMenuItem();
            item.Text = "Serial Ports Available";
            sep = new ToolStripSeparator();
            menu.Items.Add(sep);
            menu.Items.Add(item);

            string[] ports = SerialPort.GetPortNames();
            foreach (string port in ports)
            {
                string portName = port;
                string regexPattern = @"\D*(\d+)\D*";
                portName = "COM" + System.Text.RegularExpressions.Regex.Replace(portName, regexPattern, "$1");
                item = new ToolStripMenuItem();
                string deviceName = getDeviceName(portName);
                item.Text = portName + " | " + deviceName;
                item.Checked = portName == portSelected;
                item.Click += (sender, e) => Selected_Serial(sender, e, portName);
                item.Image = Resources.Serial;
                menu.Items.Add(item);
            }
            sep = new ToolStripSeparator();
            menu.Items.Add(sep);
            item = new ToolStripMenuItem();
            item.Text = "Refresh";
            item.Click += refresh_Click;
            menu.Items.Add(item);
            sep = new ToolStripSeparator();
            menu.Items.Add(sep);
            item = new ToolStripMenuItem();
            item.Text = "About";
            item.Click += new EventHandler(About_Click);
            item.Image = Resources.info;
            menu.Items.Add(item);
            sep = new ToolStripSeparator();
            menu.Items.Add(sep);
            item = new ToolStripMenuItem();
            item.Text = "Exit";
            item.Click += new EventHandler(Exit_Click);
            item.Image = Resources.Exit;
            menu.Items.Add(item);
        }

        private void refresh_Click(object sender, EventArgs e)
        {
            InvalidateMenu(menu);
            MethodInfo mi = typeof(NotifyIcon).GetMethod("ShowContextMenu", BindingFlags.Instance | BindingFlags.NonPublic);
            mi.Invoke(ApplicationIcon, null);
        }

        private void About_Click(object sender, EventArgs e)
        {
            AboutBox1 aboutBox = new AboutBox1();
            aboutBox.Show();
        }

        private void Item_Click(object sender, EventArgs e)
        {
            ManualPortSelect = false;
            AutomaticPortSelect = true;
            portSelected = string.Empty;
            try
            {
                if (mySerialPort.IsOpen)
                {
                    mySerialPort.Close();
                }
                if (manualIsAttached)
                {
                    isAttached = true;
                    manualIsAttached = false;
                    Vid_Pid = SPortVid_Pid;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error closing mySerialPort " + ex);
            }
        }

        void InvalidateMenu(ContextMenuStrip menu)
        {
            menu.Items.Clear();
            CreateMenuItems();
        }

        void Selected_Serial(object sender, EventArgs e, string selected_port)
        {
            string regexPattern = @"\D*(\d+)\D*";
            selected_port = "COM" + System.Text.RegularExpressions.Regex.Replace(selected_port, regexPattern, "$1");
            manualIsAttached = true;
            SPortVid_Pid = getVidPid(selected_port);
            devID = getDeviceID(selected_port);
            int indexOfVID = SPortVid_Pid.IndexOf("_") + 1;
            int indexOfPID = SPortVid_Pid.IndexOf("_", indexOfVID) + 1;
            Debug.WriteLine("SPortVid_Pid:" + SPortVid_Pid);
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
                if (mySerialPort.IsOpen)
                {
                    mySerialPort.Close();
                }
            }
            catch { }
            if (AutomaticPortSelect)
            {
                AutomaticPortSelect = false;
            }
            if (!ManualPortSelect)
            {
                ManualPortSelect = true;
            }
            portSelected = selected_port;
            Console.WriteLine("Selected port: " + selected_port);
            mySerialPort = new SerialPort(selected_port, 9600, Parity.None, 8, StopBits.One);
            mySerialPort.ReadTimeout = 500;
            mySerialPort.WriteTimeout = 500;
            mySerialPort.DtrEnable = true;
            mySerialPort.RtsEnable = true;
            try
            {
                if (!mySerialPort.IsOpen)
                {
                    mySerialPort.Open();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error opening mySerialPort " + ex);
            }
        }

        void Selected_Serial(string selected_port)
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
                AutomaticPortSelect = false;
            }
            if (!ManualPortSelect)
            {
                ManualPortSelect = true;
            }
            portSelected = selected_port;
            Console.WriteLine("Selected port: " + selected_port);
            mySerialPort = new SerialPort(selected_port, 9600, Parity.None, 8, StopBits.One);
            mySerialPort.ReadTimeout = 500;
            mySerialPort.WriteTimeout = 500;
            mySerialPort.DtrEnable = true;
            mySerialPort.RtsEnable = true;
            manualIsAttached = true;
            try
            {
                if (!mySerialPort.IsOpen)
                {
                    mySerialPort.Open();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error opening mySerialPort " + ex);
            }
        }

        private void dataCheck()
        {
            string cpuName = "CPU";
            string cpuTemp = "C";
            string cpuLoad = "c";
            string CpuFanSpeedLoad = "";
            string ramLoad = "RL";
            string ramUsed = "R";
            string ramAvailable = "RA";
            string gpuName = "GPU";
            string gpuTemp = "G";
            string gpuLoad = "c";
            string gpuCoreClock = "GCC";
            string gpuMemoryClock = "GMC";
            string gpuShaderClock = "GSC";
            string gpuMemTotal = "GMT";
            string gpuMemUsed = "GMU";
            string gpuMemLoad = "GML";
            string gpuFanSpeedLoad = "GFANL";
            string gpuFanSpeedRPM = "GRPM";
            string gpuPower = "GPWR";
            string cpuClock = "";
            int highestCPUClock = 0;

            foreach (OpenHardwareMonitor.Hardware.IHardware hw in thisComputer.Hardware)
            {
                if (hw.HardwareType.ToString().IndexOf("CPU") > -1)
                {
                    cpuName = "CPU:" + hw.Name;
                }
                else if (hw.HardwareType.ToString().IndexOf("Gpu") > -1)
                {
                    gpuName = "GPU:" + hw.Name;
                }
                hw.Update();
                foreach (OpenHardwareMonitor.Hardware.ISensor s in hw.Sensors)
                {
                    if (s.SensorType == OpenHardwareMonitor.Hardware.SensorType.Temperature)
                    {
                        if (s.Value != null)
                        {
                            int curTemp = (int)s.Value;
                            switch (s.Name)
                            {
                                case "CPU Package":
                                    cpuTemp = curTemp.ToString();
                                    break;
                                case "GPU Core":
                                    gpuTemp = curTemp.ToString();
                                    break;
                            }
                        }
                    }

                    if (s.SensorType == OpenHardwareMonitor.Hardware.SensorType.Clock)
                    {
                        if (s.Value != null)
                        {
                            int clockSpeed = (int)s.Value;
                            switch (s.Name)
                            {
                                case "GPU Core":
                                    gpuCoreClock = "|GCC" + clockSpeed.ToString();
                                    break;
                                case "GPU Memory":
                                    gpuMemoryClock = "|GMC" + clockSpeed.ToString();
                                    break;
                                case "GPU Shader":
                                    gpuShaderClock = "|GSC" + clockSpeed.ToString();
                                    break;
                            }
                            if (s.Name.IndexOf("CPU Core") > -1)
                            {
                                if (clockSpeed > highestCPUClock)
                                {
                                    highestCPUClock = clockSpeed;
                                    cpuClock = "|CHC" + highestCPUClock.ToString() + "|";
                                }
                            }
                        }
                    }

                    if (s.SensorType == OpenHardwareMonitor.Hardware.SensorType.Load)
                    {
                        if (s.Value != null)
                        {
                            int curLoad = (int)s.Value;
                            switch (s.Name)
                            {
                                case "CPU Total":
                                    cpuLoad = curLoad.ToString();
                                    break;
                                case "GPU Core":
                                    gpuLoad = curLoad.ToString();
                                    break;
                                case "Memory":
                                    ramLoad = curLoad.ToString();
                                    break;
                                case "GPU Memory":
                                    gpuMemLoad = curLoad.ToString();
                                    break;
                            }
                        }
                    }

                    if (s.SensorType == OpenHardwareMonitor.Hardware.SensorType.Data)
                    {
                        if (s.Value != null)
                        {
                            switch (s.Name)
                            {
                                case "Available Memory":
                                    decimal decimalAram = Math.Round((decimal)s.Value, 1);
                                    ramAvailable = decimalAram.ToString();
                                    break;
                            }
                        }
                    }

                    if (s.SensorType == OpenHardwareMonitor.Hardware.SensorType.Data)
                    {
                        if (s.Value != null)
                        {
                            switch (s.Name)
                            {
                                case "Used Memory":
                                    decimal decimalUram = Math.Round((decimal)s.Value, 1);
                                    ramUsed = decimalUram.ToString();
                                    break;
                            }
                        }
                    }

                    if (s.SensorType == OpenHardwareMonitor.Hardware.SensorType.SmallData)
                    {
                        if (s.Value != null)
                        {
                            switch (s.Name)
                            {
                                case "GPU Memory Total":
                                    decimal decimalGtram = Math.Round((decimal)s.Value, 0);
                                    gpuMemTotal = decimalGtram.ToString();
                                    break;
                            }
                        }
                    }

                    if (s.SensorType == OpenHardwareMonitor.Hardware.SensorType.SmallData)
                    {
                        if (s.Value != null)
                        {
                            switch (s.Name)
                            {
                                case "GPU Memory Used":
                                    decimal decimalGuram = Math.Round((decimal)s.Value, 0);
                                    gpuMemUsed = decimalGuram.ToString();
                                    break;
                            }
                        }
                    }

                    if (s.SensorType == OpenHardwareMonitor.Hardware.SensorType.Fan)
                    {
                        if (s.Value != null)
                        {
                            switch (s.Name)
                            {
                                case "GPU":
                                    decimal decimalfanRPM = Math.Round((decimal)s.Value, 1);
                                    gpuFanSpeedRPM = decimalfanRPM.ToString();
                                    break;
                            }
                        }
                    }

                    if (s.SensorType == OpenHardwareMonitor.Hardware.SensorType.Control)
                    {
                        if (s.Value != null)
                        {
                            switch (s.Name)
                            {
                                case "GPU Fan":
                                    decimal decimalGfan = Math.Round((decimal)s.Value, 1);
                                    gpuFanSpeedLoad = decimalGfan.ToString();
                                    break;
                            }
                        }
                    }

                    if (s.SensorType == OpenHardwareMonitor.Hardware.SensorType.Power)
                    {
                        if (s.Value != null)
                        {
                            switch (s.Name)
                            {
                                case "GPU Power":
                                    decimal decimalGpwr = Math.Round((decimal)s.Value, 1);
                                    gpuPower = decimalGpwr.ToString();
                                    break;
                            }
                        }
                    }

                    if (s.SensorType == OpenHardwareMonitor.Hardware.SensorType.Control)
                    {
                        if (s.Value != null)
                        {
                            switch (s.Name)
                            {
                                case "CPU Fan":
                                    decimal decimalCfan = Math.Round((decimal)s.Value, 1);
                                    CpuFanSpeedLoad = decimalCfan.ToString();
                                    break;
                            }
                        }
                    }
                }

                if (cpuTemp == "")
                {
                    foreach (OpenHardwareMonitor.Hardware.ISensor s in hw.Sensors)
                    {
                        int numTemps = 0;
                        int averageTemp = 0;
                        try
                        {
                            if (s.SensorType == OpenHardwareMonitor.Hardware.SensorType.Temperature)
                            {
                                if (s.Name.IndexOf("CPU Core") > -1)
                                {
                                    averageTemp += (int)s.Value;
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
            string stats = "C" + cpuTemp + "c " + cpuLoad + "%|G" + gpuTemp + "c " + gpuLoad + "%|R" + ramUsed + "GB|RA" + ramAvailable +
                "|RL" + ramLoad + "|GMT" + gpuMemTotal + "|GMU" + gpuMemUsed + "|GML" + gpuMemLoad + "|GFANL" + gpuFanSpeedLoad +
                "|GRPM" + gpuFanSpeedRPM + "|GPWR" + gpuPower + "|";
            Debug.WriteLine(stats);
            Debug.WriteLine(cpuName + gpuName);
            if (stats != string.Empty)
            {
                sendToArduino(stats + cpuName + gpuName + gpuCoreClock + gpuMemoryClock + gpuShaderClock + cpuClock);
            }
        }

        private bool ConnectToDevice()
        {
            string[] portNames = SerialPort.GetPortNames();
            string sInstanceName = string.Empty;
            string sPortName = string.Empty;
            bool bFound = false;
            for (int y = 0; y < portNames.Length; y++)
            {
                try
                {
                    ManagementObjectSearcher searcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_PnPEntity WHERE ClassGuid='{4d36e978-e325-11ce-bfc1-08002be10318}'");
                    foreach (ManagementObject queryObj in searcher.Get())
                    {
                        sInstanceName = queryObj["PNPDeviceID"].ToString().ToUpper();
                        if (devID != string.Empty)
                        {
                            if ((sInstanceName.IndexOf(Vid_Pid) > -1) && (sInstanceName.IndexOf(devID) > -1))
                            {
                                if ((isConnected == false) && (bFound == false))
                                {
                                    string name = queryObj["Name"].ToString();
                                    if (name.Contains("COM"))
                                    {
                                        int indexOfCOM = name.IndexOf("COM");
                                        int indexEndOfCOM = name.IndexOf(")", indexOfCOM);
                                        sPortName = name.Substring(indexOfCOM, (indexEndOfCOM - indexOfCOM));
                                        Debug.WriteLine("sPortName = " + sPortName);
                                    }
                                    mySerialPort = new SerialPort(sPortName, 9600, Parity.None, 8, StopBits.One);
                                    mySerialPort.ReadTimeout = 500;
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
                                bFound = true;
                                Debug.WriteLine("DeviceFound1");
                            }
                        }
                        else
                        {
                            if (sInstanceName.IndexOf(Vid_Pid) > -1)
                            {
                                if (isConnected == false)
                                {
                                    sPortName = queryObj["PortName"].ToString();
                                    mySerialPort = new SerialPort(sPortName, 9600, Parity.None, 8, StopBits.One);
                                    mySerialPort.ReadTimeout = 500;
                                    mySerialPort.WriteTimeout = 500;
                                    mySerialPort.DtrEnable = true;
                                    mySerialPort.RtsEnable = true;
                                }
                                bFound = true;
                                Debug.WriteLine("DeviceFound2");
                            }
                            else
                            {
                                bFound = false;
                            }
                        }
                    }
                }
                catch (ManagementException e)
                {
                    Debug.WriteLine("An error occurred while querying for WMI data: " + e.Message);
                }
            }
            return bFound;
        }

        private void sendToArduino(string arduinoData)
        {
            if (AutomaticPortSelect)
            {
                if (isConnected)
                {
                    if (!mySerialPort.IsOpen)
                    {
                        try
                        {
                            mySerialPort.Open();
                        }
                        catch (Exception e)
                        {
                            Debug.WriteLine("Error opening port: " + mySerialPort.PortName + Environment.NewLine + "Error: " + e.ToString());
                        }
                    }
                    if (mySerialPort.IsOpen)
                    {
                        try
                        {
                            mySerialPort.WriteLine(arduinoData);
                        }
                        catch (Exception e)
                        {
                            Debug.WriteLine("Error sending Serial Data: " + e.ToString());
                            if (e.ToString().IndexOf("device is not connected") > -1)
                                Debug.WriteLine("Device Removed");
                            isConnected = false;
                        }
                    }
                }
            }
            else if (ManualPortSelect)
            {
                if (manualIsAttached)
                {
                    if (!mySerialPort.IsOpen)
                    {
                        try
                        {
                            mySerialPort.Open();
                        }
                        catch (Exception e)
                        {
                            Debug.WriteLine("Error opening port: " + mySerialPort.PortName + Environment.NewLine + "Error: " + e.ToString());
                        }
                    }
                    try
                    {
                        mySerialPort.WriteLine(arduinoData);
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine("Error sending Serial Data: " + e.ToString());
                    }
                }
            }
        }

        private string getDeviceName(string port)
        {
            if (port == "COM1")
            {
                return "System Port";
            }
            string regexPattern = @"\D*(\d+)\D*";
            port = "COM" + System.Text.RegularExpressions.Regex.Replace(port, regexPattern, "$1");
            string deviceName = string.Empty;
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_PnPEntity WHERE ClassGuid='{4d36e978-e325-11ce-bfc1-08002be10318}'");
            try
            {
                foreach (ManagementObject queryObj in searcher.Get())
                {
                    string queryObjDeviceName = queryObj["Name"].ToString();
                    if (queryObjDeviceName.Contains(port))
                    {
                        deviceName = queryObj["Description"].ToString();
                    }
                }
                if (string.IsNullOrEmpty(deviceName))
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
            if (port == "COM1")
            {
                return "VID_0000&PID_0000";
            }
            string regexPattern = @"\D*(\d+)\D*";
            port = "COM" + System.Text.RegularExpressions.Regex.Replace(port, regexPattern, "$1");
            string sPortDeviceID = string.Empty;
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_PnPEntity WHERE ClassGuid='{4d36e978-e325-11ce-bfc1-08002be10318}'");
            foreach (ManagementObject queryObj in searcher.Get())
            {
                string deviceName = queryObj["Name"].ToString();
                if (deviceName.Contains(port))
                {
                    sPortDeviceID = queryObj["PNPDeviceID"].ToString();
                    Debug.WriteLine("SPortDeviceID from getVidPid:" + sPortDeviceID);
                    if (sPortDeviceID.Contains("BTHENUM"))
                    {
                        int indexOfBT = sPortDeviceID.IndexOf("_");
                        sPortDeviceID = sPortDeviceID.Substring(0, indexOfBT);
                    }
                    else
                    {
                        int indexOfVIDPID = sPortDeviceID.IndexOf("VID");
                        sPortDeviceID = sPortDeviceID.Substring(indexOfVIDPID, 17);
                    }
                }
            }
            return sPortDeviceID;
        }

        private string getDeviceID(string port)
        {
            if (port == "COM1")
            {
                return "SystemPort";
            }
            string regexPattern = @"\D*(\d+)\D*";
            port = "COM" + System.Text.RegularExpressions.Regex.Replace(port, regexPattern, "$1");
            string sPortDeviceID = string.Empty;
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_PnPEntity WHERE ClassGuid='{4d36e978-e325-11ce-bfc1-08002be10318}'");
            foreach (ManagementObject queryObj in searcher.Get())
            {
                string deviceName = queryObj["Name"].ToString();
                if (deviceName.Contains(port))
                {
                    string deviceID = queryObj["PNPDeviceID"].ToString();
                    if (deviceID.Contains("BTHENUM"))
                    {
                        int indexOfBT = deviceID.IndexOf("_");
                        sPortDeviceID = deviceID.Substring(0, indexOfBT);
                    }
                    else
                    {
                        int indexOfVIDPID = deviceID.IndexOf("VID");
                        int indexOfDevID = deviceID.IndexOf("\\", indexOfVIDPID);
                        sPortDeviceID = deviceID.Substring(indexOfDevID + 1);
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
                Debug.WriteLine("Default Device Removed");
                isConnected = false;
                isAttached = false;
                if (AutomaticPortSelect)
                {
                    try
                    {
                        mySerialPort.Dispose();
                        mySerialPort.Close();
                    }
                    catch { }
                }
            }
            if (deviceNameID.IndexOf(SPortVid_Pid) > -1)
            {
                Debug.WriteLine("Manual Port Device Removed");
                manualIsAttached = false;
                if (ManualPortSelect)
                {
                    try
                    {
                        mySerialPort.Dispose();
                        mySerialPort.Close();
                    }
                    catch { }
                }
            }
            else
            {
                Debug.WriteLine("Device:" + deviceNameID);
                Debug.WriteLine("device:" + SPortVid_Pid);
            }
        }

        public void Usb_DeviceAdded(string deviceNameID)
        {
            if (deviceNameID.IndexOf(Vid_Pid) > -1)
            {
                Debug.WriteLine("Default Device Attached");
                System.Threading.Thread.Sleep(1000);
                isAttached = true;
            }
            if (deviceNameID.IndexOf(SPortVid_Pid) > -1)
            {
                Debug.WriteLine("Manual Port Device Attached");
                System.Threading.Thread.Sleep(1000);
                if (ManualPortSelect == true)
                    Selected_Serial(portSelected);
            }
        }

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);
            if (m.Msg == UsbDeviceNotifier.WmDevicechange)
            {
                switch ((int)m.WParam)
                {
                    case UsbDeviceNotifier.DbtDeviceremovecomplete:
                        DEV_BROADCAST_DEVICEINTERFACE hdrOut = (DEV_BROADCAST_DEVICEINTERFACE)m.GetLParam(typeof(DEV_BROADCAST_DEVICEINTERFACE));
                        Usb_DeviceRemoved(hdrOut.dbcc_name);
                        break;
                    case UsbDeviceNotifier.DbtDevicearrival:
                        DEV_BROADCAST_DEVICEINTERFACE hdrIn = (DEV_BROADCAST_DEVICEINTERFACE)m.GetLParam(typeof(DEV_BROADCAST_DEVICEINTERFACE));
                        Usb_DeviceAdded(hdrIn.dbcc_name);
                        break;
                }
            }
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        internal struct DEV_BROADCAST_DEVICEINTERFACE
        {
            public int dbcc_size;
            public int dbcc_devicetype;
            public int dbcc_reserved;
            public Guid dbcc_classguid;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 255)]
            public string dbcc_name;
        }

        public void checkDevice()
        {
            string sInstanceName = string.Empty;
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_PnPEntity WHERE ClassGuid='{4d36e978-e325-11ce-bfc1-08002be10318}'");
            foreach (ManagementObject queryObj in searcher.Get())
            {
                sInstanceName = queryObj["PNPDeviceID"].ToString().ToUpper();
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
                if (mySerialPort.IsOpen)
                    mySerialPort.Close();
            }
            catch { }
            this.Dispose();
            Application.Exit();
        }

        public static void CreateINIData()
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

        public static void ModifyINIData(String name, String value)
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

        public static String ReadINIData(String name)
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

        // --- Methods for Run at Startup ---
        private bool IsAutorunEnabled()
        {
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", false))
            {
                return key?.GetValue("HardwareSerialMonitor") != null;
            }
        }

        private void SetAutorun(bool enable)
        {
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true))
            {
                if (enable)
                {
                    key.SetValue("HardwareSerialMonitor", Application.ExecutablePath);
                }
                else
                {
                    key.DeleteValue("HardwareSerialMonitor", false);
                }
            }
        }
        // --- End of autorun methods ---
    }
}
