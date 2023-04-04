namespace HKSDK.Models;

/// <summary>
/// GIGE设备信息
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct GigeDeviceInfo
{
    /// <summary>
    /// IP配置选项
    /// </summary>
    public uint CfgOption;

    /// <summary>
    /// 当前IP配置
    /// </summary>
    public uint IpCfgCurrent;

    /// <summary>
    /// 当前IP地址
    /// </summary>
    public uint CurrentIp;

    /// <summary>
    /// 当前子网掩码
    /// </summary>
    public uint CurrentSubNetMask;

    /// <summary>
    /// 当前网关
    /// </summary>
    public uint DefaultGateWay;

    /// <summary>
    /// 制造商名称
    /// </summary>
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
    public byte[] ManufacturerName;

    /// <summary>
    /// 设备型号
    /// </summary>
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
    public byte[] ModelName;

    /// <summary>
    /// 设备版本
    /// </summary>
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
    public byte[] DeviceVersion;

    /// <summary>
    /// 制造商的具体信息
    /// </summary>
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 48)]
    public byte[] ManufacturerSpecificInfo;

    /// <summary>
    /// 序列号
    /// </summary>
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
    public byte[] SerialNumber;

    /// <summary>
    /// 用户自定义名称
    /// </summary>
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
    public byte[] UserDefinedName;

    /// <summary>
    /// 网口IP地址
    /// </summary>
    public uint NetExport;

    /// <summary>
    /// 预留字段
    /// </summary>
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
    public uint[] Reserved;
}