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

using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.Win32;
using WeeHardwareStatServer.Models;
using WeeHardwareStatServer.Services.Interfaces;

namespace WeeHardwareStatServer.Services
{
    public class HWiNFOHardwareMonitorService : IHWiNFOHardwareMonitorService
    {
        public HardwareInfo AddHWiNFOStats(
            HardwareInfo stats,
            List<HWiNFOStat> hWiNFOStats,
            string registryKey)
        {
            var readings = new Dictionary<(string, string), string>();

            using (var key = Registry.CurrentUser.OpenSubKey(registryKey))
            {
                if (key == null)
                    return stats;

                var indexes = key.GetValueNames().Where(k => k.Contains("Label"))
                                 .Select(k => k.Replace("Label", ""));

                foreach (var index in indexes)
                {
                    var label = key.GetValue("Label" + index)?.ToString();
                    var sensor = key.GetValue("Sensor" + index)?.ToString();
                    var value = key.GetValue("ValueRaw" + index)?.ToString();

                    if (string.IsNullOrWhiteSpace(label) || string.IsNullOrWhiteSpace(sensor) ||
                        string.IsNullOrWhiteSpace(value))
                        continue;

                    if (decimal.TryParse(value, out var decimalValue))
                        value = decimal.Round(decimalValue).ToString(CultureInfo.InvariantCulture);

                    readings.Add((label, sensor), value);
                }
            }

            if (!readings.Any())
                return stats;

            foreach (var hWiNFOStat in hWiNFOStats)
            {
                if (readings.ContainsKey((hWiNFOStat.ReadingName, hWiNFOStat.SensorName)))
                {
                    stats[hWiNFOStat.StatName] =
                        readings[(hWiNFOStat.ReadingName, hWiNFOStat.SensorName)];
                }
            }

            return stats;
        }
    }
}

