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

public class UaClient : IDisposable
{
    #region Private Fields

    // OPC UA配置
    private readonly ApplicationConfiguration _configuration;

    private readonly IDictionary<Guid, Subscription> _subscriptionsDictionary;

    #endregion

    #region Public Properties

    /// <summary>
    /// OPC UA会话
    /// </summary>
    public ISession? Session { get; set; }

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
    public UaClient(ApplicationConfiguration configuration, UserIdentity? userIdentity)
    {
        _configuration = configuration;
        UserIdentity = userIdentity ?? new UserIdentity();
        _subscriptionsDictionary = new Dictionary<Guid, Subscription>();
    }

    #endregion

    #region Connect Disconnect

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
            if (Session is { Connected: true })
            {
                Debug.WriteLine("Session already connected!");
                return false;
            }

            Debug.WriteLine($"Connecting to... {serverUrl}");

            var endpointDescription = CoreClientUtils.SelectEndpoint(_configuration, serverUrl, useSecurity);
            var endpointConfiguration = EndpointConfiguration.Create(_configuration);
            var endpoint = new ConfiguredEndpoint(null, endpointDescription, endpointConfiguration);

            var session = await Opc.Ua.Client.Session.Create(_configuration, endpoint, false, false,
                _configuration.ApplicationName, SessionLifeTime, UserIdentity, null).ConfigureAwait(false);

            if (session is not { Connected: true }) return false;

            Session = session;

            Session.KeepAliveInterval = KeepAliveInterval;

            Debug.WriteLine($"New Session Created with SessionName = {Session.SessionName}");

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
        if (Session == null) return true;

        Debug.WriteLine("Disconnecting...");

        await Session.RemoveSubscriptionsAsync(_subscriptionsDictionary.Values);
        await Session.CloseAsync();
        Session.Dispose();
        Session = null;

        Debug.WriteLine("Session Disconnected.");

        return true;
    }

    public void Dispose()
    {
        Utils.SilentDispose(Session);
    }

    #endregion

    #region Get Node Info

    /// <summary>
    /// 获取指定节点下的子节点
    /// </summary>
    /// <param name="nodeId">指定的节点</param>
    /// <returns></returns>
    public IEnumerable<NodeModel> GetChildNodes(NodeId? nodeId)
    {
        if (Session is null) throw new NullReferenceException("Session is Null");

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

    /// <summary>
    /// 获取节点某个属性的信息
    /// </summary>
    /// <param name="nodeId">节点的ID</param>
    /// <param name="attributeIds">属性ID，值参考Attributes枚举</param>
    /// <returns>获取的结果</returns>
    /// <exception cref="NullReferenceException"></exception>
    public DataValueCollection GetNodeAttributesInfo(NodeId nodeId, List<uint> attributeIds)
    {
        if (Session is null) throw new NullReferenceException("Session is Null");

        var nodesToRead = new ReadValueIdCollection();
        nodesToRead.AddRange(attributeIds.Select(attributeId =>
            new ReadValueId { NodeId = nodeId, AttributeId = attributeId }));

        // read all values.
        Session.Read(null, 0, TimestampsToReturn.Neither, nodesToRead, out var results,
            out var diagnosticInfos);
        ClientBase.ValidateResponse(results, nodesToRead);
        ClientBase.ValidateDiagnosticInfos(diagnosticInfos, nodesToRead);

        return results;
    }

    /// <summary>
    /// 获取节点某个属性的信息
    /// </summary>
    /// <param name="nodeId">节点的ID</param>
    /// <param name="attributeId">属性ID，值参考Attributes枚举</param>
    /// <returns>获取的结果</returns>
    /// <exception cref="NullReferenceException"></exception>
    public async Task<DataValue> GetNodeAttributesInfoAsync(NodeId nodeId, uint attributeId)
    {
        if (Session is null) throw new NullReferenceException("Session is Null");

        var nodesToRead = new ReadValueIdCollection
        {
            new ReadValueId
            {
                NodeId = nodeId,
                AttributeId = attributeId
            }
        };

        // read all values.
        var response =
            await Session.ReadAsync(null, 0, TimestampsToReturn.Neither, nodesToRead, CancellationToken.None);
        ClientBase.ValidateResponse(response.Results, nodesToRead);
        ClientBase.ValidateDiagnosticInfos(response.DiagnosticInfos, nodesToRead);

        return response.Results.First();
    }

    /// <summary>
    /// 获取节点信息
    /// </summary>
    /// <param name="nodeId">节点ID</param>
    /// <returns>返回的结果</returns>
    public async Task<NodeInfo> GetNodeInfoAsync(NodeId nodeId)
    {
        var result = await GetNodesInfoAsync(new List<NodeId> { nodeId });
        return result.First();
    }

    /// <summary>
    /// 获取节点列表信息
    /// </summary>
    /// <param name="nodeIds">节点列表</param>
    /// <returns>节点信息列表</returns>
    /// <exception cref="NullReferenceException">输入参数为空</exception>
    public async Task<IEnumerable<NodeInfo>> GetNodesInfoAsync(List<NodeId> nodeIds)
    {
        if (Session is null) throw new NullReferenceException("Session is Null");

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
            await Session.ReadAsync(null, 0, TimestampsToReturn.Both, nodesToRead, CancellationToken.None);
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
                if (info[1] is not null)
                {
                    nodeInfo.Type = info[1]?.WrappedValue.TypeInfo.BuiltInType.ToString() ?? "";
                    nodeInfo.Value = info[1]?.WrappedValue.Value?.ToString() ?? "";
                    nodeInfo.Name = info[2]?.WrappedValue.Value?.ToString() ?? "";
                    nodeInfo.Description = info[3]?.WrappedValue.Value?.ToString() ?? "";
                }
            }
            else if ((NodeClass)info[0].WrappedValue.Value == NodeClass.Object)
            {
                nodeInfo.Type = ((NodeClass)info[0].WrappedValue.Value).ToString();
                nodeInfo.Value = "";
                nodeInfo.Name = info[2].WrappedValue.Value.ToString() ?? "";
                nodeInfo.Description = info[3]?.WrappedValue.Value?.ToString() ?? "";
            }

            result.Add(nodeInfo);
        }

        return result;
    }

    #endregion

    #region Read Write

    /// <summary>
    /// 读取某个节点的值
    /// </summary>
    /// <param name="nodeId">节点的id</param>
    /// <typeparam name="T">期望的节点的值类型</typeparam>
    /// <returns>读取的值</returns>
    /// <exception cref="NullReferenceException">参数为空</exception>
    public async Task<T?> ReadValueAsync<T>(NodeId nodeId)
    {
        if (Session is null) throw new NullReferenceException("Session is Null");

        var result = await Session.ReadValueAsync(nodeId);
        if (result is null) return default;

        return (T)result.Value;
    }

    /// <summary>
    /// 读取一系列节点的值
    /// </summary>
    /// <param name="nodeIds">节点集合</param>
    /// <typeparam name="T">值的类型</typeparam>
    /// <returns>值结果</returns>
    /// <exception cref="NullReferenceException">参数为空</exception>
    public async IAsyncEnumerable<T> ReadValuesAsync<T>(List<NodeId> nodeIds)
    {
        if (Session is null) throw new NullReferenceException("Session is Null");

        var (dataValueCollection, _) = await Session.ReadValuesAsync(nodeIds);

        if (dataValueCollection is null) yield break;

        foreach (var dataValue in dataValueCollection.Where(dataValue => dataValue is not null))
        {
            yield return (T)dataValue.Value;
        }
    }

    /// <summary>
    /// 写入一个值
    /// </summary>
    /// <param name="nodeId">节点的ID</param>
    /// <param name="value">节点的值</param>
    /// <typeparam name="T">值类型</typeparam>
    public async void WriteValue<T>(NodeId nodeId, T value)
    {
        if (Session is null) throw new NullReferenceException("Session is Null");

        var valueToWrite = new WriteValue
        {
            NodeId = nodeId,
            AttributeId = Attributes.Value,
            Value =
            {
                Value = value,
                StatusCode = StatusCodes.Good,
                ServerTimestamp = DateTime.MinValue,
                SourceTimestamp = DateTime.MinValue
            }
        };
        var valuesToWrite = new WriteValueCollection
        {
            valueToWrite
        };

        var _ = await Session.WriteAsync(null, valuesToWrite, CancellationToken.None);
    }

    #endregion

    #region PubSub Subscribe

    /// <summary>
    /// 订阅数据改变
    /// </summary>
    /// <param name="key">订阅的组ID</param>
    /// <param name="nodeModels">节点模型</param>
    /// <param name="changeCallBack">数据改变回调</param>
    /// <exception cref="NullReferenceException">参数为空</exception>
    public void SubscribeToDataChanges(Guid key, IList<NodeModel> nodeModels,
        Action<MonitoredItem, MonitoredItemNotificationEventArgs> changeCallBack)
    {
        if (Session is null) throw new NullReferenceException("Session is Null");

        var subscription = new Subscription(Session.DefaultSubscription)
        {
            DisplayName = key.ToString(),
            PublishingEnabled = true,
            PublishingInterval = 0,
            LifetimeCount = uint.MaxValue,
            MaxNotificationsPerPublish = uint.MaxValue,
            Priority = 100
        };

        Session.AddSubscription(subscription);
        subscription.Create();

        foreach (var nodeModel in nodeModels)
        {
            var item = new MonitoredItem(subscription.DefaultItem)
            {
                StartNodeId = nodeModel.NodeId,
                AttributeId = Attributes.Value,
                DisplayName = nodeModel.Name,
                SamplingInterval = 100,
                QueueSize = 10,
                DiscardOldest = true
            };
            item.Notification += changeCallBack.Invoke;

            subscription.AddItem(item);
        }

        subscription.ApplyChanges();
        _subscriptionsDictionary.Add(key, subscription);
    }

    #endregion

    #region Private Methods

    // 获取节点引用的描述
    private ReferenceDescriptionCollection GetReferenceDescriptionCollection(NodeId? sourceId)
    {
        var result = new ReferenceDescriptionCollection();

        if (Session is null) return result;

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

        Session.Browse(null, null, (uint)RequestMaxNodeCountPerNode, browseDescriptionCollection,
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
            Session.BrowseNext(null, true, continuationPoints, out var results, out var diagnosticInfos);

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

    #endregion
}