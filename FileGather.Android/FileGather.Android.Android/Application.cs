using Android.App;
using Android.Runtime;
using Avalonia;
using Avalonia.Android;
using FileGather.Android.Services;

namespace FileGather.Android.Android
{
    [Application]
    public class Application : AvaloniaAndroidApplication<App>
    {
        protected Application(nint javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {
        }

        protected override AppBuilder CustomizeAppBuilder(AppBuilder builder)
        {
            PlatformServices.StoragePermission = new StoragePermissionService();
            return base.CustomizeAppBuilder(builder)
            .WithInterFont();
        }
    }
}
