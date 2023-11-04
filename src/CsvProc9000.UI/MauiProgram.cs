using CsvProc9000.UI.Dialogues;
using CsvProc9000.UI.Platforms.Windows;
using CsvProc9000.UI.Settings;
using Microsoft.Extensions.Logging;

namespace CsvProc9000.UI;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts => { fonts.AddFont("Quicksand-VariableFont_wght.ttf", "Quicksand");});

        builder.Services.AddMauiBlazorWebView();

#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.AddDebug();
#endif

        builder
            .Services
            .AddSingleton<IFolderPicker, WindowsFolderPicker>();

        builder
            .Services
            .AddSingleton<ISettingsLoader, SettingsLoader>();

        return builder.Build();
    }
}
