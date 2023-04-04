using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Platform;

namespace CameraDemo.Controls;

/// <summary>
/// 海康相机控件
/// </summary>
public class HkCameraControl : NativeControlHost
{
    public static readonly StyledProperty<IntPtr> HandleProperty = AvaloniaProperty.Register<HkCameraControl, IntPtr>(
        "Handle", default, false, BindingMode.OneWayToSource);

    /// <summary>
    /// 窗口句柄
    /// </summary>
    public IntPtr Handle
    {
        get => GetValue(HandleProperty);
        set => SetValue(HandleProperty, value);
    }

    protected override IPlatformHandle CreateNativeControlCore(IPlatformHandle parent)
    {
        var handle = base.CreateNativeControlCore(parent);
        this.Handle = handle.Handle;
        return handle;
    }
}