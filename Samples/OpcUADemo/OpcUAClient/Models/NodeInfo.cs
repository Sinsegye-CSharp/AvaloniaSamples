namespace OpcUAClient.Models;

public class NodeInfo
{
    /// <summary>
    /// 节点Id
    /// </summary>
    public string NodeId { get; set; } = string.Empty;

    /// <summary>
    /// 节点名称
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 节点的值
    /// </summary>
    public string Value { get; set; } = string.Empty;

    /// <summary>
    /// 节点的类型
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// 节点描述
    /// </summary>
    public string Description { get; set; } = string.Empty;

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