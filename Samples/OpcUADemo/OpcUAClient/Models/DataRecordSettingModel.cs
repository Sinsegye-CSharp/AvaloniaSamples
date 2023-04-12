namespace OpcUAClient.Models;

/// <summary>
/// 数据记录设置Model
/// </summary>
public class DataRecordSettingModel
{
    /// <summary>
    /// 数据采集周期(单位：s)
    /// </summary>
    public int AcquisitionCycle { get; set; } = 1;

    /// <summary>
    /// 是否存储到数据库
    /// </summary>
    public bool IsSaveToDatabase { get; set; }

    /// <summary>
    /// 数据库地址
    /// </summary>
    public string DatabasePath { get; set; } = string.Empty;

    /// <summary>
    /// 数据库名称
    /// </summary>
    public string DatabaseName { get; set; } = string.Empty;

    /// <summary>
    /// 用户名
    /// </summary>
    public string User { get; set; } = string.Empty;

    /// <summary>
    /// 密码
    /// </summary>
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// 数据库端口
    /// </summary>
    public int Port { get; set; } = 3306;

    /// <summary>
    /// 数据库连接字符串
    /// </summary>
    public string DatabaseConnectionString =>
        $"server={this.DatabasePath};port={this.Port};uid={this.User};password={this.Password}; database={DatabaseName};sslMode=none;CharSet=utf8;";
}