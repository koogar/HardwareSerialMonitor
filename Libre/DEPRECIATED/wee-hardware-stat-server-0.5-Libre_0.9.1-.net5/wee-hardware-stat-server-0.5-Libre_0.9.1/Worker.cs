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
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WeeHardwareStatServer.Extensions;
using WeeHardwareStatServer.Models;
using WeeHardwareStatServer.Services.Interfaces;

namespace WeeHardwareStatServer
{
    public class Worker : BackgroundService
    {
        private readonly IHardwareMonitorService _hardwareMonitorService;
        private readonly IHWiNFOHardwareMonitorService _hWiNFOHardwareMonitorService;
        private readonly OutputSettings _outputSettings;
        private readonly ISerialPortService _serialPortService;
        private readonly ILogger<Worker> _logger;

        public Worker(
            IHardwareMonitorService hardwareMonitorService,
            IHWiNFOHardwareMonitorService hWiNfoHardwareMonitorService,
            OutputSettings outputSettings,
            ISerialPortService serialPortService,
            ILogger<Worker> logger)
        {
            _outputSettings = outputSettings;
            _hardwareMonitorService = hardwareMonitorService;
            _hWiNFOHardwareMonitorService = hWiNfoHardwareMonitorService;
            _serialPortService = serialPortService;
            _logger = logger;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Sending data at: {time}", DateTimeOffset.Now);
                var stats = _hardwareMonitorService.GetStats();
                if (_outputSettings.EnableHWiNFO)
                {
                    stats = _hWiNFOHardwareMonitorService.AddHWiNFOStats(
                        stats,
                        _outputSettings.HWiNFOStats,
                        _outputSettings.HWiNFORegistryKey);
                }

                var data = _outputSettings.CustomOutput
                    ? _outputSettings.CustomOutputFormat.FormatCustomOutput(stats, "0")
                    : stats.FormatOutput();
                _serialPortService.SendData(data);
                Thread.Sleep(_outputSettings.OutputInterval);
            }

            return Task.CompletedTask;
        }
    }
}
