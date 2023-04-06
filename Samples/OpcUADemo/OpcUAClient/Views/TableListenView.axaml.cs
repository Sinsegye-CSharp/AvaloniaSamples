using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace OpcUAClient.Views;

public partial class TableListenView : UserControl
{
    public TableListenView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}