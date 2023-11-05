using CsvProc9000.Model.Configuration;

namespace CsvProc9000.UI.Wpf.Settings;

/// <summary>
///     Loads the settings of the service from a given directory
/// </summary>
internal interface ISettingsManager
{
    /// <summary>
    ///     Loads the settings of the service from the given directory
    /// </summary>
    /// <param name="pathToDirectory">The directory to load the settings from</param>
    /// <returns>The loaded settings</returns>
    CsvProcessorOptions Load(string pathToDirectory);

    /// <summary>
    ///     Writes the given settings to the given file
    /// </summary>
    /// <param name="pathToDirectory">Where the settings should be written</param>
    /// <param name="settings">The settings that should be written to disk</param>
    /// <param name="makeBackup">Whether or not the specified settings should be backed up</param>
    void Write(string pathToDirectory, CsvProcessorOptions settings, bool makeBackup);
}
