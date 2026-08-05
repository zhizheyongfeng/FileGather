using System;
using FileGather.Android.Services;

namespace FileGather.Android.Android;

public class StoragePermissionService : IStoragePermissionService
{
    public bool HasFullStorageAccess()
    {
        return OperatingSystem.IsAndroidVersionAtLeast(30) && global::Android.OS.Environment.IsExternalStorageManager;
    }

    public void RequestFullStorageAccess()
    {
        if (!OperatingSystem.IsAndroidVersionAtLeast(30))
            return;

        var context = global::Android.App.Application.Context;
        var intent = new global::Android.Content.Intent(global::Android.Provider.Settings.ActionManageAppAllFilesAccessPermission);
        intent.SetData(global::Android.Net.Uri.Parse("package:" + context.PackageName));
        intent.AddFlags(global::Android.Content.ActivityFlags.NewTask);
        context.StartActivity(intent);
    }
}
