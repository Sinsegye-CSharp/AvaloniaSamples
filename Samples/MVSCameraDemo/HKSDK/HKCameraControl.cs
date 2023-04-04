global using System.Runtime.InteropServices;
using HKSDK.Models;

namespace HKSDK;

/// <summary>
/// 接收图片数据委托
/// </summary>
public delegate void ReceiveImageDataDelegate(IntPtr dataPtr, IntPtr imageFrameInfoPtr, IntPtr userCustom);

public static class HkCameraControl
{
    /// <summary>
    /// 枚举网络中的设备
    /// </summary>
    /// <param name="layerType">传输协议类型，按位传入，可传多个</param>
    /// <param name="pstDevList">设备列表</param>
    /// <returns>接口是否运行成功</returns>
    [DllImport("Lib/libMvCameraControl")]
    public static extern int MV_CC_EnumDevices(uint layerType, out DeviceInfoList pstDevList);

    /// <summary>
    /// 创建句柄
    /// </summary>
    /// <param name="handle">设备句柄</param>
    /// <param name="pstDevInfo">设备信息</param>
    /// <returns>接口是否运行成功</returns>
    [DllImport("Lib/libMvCameraControl")]
    public static extern int MV_CC_CreateHandle(out IntPtr handle, IntPtr pstDevInfo);

    /// <summary>
    /// 销毁句柄
    /// </summary>
    /// <param name="handle">设备句柄</param>
    /// <returns>接口是否运行成功</returns>
    [DllImport("Lib/libMvCameraControl")]
    public static extern int MV_CC_DestroyHandle(IntPtr handle);

    /// <summary>
    /// 打开设备
    /// </summary>
    /// <param name="handle">设备句柄</param>
    /// <param name="nAccessMode">访问模式</param>
    /// <param name="nSwitchoverKey">访问秘钥</param>
    /// <returns>接口是否运行成功</returns>
    [DllImport("Lib/libMvCameraControl")]
    public static extern int MV_CC_OpenDevice(IntPtr handle, uint nAccessMode, ushort nSwitchoverKey);

    /// <summary>
    /// 关闭设备
    /// </summary>
    /// <param name="handle">设备句柄</param>
    /// <returns>接口是否运行成功</returns>
    [DllImport("Lib/libMvCameraControl")]
    public static extern int MV_CC_CloseDevice(IntPtr handle);

    /// <summary>
    /// 开始采集
    /// </summary>
    /// <param name="handle">设备句柄</param>
    /// <returns>接口是否运行成功</returns>
    [DllImport("Lib/libMvCameraControl")]
    public static extern int MV_CC_StartGrabbing(IntPtr handle);

    /// <summary>
    /// 停止采集
    /// </summary>
    /// <param name="handle">设备句柄</param>
    /// <returns>接口是否运行成功</returns>
    [DllImport("Lib/libMvCameraControl")]
    public static extern int MV_CC_StopGrabbing(IntPtr handle);

    /// <summary>
    /// 显示图像
    /// </summary>
    /// <param name="handle">设备句柄</param>
    /// <param name="windowHandle">显示图像的窗口句柄</param>
    /// <returns>接口是否运行成功</returns>
    [DllImport("Lib/libMvCameraControl")]
    public static extern int MV_CC_Display(IntPtr handle, IntPtr windowHandle);

    /// <summary>
    /// 注册回调函数，接收图像数据
    /// </summary>
    /// <param name="handle">设备句柄</param>
    /// <param name="receiveImageCallback">接收图片数据的回调</param>
    /// <param name="userCustom">用户自定义变量</param>
    /// <returns>接口是否运行成功</returns>
    [DllImport("Lib/libMvCameraControl")]
    public static extern int MV_CC_RegisterImageCallBackEx(IntPtr handle,
        ReceiveImageDataDelegate receiveImageCallback, IntPtr userCustom);

    /// <summary>
    /// 存储图像数据
    /// </summary>
    /// <param name="handle">设备句柄</param>
    /// <param name="saveImageParams">保存图像参数</param>
    /// <returns>接口是否运行成功</returns>
    [DllImport("Lib/libMvCameraControl")]
    public static extern int MV_CC_SaveImageEx2(IntPtr handle, ref SaveImageParams saveImageParams);

    /// <summary>
    /// 获取Float类型的设置项
    /// </summary>
    /// <param name="handle">设备句柄</param>
    /// <param name="strKey">设置项字符串名称</param>
    /// <param name="floatSetting">设置项结构</param>
    /// <returns>操作结果</returns>
    [DllImport("Lib/libMvCameraControl")]
    public static extern int MV_CC_GetFloatValue(IntPtr handle, string strKey, out FloatSettingModel floatSetting);
    
    /// <summary>
    /// 设置Float类型的设置项
    /// </summary>
    /// <param name="handle">设备句柄</param>
    /// <param name="strKey">设置项字符串名称</param>
    /// <param name="value">要设置的值</param>
    /// <returns>操作结果</returns>
    [DllImport("Lib/libMvCameraControl")]
    public static extern int MV_CC_SetFloatValue(IntPtr handle, string strKey, float value);
}