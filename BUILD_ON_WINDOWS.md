# 在 Windows 上构建 FileGather.exe

本文件面向 **Windows 电脑的用户**：从 GitHub 拉取源代码后，在本地打包出可直接双击运行的 `FileGather.exe`。

> 简单说，只需要一个条件：**电脑上装了 .NET SDK**（注意是 SDK，不是 Runtime）。满足后照下面「第 3 步」执行一条命令即可。

---

## 打包需要哪些条件

| 条件 | 说明 | 是否必须 |
|------|------|---------|
| **.NET SDK 10.0** | 编译器/构建工具。**不是** Runtime（运行时），两者区别见下 | ✅ 必须 |
| **Git**（可选） | 用于命令行拉取代码；不装的话可用网页下载 ZIP | 可选 |
| **目标电脑装 .NET** | 打包用的是 `--self-contained`，exe 自带运行时，目标电脑**无需**安装任何东西 | ❌ 不需要 |

**SDK 和 Runtime 的区别**
- **SDK**（含 `dotnet` 构建命令）→ **打包时**需要
- **Runtime**（仅运行 exe）→ **运行**别人打好的 exe 才需要；本项目的 exe 已自包含运行时，连这个都不需要

---

## 第 1 步：检查电脑是否已具备打包条件

打开命令提示符（`Win + R` 输入 `cmd` 回车），输入：

```
dotnet --version
```

看到几种结果，走对应分支：

| 命令结果 | 含义 | 下一步 |
|---------|------|--------|
| 显示版本号，如 `10.0.302` | **已装好 .NET SDK**，条件已满足 | 直接跳去 **第 3 步 打包** |
| 显示 `9.x`、`8.x` 等低版本 | 装了旧版 SDK，可能不适配本项目 | 去 **第 2 步** 安装 10.0 |
| 报错 "未找到命令" / "'dotnet' 不是内部或外部命令" | **未安装** | 去 **第 2 步** 安装 |

---

## 第 2 步：安装 .NET SDK（仅当你没装或版本过低时）

访问 https://dotnet.microsoft.com/download/dotnet/10.0

- 下载 **.NET SDK 10.0.x**（下载列表里带 **"SDK"** 字样那一项，**不要**下 "Runtime"）
- 双击安装，一路默认即可
- 安装完成后，**关掉并重新打开**命令提示符，再执行 `dotnet --version` 确认能显示版本号

> 你的电脑如果已经装了 Visual Studio（带 .NET 桌面开发工作负载），SDK 通常已包含，直接执行第 1 步检查即可。

---

## 第 3 步：获取源代码

任选一种方式：

**方式 A：命令行克隆（需要 Git）**
安装 [Git for Windows](https://git-scm.com/download/win)（一路默认）后执行（用 HTTPS 地址，避免 SSH 密钥配置问题）：
```
git clone https://github.com/zhizheyongfeng/FileGather.git
cd FileGather
```

**方式 B：网页下载压缩包（不需要 Git）**
1. 打开仓库网页 `https://github.com/zhizheyongfeng/FileGather`
2. 点绿色的 **Code** 按钮 → **Download ZIP**
3. 解压到任意目录，进入解压后的 `FileGather` 文件夹

---

## 第 4 步：打包

> **重要**：下面命令都必须在 **`FileGather` 项目文件夹内**执行（能看到 `FileGather.csproj` 这个文件的那个目录）。
> 用哪条命令，取决于你的终端类型，先看下方 [如何判断自己的终端](#如何判断自己的终端)。

### 方式一：命令提示符 cmd（推荐）

在 `FileGather` 文件夹的**地址栏**输入 `cmd` 回车，打开后执行：

```
dotnet publish -c Release -r win-x64 --self-contained true ^
  -p:PublishSingleFile=true ^
  -p:IncludeNativeLibrariesForSelfExtract=true ^
  -p:DebugType=None -p:DebugSymbols=false
```

### 方式二：bash（Git Bash / WSL 等）

如果在 **bash** 里执行，**不要用 `^` 换行符**（那是 cmd 的语法，bash 里会报 `command not found`）。把命令写成一整行：

```
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:DebugType=None -p:DebugSymbols=false
```

### 方式三：PowerShell

PowerShell 里用反引号 `` ` `` 换行，或同样写成一整行：

```
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:DebugType=None -p:DebugSymbols=false
```

> 通用说明：
> - 第一次构建会自动下载依赖，需要几分钟，请耐心等待，看到 `Build succeeded` 即成功
> - `--self-contained true` 会把 .NET 运行时打包进 exe，**目标电脑上不需要再装任何东西**
> - 如果是 **ARM 架构** 的 Windows（部分 Surface / 骁龙笔记本），把 `win-x64` 换成 `win-arm64`

### 如何判断自己的终端

| 现象 | 终端类型 | 用哪个命令 |
|------|---------|-----------|
| 黑色窗口，标题是 **cmd.exe**，路径形如 `C:\...` | 命令提示符 | 方式一 |
| 路径形如 `/c/...`、`/home/...`，输入 `^` 不生效 | bash（Git Bash / WSL） | 方式二 |
| 蓝色窗口，标题是 **Windows PowerShell**，提示符是 `PS C:\...` | PowerShell | 方式三 |

---

## 第 5 步：找到 exe

构建完成后，exe 位于：

```
FileGather\bin\Release\net10.0\win-x64\publish\FileGather.exe
```

这个文件 **单个即可使用**，拷贝到任意位置双击即可运行，无需在目标电脑安装任何东西。

---

## 常见问题

**Q：报错 "'dotnet' 不是内部或外部命令"**
A：SDK 没装好或环境变量没刷新。重新运行一次安装程序，并**关闭命令提示符再重新打开**。

**Q：报错 `MSB1009: 项目文件不存在`，或 `bash: -p:... command not found`**
A：两种常见原因：
1. **没在项目文件夹里**——先 `cd FileGather`（或用 Download ZIP 解压后进入那个目录），确认能看到 `FileGather.csproj` 再执行。
2. **终端类型不匹配**——如果提示 `command not found`，说明你在 **bash**（Git Bash / WSL）里用了 cmd 的 `^` 换行语法。请按上方「第 4 步」**方式二**，把命令写成**一整行**再执行。

**Q：装了新版 SDK 但命令还是提示没装？**
A：确认下载的是 "SDK" 而不是 "Runtime"，并且**重新打开**命令提示符（旧的不会自动加载新环境变量）。

**Q：下载 GitHub 很慢 / 克隆失败**
A：用"方式 B"的 Download ZIP 直接下载压缩包，或使用代理 / 镜像加速。

**Q：构建失败，提示依赖错误**
A：先执行 `dotnet restore` 再重新构建。若仍失败，把完整报错截图发到仓库 Issues。
