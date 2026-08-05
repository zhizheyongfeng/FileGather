# 在 Linux 上构建 FileGather Android APK

本文介绍如何从源代码构建出可在 Android 手机上安装的 APK。开发/构建环境为 **Linux（Debian 12 已验证）**。

> 只想直接安装：跳过本文，直接用打包好的 `com.zhizheyongfeng.filegather-Signed.apk` 安装到手机即可（见 [README.md](README.md)）。

---

## 一、需要准备的条件

| 条件 | 版本 | 说明 |
|------|------|------|
| **.NET SDK** | 10.0.x | 编译器/构建工具 |
| **Android 工作负载** | android（含 Mono 运行时） | 通过 `dotnet workload install android` 安装 |
| **JDK** | 17 | Android 工具链需要（Android Gradle 层） |
| **Android SDK** | platform 34+ / build-tools | 含 `aapt2`、`d8`、`zipalign` 等工具 |

### 1. 安装 .NET SDK

```bash
curl -sSL https://dot.net/v1/dotnet-install.sh | bash
export PATH="$HOME/.dotnet:$PATH"   # 建议写入 ~/.bashrc
dotnet --version                    # 应显示 10.0.x
```

### 2. 安装 Android 工作负载

```bash
dotnet workload install android
```

国内网络较慢时，可先添加腾讯云 NuGet 镜像，加速 workload 与依赖下载：

```bash
dotnet nuget add source https://mirrors.cloud.tencent.com/repository/nuget-group/ \
  -n tencent-mirror
```

### 3. 安装 JDK 17 与 Android SDK

Debian 12：

```bash
sudo apt install openjdk-17-jdk
```

Android SDK 可从 https://developer.android.com/studio 下载「Command line tools only」，解压后用 `sdkmanager` 安装所需组件：

```bash
SDKMANAGER=~/Android/Sdk/cmdline-tools/latest/bin/sdkmanager
$SDKMANAGER "platforms;android-36" "build-tools;36.0.0" "platform-tools"
```

并把 SDK 路径告知构建（写入 `~/.bashrc`）：

```bash
export ANDROID_HOME="$HOME/Android/Sdk"
export ANDROID_SDK_ROOT="$HOME/Android/Sdk"
```

---

## 二、构建 APK

在 `FileGather.Android` 目录下执行：

```bash
# Debug 包（含调试信息，速度更快）
dotnet build FileGather.Android.Android/FileGather.Android.Android.csproj -c Debug

# Release 包（推荐交付给手机安装）
dotnet publish FileGather.Android.Android/FileGather.Android.Android.csproj -c Release -f net10.0-android
```

产物（选择带 `Signed` 后缀的那个）：

```
FileGather.Android.Android/bin/Release/net10.0-android/com.zhizheyongfeng.filegather-Signed.apk
```

> 默认只编译 `android-arm64`（当前绝大多数手机架构，APK 体积最小）。
> 如需兼容 x86_64 模拟器等，把 `FileGather.Android.Android.csproj` 里的
> `<RuntimeIdentifiers>android-arm64</RuntimeIdentifiers>` 改为 `android-arm64;android-x64;android-arm`。

---

## 三、安装到手机

1. 把 `com.zhizheyongfeng.filegather-Signed.apk` 传到手机（微信文件传输 / U 盘 / 数据线均可）
2. 手机上点击该文件 → 允许「安装未知来源应用」→ 完成安装
3. 打开应用，顶部会出现黄色提示条，点「去授权」进入系统设置，开启 **所有文件访问** 权限
   - 这是 Android 10+「分区存储」的限制：不经授权，应用无法用文件路径直接读写手机存储
   - 也可以手动到：系统设置 → 应用 → 文件收集器 → 权限 → 所有文件访问 → 允许
4. 回到应用后点「去授权」（此时变为重新检测）或直接点「浏览…」选文件夹，即可正常使用

---

## 四、常见问题

**Q：`error NU1102: Unable to find package Microsoft.NETCore.App.Runtime.Mono.linux-x64`**

A：这是 .NET 10 Android 工作负载在 Linux 宿主上的已知缺陷。Release 默认开启裁剪（`PublishTrimmed`），会把宿主（linux-x64）也加入运行时包，而微软从未发布过该 linux-x64 Mono 运行时的 10.x 版本。本项目已在 csproj 中通过关闭裁剪与 AOT 规避：

```xml
<AndroidEnableProfiledAot>false</AndroidEnableProfiledAot>
<AndroidEnableAot>false</AndroidEnableAot>
<RunAOTCompilation>false</RunAOTCompilation>
<PublishTrimmed>false</PublishTrimmed>
```

副作用：APK 稍大、首启动略慢（使用解释器而非 AOT），不影响功能。若微软后续修复，可在 Windows 上开启裁剪/AOT 以获得更小更快的包。

**Q：`XA0036: 'AndroidSupportedAbis' is no longer supported`**

A：旧属性已废弃，请用 `<RuntimeIdentifiers>` 指定 ABI。

**Q：首次打开扫码/复制提示权限不足**

A：按「三、安装到手机」第 3 步开启「所有文件访问」权限。

---

## 五、与桌面版的关系

- 逻辑（检索 / 筛选 / 复制 / 移动 / 增量同步）与桌面版共用同一套代码
- 差异仅在平台层：Android 用系统文件夹选择器（SAF）+ 存储权限服务；桌面用原生目录选择器
