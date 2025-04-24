using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using LibreHardwareMonitor.Hardware;
using HardwareSerialMonitor_v2.Models;
using HardwareSerialMonitor_v2.Services;
using HardwareSerialMonitor_v2.Services.Interfaces;

namespace HardwareSerialMonitor_v2
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // Suppress console output
            Console.SetOut(TextWriter.Null);

            CreateHostBuilder(args).Build().Run();
        }

        private static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseWindowsService()
                .ConfigureLogging(logging =>
                {
                    // Disable console logging
                    logging.ClearProviders();
                })
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
