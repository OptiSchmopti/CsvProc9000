using CsvProc9000.BackgroundServices;
using CsvProc9000.Csv;
using CsvProc9000.Csv.Contracts;
using CsvProc9000.Jobs;
using CsvProc9000.Jobs.Contracts;
using CsvProc9000.Model.Configuration;
using CsvProc9000.Utils;
using CsvProc9000.Utils.Contracts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO.Abstractions;
using System.Text;
using System.Threading.Tasks;
using ILogger = Serilog.ILogger;

namespace CsvProc9000
{
    [ExcludeFromCodeCoverage] // startup stuff
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
                    builder.ClearProviders();

                    var logger = ConfigureSerilogLogging(context.Configuration);
                    builder.AddSerilog(logger);

                    builder.AddFilter("Microsoft", LogLevel.Warning);
                    builder.AddFilter("Microsoft.Hosting.Lifetime", LogLevel.Warning);
                })
                .ConfigureServices(ConfigureServices);
        }

        private static ILogger ConfigureSerilogLogging(IConfiguration configuration)
        {
            var logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .WriteTo.File(
                    $"{AppDomain.CurrentDomain.BaseDirectory}/logs/application-log.log", 
                    fileSizeLimitBytes: null, 
                    retainedFileCountLimit: null, 
                    rollingInterval: RollingInterval.Day)
                .ReadFrom.Configuration(configuration)
                .CreateLogger();

            Log.Logger = logger;
            return logger;
        }

        private static void ConfigureServices(HostBuilderContext context, IServiceCollection services)
        {
            var processorOptionsSection = context.Configuration.GetSection("CsvProcessor");
            services.Configure<CsvProcessorOptions>(processorOptionsSection);

            services.AddSingleton<IFileSystem, FileSystem>();

            services.AddSingleton<IApplyRulesToCsvFile, ApplyRulesToCsvFile>();
            services.AddSingleton<ICsvExporter, CsvExporter>();
            services.AddSingleton<ICsvImporter, CsvImporter>();
            services.AddSingleton<ICsvWriterFactory, CsvWriterFactory>();
            services.AddSingleton<ICsvReaderFactory, CsvReaderFactory>();

            services.AddSingleton<IJobPool, JobPool>();
            services.AddTransient<ICsvProcessJobThread, CsvProcessJobThread>();
            services.AddTransient<ICsvProcessJobWorker, CsvProcessJobWorker>();

            services.AddSingleton<ICsvProcessJobThreadFactory, CsvProcessJobThreadFactory>();

            services.AddSingleton<IFileHelper, FileHelper>();

            services.AddHostedService<CsvFileWatcherBackgroundService>();
            services.AddHostedService<CsvExistingFileWatcherBackgroundService>();
            services.AddHostedService<CsvProcessJobThreadSpawnerBackgroundService>();
            
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }
    }
}
