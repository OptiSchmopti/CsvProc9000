using CsvProc9000.Model.Configuration;

namespace CsvProc9000.UI.States;

/// <summary>
///     State for configuring the settings
/// </summary>
internal interface IConfigurationState
{
    /// <summary>
    ///     Reads in the settings from the given path
    /// </summary>
    /// <param name="pathToSettings">The path to read in the settings from</param>
    void ReadSettings(string pathToSettings);

    /// <summary>
    ///     Gets the rule from the given index
    /// </summary>
    /// <param name="index">The index to get the rule from</param>
    /// <returns>The rule that was gotten from the index</returns>
    Rule GetRuleAt(int index);

    /// <summary>
    ///     The read in settings
    /// </summary>
    CsvProcessorOptions Settings { get; }
}
