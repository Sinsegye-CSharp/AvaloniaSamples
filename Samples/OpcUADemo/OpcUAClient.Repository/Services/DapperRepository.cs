using Dapper.Contrib.Extensions;
using MySqlConnector;
using OpcUAClient.Repository.Models;

namespace OpcUAClient.Repository.Services;

public class DapperRepository : IRepository
{
    public string ConnectionString { get; set; } = string.Empty;

    public async Task AddData(HistoryDataModel historyData)
    {
        await using var connection = new MySqlConnection(ConnectionString);
        await connection.OpenAsync();

        await connection.InsertAsync(historyData);
    }

    public IRepository UseSource(string connectionString)
    {
        this.ConnectionString = connectionString;
        return this;
    }
}