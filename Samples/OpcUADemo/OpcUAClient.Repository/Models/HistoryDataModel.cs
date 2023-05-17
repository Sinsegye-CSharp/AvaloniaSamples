namespace OpcUAClient.Repository.Models;

[Dapper.Contrib.Extensions.Table("history_data")]
public class HistoryDataModel
{
    /// <summary>
    /// 节点ID
    /// </summary>
    public string NodeId { get; set; } = string.Empty;

    /// <summary>
    /// 节点名称
    /// </summary>
    public string NodeName { get; set; } = string.Empty;

    /// <summary>
    /// 节点的值
    /// </summary>
    public float NodeValue { get; set; }

    /// <summary>
    /// 记录时间
    /// </summary>
    public DateTime RecordTime { get; set; }
}