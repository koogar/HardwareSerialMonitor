using System.Collections.Generic;
using WeeHardwareStatServer.Models;

namespace WeeHardwareStatServer.Services.Interfaces
{
    public interface IHWiNFOHardwareMonitorService
    {
        HardwareInfo AddHWiNFOStats(
            HardwareInfo stats,
            List<HWiNFOStat> hWiNFOStats,
            string registryKey);
    }
}