using FubarDev.FtpServer;
using FubarDev.FtpServer.FileSystem.DotNet;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System;
using System.IO;
using System.Threading.Tasks;

namespace LocalFtpServer
{
    class Program
    {
        static async Task Main(string[] args)
        {
            InitializeLogging();

            using var serviceProvider = GetServices(GetConfig()).BuildServiceProvider();
            var ftpServerHost = serviceProvider.GetRequiredService<IFtpServerHost>();

            await ftpServerHost.StartAsync();

            var keyInfo = new ConsoleKeyInfo();
            Console.WriteLine("Press 'x' to stop the server");

            while (keyInfo.Key != ConsoleKey.X)
            {                
                keyInfo = Console.ReadKey();
            }

            await ftpServerHost.StopAsync();
        }

        private static IServiceCollection GetServices(IConfigurationRoot configuration)
        {
            return new ServiceCollection()
                .AddLogging(opt => opt.AddSerilog())
                .Configure<DotNetFileSystemOptions>(opt => opt.RootPath = configuration["ftpFolder"])
                .AddFtpServer(builder =>
                    builder
                    .UseDotNetFileSystem()
                    .EnableAnonymousAuthentication())
                .Configure<FtpServerOptions>(opt => opt.ServerAddress = configuration["serverAddress"]);
        }

        private static IConfigurationRoot GetConfig()
        {
            return new ConfigurationBuilder()
                        .SetBasePath(Directory.GetCurrentDirectory())
                        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                        .Build();
        }

        private static void InitializeLogging()
        {
            Log.Logger = new LoggerConfiguration()
                                .MinimumLevel.Verbose()
                                .Enrich.FromLogContext()
                                .WriteTo.Console()
                                .CreateLogger();
        }
    }
}
