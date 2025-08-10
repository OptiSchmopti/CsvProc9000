using CsvProc9000.Model.Configuration;

namespace CsvProc9000.UI.Wpf.States;

/// <summary>
///     State for configuring the settings
/// </summary>
internal interface IConfigurationState
{
    /// <summary>
    ///     The directory of the settings
    /// </summary>
    string SettingsDirectory { get; set; }

    /// <summary>
    ///     The read in settings
    /// </summary>
    CsvProcessorOptions Settings { get; }
    
    /// <summary>
    ///     Reads in the settings from the given path
    /// </summary>
    void ReadSettings();

    /// <summary>
    ///     Gets the field rule from the given index
    /// </summary>
    /// <param name="index">The index to get the rule from</param>
    /// <returns>The rule that was gotten from the index</returns>
    FieldRule GetFieldRuleAt(int index);

    /// <summary>
    ///     Gets the outbox rule from the given index
    /// </summary>
    /// <param name="index">The index to get the rule from</param>
    /// <returns>The rule that was gotten from the index</returns>
    OutboxRule GetOutboxRuleAt(int index);

    /// <summary>
    ///     Writes the settings to the desired settings path
    /// </summary>
    /// <param name="makeBackup">Whether a backup should be made</param>
    void WriteSettings(bool makeBackup);
}
