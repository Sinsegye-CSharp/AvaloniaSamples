using System.Reactive;
using OpcUAClient.Models;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace OpcUAClient.ViewModels;

public class SettingWindowViewModel : ViewModelBase
{
    /// <summary>
    /// 采集周期
    /// </summary>
    [Reactive]
    public int AcquisitionCycle { get; set; }

    /// <summary>
    /// 是否存储到数据库
    /// </summary>
    [Reactive]
    public bool IsSaveToDatabase { get; set; }

    /// <summary>
    /// 数据库地址
    /// </summary>
    [Reactive]
    public string DatabasePath { get; set; } = string.Empty;

    /// <summary>
    /// 数据库名称
    /// </summary>
    [Reactive]
    public string DatabaseName { get; set; } = string.Empty;

    /// <summary>
    /// 用户名
    /// </summary>
    [Reactive]
    public string User { get; set; } = string.Empty;

    /// <summary>
    /// 密码
    /// </summary>
    [Reactive]
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// 数据库端口
    /// </summary>
    [Reactive]
    public int Port { get; set; }

    /// <summary>
    /// 确定命令
    /// </summary>
    public ReactiveCommand<Unit, DataRecordSettingModel> OkCommand { get; }

    /// <summary>
    /// 构造函数
    /// </summary>
    public SettingWindowViewModel()
    {
        OkCommand = ReactiveCommand.Create(Ok, this.WhenAnyValue(x => x.DatabasePath, 
            x => x.DatabaseName, x => x.User,
            x => x.Password,x=>x.IsSaveToDatabase,
            (databasePath, databaseName, user, password,isSave) =>
            {
                if (!isSave)
                {
                    return true;
                }
                return !string.IsNullOrWhiteSpace(databasePath) &&
                       !string.IsNullOrWhiteSpace(databaseName) &&
                       !string.IsNullOrWhiteSpace(user) &&
                       !string.IsNullOrWhiteSpace(password);
            }));
    }


    private DataRecordSettingModel Ok()
    {
        return new DataRecordSettingModel()
        {
            AcquisitionCycle = this.AcquisitionCycle,
            IsSaveToDatabase = this.IsSaveToDatabase,
            DatabasePath = this.DatabasePath,
            DatabaseName = this.DatabaseName,
            Port = this.Port,
            User = this.User,
            Password = this.Password
        };
    }
}