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

namespace WeeHardwareStatServer.Models
{
    public class OutputSettings
    {
        public bool EnableHWiNFO { get; set; }
        public bool CustomOutput { get; set; }
        public string CustomOutputFormat { get; set; }
        public int OutputInterval { get; set; }
        public string HWiNFORegistryKey { get; set; }
        public List<HWiNFOStat> HWiNFOStats { get; set; }
    }
}