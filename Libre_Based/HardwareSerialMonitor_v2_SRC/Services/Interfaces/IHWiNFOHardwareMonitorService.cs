using System.Collections.Generic;
using HardwareSerialMonitor_v2.Models;

namespace HardwareSerialMonitor_v2.Services.Interfaces
{
    public interface IHWiNFOHardwareMonitorService
    {
        HardwareInfo AddHWiNFOStats(
            HardwareInfo stats,
            List<HWiNFOStat> hWiNFOStats,
            string registryKey);
    }
}