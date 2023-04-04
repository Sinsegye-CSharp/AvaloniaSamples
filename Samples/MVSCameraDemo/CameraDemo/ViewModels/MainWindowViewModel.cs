using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Metadata;
using HKSDK;
using HKSDK.Enums;
using HKSDK.Models;
using ReactiveUI;
using Image = System.Drawing.Image;

namespace CameraDemo.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    #region Private Fields

    // 图像接收回调函数
    private readonly ReceiveImageDataDelegate _receiveImageData;

    // 相机句柄
    private IntPtr _cameraHandel;

    // 是否保存图片
    private bool _isSaveImage;

    private readonly IDictionary<string, IntPtr> _cameraIntPtrDictionary;

    #endregion

    #region Public Properties

    private IntPtr _handle;

    /// <summary>
    /// 播放视频的窗口句柄
    /// </summary>
    public IntPtr Handle
    {
        get => _handle;
        set => this.RaiseAndSetIfChanged(ref _handle, value);
    }

    /// <summary>
    /// 相机列表
    /// </summary>
    public ObservableCollection<string> CameraList { get; }

    private string _selectedCamera = "";

    /// <summary>
    /// 选中的相机
    /// </summary>
    public string SelectedCamera
    {
        get => _selectedCamera;
        set => this.RaiseAndSetIfChanged(ref _selectedCamera, value);
    }

    private string _currentFps = "";

    /// <summary>
    /// 当前帧率
    /// </summary>
    public string CurrentFps
    {
        get => _currentFps;
        set => this.RaiseAndSetIfChanged(ref _currentFps, value);
    }

    private string _resultingFrameRate = "";

    /// <summary>
    /// 实际帧速率帧率
    /// </summary>
    public string ResultingFrameRate
    {
        get => _resultingFrameRate;
        set => this.RaiseAndSetIfChanged(ref _resultingFrameRate, value);
    }

    private string _settingFps = "";

    /// <summary>
    /// 要设置的帧率
    /// </summary>
    public string SettingFps
    {
        get => _settingFps;
        set => this.RaiseAndSetIfChanged(ref _settingFps, value);
    }

    private bool _isPlaying;

    /// <summary>
    /// 是否正在播放
    /// </summary>
    public bool IsPlaying
    {
        get => _isPlaying;
        set => this.RaiseAndSetIfChanged(ref _isPlaying, value);
    }

    private string _imageSavePath =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "CameraTest");

    /// <summary>
    /// 图片保存路径
    /// </summary>
    public string ImageSavePath
    {
        get => _imageSavePath;
        set => this.RaiseAndSetIfChanged(ref _imageSavePath, value);
    }

    #endregion

    #region Ctor

    /// <summary>
    /// 构造函数
    /// </summary>
    public MainWindowViewModel()
    {
        _receiveImageData = OnReceiveImage;
        CameraList = new ObservableCollection<string>();
        _cameraIntPtrDictionary = new Dictionary<string, IntPtr>();

        Init();
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// 开始播放
    /// </summary>
    public void StartPlay()
    {
        var isOk = HkCameraControl.MV_CC_CreateHandle(out _cameraHandel, _cameraIntPtrDictionary[SelectedCamera]);
        Console.WriteLine($"创建句柄{isOk:x8}");

        isOk = HkCameraControl.MV_CC_OpenDevice(_cameraHandel, 1, 0);
        Console.WriteLine($"打开设备{isOk:x8}");

        isOk = HkCameraControl.MV_CC_RegisterImageCallBackEx(_cameraHandel, _receiveImageData, new IntPtr());
        Console.WriteLine($"注册取图回调{isOk:x8}");

        isOk = HkCameraControl.MV_CC_StartGrabbing(_cameraHandel);
        Console.WriteLine($"开始采集{isOk:x8}");

        isOk = HkCameraControl.MV_CC_Display(_cameraHandel, Handle);
        Console.WriteLine($"开始播放{isOk:x8}");

        this.IsPlaying = true;

        GetFpsValue();
    }

    /// <summary>
    /// 是否可以开始播放
    /// </summary>
    /// <returns></returns>
    [DependsOn(nameof(SelectedCamera))]
    public bool CanStartPlay(object _) => !string.IsNullOrWhiteSpace(SelectedCamera);

    /// <summary>
    /// 停止播放
    /// </summary>
    public void Stop()
    {
        var b = HkCameraControl.MV_CC_StopGrabbing(_cameraHandel);
        Console.WriteLine($"停止采集{b:x8}");

        b = HkCameraControl.MV_CC_CloseDevice(_cameraHandel);
        Console.WriteLine($"关闭设备{b:x8}");

        b = HkCameraControl.MV_CC_DestroyHandle(_cameraHandel);
        Console.WriteLine($"释放句柄{b:x8}");

        this.IsPlaying = false;
        this.CurrentFps = "";
        this.SettingFps = "";
        this.ResultingFrameRate = "";
    }

    /// <summary>
    /// 抓取图片
    /// </summary>
    public void GrabPicture()
    {
        _isSaveImage = true;
    }

    /// <summary>
    /// 修改帧率
    /// </summary>
    public void ModifyFps()
    {
        var isOk = HkCameraControl.MV_CC_SetFloatValue(_cameraHandel, "AcquisitionFrameRate", float.Parse(SettingFps));
        if (isOk != 0)
        {
            Console.WriteLine($"设置帧率失败，错误码{isOk:x8}");
            return;
        }

        GetFpsValue();
    }

    [DependsOn(nameof(SettingFps))]
    public bool CanModifyFps(object _)
    {
        if (SettingFps == CurrentFps) return false;
        return float.TryParse(SettingFps, out var _);
    }

    /// <summary>
    /// 更改保存位置
    /// </summary>
    public async Task ChangeSavePath(Window window)
    {
        OpenFolderDialog dialog = new OpenFolderDialog();
        var result = await dialog.ShowAsync(window);
        if(string.IsNullOrWhiteSpace(result)) return;

        this.ImageSavePath = result;
    }

    #endregion

    #region Private Methods

    // 初始化，获取相机列表等
    private void Init()
    {
        var isOk = HkCameraControl.MV_CC_EnumDevices((uint)DeviceLayerType.Gige, out var deviceList);

        if (isOk != 0)
        {
            Console.WriteLine($"请求出错，错误码{isOk:x8}");
            return;
        }

        if (deviceList.DeviceNumber == 0)
        {
            Console.WriteLine($"没有可用设备");
            return;
        }

        Console.WriteLine($"获取到{deviceList.DeviceNumber}个设备");
        for (var i = 0; i < deviceList.DeviceNumber; i++)
        {
            Console.WriteLine($"    设备{i + 1}：");

            var devicePtr = deviceList.DeviceInfos[i];
            var device = Marshal.PtrToStructure<DeviceInfo>(devicePtr);

            var deviceModel = Encoding.UTF8.GetString(device.GigEInfo.ModelName).Trim('\0');
            Console.WriteLine($"        名称：{deviceModel}");

            var deviceSerialNumber = Encoding.UTF8.GetString(device.GigEInfo.SerialNumber).Trim('\0');
            Console.WriteLine($"        序列号：{deviceSerialNumber}");

            var deviceVersion = Encoding.UTF8.GetString(device.GigEInfo.DeviceVersion);
            Console.WriteLine($"        版本：{deviceVersion}");

            var ip1 = (device.GigEInfo.CurrentIp & 0xff000000) >> 24;
            var ip2 = (device.GigEInfo.CurrentIp & 0x00ff0000) >> 16;
            var ip3 = (device.GigEInfo.CurrentIp & 0x0000ff00) >> 8;
            var ip4 = device.GigEInfo.CurrentIp & 0x000000ff;
            Console.WriteLine($"        设备IP：{ip1}:{ip2}:{ip3}:{ip4}");

            _cameraIntPtrDictionary.Add($"{deviceModel}({deviceSerialNumber})", devicePtr);
            CameraList.Add($"{deviceModel}({deviceSerialNumber})");
        }

        // ReSharper disable once AccessToStaticMemberViaDerivedType
        if (App.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime lifetime)
        {
            lifetime.Exit += (_, _) => { Stop(); };
        }
    }

    // 接收到图片，进行处理，保存
    private void OnReceiveImage(IntPtr dataPtr, IntPtr imageFrameInfoPtr, IntPtr userCustom)
    {
        if (!_isSaveImage) return;
        var frameInfo = Marshal.PtrToStructure<FrameOutInfo>(imageFrameInfoPtr);
        Console.WriteLine(
            $"{frameInfo.HostTimeStamp}获取到图像：{frameInfo.Width}x{frameInfo.Height}，格式：{frameInfo.PixelFormatType}");
        _isSaveImage = false;

        var bufferSize = frameInfo.Width * frameInfo.Height * 4 + 2048;

        // 创建Byte数组内存指针，用于接收图片数据
        var imageDataPtr = Marshal.AllocHGlobal(bufferSize);
        Marshal.WriteByte(imageDataPtr, 0);

        // 调用SDK进行图片转换
        var saveImageParams = new SaveImageParams
        {
            ImageData = dataPtr,
            DataLength = frameInfo.FrameLength,
            PixelFormatType = frameInfo.PixelFormatType,
            Width = frameInfo.Width,
            Height = frameInfo.Height,
            ImageType = ImageType.Jpeg,
            BufferSize = (uint)bufferSize,
            JpgQuality = 80,
            ImageBuffer = imageDataPtr
        };
        var b = HkCameraControl.MV_CC_SaveImageEx2(_cameraHandel, ref saveImageParams);
        Console.WriteLine($"图片转换{b:x8}");

        // 将内存指针中的图片数据拷贝到托管数组中
        var imageBytes = new byte[bufferSize];
        Marshal.Copy(imageDataPtr, imageBytes, 0, bufferSize);

        // 释放自己创建的内存指针
        Marshal.FreeHGlobal(imageDataPtr);

        // 保存图片
        using var stream = new MemoryStream(imageBytes);
        var bitmap = Image.FromStream(stream);

        try
        {
            if (!Directory.Exists(ImageSavePath))
            {
                Directory.CreateDirectory(ImageSavePath);
            }

            var dateTime = DateTimeOffset.FromUnixTimeMilliseconds(frameInfo.HostTimeStamp).LocalDateTime;
            bitmap?.Save(Path.Combine(ImageSavePath, $"{dateTime:yyyy-MM-dd HH:mm:ss}.jpg"), ImageFormat.Jpeg);

            Console.WriteLine($"保存完成");
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

    // 获取当前帧率
    private void GetFpsValue()
    {
        var isOk = HkCameraControl.MV_CC_GetFloatValue(_cameraHandel, "AcquisitionFrameRate", out var fpsSetting);
        if (isOk != 0)
        {
            Console.WriteLine($"获取失败，错误码{isOk:x8}");
            return;
        }

        isOk = HkCameraControl.MV_CC_GetFloatValue(_cameraHandel, "ResultingFrameRate", out var resultingFrameRate);
        if (isOk != 0)
        {
            Console.WriteLine($"获取失败，错误码{isOk:x8}");
            return;
        }

        this.CurrentFps = fpsSetting.CurrentValue.ToString("0.00");
        this.SettingFps = fpsSetting.CurrentValue.ToString("0.00");
        this.ResultingFrameRate = resultingFrameRate.CurrentValue.ToString("0.00");
    }

    #endregion
}