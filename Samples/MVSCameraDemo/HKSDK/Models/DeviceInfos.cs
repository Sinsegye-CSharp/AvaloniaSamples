namespace HKSDK.Models;

/// <summary>
/// 设备信息列表
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct DeviceInfoList
{
    /// <summary>
    /// 设备数量
    /// </summary>
    public uint DeviceNumber;

    /// <summary>
    /// 设备信息数组（最大256个设备）
    /// </summary>
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
    public IntPtr[] DeviceInfos;
}