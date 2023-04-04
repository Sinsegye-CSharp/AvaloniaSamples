using ReactiveUI;

namespace AvaloniaDemo.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private string _title = "曲线图演示Demo";

    /// <summary>
    /// 程序标题
    /// </summary>
    public string Title
    {
        get => _title;
        set => this.RaiseAndSetIfChanged(ref _title, value);
    }

    private ChartViewModel _chartViewModel=new();

    public ChartViewModel ChartViewModel
    {
        get => _chartViewModel;
        set => this.RaiseAndSetIfChanged(ref _chartViewModel, value);
    }
}