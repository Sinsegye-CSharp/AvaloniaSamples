using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using AvaloniaDemo.ViewModels;
using ReactiveUI;

namespace AvaloniaDemo.Views;

public partial class ChartView : ReactiveUserControl<ChartViewModel>
{
    public ChartView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        this.WhenActivated(disposable =>
        {
            
        });
        AvaloniaXamlLoader.Load(this);
    }
}