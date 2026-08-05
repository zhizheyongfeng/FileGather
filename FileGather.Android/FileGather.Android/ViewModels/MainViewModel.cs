using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FileGather.Android.Models;
using FileGather.Android.Services;

namespace FileGather.Android.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    private readonly FileScanner _scanner = new();
    private readonly FileTransfer _transfer = new();

    [ObservableProperty]
    public partial string SourcePath { get; set; } = "";

    [ObservableProperty]
    public partial string TargetPath { get; set; } = "";

    [ObservableProperty]
    public partial string Extensions { get; set; } = "docx,doc,pdf,xlsx,xls,pptx,ppt";

    [ObservableProperty]
    public partial string Keyword { get; set; } = "";

    [ObservableProperty]
    public partial bool IncludeSubfolders { get; set; } = true;

    [ObservableProperty]
    public partial bool IsMove { get; set; }

    [ObservableProperty]
    public partial bool SkipExistingSame { get; set; } = true;

    [ObservableProperty]
    public partial bool IsScanning { get; set; }

    [ObservableProperty]
    public partial bool IsTransferring { get; set; }

    [ObservableProperty]
    public partial bool HasResults { get; set; }

    [ObservableProperty]
    public partial string StatusText { get; set; } = "就绪";

    [ObservableProperty]
    public partial string ResultSummary { get; set; } = "尚未检索";

    [ObservableProperty]
    public partial double ProgressValue { get; set; }

    [ObservableProperty]
    public partial double ProgressMax { get; set; } = 1;

    public ObservableCollection<ScannedFile> Results { get; } = new();

    [ObservableProperty]
    public partial bool NeedsStorageAccess { get; set; }

    public MainViewModel()
    {
        RefreshStorageAccess();
    }

    private void RefreshStorageAccess()
    {
        NeedsStorageAccess = PlatformServices.StoragePermission?.HasFullStorageAccess() == false;
    }

    [RelayCommand]
    private void GrantStorageAccess()
    {
        RefreshStorageAccess();
        if (NeedsStorageAccess)
        {
            StatusText = "请在系统设置中开启「所有文件访问」，返回后点击右上角按钮重新检测";
            PlatformServices.StoragePermission?.RequestFullStorageAccess();
        }
    }

    [RelayCommand]
    private async Task BrowseSourceAsync(Window? window)
    {
        var path = await PickFolderAsync(window);
        if (!string.IsNullOrEmpty(path))
            SourcePath = path;
    }

    [RelayCommand]
    private async Task BrowseTargetAsync(Window? window)
    {
        var path = await PickFolderAsync(window);
        if (!string.IsNullOrEmpty(path))
            TargetPath = path;
    }

    [RelayCommand]
    private async Task ScanAsync()
    {
        RefreshStorageAccess();
        if (NeedsStorageAccess)
        {
            StatusText = "请先授予「所有文件访问」权限，才能读取手机上的文件";
            return;
        }

        if (string.IsNullOrWhiteSpace(SourcePath) || !Directory.Exists(SourcePath))
        {
            StatusText = "请先选择有效的源文件夹";
            return;
        }

        var exts = ParseExtensions(Extensions);
        if (exts.Count == 0)
        {
            StatusText = "请至少填写一个文件扩展名";
            return;
        }

        var sourceDir = Path.GetFullPath(SourcePath);

        IsScanning = true;
        StatusText = "正在扫描…";
        Results.Clear();

        string? excludeDir = null;
        if (Directory.Exists(TargetPath))
        {
            var targetDir = Path.GetFullPath(TargetPath);
            if (IsSamePath(sourceDir, targetDir))
            {
                IsScanning = false;
                StatusText = "源文件夹和目标文件夹不能相同";
                return;
            }
            if (IsWithinOrSame(targetDir, sourceDir))
                excludeDir = targetDir;
        }

        var keyword = Keyword?.Trim();
        var includeSubfolders = IncludeSubfolders;

        var files = await Task.Run(() =>
            _scanner.Scan(sourceDir, exts, keyword, includeSubfolders, excludeDir));

        foreach (var f in files)
            Results.Add(f);

        IsScanning = false;
        HasResults = Results.Count > 0;
        ResultSummary = Results.Count == 0 ? "未找到符合条件的文件" : $"找到 {Results.Count} 个文件";
        StatusText = ResultSummary;
    }

    [RelayCommand]
    private async Task TransferAsync()
    {
        RefreshStorageAccess();
        if (NeedsStorageAccess)
        {
            StatusText = "请先授予「所有文件访问」权限，才能写入目标文件夹";
            return;
        }

        if (Results.Count == 0)
        {
            StatusText = "请先检索文件";
            return;
        }

        if (string.IsNullOrWhiteSpace(TargetPath))
        {
            StatusText = "请选择目标文件夹";
            return;
        }

        var targetDir = Path.GetFullPath(TargetPath);
        if (IsSamePath(Path.GetFullPath(SourcePath), targetDir))
        {
            StatusText = "源文件夹和目标文件夹不能相同";
            return;
        }

        Directory.CreateDirectory(targetDir);

        IsTransferring = true;
        ProgressValue = 0;
        ProgressMax = Results.Count;

        var files = Results.ToList();
        var move = IsMove;
        var skipExisting = SkipExistingSame;
        var done = 0;

        var result = await Task.Run(() =>
            _transfer.Transfer(files, targetDir, move, skipExisting,
                onFileDone: _ => Dispatcher.UIThread.Post(() => ProgressValue = Interlocked.Increment(ref done))));

        IsTransferring = false;
        ProgressValue = ProgressMax;

        var action = move ? "移动" : "复制";
        if (result.Skipped > 0)
            StatusText = $"{action}完成：成功 {result.Succeeded} 个，跳过已存在 {result.Skipped} 个，失败 {result.Failed} 个";
        else
            StatusText = $"{action}完成：成功 {result.Succeeded} 个，失败 {result.Failed} 个";

        if (result.Failed > 0)
            StatusText += $"　（{string.Join("；", result.Errors.Take(2))}）";

        if (move)
        {
            foreach (var path in result.SucceededPaths)
            {
                var item = Results.FirstOrDefault(f => string.Equals(f.FullPath, path, StringComparison.OrdinalIgnoreCase));
                if (item is not null)
                    Results.Remove(item);
            }
            HasResults = Results.Count > 0;
            ResultSummary = Results.Count == 0 ? "全部文件已移动完成" : $"剩余 {Results.Count} 个文件（失败项未移动）";
        }
    }

    private static async Task<string?> PickFolderAsync(Window? window)
    {
        if (window is null)
            return null;

        var topLevel = TopLevel.GetTopLevel(window);
        if (topLevel is null)
            return null;

        var folders = await topLevel.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            Title = "选择文件夹",
            AllowMultiple = false,
        });

        return folders.Count > 0 ? folders[0].TryGetLocalPath() : null;
    }

    private static List<string> ParseExtensions(string input)
    {
        return input
            .Split(new[] { ',', '，', ';', '；', ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(e => e.Trim().TrimStart('.').ToLowerInvariant())
            .Where(e => e.Length > 0)
            .Distinct()
            .ToList();
    }

    private static bool IsSamePath(string a, string b)
    {
        return string.Equals(
            Path.TrimEndingDirectorySeparator(Path.GetFullPath(a)),
            Path.TrimEndingDirectorySeparator(Path.GetFullPath(b)),
            StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsWithinOrSame(string child, string parent)
    {
        if (IsSamePath(child, parent))
            return true;
        var childFull = Path.TrimEndingDirectorySeparator(Path.GetFullPath(child));
        var parentFull = Path.TrimEndingDirectorySeparator(Path.GetFullPath(parent));
        return childFull.StartsWith(parentFull + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase);
    }
}
