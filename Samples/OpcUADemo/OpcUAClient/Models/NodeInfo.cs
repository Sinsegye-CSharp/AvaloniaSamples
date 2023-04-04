using System;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace OpcUAClient.Models;

public class NodeInfo : ReactiveObject
{
    /// <summary>
    /// 节点Id
    /// </summary>
    public string NodeId { get; set; } = string.Empty;

    /// <summary>
    /// 节点名称
    /// </summary>
    public string Name { get; set; } = String.Empty;

    /// <summary>
    /// 节点的值
    /// </summary>
    [Reactive]
    public string Value { get; set; } = String.Empty;

    /// <summary>
    /// 节点的类型
    /// </summary>
    public string Type { get; set; } = String.Empty;

    /// <summary>
    /// 节点描述
    /// </summary>
    public string Description { get; set; } = String.Empty;

    /// <summary>
    /// 克隆
    /// </summary>
    public NodeInfo Clone()
    {
        return new NodeInfo
        {
            NodeId = this.NodeId,
            Type = this.Type,
            Name = this.Name,
            Description = this.Description,
            Value = this.Value
        };
    }
}