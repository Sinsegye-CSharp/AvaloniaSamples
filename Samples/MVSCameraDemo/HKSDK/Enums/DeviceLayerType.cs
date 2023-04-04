namespace HKSDK.Enums;

/// <summary>
/// 设备传输层协议类型
/// </summary>
public enum DeviceLayerType : uint
{
    /// <summary>
    /// 未知类型
    /// </summary>
    Unknown = 0x00000000,

    /// <summary>
    /// GIGE设备
    /// </summary>
    Gige = 0x00000001,

    /// <summary>
    /// 1394-a/b设备
    /// </summary>
    M1394 = 0x00000002,

    /// <summary>
    /// USB3.0设备
    /// </summary>
    Usb = 0x00000004,

    /// <summary>
    /// CameraLink设备
    /// </summary>
    CameraLink = 0x00000008
}