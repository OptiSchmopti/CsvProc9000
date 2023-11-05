using CsvProc9000.UI.Wpf.Dialogues;
using CsvProc9000.UI.Wpf.Settings;
using CsvProc9000.UI.Wpf.States;
using Microsoft.Extensions.DependencyInjection;

namespace CsvProc9000.UI.Wpf;

public partial class MainWindow
{
    public MainWindow()
    {
        InitializeComponent();

        var serviceCollection = new ServiceCollection();
        serviceCollection.AddWpfBlazorWebView();

#if DEBUG
        serviceCollection.AddBlazorWebViewDeveloperTools();
#endif
        
        serviceCollection.AddSingleton<IFolderPicker, WindowsFolderPicker>();
        serviceCollection.AddSingleton<ISettingsManager, SettingsManager>();
        serviceCollection.AddSingleton<IConfigurationState, ConfigurationState>();
        serviceCollection.AddSingleton<IConfigurationSerializer, ConfigurationSerializer>();
        
        Resources.Add("services", serviceCollection.BuildServiceProvider());
    }
}
