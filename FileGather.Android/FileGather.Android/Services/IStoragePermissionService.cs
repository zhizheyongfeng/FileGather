namespace FileGather.Android.Services;

/// <summary>
/// 平台相关的存储权限能力。Android 10+ 分区存储下，应用需要通过「所有文件访问」
/// 权限才能用 System.IO 直接读写用户文件夹；桌面等平台则始终视为已授权。
/// </summary>
public interface IStoragePermissionService
{
    bool HasFullStorageAccess();
    void RequestFullStorageAccess();
}
