using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;

namespace FileGather.Android.Models;

/// <summary>
/// 检索结果树节点：文件夹节点（含子文件夹/文件）或文件叶子节点。
/// 文件夹节点汇总其下所有命中文件的数量与大小。
/// </summary>
public class FileTreeNode : ObservableObject
{
    public string Name { get; init; } = "";
    public string FullPath { get; init; } = "";
    public bool IsFolder { get; init; }
    public ScannedFile? File { get; init; }
    public FileTreeNode? Parent { get; set; }

    public ObservableCollection<FileTreeNode> Children { get; } = new();

    public long SizeBytes { get; private set; }
    public int FileCount { get; private set; }

    private bool _isExpanded;
    public bool IsExpanded
    {
        get => _isExpanded;
        set => SetProperty(ref _isExpanded, value);
    }

    public string DisplayText => IsFolder
        ? $"📁 {Name} ({FileCount}个 / {FormatSize(SizeBytes)})"
        : $"📄 {Name}";

    public string FileDetail => File is null ? "" : $"{File.SizeText} · {File.ModifiedText}";

    public void AddChild(FileTreeNode child)
    {
        Children.Add(child);
        child.Parent = this;
        UpdateSummary(child.SizeBytes, child.FileCount);
    }

    public void RemoveChild(FileTreeNode child)
    {
        Children.Remove(child);
        UpdateSummary(-child.SizeBytes, -child.FileCount);
    }

    private void UpdateSummary(long deltaSize, int deltaCount)
    {
        SizeBytes += deltaSize;
        FileCount += deltaCount;
        OnPropertyChanged(nameof(DisplayText));
        Parent?.UpdateSummary(deltaSize, deltaCount);
    }

    internal static string FormatSize(long bytes)
    {
        string[] units = { "B", "KB", "MB", "GB" };
        double size = bytes;
        int unit = 0;
        while (size >= 1024 && unit < units.Length - 1)
        {
            size /= 1024;
            unit++;
        }
        return $"{size:0.##} {units[unit]}";
    }

    /// <summary>
    /// 从命中文件列表构建源目录的层级树，并建立 文件完整路径 → 叶子节点 的索引。
    /// </summary>
    public static FileTreeNode BuildTree(
        string sourceDir, IReadOnlyList<ScannedFile> files, Dictionary<string, FileTreeNode> fileNodeIndex)
    {
        var rootName = Path.GetFileName(Path.TrimEndingDirectorySeparator(sourceDir));
        if (string.IsNullOrEmpty(rootName))
            rootName = sourceDir;

        var root = new FileTreeNode
        {
            Name = rootName,
            FullPath = Path.GetFullPath(sourceDir),
            IsFolder = true,
        };
        root.IsExpanded = true;

        var dirs = new Dictionary<string, FileTreeNode>(StringComparer.OrdinalIgnoreCase)
        {
            [root.FullPath] = root,
        };

        foreach (var file in files)
        {
            var fileDir = Path.GetDirectoryName(file.FullPath) ?? root.FullPath;
            var parent = GetOrCreateDir(dirs, fileDir, root.FullPath);

            var leaf = new FileTreeNode
            {
                Name = Path.GetFileName(file.FullPath),
                FullPath = file.FullPath,
                IsFolder = false,
                File = file,
                SizeBytes = file.SizeBytes,
                FileCount = 1,
            };
            parent.AddChild(leaf);
            fileNodeIndex[file.FullPath] = leaf;
        }

        return root;
    }

    private static FileTreeNode GetOrCreateDir(
        Dictionary<string, FileTreeNode> dirs, string dir, string rootFull)
    {
        if (dirs.TryGetValue(dir, out var existing))
            return existing;

        var fullDir = Path.TrimEndingDirectorySeparator(Path.GetFullPath(dir));
        var parentDir = Path.GetDirectoryName(fullDir);
        if (string.IsNullOrEmpty(parentDir) || IsSamePath(fullDir, rootFull))
        {
            // 不应走到这里：非根目录一定有父目录
            throw new InvalidOperationException($"目录 {dir} 没有可挂载的父节点");
        }

        var parent = GetOrCreateDir(dirs, parentDir, rootFull);
        var node = new FileTreeNode
        {
            Name = Path.GetFileName(fullDir),
            FullPath = fullDir,
            IsFolder = true,
        };
        parent.AddChild(node);
        dirs[fullDir] = node;
        return node;
    }

    private static bool IsSamePath(string a, string b)
    {
        return string.Equals(
            Path.TrimEndingDirectorySeparator(Path.GetFullPath(a)),
            Path.TrimEndingDirectorySeparator(Path.GetFullPath(b)),
            StringComparison.OrdinalIgnoreCase);
    }
}
