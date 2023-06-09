## OpcUADemo项目简介

此Demo是一个OPC UA客户端Demo，使用需要配合已有的OPC UA服务器，此Demo具有以下功能：

 * 服务器变量浏览
 * 服务器变量具体数值读取
 * 监测服务器数值型数据，并进行曲线绘制
 * 监听特殊变量，并提供修改功能。

### Demo使用说明

##### 1、与服务器连接

使用此Demo首先需要连接OPC UA服务器，服务器登录方式为用户名、密码方式，在软件指定区域内输入服务器地址、用户名、密码，点击连接按钮既可登录

软件登录后，会自动加载服务端中的变量，并以树形图展示出来，如果点击登录后，左侧没有出现变量树，则登录出现问题。

注意：变量树采用了分级加载方式，点击每级展开按钮时，会加载子节点，如没有展开按钮，则代表此节点没有子节点，不可展开

##### 2、监听变量并进行曲线绘制

连接服务端成功，并加载出变量树后，可以对``数值``型的变量进行订阅，订阅后的数值，会显示在右侧``曲线监听值列表``中，展示形式为``变量名称（变量的节点ID）``，将需要监听的值都添加到列表后，点击``开始监听``按钮，既可绘制曲线，具体操作步骤如下：
1.  输入服务器地址、用户名以及密码，点击``连接``按钮
2.  在左侧变量树中，选取需要绘制的变量，选中并点击右键，在右键菜单中点击``添加订阅``按钮
3.  在右侧``曲线监听值列表``核对需要绘制的值是否正确
4.  点击``开始监听``按钮，曲线开始绘制

##### 3、监听特殊变量并进行值修改

连接服务端成功，并加载出变量树后，可以对变量（不包括数组）进行重点监听，订阅后的变量，会显示在右侧``重点值监视区``中，并提供修改数值的输入框和按钮，点击``开始监听``按钮，既可实时读取相应的值，并进行修改，具体操作步骤如下：
1.  输入服务器地址、用户名以及密码，点击``连接``按钮
2.  在左侧变量树中，选取需要绘制的变量，选中并点击右键，在右键菜单中点击``添加到重点监听``按钮
3.  在右侧``重点值监视区``核对需要绘制的值是否正确
4.  点击``开始监听``按钮，进行数据监听

##### 4、依赖说明

此Demo使用了一些第三方组件，可根据需求选用：

* ``LiveChartsCore.SkiaSharpView.Avalonia``：用于曲线图的绘制
* ``OPCFoundation.NetStandard.Opc.Ua.Client``：OPC协会发布的.net使用OPC UA协议的包
* ``ReactiveUI.Fody``：ReactiveUI的功能扩展包，用于快速编写ViewModel中的属性

### Q&A

1、为什么变量树右键菜单中，添加订阅按钮是灰的？


订阅绘制曲线只可以订阅数值型变量，如果所选变量树节点有子节点或者是对象、字符串以及数组时，不可进行订阅 


2、为什么变量树右键菜单中，添加到重点监听是灰的？


订阅绘制曲线只可以订阅变量，如果所选变量树节点有子节点或者是对象、数组时，不可进行监听
