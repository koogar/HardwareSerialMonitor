#region License
// Wee Hardware Stat Server
// Copyright (C) 2021 Vinod Mishra and contributors
// 
// This program is free software; you can redistribute it and/or
// modify it under the terms of the GNU General Public License
// as published by the Free Software Foundation; either version 2
// of the License, or (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program; If not, see <http://www.gnu.org/licenses/>.
#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;

namespace WeeHardwareStatServer.Models
{
    public class HardwareInfo
    {
        public HardwareInfo()
        {
            HWiNFOStats = new Dictionary<string, string>();
        }

        [Description("CN")]
        public string CpuName { get; set; }

        [Description("CT")]
        public string CpuTemperature { get; set; }

        [Description("CL")]
        public string CpuLoad { get; set; }

        [Description("CF")]
        public string CpuFanSpeedLoad { get; set; }

        [Description("CC")]
        public string CpuClock { get; set; }

        [Description("GN")]
        public string GpuName { get; set; }

        [Description("GT")]
        public string GpuTemperature { get; set; }

        [Description("GL")]
        public string GpuLoad { get; set; }

        [Description("GCC")]
        public string GpuCoreClock { get; set; }

        [Description("GMC")]
        public string GpuMemoryClock { get; set; }

        [Description("GSC")]
        public string GpuShaderClock { get; set; }

        [Description("GMT")]
        public string GpuMemoryTotal { get; set; }

        [Description("GFSL")]
        public string GpuFanSpeedLoad { get; set; }

        [Description("GFSR")]
        public string GpuFanSpeedRpm { get; set; }

        [Description("GML")]
        public string GpuMemoryLoad { get; set; }

        [Description("GP")]
        public string GpuPower { get; set; }

        [Description("GMU")]
        public string GpuMemoryUsed { get; set; }

        [Description("RL")]
        public string RamLoad { get; set; }

        [Description("RU")]
        public string RamUsed { get; set; }

        [Description("RA")]
        public string RamAvailable { get; set; }

        [Description("EUS")]
        public string EthernetUploadSpeed { get; set; }

        [Description("EDS")]
        public string EthernetDownloadSpeed { get; set; }

        public Dictionary<string, string> HWiNFOStats { get; }

        public object this[string propertyName]
        {
            get
            {
                var myType = typeof(HardwareInfo);
                var myPropInfo = myType.GetProperty(propertyName);
                return myPropInfo?.GetValue(this, null);
            }
            set
            {
                var myType = typeof(HardwareInfo);
                var myPropInfo = myType.GetProperty(propertyName);
                if (myPropInfo != null)
                    myPropInfo.SetValue(this, value, null);
                else
                    HWiNFOStats[propertyName] = value.ToString();
            }

        }
    }
}