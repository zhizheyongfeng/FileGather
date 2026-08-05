# 文件收集器 Android 版 (FileGather.Android)

「文件收集器」的 Android 手机版本，与桌面版逻辑完全一致：

**选择源文件夹 → 递归扫描（按扩展名 / 文件名关键词筛选）→ 选择目标文件夹 → 复制或移动文件（扁平化收集）**。

- 基于 [Avalonia UI](https://avaloniaui.net/)（跨平台 UI 框架）与 [.NET 10](https://dotnet.microsoft.com/)
- 与桌面版共用同一套检索 / 传输 / 增量同步逻辑
- 通过 Android SAF（存储访问框架）授权文件夹，配合「所有文件访问」权限可读写任意目录

## 在手机上安装 APK

直接安装打包好的 APK 文件即可：

1. 把 `FileGather.Android.Android/bin/Release/net10.0-android/<包名>-Signed.apk`（或 Debug 版 apk）传到手机上
2. 手机上打开该文件，允许「安装未知来源应用」
3. 首次打开时按提示授予存储权限

> 注意：Android 10+ 有「分区存储（Scoped Storage）」限制，用系统自带的文件夹选择器授权过的目录才能读写，这也是本应用采用文件夹选择器（而非直接输入路径）的原因。

## 构建 APK

> 详细步骤见 [BUILD_ON_ANDROID.md](BUILD_ON_ANDROID.md)

```bash
# 构建 Debug APK
dotnet build FileGather.Android.Android/FileGather.Android.Android.csproj -c Debug

# 构建 Release APK
dotnet publish FileGather.Android.Android/FileGather.Android.Android.csproj -c Release -f net10.0-android
```
