using System;
using CsvProc9000.Workers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace CsvProc9000
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            InitializeLogging();
            
            try
            {
                Log.Information("Starting Up...");
                CreateHostBuilder(args).Build().Run();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Application start-up failed!");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        private static void InitializeLogging()
        {
            Log.Logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .WriteTo.File("./logs/application-log.log")
                .CreateLogger();
        }

        private static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host
                .CreateDefaultBuilder(args)
                .UseWindowsService()
                .UseSerilog()
                .ConfigureServices(ConfigureServices);
        }

        private static void ConfigureServices(HostBuilderContext context, IServiceCollection services)
        {
            services.AddHostedService<CsvProcessorWorker>();
        }
    }
}