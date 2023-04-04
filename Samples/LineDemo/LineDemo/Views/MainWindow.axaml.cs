using Avalonia;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using AvaloniaDemo.ViewModels;
using ReactiveUI;

namespace AvaloniaDemo.Views;

public partial class MainWindow : ReactiveWindow<MainWindowViewModel>
{
    public MainWindow()
    {
        this.WhenActivated(disposables =>
        {
        });
        AvaloniaXamlLoader.Load(this);
        
#if DEBUG
        this.AttachDevTools();
#endif
    }
}