using CsvProc9000.Model.Configuration;
using Microsoft.Extensions.Configuration;

namespace CsvProc9000.UI.Settings;

/// <inheritdoc />
internal sealed class SettingsLoader : ISettingsLoader
{
    /// <inheritdoc />
    public CsvProcessorOptions Load(string pathToDirectory)
    {
        if (!Directory.Exists(pathToDirectory))
        {
            throw new DirectoryNotFoundException($"Unable to find directory '{pathToDirectory}'");
        }

        var pathToSettings = Path.Combine(pathToDirectory, "appsettings.json");
        if (!File.Exists(pathToSettings))
        {
            throw new FileNotFoundException($"Expected a file at '{pathToSettings}' but couldn't find a file");
        }

        var settings = LoadSettingsFromFile(pathToSettings);
        return settings;
    }

    private static CsvProcessorOptions LoadSettingsFromFile(string pathToSettingsFile)
    {
        var configurationBuilder = new ConfigurationBuilder();
        configurationBuilder.AddJsonFile(pathToSettingsFile);

        var configuration = configurationBuilder.Build();

        var section = configuration.GetSection("CsvProcessor");
        var settings = section.Get<CsvProcessorOptions>();

        return settings;
    }
}
