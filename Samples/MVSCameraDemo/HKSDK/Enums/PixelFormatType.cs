namespace HKSDK.Enums;

/// <summary>
/// 图像像素格式
/// </summary>
public enum PixelFormatType: long
{
    Mono8 = 0x01080001,
    Mono10 = 0x01100003,
    Mono10Packed = 0x010C0004,
    Mono12 = 0x01100005,
    Mono12Packed = 0x010C0006,
    Mono16 = 0x01100007,
    RGB8Packed = 0x02180014,
    YUV422_8 = 0x02100032,
    YUV422_8_UYVY = 0x0210001F,
    BayerGR8 = 0x01080008,
    BayerRG8 = 0x01080009,
    BayerGB8 = 0x0108000A,
    BayerBG8 = 0x0108000B,
    BayerGB10 = 0x0110000e,
    BayerGB12 = 0x01100012,
    BayerGB12Packed = 0x010C002C
}