using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using Avalonia.Controls;
using DynamicData;
using Opc.Ua;
using Opc.Ua.Client;
using Opc.Ua.Configuration;
using OpcUAClient.Models;
using ReactiveUI.Fody.Helpers;

namespace OpcUAClient.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    // 应用名称
    private readonly string _applicationName = "OpcUa Client";

    // OPC UA客户端
    private readonly UAClient? _client;

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
    /// 选中的节点的信息
    /// </summary>
    [Reactive]
    public NodeInfo SelectedNodeInfo { get; set; }

    private IDictionary<string, int> _listenNodeInfosDictionary;

    public ObservableCollection<NodeInfo> ListenNodeInfos { get; set; }

    public MainWindowViewModel()
    {
        var app = new ApplicationInstance
        {
            ApplicationName = _applicationName,
            ApplicationType = ApplicationType.Client,
            ConfigSectionName = "OpcUAClient"
        };

        var configuration = app.LoadApplicationConfiguration(false).Result;

        var _ = app.CheckApplicationInstanceCertificate(false, 0).Result;
        _client = new UAClient(configuration, null);
        NodeTree = new ObservableCollection<NodeModel>();
        _listenNodeInfosDictionary = new System.Collections.Generic.Dictionary<string, int>();
        ListenNodeInfos = new ObservableCollection<NodeInfo>();
        SelectedNodeInfo = new NodeInfo();
    }

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
    /// 获取节点信息
    /// </summary>
    public async Task GetNodeInfo(SelectionChangedEventArgs args)
    {
        if (_client is null) return;
        if (args.AddedItems.Count == 0 || args.AddedItems[0] == null)
        {
            return;
        }

        var node = (NodeModel)args.AddedItems[0]!;
        SelectedNodeInfo = await _client.GetNodeInfoAsync(node.NodeId);
    }

    public async void AddSubscribe(NodeModel nodeModel)
    {
        if (_client is null) return;

        _client.SubscribeToDataChanges(Guid.NewGuid(), new List<NodeModel> { nodeModel }, OnDataChanged);

        var nodeInfo = await _client.GetNodeInfoAsync(nodeModel.NodeId);
        ListenNodeInfos.Add(nodeInfo);
        _listenNodeInfosDictionary.Add(nodeInfo.NodeId, ListenNodeInfos.Count - 1);
    }

    // 当监听值修改时
    private void OnDataChanged(MonitoredItem monitoredItem, MonitoredItemNotificationEventArgs e)
    {
        if (e.NotificationValue is MonitoredItemNotification notification)
        {
            ListenNodeInfos[_listenNodeInfosDictionary[monitoredItem.StartNodeId.Format()]].Value =
                notification.Value?.WrappedValue.Value?.ToString() ?? "";
        }
    }
}