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

using LibreHardwareMonitor.Hardware;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using WeeHardwareStatServer.Models;
using WeeHardwareStatServer.Services;
using WeeHardwareStatServer.Services.Interfaces;

namespace WeeHardwareStatServer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        private static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseWindowsService()
                .ConfigureAppConfiguration(
                    (_, configApp) => configApp.AddCommandLine(args))
                .ConfigureServices(
                    (hostContext, services) => ConfigureServices(
                        hostContext.Configuration,
                        services));

        private static void ConfigureServices(
            IConfiguration configuration,
            IServiceCollection services)
        {
            services.AddHostedService<Worker>();
            services.AddSingleton(GetConfigSection<OutputSettings>(configuration));
            services.AddSingleton(GetConfigSection<SerialPortSettings>(configuration));
            services.AddSingleton<IVisitor, UpdateVisitor>();
            services.AddSingleton<IHardwareMonitorService, LibreHardwareMonitorService>();
            services.AddSingleton<IHWiNFOHardwareMonitorService, HWiNFOHardwareMonitorService>();
            services.AddSingleton<ISerialPortService, SerialPortService>();
            var computer = new Computer
            {
                IsCpuEnabled = true,
                IsGpuEnabled = true,
                IsMemoryEnabled = true,
                IsMotherboardEnabled = true,
                IsControllerEnabled = true,
                IsNetworkEnabled = true, 
                IsStorageEnabled = true
            };
            computer.Open();
            services.AddSingleton<IComputer>(computer);
        }

        private static T GetConfigSection<T>(IConfiguration configuration, string name = null)
            where T : new()
        {
            var configSection = new T();
            configuration.GetSection(name ?? configSection.GetType().Name)
                         .Bind(configSection);
            return configSection;
        }

    }
}
