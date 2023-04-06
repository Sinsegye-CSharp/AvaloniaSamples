using Opc.Ua;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace OpcUAClient.Models;

/// <summary>
/// 节点监听Model
/// </summary>
public class NodeListenModel: ReactiveObject
{
    /// <summary>
    /// 节点名称
    /// </summary>
    public string Name { get; set; } = "";

    /// <summary>
    /// 节点Id
    /// </summary>
    public string NodeId { get; set; } = "";

    /// <summary>
    /// 节点的值，以字符串形式标识
    /// </summary>
    [Reactive]
    public string Value { get; set; } = "";
    
    /// <summary>
    /// 节点在Opc Ua中的类型
    /// </summary>
    public BuiltInType BuiltInType { get; set; }
}