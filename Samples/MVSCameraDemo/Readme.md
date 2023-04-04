## MVSCameraDemo项目简介

此Demo是连接海康工业相机MVS的演示Demo，开发依赖为.net 6，运行环境为Linux 64位，引用了海康MVS的Linux SDK（C语言版）。  

运行此项目还需要安装特殊依赖：`sudo apt install libgdiplus`,此库是.net进行Bitmap保存为图片的必须库，否则保存图片会失败。

注意：此项目中有图片保存功能，代码中默认位置为桌面，如自己修改保存位置，需特别注意权限问题，Linux权限较严，很可能保存失败。

### 项目目录介绍

* HKSDK：引用的MVS的Linux SDK，转为C#的相应方法
* CameraDemo：使用Avalonia UI进行的桌面程序开发，引用了HKSDK项目，具有扫描网络中相机、相机视频播放、相机图片保存以及帧率设置等功能。

### 特别注意

虽然HKSDK中，添加了对MVS的Linux SDK的直接引用（Lib文件夹下），但某些Linux分发版中，扔无法加载相应依赖，需要启动的时候手动进行环境变量的添加：`LD_LIBRARY_PATH=/Lib`，其中键不可更改，值修改为自己对应目录
