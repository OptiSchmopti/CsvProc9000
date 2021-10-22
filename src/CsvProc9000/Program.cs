using System;
using System.IO.Abstractions;
using System.Threading.Tasks;
using CsvProc9000.Options;
using CsvProc9000.Workers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace CsvProc9000
{
    public static class Program
    {
        public static async Task Main(string[] args)
        {
            try
            {
                await CreateHostBuilder(args).Build().RunAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Application start-up failed!\n{ex}");
                Log.Fatal(ex, "Application start-up failed!");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        private static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host
                .CreateDefaultBuilder(args)
                .UseWindowsService()
                .ConfigureLogging((context, builder) =>
                {
                    var logger = ConfigureLogging(context.Configuration);
                    builder.AddSerilog(logger);
                })
                .ConfigureServices(ConfigureServices);
        }

        private static ILogger ConfigureLogging(IConfiguration configuration)
        {
            var logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .WriteTo.File("./logs/application-log.log")
                .ReadFrom.Configuration(configuration)
                .CreateLogger();

            Log.Logger = logger;
            return logger;
        }

        private static void ConfigureServices(HostBuilderContext context, IServiceCollection services)
        {
            var processorOptionsSection = context.Configuration.GetSection("CsvProcessorOptions");
            services.Configure<CsvProcessorOptions>(processorOptionsSection);

            services.AddSingleton<IFileSystem, FileSystem>();
            
            services.AddHostedService<CsvProcessorWorker>();
        }
    }
}