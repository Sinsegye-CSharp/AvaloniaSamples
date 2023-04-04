using HKSDK.Enums;

namespace HKSDK.Models;

/// <summary>
/// 保存图像参数
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct SaveImageParams
{
    /// <summary>
    /// 指向图像数据的指针，类型为char
    /// </summary>
    public IntPtr ImageData;

    /// <summary>
    /// 数据长度
    /// </summary>
    public uint DataLength;

    /// <summary>
    /// 输入数据的像素格式
    /// </summary>
    public PixelFormatType PixelFormatType;

    /// <summary>
    /// 图像宽
    /// </summary>
    public ushort Width;

    /// <summary>
    /// 图像高
    /// </summary>
    public ushort Height;

    /// <summary>
    /// 输出图片缓存的指针，unsigned char类型
    /// </summary>
    public IntPtr ImageBuffer;

    /// <summary>
    /// 输出图片的大小
    /// </summary>
    public uint ImageLength;

    /// <summary>
    /// 提供的输出缓冲区大小
    /// </summary>
    public uint BufferSize;

    /// <summary>
    /// 输出的图片类型
    /// </summary>
    public ImageType ImageType;

    /// <summary>
    /// JPG编码质量（50-99），其他格式无效
    /// </summary>
    public uint JpgQuality;

    /// <summary>
    /// 插值方法
    /// </summary>
    public uint MethodValue;

    /// <summary>
    /// 预留字段
    /// </summary>
    [MarshalAs(UnmanagedType.ByValArray,SizeConst = 3)]
    public uint[] Reserved;
}