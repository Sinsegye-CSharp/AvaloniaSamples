using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using OpcUAClient.Models;
using OpcUAClient.ViewModels;
using ReactiveUI;

namespace OpcUAClient.Views;

public partial class LineListenView : ReactiveUserControl<LineListenViewModel>
{
    public LineListenView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);

        this.WhenActivated(d => d(ViewModel!.OpenSettingDialog.RegisterHandler(DoShowDialogAsync)));
    }

    private async Task DoShowDialogAsync(
        InteractionContext<SettingWindowViewModel, DataRecordSettingModel?> interaction)
    {
        var dialog = new SettingWindowView
        {
            DataContext = interaction.Input
        };
        
        var mainWindow = ((IClassicDesktopStyleApplicationLifetime)Application.Current!.ApplicationLifetime!).MainWindow;
        var result = await dialog.ShowDialog<DataRecordSettingModel>(mainWindow);
        interaction.SetOutput(result);
    }
}