using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace OpcUAClient.Views;

public partial class LineListenView : UserControl
{
    public LineListenView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}