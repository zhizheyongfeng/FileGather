using Android.App;
using Android.Content.PM;
using Avalonia;
using Avalonia.Android;

namespace FileGather.Android.Android;

[Activity(
    Label = "文件收集器",
    Theme = "@style/MyTheme.NoActionBar",
    Icon = "@drawable/icon",
    MainLauncher = true,
    ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize | ConfigChanges.UiMode)]
public class MainActivity : AvaloniaMainActivity
{
}
