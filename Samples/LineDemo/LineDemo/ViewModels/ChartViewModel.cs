using System;
using System.Collections.ObjectModel;
using Avalonia.Threading;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;

namespace AvaloniaDemo.ViewModels;

public class ChartViewModel : ViewModelBase
{
    #region Private Fileds

    // 动态折线1数据
    private readonly ObservableCollection<DateTimePoint> _observableValues1;

    // 动态折线2数据
    private readonly ObservableCollection<DateTimePoint> _observableValues2;

    // 生成随机数据的函数
    private readonly Random _random = new();

    #endregion

    /// <summary>
    /// 构造函数
    /// </summary>
    public ChartViewModel()
    {
        _observableValues1 = new ObservableCollection<DateTimePoint>
        {
            new(DateTime.Now.AddSeconds(-1), 0),
        };
        _observableValues2 = new ObservableCollection<DateTimePoint>
        {
            new(DateTime.Now.AddSeconds(-1), 0),
        };

        CartesianLineChartXAxis = new[]
        {
            new Axis
            {
                MinLimit = DateTime.Now.Ticks,
                MinStep = TimeSpan.FromSeconds(1).Ticks,
                UnitWidth = TimeSpan.FromSeconds(1).Ticks,
                Labeler = value=>new DateTime((long)value).ToString("HH:mm:ss"),
                TicksPaint = new SolidColorPaint(SKColors.Black)
            }
        };
        CartesianChartYAxis = new[]
        {
            new Axis
            {
                MinLimit = 0,
                MinStep = 1,
            }
        };

        CartesianChartLineSeries = new ObservableCollection<ISeries>
        {
            new LineSeries<DateTimePoint>
            {
                Name = "城市规模",
                Values = _observableValues1,
            },
            new LineSeries<DateTimePoint>
            {
                Name = "城市经济",
                Values = _observableValues2,
            },
        };

        UpdateLine();
    }

    #region Public Properties

    /// <summary>
    /// 折线图X轴设置
    /// </summary>
    public Axis[] CartesianLineChartXAxis { get; set; }

    /// <summary>
    /// 折线图Y轴设置
    /// </summary>
    public Axis[] CartesianChartYAxis { get; set; }

    /// <summary>
    /// 折线图曲线
    /// </summary>
    public ObservableCollection<ISeries> CartesianChartLineSeries { get; set; }

    /// <summary>
    /// 提示框绘制设置
    /// </summary>
    public SolidColorPaint TooltipTextPaint { get; set; } =
        new()
        {
            Color = SKColors.Black,
            SKTypeface = SKFontManager.Default.MatchCharacter('汉'),
        };

    #endregion

    #region Private Methods

    // 更新曲线
    private void UpdateLine()
    {
        var timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1)
        };
        timer.Tick += (_, _) =>
        {
            if (_observableValues1.Count > 10)
            {
                CartesianLineChartXAxis[0].MinLimit = _observableValues1[^10].DateTime.Ticks;
            }
            _observableValues1.Add(new DateTimePoint(DateTime.Now, _random.Next(1, 10)));
            _observableValues2.Add(new DateTimePoint(DateTime.Now, _random.Next(1, 10)));
        };
        timer.Start();
    }

    #endregion
}