using HKSDK.Enums;

namespace HKSDK.Models;

/// <summary>
/// 输出的帧信息
/// </summary>
public struct FrameOutInfo
{
    /// <summary>
    /// 图像宽
    /// </summary>
    public ushort Width;

    /// <summary>
    /// 图像高
    /// </summary>
    public ushort Height;
    
    /// <summary>
    /// 像素格式
    /// </summary>
    public PixelFormatType PixelFormatType;

    /// <summary>
    /// 帧号
    /// </summary>
    public uint FrameNumber;

    /// <summary>
    /// 时间戳高32位
    /// </summary>
    public uint DevTimeStampHeight;
    
    /// <summary>
    /// 时间戳低32位
    /// </summary>
    public uint DevTimeStampLow;

    /// <summary>
    /// 保留，8字节对齐
    /// </summary>
    public uint Reserved;

    /// <summary>
    /// 主机生成的时间戳
    /// </summary>
    public long HostTimeStamp;

    /// <summary>
    /// 帧长度
    /// </summary>
    public uint FrameLength;

    /// <summary>
    /// 设备水印时标
    /// </summary>
    public uint SecondCount;

    /// <summary>
    /// 周期数
    /// </summary>
    public uint CycleCount;

    /// <summary>
    /// 周期偏移量
    /// </summary>
    public uint CycleOffset;

    /// <summary>
    /// 增益
    /// </summary>
    public float Gain;

    /// <summary>
    /// 曝光时间
    /// </summary>
    public float ExposureTime;

    /// <summary>
    /// 平均亮度
    /// </summary>
    public uint AverageBrightness;

    /// <summary>
    /// 白平衡相关
    /// </summary>
    public uint Red;

    /// <summary>
    /// 绿色
    /// </summary>
    public uint Green;

    /// <summary>
    /// 蓝色
    /// </summary>
    public uint Blue;

    /// <summary>
    /// 总帧数
    /// </summary>
    public uint FrameCounter;

    /// <summary>
    /// 触发计数
    /// </summary>
    public uint TriggerIndex;

    /// <summary>
    /// 输入
    /// </summary>
    public uint Input;

    /// <summary>
    /// 输出
    /// </summary>
    public uint Output;

    /// <summary>
    /// ROI区域
    /// </summary>
    public ushort OffsetX;

    /// <summary>
    /// 垂直偏移量
    /// </summary>
    public ushort OffsetY;

    /// <summary>
    /// Chunk宽
    /// </summary>
    public ushort ChunkWidth;

    /// <summary>
    /// Chunk高
    /// </summary>
    public ushort ChunkHeight;

    /// <summary>
    /// 本帧丢包数
    /// </summary>
    public uint LostPacket;

    /// <summary>
    /// 未解析的ChunkData个数
    /// </summary>
    public uint UnparsedChunkNumber;
}