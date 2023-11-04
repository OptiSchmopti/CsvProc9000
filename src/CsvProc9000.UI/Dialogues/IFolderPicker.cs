namespace CsvProc9000.UI.Dialogues;

/// <summary>
///     A dialogue to let the user pick a folder
/// </summary>
internal interface IFolderPicker
{
    /// <summary>
    ///     Shows the dialogue and lets the user pick a folder
    /// </summary>
    /// <param name="title">A title for the dialogue</param>
    /// <returns>The path to the folder that the user picked</returns>
    string Show(string title);
}
