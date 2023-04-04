using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DynamicData;
using Opc.Ua;
using Opc.Ua.Client;
using OpcUAClient.Models;

namespace OpcUAClient;

public class UAClient : IDisposable
{
    #region Private Fields

    // OPC UA配置
    private readonly ApplicationConfiguration _configuration;

    // OPC UA会话
    private ISession? _session;

    private readonly IDictionary<Guid, Subscription> _subscriptionsDictionary;

    #endregion

    #region Public Properties

    /// <summary>
    /// The session keepalive interval to be used in ms.
    /// </summary>
    public int KeepAliveInterval { get; set; } = 5000;

    /// <summary>
    /// 和Server连接Session的LifeTime
    /// </summary>
    public uint SessionLifeTime { get; set; } = 30 * 1000;

    /// <summary>
    /// 用户连接Server的凭据
    /// </summary>
    public IUserIdentity UserIdentity { get; set; }

    /// <summary>
    /// 请求时每个节点下最大的子节点数量
    /// </summary>
    public int RequestMaxNodeCountPerNode { get; set; } = 200;

    #endregion

    #region Constructors

    /// <summary>
    /// 初始化一个OPC UA客户端
    /// </summary>
    /// <param name="configuration">OPC UA客户端的配置</param>
    /// <param name="userIdentity">连接服务端的凭据</param>
    public UAClient(ApplicationConfiguration configuration, UserIdentity? userIdentity)
    {
        _configuration = configuration;
        UserIdentity = userIdentity ?? new UserIdentity();
        _subscriptionsDictionary = new Dictionary<Guid, Subscription>();
    }

    #endregion

    /// <summary>
    /// 连接OPC UA Server
    /// </summary>
    /// <param name="serverUrl">服务器地址</param>
    /// <param name="useSecurity">是否使用安全登录</param>
    /// <returns>登录是成功</returns>
    /// <exception cref="ArgumentNullException">参数为NUll错误，指服务器地址为空</exception>
    public async Task<bool> ConnectAsync(string serverUrl, bool useSecurity = true)
    {
        if (string.IsNullOrWhiteSpace(serverUrl)) throw new ArgumentNullException(nameof(serverUrl));

        try
        {
            if (_session is { Connected: true })
            {
                Debug.WriteLine("Session already connected!");
                return false;
            }

            Debug.WriteLine($"Connecting to... {serverUrl}");

            var endpointDescription = CoreClientUtils.SelectEndpoint(_configuration, serverUrl, useSecurity);
            var endpointConfiguration = EndpointConfiguration.Create(_configuration);
            var endpoint = new ConfiguredEndpoint(null, endpointDescription, endpointConfiguration);

            var session = await Session.Create(_configuration, endpoint, false, false,
                _configuration.ApplicationName, SessionLifeTime, UserIdentity, null).ConfigureAwait(false);

            if (session is not { Connected: true }) return false;

            _session = session;

            _session.KeepAliveInterval = KeepAliveInterval;

            Debug.WriteLine($"New Session Created with SessionName = {_session.SessionName}");

            return true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Create Session Error : {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// 断开连接
    /// </summary>
    public async Task<bool> DisconnectAsync()
    {
        if (_session == null) return true;

        Debug.WriteLine("Disconnecting...");

        await _session.RemoveSubscriptionsAsync(_subscriptionsDictionary.Values);
        await _session.CloseAsync();
        _session.Dispose();
        _session = null;

        Debug.WriteLine("Session Disconnected.");

        return true;
    }

    public void Dispose()
    {
        Utils.SilentDispose(_session);
    }

    /// <summary>
    /// 获取指定节点下的子节点
    /// </summary>
    /// <param name="nodeId">指定的节点</param>
    /// <returns></returns>
    public IEnumerable<NodeModel> GetChildNodes(NodeId? nodeId)
    {
        if (_session is null) throw new NullReferenceException("Session is Null");

        var referenceDescriptionCollection = GetReferenceDescriptionCollection(nodeId);
        var result = new List<NodeModel>();

        foreach (var referenceDescription in referenceDescriptionCollection)
        {
            var treeNode = new NodeModel
            {
                Name = referenceDescription.DisplayName.Text,
                NodeId = referenceDescription.NodeId.Format(),
                HasChild = GetReferenceDescriptionCollection(referenceDescription.NodeId.Format()).Count > 0
            };
            result.Add(treeNode);
        }

        return result;
    }

    public async Task<NodeInfo> GetNodeInfoAsync(NodeId nodeId)
    {
        var result = await GetNodesInfoAsync(new List<NodeId> { nodeId });
        return result.First();
    }

    public async Task<IEnumerable<NodeInfo>> GetNodesInfoAsync(List<NodeId> nodeIds)
    {
        if (_session is null) throw new NullReferenceException("Session is Null");

        var nodesToRead = new ReadValueIdCollection();

        foreach (var nodeId in nodeIds)
        {
            nodesToRead.Add(new ReadValueId
            {
                NodeId = nodeId,
                AttributeId = Attributes.NodeClass
            });
            nodesToRead.Add(new ReadValueId
            {
                NodeId = nodeId,
                AttributeId = Attributes.Value
            });
            nodesToRead.Add(new ReadValueId
            {
                NodeId = nodeId,
                AttributeId = Attributes.DisplayName
            });
            nodesToRead.Add(new ReadValueId
            {
                NodeId = nodeId,
                AttributeId = Attributes.Description,
            });
        }

        // read all values.
        var response =
            await _session.ReadAsync(null, 0, TimestampsToReturn.Neither, nodesToRead, CancellationToken.None);
        ClientBase.ValidateResponse(response.Results, nodesToRead);
        ClientBase.ValidateDiagnosticInfos(response.DiagnosticInfos, nodesToRead);

        // 对结果进行分割
        var infoArray = response.Results.Chunk(4).ToList();

        var result = new List<NodeInfo>();

        for (int i = 0; i < infoArray.Count; i++)
        {
            var info = infoArray[i];
            var nodeInfo = new NodeInfo
            {
                NodeId = nodeIds[i].Format()
            };
            if ((NodeClass)info[0].WrappedValue.Value == NodeClass.Variable)
            {
                nodeInfo.Type = info[0]?.WrappedValue.TypeInfo.BuiltInType.ToString() ?? "";
                nodeInfo.Value = info[1]?.WrappedValue.Value?.ToString() ?? "";
                nodeInfo.Name = info[2]?.WrappedValue.Value?.ToString() ?? "";
                nodeInfo.Description = info[3]?.WrappedValue.Value?.ToString() ?? "";
            }
            else if ((NodeClass)info[0].WrappedValue.Value == NodeClass.Object)
            {
                nodeInfo.Type = ((NodeClass)info[0].WrappedValue.Value).ToString() ?? "";
                nodeInfo.Value = "";
                nodeInfo.Name = info[2].WrappedValue.Value.ToString() ?? "";
                nodeInfo.Description = info[3]?.WrappedValue.Value?.ToString() ?? "";
            }

            result.Add(nodeInfo);
        }

        return result;
    }

    public void SubscribeToDataChanges(Guid key, IList<NodeModel> nodeModels,
        Action<MonitoredItem, MonitoredItemNotificationEventArgs> changeCallBack)
    {
        if (_session is null) throw new NullReferenceException("Session is Null");

        var subscription = new Subscription(_session.DefaultSubscription)
        {
            DisplayName = key.ToString(),
            PublishingEnabled = true,
            PublishingInterval = 0,
            LifetimeCount = uint.MaxValue,
            MaxNotificationsPerPublish = uint.MaxValue,
            Priority = 100
        };
        
        _session.AddSubscription(subscription);
        subscription.Create();

        foreach (var nodeModel in nodeModels)
        {
            var itme = new MonitoredItem(subscription.DefaultItem)
            {
                StartNodeId = nodeModel.NodeId,
                AttributeId = Attributes.Value,
                DisplayName = nodeModel.Name,
                SamplingInterval = 100,
                QueueSize = 10,
                DiscardOldest = true
            };
            itme.Notification += changeCallBack.Invoke;

            subscription.AddItem(itme);
        }

        subscription.ApplyChanges();
        _subscriptionsDictionary.Add(key, subscription);
    }

    // 获取节点引用的描述
    private ReferenceDescriptionCollection GetReferenceDescriptionCollection(NodeId? sourceId)
    {
        var result = new ReferenceDescriptionCollection();

        if (_session is null) return result;

        var browseTemplate = new BrowseDescription
        {
            NodeId = sourceId ?? ObjectIds.ObjectsFolder,
            BrowseDirection = BrowseDirection.Forward,
            ReferenceTypeId = ReferenceTypeIds.HierarchicalReferences,
            IncludeSubtypes = true,
            NodeClassMask = (uint)NodeClass.Unspecified,
            ResultMask = (uint)BrowseResultMask.All
        };
        var browseDescriptionCollection = new BrowseDescriptionCollection { browseTemplate };

        _session.Browse(null, null, (uint)RequestMaxNodeCountPerNode, browseDescriptionCollection,
            out var browseResultCollection, out var diagnosticsInfoCollection);
        ClientBase.ValidateResponse(browseResultCollection, browseDescriptionCollection);
        ClientBase.ValidateDiagnosticInfos(diagnosticsInfoCollection, browseDescriptionCollection);

        if (browseResultCollection is { Count: 0 }) return result;

        var continuationPoints = new ByteStringCollection();

        for (var i = 0; i < browseResultCollection.Count; i++)
        {
            if (StatusCode.IsBad(browseResultCollection[i].StatusCode))
            {
                // this error indicates that the server does not have enough simultaneously active 
                // continuation points. This request will need to be resent after the other operations
                // have been completed and their continuation points released.
                // if (browseResultCollection[ii].StatusCode == StatusCodes.BadNoContinuationPoints)
                // {
                //     unprocessedOperations.Add( nodesToBrowse[ii] );
                // }

                continue;
            }

            if (browseResultCollection[i].References is { Count: 0 }) continue;

            result.Add(browseResultCollection[i].References);

            if (browseResultCollection[i].ContinuationPoint is not null)
            {
                continuationPoints.Add(browseResultCollection[i].ContinuationPoint);
            }
        }

        var revisedContinuationPoint = new ByteStringCollection();
        while (continuationPoints.Count > 0)
        {
            // continue browse operation.
            _session.BrowseNext(null, true, continuationPoints, out var results, out var diagnosticInfos);

            ClientBase.ValidateResponse(results, continuationPoints);
            ClientBase.ValidateDiagnosticInfos(diagnosticInfos, continuationPoints);

            for (var i = 0; i < continuationPoints.Count; i++)
            {
                if (StatusCode.IsBad(results[i].StatusCode))
                {
                    continue;
                }

                if (results[i].References.Count == 0)
                {
                    continue;
                }

                // save results.
                result.AddRange(results[i].References);

                // check for continuation point.
                if (results[i].ContinuationPoint != null)
                {
                    revisedContinuationPoint.Add(results[i].ContinuationPoint);
                }
            }

            // check if browsing must continue;
            continuationPoints = revisedContinuationPoint;
        }

        return result;
    }
}