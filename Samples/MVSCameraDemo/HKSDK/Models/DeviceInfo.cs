using System.Runtime.InteropServices;

namespace HKSDK.Models;

/// <summary>
/// 设备信息
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct DeviceInfo
{
    /// <summary>
    /// 设备主版本
    /// </summary>
    public ushort MajorVer;
    
    /// <summary>
    /// 设备次版本
    /// </summary>
    public ushort MinorVer;
    
    /// <summary>
    /// 高MAC地址
    /// </summary>
    public uint MacAddrHigh;
    
    /// <summary>
    /// 低MAC地址
    /// </summary>
    public uint MacAddrLow;
    
    /// <summary>
    /// 设备传输的协议层类型
    /// </summary>
    public uint LayerType;

    /// <summary>
    /// 预留字段
    /// </summary>
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
    public uint[] Reserved;
    
    /// <summary>
    /// Gige设备信息
    /// </summary>
    public GigeDeviceInfo GigEInfo;
}