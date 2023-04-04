using System.Collections.ObjectModel;

namespace OpcUAClient.Models;

/// <summary>
/// 节点Model
/// </summary>
public class NodeModel
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
    /// 是否含有子节点
    /// </summary>
    public bool HasChild { get; set; }
    
    /// <summary>
    /// 子节点集合
    /// </summary>
    public ObservableCollection<NodeModel> Child { get; set; } = new();
}