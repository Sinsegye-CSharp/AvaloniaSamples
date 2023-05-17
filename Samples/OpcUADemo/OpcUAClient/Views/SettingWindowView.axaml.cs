using Avalonia;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using OpcUAClient.ViewModels;
using ReactiveUI;
using System;

namespace OpcUAClient.Views;

public partial class SettingWindowView : ReactiveWindow<SettingWindowViewModel>
{
    public SettingWindowView()
    {
        InitializeComponent();
#if DEBUG
        this.AttachDevTools();
#endif
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
        
        // 订阅窗体关闭事件，执行OKCommand时，会执行关闭事件
        this.WhenActivated(d => d(ViewModel!.OkCommand.Subscribe(Close)));
    }
}