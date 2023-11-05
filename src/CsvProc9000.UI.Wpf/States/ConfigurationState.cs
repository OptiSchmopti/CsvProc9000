using CsvProc9000.Model.Configuration;
using CsvProc9000.UI.Wpf.Settings;
using System;
using System.Collections.Generic;

namespace CsvProc9000.UI.Wpf.States;

internal sealed class ConfigurationState : IConfigurationState
{
    private readonly ISettingsManager _settingsManager;

    /// <summary>
    ///     Constructor
    /// </summary>
    public ConfigurationState(ISettingsManager settingsManager)
    {
        _settingsManager = settingsManager ?? throw new ArgumentNullException(nameof(settingsManager));
    }

    /// <inheritdoc />
    public string SettingsDirectory { get; set; }
    
    /// <inheritdoc />
    public CsvProcessorOptions Settings { get; private set; }
    
    /// <inheritdoc />
    public void ReadSettings()
    {
        var settings = _settingsManager.Load(SettingsDirectory);
        settings.Rules ??= new List<Rule>();
        Settings = settings;
    }

    /// <inheritdoc />
    public void WriteSettings(bool makeBackup)
    {
        _settingsManager.Write(SettingsDirectory, Settings, makeBackup);
    }

    /// <inheritdoc />
    public Rule GetRuleAt(int index)
    {
        return Settings.Rules[index];
    }
}
