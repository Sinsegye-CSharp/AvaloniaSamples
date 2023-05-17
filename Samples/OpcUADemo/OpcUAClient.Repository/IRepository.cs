using OpcUAClient.Repository.Models;

namespace OpcUAClient.Repository;

public interface IRepository
{
    /// <summary>
    /// 连接字符串
    /// </summary>
    public string ConnectionString { get; set; }

    /// <summary>
    /// 添加一条数据
    /// </summary>
    /// <param name="historyData">要添加的数据</param>
    Task AddData(HistoryDataModel historyData);

    /// <summary>
    /// 使用指定的数据源
    /// </summary>
    /// <param name="connectionString">数据源的连接字符串</param>
    /// <returns>数据库实体</returns>
    IRepository UseSource(string connectionString);
}