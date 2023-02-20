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
using System.Linq;
using System.Text.RegularExpressions;
using WeeHardwareStatServer.Models;

namespace WeeHardwareStatServer.Extensions
{
    public static class HardwareInfoExtensions
    {
        public static string FormatCustomOutput(
            this string self,
            HardwareInfo hardwareInfo,
            string nullValueReplacement = "")
        {
            var placeholders = Regex.Matches(self, @"\{(.*?)\}");
            foreach (Match placeholder in placeholders)
            {
                var placeholderValue = placeholder.Value;
                var placeholderPropertyName = placeholderValue.Replace("{", "").Replace("}", "");
                var property = hardwareInfo.GetType().GetProperty(placeholderPropertyName);
                var value = property?.GetValue(hardwareInfo)?.ToString() ??
                            nullValueReplacement;
                self = self.Replace(placeholderValue, value);
            }

            return self;
        }

        public static string FormatOutput(this HardwareInfo hardwareInfo)
        {
            var result = new List<string>();
            foreach (var prop in hardwareInfo.GetType().GetProperties())
            {
                var keyName = Attribute.IsDefined(prop, typeof(DescriptionAttribute))
                    ? (Attribute.GetCustomAttribute(prop, typeof(DescriptionAttribute)) as
                        DescriptionAttribute)?.Description
                    : null;
                if (!string.IsNullOrWhiteSpace(keyName))
                {
                    var value = prop.GetValue(hardwareInfo)?.ToString();
                    result.Add($"{keyName}:{value}");
                }
            }

            result.AddRange(
                hardwareInfo.HWiNFOStats.Select(
                    stat => $"{stat.Key}:{stat.Value}"));

            return $"{string.Join('#', result)}#|";
        }
    }
}
