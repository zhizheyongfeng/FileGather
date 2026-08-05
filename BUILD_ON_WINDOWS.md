# 在 Windows 上构建 FileGather.exe

本文件面向 **Windows 电脑的用户**：从 GitHub 拉取源代码后，在本地打包出可直接双击运行的 `FileGather.exe`。

## 准备工作

### 1. 安装 .NET SDK

访问 https://dotnet.microsoft.com/download/dotnet/10.0

- 下载 **.NET SDK 10.0.x**（带 "SDK" 字样，**不是** "Runtime"）
- 双击安装，一路默认即可

> 验证是否装好：打开命令提示符（`Win + R`，输入 `cmd`，回车），输入：
> ```
> dotnet --version
> ```
> 显示类似 `10.0.302` 即为成功。

### 2. 获取源代码

安装 [Git for Windows](https://git-scm.com/download/win)（一路默认），然后任选一种方式：

**方式 A：命令行克隆**
```
git clone git@github.com:zhizheyongfeng/FileGather.git
cd FileGather
```

**方式 B：直接下载压缩包**
1. 打开仓库网页 `https://github.com/zhizheyongfeng/FileGather`
2. 点绿色的 **Code** 按钮 → **Download ZIP**
3. 解压到任意目录，进入解压后的 `FileGather` 文件夹

## 构建

在 `FileGather` 文件夹内打开命令提示符（在文件夹地址栏输入 `cmd` 回车），执行：

```
dotnet publish -c Release -r win-x64 --self-contained true ^
  -p:PublishSingleFile=true ^
  -p:IncludeNativeLibrariesForSelfExtract=true ^
  -p:DebugType=None -p:DebugSymbols=false
```

> 说明：
> - 第一次构建会自动下载依赖，需要几分钟，请耐心等待
> - `--self-contained true` 表示把 .NET 运行时打包进 exe，目标电脑上**不需要**再装任何东西
> - 如果想发布 **ARM 架构** 的 Windows（如部分 Surface / 骁龙笔记本），把 `win-x64` 换成 `win-arm64`

## 找到 exe

构建完成后，exe 位于：

```
FileGather\bin\Release\net10.0\win-x64\publish\FileGather.exe
```

这个文件 **单个即可使用**，拷贝到任意位置双击即可运行，无需安装 .NET 运行时。

## 常见问题

**Q：报错 "未找到与命令匹配" 或 "'dotnet' 不是内部或外部命令"**
A：说明 .NET SDK 没装好或环境变量没刷新。重新运行一次安装程序，或**关闭命令提示符再重新打开**。

**Q：下载 GitHub 很慢 / 克隆失败**
A：用上面"方式 B"的 Download ZIP 直接下载压缩包，或使用代理 / 镜像加速。

**Q：构建失败，提示依赖错误**
A：先执行 `dotnet restore` 再重新构建。若仍失败，把完整报错截图发到仓库 Issues。
