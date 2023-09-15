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
using System.Globalization;
using System.Linq;
using LibreHardwareMonitor.Hardware;
using WeeHardwareStatServer.Models;
using WeeHardwareStatServer.Services.Interfaces;

namespace WeeHardwareStatServer.Services
{
    public class LibreHardwareMonitorService : IHardwareMonitorService
    {
        private readonly IComputer _computer;
        private readonly IVisitor _visitor;

        public LibreHardwareMonitorService(
            IComputer computer,
            IVisitor visitor)
        {
            _computer = computer;
            _visitor = visitor;
        }

        public HardwareInfo GetStats()
        {
            _computer.Accept(_visitor);
            var result = new HardwareInfo();
            float cpuClock = 0;
            float cpuTemperature = 0;
            var tempCount = 0;
            float amdTemperature = 0;
            var amdTempCount = 0;
            foreach (var hardware in _computer.Hardware)
            {
                hardware.Update();
                switch (hardware.HardwareType)
                {
                    case HardwareType.Cpu:
                        result.CpuName = hardware.Name;
                        break;
                    case HardwareType.GpuAmd:
                    case HardwareType.GpuNvidia:
                        //LibreHardwareMonitor is broken and can output NVIDIA twice in the name so temporary fix for now
                        result.GpuName = hardware.Name.Replace("NVIDIA NVIDIA", "NVIDIA");
                        break;
                }

                foreach (var sensor in hardware.Sensors)
                {
                    if (sensor.Value != null)
                    {
                        var value = Math.Round(sensor.Value.Value, 0)
                                        .ToString(CultureInfo.InvariantCulture);
                        switch ((sensor.Name, sensor.SensorType, hardware.HardwareType))
                        {
                            case (_, SensorType.Temperature, HardwareType.Cpu)
                                when sensor.Name.Contains("Package"):
                                result.CpuTemperature = value;
                                break;
                            case ("GPU Core", SensorType.Temperature, _):
                                result.GpuTemperature = value;
                                break;
                            case ("GPU Core", SensorType.Clock, _):
                                result.GpuCoreClock = value;
                                break;
                            case ("GPU Memory", SensorType.Clock, _):
                                result.GpuMemoryClock = value;
                                break;
                            case ("GPU Shader", SensorType.Clock, _):
                                result.GpuShaderClock = value;
                                break;
                            case (_, SensorType.Clock, HardwareType.Cpu)
                                when sensor.Name.Contains("Core") && cpuClock < sensor.Value:
                                cpuClock = sensor.Value.Value;
                                break;
                            case (_, SensorType.Temperature, HardwareType.Cpu)
                                when sensor.Name.Contains("Core"):
                                cpuTemperature += sensor.Value.Value;
                                tempCount++;
                                break;
                            case (_, SensorType.Temperature, HardwareType.Cpu)
                                when sensor.Name.Contains("CCD"):
                                amdTemperature += sensor.Value.Value;
                                amdTempCount++;
                                break;
                            case ("CPU Total", SensorType.Load, HardwareType.Cpu):
                                result.CpuLoad = value;
                                break;
                            case ("GPU Core", SensorType.Load, _):
                                result.GpuLoad = value;
                                break;
                            case ("Memory", SensorType.Load, HardwareType.Memory):
                                result.RamLoad = value;
                                break;
                            case ("GPU Memory", SensorType.Load, _):
                                result.GpuMemoryLoad = value;
                                break;
                            case ("Memory Available", SensorType.Data, HardwareType.Memory):
                                var availableRam = Math.Round((decimal)sensor.Value, 1);
                                result.RamAvailable =
                                    availableRam.ToString(CultureInfo.InvariantCulture);
                                break;
                            case ("Memory Used", SensorType.Data, HardwareType.Memory):
                                var usedRam = Math.Round((decimal)sensor.Value, 1);
                                result.RamUsed = usedRam.ToString(CultureInfo.InvariantCulture);
                                break;
                            case ("GPU Memory Total", SensorType.SmallData, _):
                                var gpuMemoryTotal = Math.Round((decimal)sensor.Value, 0);
                                result.GpuMemoryTotal =
                                    gpuMemoryTotal.ToString(CultureInfo.InvariantCulture);
                                break;
                            case ("GPU Memory Used", SensorType.SmallData, _):
                                var gpuMemoryUsed = Math.Round((decimal)sensor.Value, 0);
                                result.GpuMemoryUsed =
                                    gpuMemoryUsed.ToString(CultureInfo.InvariantCulture);
                                break;
                            case ("GPU", SensorType.Fan, _):
                                var gpuFanRpm = Math.Round((decimal)sensor.Value, 1);
                                result.GpuFanSpeedRpm =
                                    gpuFanRpm.ToString(CultureInfo.InvariantCulture);
                                break;
                            case ("GPU Fan", SensorType.Control, _):
                                var gpuFan = Math.Round((decimal)sensor.Value, 1);
                                result.GpuFanSpeedLoad =
                                    gpuFan.ToString(CultureInfo.InvariantCulture);
                                break;
                            case ("GPU Power", SensorType.Power, _):
                            case ("GPU Package", SensorType.Power, _):
                                var gpuPower = Math.Round((decimal)sensor.Value, 1);
                                result.GpuPower = gpuPower.ToString(CultureInfo.InvariantCulture);
                                break;
                            case ("CPU Fan", SensorType.Control, _):
                                var cpuFan = Math.Round((decimal)sensor.Value, 1);
                                result.CpuFanSpeedLoad =
                                    cpuFan.ToString(CultureInfo.InvariantCulture);
                                break;
                            case ("Upload Speed", SensorType.Throughput, HardwareType.Network)
                                when hardware.Name == "Ethernet":
                                result.EthernetUploadSpeed = GetNetworkSpeed(sensor.Value.Value);
                                break;
                            case ("Download Speed", SensorType.Throughput, HardwareType.Network)
                                when hardware.Name == "Ethernet":
                                result.EthernetDownloadSpeed = GetNetworkSpeed(sensor.Value.Value);
                                break;
                        }
                    }
                }
            }

            result.CpuClock = Math.Round(cpuClock, 0).ToString(CultureInfo.InvariantCulture);

            if (string.IsNullOrWhiteSpace(result.CpuTemperature))
                result.CpuTemperature = Math.Round((cpuTemperature / tempCount), 0)
                                            .ToString(CultureInfo.InvariantCulture);
            if (amdTempCount > 0 && result.CpuName.Contains("AMD"))
                result.CpuTemperature = Math.Round(
                                                (amdTemperature + cpuTemperature) /
                                                (amdTempCount + tempCount),
                                                0)
                                            .ToString(CultureInfo.InvariantCulture);
            return result;
        }

        private static string GetNetworkSpeed(float sensorValue)
        {
            var decimalValue = Math.Round((decimal)sensorValue, 0) * 8;
            switch (decimalValue)
            {
                case > 524288:
                    decimalValue = Math.Round(decimalValue / 1048576, 1);
                    return decimalValue.ToString(CultureInfo.InvariantCulture) +
                           "Mb/s";
                case > 512:
                    decimalValue = Math.Round(decimalValue / 1024, 1);
                    return decimalValue.ToString(CultureInfo.InvariantCulture) +
                           "Kb/s";
                default:
                    return decimalValue.ToString(CultureInfo.InvariantCulture) + "b/s";
            }
        }
    }
}