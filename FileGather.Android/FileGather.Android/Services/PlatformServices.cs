namespace FileGather.Android.Services;

/// <summary>
/// 简单服务定位器：共享项目不引用 Android 启动项目，由启动项目在应用初始化时注入平台实现。
/// </summary>
public static class PlatformServices
{
    public static IStoragePermissionService? StoragePermission { get; set; }
}
