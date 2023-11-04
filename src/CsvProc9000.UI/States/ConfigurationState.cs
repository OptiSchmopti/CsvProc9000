using CsvProc9000.Model.Configuration;
using CsvProc9000.UI.Settings;

namespace CsvProc9000.UI.States;

internal sealed class ConfigurationState : IConfigurationState
{
    private readonly ISettingsLoader _settingsLoader;

    /// <inheritdoc />
    public CsvProcessorOptions Settings { get; private set; }

    /// <summary>
    ///     Constructor
    /// </summary>
    public ConfigurationState(ISettingsLoader settingsLoader)
    {
        _settingsLoader = settingsLoader ?? throw new ArgumentNullException(nameof(settingsLoader));
    }
    
    /// <inheritdoc />
    public void ReadSettings(string pathToSettings)
    {
        var settings = _settingsLoader.Load(pathToSettings);
        settings.Rules ??= new List<Rule>();
        Settings = settings;
    }

    /// <inheritdoc />
    public Rule GetRuleAt(int index)
    {
        return Settings.Rules[index];
    }
}
