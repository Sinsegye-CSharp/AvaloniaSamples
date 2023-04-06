using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Metadata;
using Avalonia.Threading;
using DynamicData;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.Drawing;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using Opc.Ua;
using Opc.Ua.Configuration;
using OpcUAClient.Models;
using ReactiveUI.Fody.Helpers;
using SkiaSharp;

namespace OpcUAClient.ViewModels;

public class LineListenViewModel : ViewModelBase
{
    #region Private Fields

    // 应用名称
    private readonly string _applicationName = "OpcUa Client";

    // OPC UA客户端
    private readonly UaClient? _client;

    // 图表中绘制的曲线
    private readonly List<ObservableCollection<DateTimePoint>> _observableCollections;

    // 是否第一次绘制图表
    private bool _isFirst;

    // 图表计时器，用来控制请求Server数据的时间间隔
    private readonly DispatcherTimer _chartTimer;

    // 数值计时器，用来控制请求Server数据的时间间隔
    private readonly DispatcherTimer _valueTimer;

    #endregion

    #region Public Properties

    /// <summary>
    /// 服务器地址
    /// </summary>
    [Reactive]
    public string ServerPath { get; set; } = "opc.tcp://192.168.110.100:4840";

    /// <summary>
    /// 用户名
    /// </summary>
    [Reactive]
    public string UserName { get; set; } = "sii";

    /// <summary>
    /// 密码
    /// </summary>
    [Reactive]
    public string Password { get; set; } = "1";

    /// <summary>
    /// 节点树
    /// </summary>
    public ObservableCollection<NodeModel> NodeTree { get; set; }

    /// <summary>
    /// 监听的曲线列表
    /// </summary>
    public ObservableCollection<NodeModel> ListenModeCollection { get; set; }

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

    /// <summary>
    /// 重点监视的值
    /// </summary>
    public ObservableCollection<NodeListenModel> ImportantListenModelCollection { get; set; }

    #endregion

    #region Ctor

    /// <summary>
    /// 构造函数
    /// </summary>
    public LineListenViewModel()
    {
        var app = new ApplicationInstance
        {
            ApplicationName = _applicationName,
            ApplicationType = ApplicationType.Client,
            ConfigSectionName = "OpcUAClient"
        };

        var configuration = app.LoadApplicationConfiguration(false).Result;

        var _ = app.CheckApplicationInstanceCertificate(false, 0).Result;
        _client = new UaClient(configuration, null);
        NodeTree = new ObservableCollection<NodeModel>();
        ListenModeCollection = new ObservableCollection<NodeModel>();

        _chartTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1)
        };
        _chartTimer.Tick += GetChartValue;

        _valueTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1)
        };
        _valueTimer.Tick += GetImportantListenValue;

        CartesianLineChartXAxis = new[]
        {
            new Axis
            {
                MinLimit = DateTime.Now.Ticks,
                MinStep = TimeSpan.FromSeconds(1).Ticks,
                UnitWidth = TimeSpan.FromSeconds(1).Ticks,
                SeparatorsPaint = new SolidColorPaint(SKColors.LightSlateGray) { StrokeThickness = 2 },
                Labeler = value => new DateTime((long)value).ToString("HH:mm:ss"),
                TicksPaint = new SolidColorPaint(SKColors.Black),
                SeparatorsAtCenter = true,
                LabelsPaint = new SolidColorPaint()
                {
                    Color = SKColors.Black,
                    SKTypeface = SKFontManager.Default.MatchCharacter('汉'),
                }
            }
        };

        CartesianChartYAxis = new[]
        {
            new Axis
            {
                LabelsPaint = new SolidColorPaint()
                {
                    Color = SKColors.Black,
                    SKTypeface = SKFontManager.Default.MatchCharacter('汉'),
                },
                Labeler = value => value.ToString(CultureInfo.InvariantCulture)
            }
        };

        CartesianChartLineSeries = new ObservableCollection<ISeries>();

        _observableCollections = new List<ObservableCollection<DateTimePoint>>();
        ImportantListenModelCollection = new ObservableCollection<NodeListenModel>();
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// 连接服务器
    /// </summary>
    public async Task ConnectionServerAsync()
    {
        if (_client is null) return;

        _client.UserIdentity = new UserIdentity(UserName, Password);

        var isConnected = await _client.ConnectAsync(ServerPath);

        Debug.WriteLine($"Server连接结果：{isConnected}");

        LoadNodeTree();
    }

    /// <summary>
    /// 加载节点树
    /// </summary>
    public void LoadNodeTree()
    {
        if (_client is null) return;

        var nodes = _client.GetChildNodes(null);
        NodeTree.AddRange(nodes);
    }

    /// <summary>
    /// 断开连接
    /// </summary>
    public async Task DisconnectAsync()
    {
        if (_client is null) return;
        await _client.DisconnectAsync();
    }

    /// <summary>
    /// 展开节点
    /// </summary>
    /// <param name="node">展开的节点</param>
    public void Expand(NodeModel node)
    {
        if (_client is null) return;
        if (node.Child.Count is not 0) return;

        var childNodes = _client.GetChildNodes(node.NodeId);

        node.Child.AddRange(childNodes);
    }

    /// <summary>
    /// 添加订阅
    /// </summary>
    public void AddSubscribe(NodeModel nodeModel)
    {
        this.ListenModeCollection.Add(nodeModel);
    }

    [DependsOn(nameof(ListenModeCollection))]
    public bool CanAddSubscribe(object parameter)
    {
        if (parameter is not NodeModel nodeModel || ListenModeCollection.Contains(nodeModel)) return false;
        if (_client is null) return false;
        var dataValue = _client.GetNodeAttributesInfo(nodeModel.NodeId, new List<uint>()
        {
            Attributes.NodeClass,
            Attributes.Value
        });

        if ((NodeClass)dataValue[0].WrappedValue.Value != NodeClass.Variable) return false;
        if (dataValue[1].WrappedValue.TypeInfo.BuiltInType is BuiltInType.Int16 or BuiltInType.Int32
            or BuiltInType.Int64 or BuiltInType.Integer or BuiltInType.UInt16 or BuiltInType.UInt32
            or BuiltInType.UInt64 or BuiltInType.UInteger or BuiltInType.Float or BuiltInType.Double) return true;

        return false;
    }

    /// <summary>
    /// 移除订阅
    /// </summary>
    /// <param name="nodeModel">要订阅的节点</param>
    public void RemoveSubscribe(NodeModel nodeModel)
    {
        this.ListenModeCollection.Remove(nodeModel);
    }

    [DependsOn(nameof(ListenModeCollection))]
    public bool CanRemoveSubscribe(object parameter) =>
        parameter is NodeModel nodeModel && ListenModeCollection.Contains(nodeModel);

    /// <summary>
    /// 添加到重点监听
    /// </summary>
    /// <param name="nodeModel">要监听的节点</param>
    public void AddToImportantListen(NodeModel nodeModel)
    {
        if (this.ImportantListenModelCollection.Any(n => n.NodeId == nodeModel.NodeId))
        {
            return;
        }

        if (_client?.Session is null) return;
        var dataValue = _client.Session.ReadValue(nodeModel.NodeId);

        var node = new NodeListenModel
        {
            Name = nodeModel.Name,
            NodeId = nodeModel.NodeId,
            BuiltInType = dataValue.WrappedValue.TypeInfo.BuiltInType,
            Value = dataValue.WrappedValue.Value.ToString() ?? ""
        };
        this.ImportantListenModelCollection.Add(node);
    }

    [DependsOn(nameof(ImportantListenModelCollection))]
    public bool CanAddToImportantListen(object parameter)
    {
        if (parameter is not NodeModel nodeModel ||
            ImportantListenModelCollection.Any(n => n.NodeId == nodeModel.NodeId)) return false;
        if (_client is null) return false;
        var dataValue = _client.GetNodeAttributesInfo(nodeModel.NodeId, new List<uint>()
        {
            Attributes.NodeClass,
            Attributes.Value
        });

        if ((NodeClass)dataValue[0].WrappedValue.Value != NodeClass.Variable) return false;
        return dataValue[1].WrappedValue.TypeInfo.ValueRank is ValueRanks.Scalar;
    }

    /// <summary>
    /// 移除到重点监听
    /// </summary>
    /// <param name="nodeModel">要监听的节点</param>
    public void RemoveImportantListen(NodeModel nodeModel)
    {
        var node = this.ImportantListenModelCollection.FirstOrDefault(n => n.NodeId == nodeModel.NodeId);
        if (node is not null)
        {
            this.ImportantListenModelCollection.Remove(node);
        }
    }

    public bool CanRemoveImportantListen(object parameter) =>
        parameter is NodeModel nodeModel && ImportantListenModelCollection.Any(n => n.NodeId == nodeModel.NodeId);

    /// <summary>
    /// 开始绘制图表
    /// </summary>
    public void StartDraw()
    {
        CartesianChartLineSeries.Clear();
        _observableCollections.Clear();
        foreach (var nodeModel in ListenModeCollection)
        {
            var seriesValues = new ObservableCollection<DateTimePoint>();
            var series = new LineSeries<DateTimePoint>()
            {
                Name = nodeModel.Name,
                Values = seriesValues,
                Fill = null,
                GeometryFill = null,
                GeometrySize = 2,
                DataPadding = new LvcPoint(0, 0),
            };
            _observableCollections.Add(seriesValues);
            CartesianChartLineSeries.Add(series);
        }

        _isFirst = true;
        if (ListenModeCollection is not { Count: 0 })
        {
            _chartTimer.Start();
        }

        if (ImportantListenModelCollection is not { Count: 0 })
        {
            _valueTimer.Start();
        }
    }

    /// <summary>
    /// 停止绘制图表
    /// </summary>
    public void StopDraw()
    {
        _chartTimer.Stop();
        _valueTimer.Start();
        _isFirst = true;
    }

    /// <summary>
    /// 修改值
    /// </summary>
    public void ModifyValue(NodeListenModel nodeListenModel)
    {
        switch (nodeListenModel.BuiltInType)
        {
            case BuiltInType.Boolean:
                _client?.WriteValue(nodeListenModel.NodeId, bool.Parse(nodeListenModel.Value));
                break;
            case BuiltInType.SByte:
                break;
            case BuiltInType.Byte:
                _client?.WriteValue(nodeListenModel.NodeId, byte.Parse(nodeListenModel.Value));
                break;
            case BuiltInType.Int16:
                _client?.WriteValue(nodeListenModel.NodeId, short.Parse(nodeListenModel.Value));
                break;
            case BuiltInType.UInt16:
                _client?.WriteValue(nodeListenModel.NodeId, ushort.Parse(nodeListenModel.Value));
                break;
            case BuiltInType.Int32:
                _client?.WriteValue(nodeListenModel.NodeId, int.Parse(nodeListenModel.Value));
                break;
            case BuiltInType.UInt32:
                _client?.WriteValue(nodeListenModel.NodeId, uint.Parse(nodeListenModel.Value));
                break;
            case BuiltInType.Int64:
                _client?.WriteValue(nodeListenModel.NodeId, long.Parse(nodeListenModel.Value));
                break;
            case BuiltInType.UInt64:
                _client?.WriteValue(nodeListenModel.NodeId, ulong.Parse(nodeListenModel.Value));
                break;
            case BuiltInType.Float:
                _client?.WriteValue(nodeListenModel.NodeId, float.Parse(nodeListenModel.Value));
                break;
            case BuiltInType.Double:
                _client?.WriteValue(nodeListenModel.NodeId, double.Parse(nodeListenModel.Value));
                break;
            case BuiltInType.String:
                _client?.WriteValue(nodeListenModel.NodeId, nodeListenModel.Value);
                break;
            case BuiltInType.DateTime:
                _client?.WriteValue(nodeListenModel.NodeId, DateTime.Parse(nodeListenModel.Value));
                break;
            case BuiltInType.Guid:
                _client?.WriteValue(nodeListenModel.NodeId, Guid.Parse(nodeListenModel.Value));
                break;
            case BuiltInType.LocalizedText:
                _client?.WriteValue(nodeListenModel.NodeId, nodeListenModel.Value);
                break;
            case BuiltInType.Number:
                _client?.WriteValue(nodeListenModel.NodeId, int.Parse(nodeListenModel.Value));
                break;
            case BuiltInType.Integer:
                _client?.WriteValue(nodeListenModel.NodeId, int.Parse(nodeListenModel.Value));
                break;
            case BuiltInType.UInteger:
                _client?.WriteValue(nodeListenModel.NodeId, uint.Parse(nodeListenModel.Value));
                break;
        }
    }

    #endregion

    #region Private Methods

    // 按照时间获取图表需要绘制的值
    private async void GetChartValue(object? sender, EventArgs args)
    {
        if (_client?.Session is null) return;
        if (ListenModeCollection is { Count: 0 }) return;

        var (dataValueCollection, _) =
            await _client.Session.ReadValuesAsync(ListenModeCollection.Select(n => NodeId.Parse(n.NodeId)).ToList());

        for (var i = 0; i < dataValueCollection.Count; i++)
        {
            var value = double.Parse(dataValueCollection[i].WrappedValue.Value.ToString() ?? "0");
            var time = dataValueCollection[i].ServerTimestamp
                .AddMilliseconds(-dataValueCollection[i].ServerTimestamp.Millisecond).ToLocalTime();
            Console.WriteLine($"{time}:{value}");

            if (_isFirst)
            {
                _isFirst = false;
                CartesianLineChartXAxis[0].MinLimit = time.Ticks;
            }

            _observableCollections[i].Add(new DateTimePoint(time, value));
        }
    }

    // 按照时间读取重点监视的值
    private async void GetImportantListenValue(object? sender, EventArgs args)
    {
        if (_client?.Session is null) return;
        if (ImportantListenModelCollection is { Count: 0 }) return;

        var (dataValueCollection, _) =
            await _client.Session.ReadValuesAsync(ImportantListenModelCollection.Select(n => NodeId.Parse(n.NodeId))
                .ToList());

        for (var i = 0; i < dataValueCollection.Count; i++)
        {
            var value = dataValueCollection[i].WrappedValue.Value.ToString() ?? "0";

            ImportantListenModelCollection[i].Value = value;
        }
    }

    #endregion
}