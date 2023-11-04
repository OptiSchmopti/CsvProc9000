using CsvProc9000.Model.Configuration;

namespace CsvProc9000.UI.Settings;

/// <summary>
///     Loads the settings of the service from a given directory
/// </summary>
internal interface ISettingsLoader
{
    /// <summary>
    ///     Loads the settings of the service from the given directory
    /// </summary>
    /// <param name="pathToDirectory">The directory to load the settings from</param>
    /// <returns>The loaded settings</returns>
    CsvProcessorOptions Load(string pathToDirectory);
}
