using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Platform;
using OpcUAClient.Repository;
using OpcUAClient.Repository.Services;
using OpcUAClient.ViewModels;
using OpcUAClient.Views;

namespace OpcUAClient;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainWindowViewModel(),
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
    
    public override void RegisterServices()
    {
        AvaloniaLocator.CurrentMutable.Bind<IFontManagerImpl>().ToConstant(new FontManager());
        AvaloniaLocator.CurrentMutable.Bind<IRepository>().ToTransient<DapperRepository>();
        base.RegisterServices();
    }
}