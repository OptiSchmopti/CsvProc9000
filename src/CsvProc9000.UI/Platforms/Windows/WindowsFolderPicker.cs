using CsvProc9000.UI.Dialogues;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace CsvProc9000.UI.Platforms.Windows;

/// <summary>
///     A windows specific <see cref="IFolderPicker"/>
/// </summary>
internal sealed class WindowsFolderPicker : IFolderPicker
{
    /// <inheritdoc />
    public string Show(string title)
    {
        var dialog = new CommonOpenFileDialog(title);

        dialog.IsFolderPicker = true;
        dialog.Multiselect = false;
        dialog.Title = title;

        var result = dialog.ShowDialog();

        return result == CommonFileDialogResult.Ok 
            ? dialog.FileName 
            : string.Empty;
    }
}
