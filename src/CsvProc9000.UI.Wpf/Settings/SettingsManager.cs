using CsvProc9000.Model.Configuration;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace CsvProc9000.UI.Wpf.Settings;

/// <inheritdoc />
internal sealed class SettingsManager : ISettingsManager
{
    private const string CsvProcessorSection = "CsvProcessor";
    private const string BackupDateFormat = "yyyy-MM-dd_HH_mm_ss";
    
    private readonly IConfigurationSerializer _configurationSerializer;

    /// <summary>
    ///     Constructor
    /// </summary>
    public SettingsManager(IConfigurationSerializer configurationSerializer)
    {
        _configurationSerializer = configurationSerializer ?? throw new ArgumentNullException(nameof(configurationSerializer));
    }
    
    /// <inheritdoc />
    public CsvProcessorOptions Load(string pathToDirectory)
    {
        var pathToSettings = GetSettingsPath(pathToDirectory);
        var configuration = ReadCurrentConfiguration(pathToSettings);
        var settings = LoadSettingsFromFile(configuration);
        return settings;
    }

    /// <inheritdoc />
    public void Write(string pathToDirectory, CsvProcessorOptions settings, bool makeBackup)
    {
        var pathToSettings = GetSettingsPath(pathToDirectory);
        var configuration = ReadCurrentConfiguration(pathToSettings);

        if (makeBackup)
        {
            MakeBackup(pathToSettings);
        }
        
        var currentConfigurationSerialized = _configurationSerializer.Serialize(configuration);
        
        ApplySettings(settings, currentConfigurationSerialized);

        var jsonString = currentConfigurationSerialized.ToJsonString(new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(pathToSettings, jsonString);
    }

    private static string GetSettingsPath(string pathToDirectory)
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

        return pathToSettings;
    }

    private static IConfiguration ReadCurrentConfiguration(string pathToSettings)
    {
        var configurationBuilder = new ConfigurationBuilder();
        configurationBuilder.AddJsonFile(pathToSettings);

        var configuration = configurationBuilder.Build();

        return configuration;
    }

    private static void MakeBackup(string pathToSettingsFile)
    {
        var fileDirectory = Path.GetDirectoryName(pathToSettingsFile);
        if (string.IsNullOrWhiteSpace(fileDirectory))
        {
            throw new InvalidOperationException("Couldn't get directory of settings file for some reason");
        }

        var backupFileName = Path.Combine(fileDirectory, $"appsettings-BACKUP-{DateTime.Now.ToString(BackupDateFormat)}.json");

        File.Copy(pathToSettingsFile, backupFileName);
    }

    private static CsvProcessorOptions LoadSettingsFromFile(IConfiguration configuration)
    {
        var section = configuration.GetSection(CsvProcessorSection);
        var settings = section.Get<CsvProcessorOptions>();

        return settings;
    }
    
    private static void ApplySettings(CsvProcessorOptions settings, JsonNode currentConfigurationSerialized)
    {
        var settingsSerialized = JsonSerializer.Serialize(settings);
        var settingsJsonNode = JsonNode.Parse(settingsSerialized);
        
        currentConfigurationSerialized[CsvProcessorSection] = settingsJsonNode;
    }
}
