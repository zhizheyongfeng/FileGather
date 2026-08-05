using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FileGather.Models;

namespace FileGather.Services;

public class FileScanner
{
    /// <summary>
    /// 递归扫描源目录，按扩展名和文件名关键词过滤，返回匹配的文件列表。
    /// </summary>
    /// <param name="sourceDir">源目录（绝对路径）</param>
    /// <param name="extensions">扩展名集合，不含点号、小写，如 { "docx", "pdf" }</param>
    /// <param name="keyword">文件名关键词（忽略大小写），为空则不过滤</param>
    /// <param name="includeSubfolders">是否包含子文件夹</param>
    /// <param name="excludeDir">需要跳过的目录（如位于源目录内的目标目录），可为 null</param>
    public IReadOnlyList<ScannedFile> Scan(
        string sourceDir,
        IReadOnlyCollection<string> extensions,
        string? keyword,
        bool includeSubfolders,
        string? excludeDir)
    {
        var results = new List<ScannedFile>();
        var options = new EnumerationOptions
        {
            RecurseSubdirectories = includeSubfolders,
            IgnoreInaccessible = true,
            AttributesToSkip = FileAttributes.Hidden | FileAttributes.System,
        };

        foreach (var file in Directory.EnumerateFiles(sourceDir, "*", options))
        {
            if (!string.IsNullOrEmpty(excludeDir) && IsWithin(file, excludeDir))
                continue;

            var ext = Path.GetExtension(file).TrimStart('.').ToLowerInvariant();
            if (!extensions.Contains(ext))
                continue;

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var name = Path.GetFileName(file);
                if (!name.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                    continue;
            }

            var info = new FileInfo(file);
            results.Add(new ScannedFile
            {
                FullPath = file,
                SizeBytes = info.Length,
                LastModified = info.LastWriteTime,
            });
        }

        return results;
    }

    private static bool IsWithin(string child, string parent)
    {
        var childFull = Path.TrimEndingDirectorySeparator(Path.GetFullPath(child));
        var parentFull = Path.TrimEndingDirectorySeparator(Path.GetFullPath(parent));
        return childFull.StartsWith(parentFull + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase);
    }
}
