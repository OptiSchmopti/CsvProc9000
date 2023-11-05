using CsvProc9000.Model.Configuration;
using CsvProc9000.UI.Wpf.Settings;
using System;
using System.Collections.Generic;
using System.Windows;

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
        try
        {
            var settings = _settingsManager.Load(SettingsDirectory);
            settings.Rules ??= new List<Rule>();
            Settings = settings;
        }
        catch (Exception exception)
        {
            MessageBox.Show($"Was unable to load settings.\n\nFollowing was the cause: {exception.Message}", "Unable to load settings", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /// <inheritdoc />
    public void WriteSettings(bool makeBackup)
    {
        try
        {
            _settingsManager.Write(SettingsDirectory, Settings, makeBackup);
            MessageBox.Show("Successfully written settings", "Success", MessageBoxButton.OK, MessageBoxImage.None);
        }
        catch (Exception exception)
        {
            MessageBox.Show($"Was unable to write settings.\n\nFollowing was the cause: {exception.Message}", "Unable to write settings", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /// <inheritdoc />
    public Rule GetRuleAt(int index)
    {
        return Settings.Rules[index];
    }
}
