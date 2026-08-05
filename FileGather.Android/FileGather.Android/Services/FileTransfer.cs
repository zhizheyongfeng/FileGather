using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using FileGather.Android.Models;

namespace FileGather.Android.Services;

public class FileTransfer
{
    /// <summary>
    /// 将文件扁平化复制/移动到目标目录，重名时自动加 (1)、(2) 等后缀。
    /// </summary>
    /// <param name="files">要处理的文件</param>
    /// <param name="targetDir">目标目录</param>
    /// <param name="move">true 移动，false 复制</param>
    /// <param name="skipExistingSame">增量同步：目标已存在内容相同的文件时跳过，不产生新副本</param>
    /// <param name="onFileDone">每处理完一个文件回调（含跳过的），用于进度显示</param>
    public TransferResult Transfer(
        IReadOnlyList<ScannedFile> files,
        string targetDir,
        bool move,
        bool skipExistingSame = true,
        Action<ScannedFile>? onFileDone = null)
    {
        Directory.CreateDirectory(targetDir);

        int succeeded = 0;
        int skipped = 0;
        var succeededPaths = new List<string>();
        var errors = new List<string>();
        var usedNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        // 目标目录已有文件的索引：文件名 -> (大小, 哈希)，哈希懒计算缓存
        var existing = new Dictionary<string, ExistingInfo>(StringComparer.OrdinalIgnoreCase);
        var byStem = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

        foreach (var path in Directory.EnumerateFiles(targetDir, "*", SearchOption.TopDirectoryOnly))
        {
            var name = Path.GetFileName(path);
            usedNames.Add(name);
            existing[name] = new ExistingInfo { Size = new FileInfo(path).Length };
            var stem = Path.GetFileNameWithoutExtension(name);
            if (!byStem.TryGetValue(stem, out var list))
                byStem[stem] = list = new List<string>();
            list.Add(name);
        }

        foreach (var file in files)
        {
            try
            {
                var srcName = Path.GetFileName(file.FullPath);
                var srcStem = Path.GetFileNameWithoutExtension(srcName);

                if (skipExistingSame && byStem.TryGetValue(srcStem, out var candidates))
                {
                    string? srcHash = null;
                    bool matched = false;
                    foreach (var candName in candidates)
                    {
                        if (!existing.TryGetValue(candName, out var cand) || cand.Size != file.SizeBytes)
                            continue;

                        // 大小相同才需要比内容，源哈希只算一次
                        srcHash ??= ComputeHash(file.FullPath);
                        cand.Hash ??= ComputeHash(Path.Combine(targetDir, candName));
                        if (cand.Hash == srcHash)
                        {
                            matched = true;
                            break;
                        }
                    }

                    if (matched)
                    {
                        skipped++;
                        onFileDone?.Invoke(file);
                        continue;
                    }
                }

                var destName = GetUniqueName(srcName, usedNames);
                var destPath = Path.Combine(targetDir, destName);

                if (move)
                    File.Move(file.FullPath, destPath);
                else
                    File.Copy(file.FullPath, destPath, false);

                usedNames.Add(destName);
                succeeded++;
                succeededPaths.Add(file.FullPath);
            }
            catch (Exception ex)
            {
                errors.Add($"{file.FullPath}：{ex.Message}");
            }

            onFileDone?.Invoke(file);
        }

        return new TransferResult
        {
            Succeeded = succeeded,
            Skipped = skipped,
            Failed = files.Count - succeeded - skipped,
            SucceededPaths = succeededPaths,
            Errors = errors,
        };
    }

    private static string GetUniqueName(string fileName, HashSet<string> usedNames)
    {
        if (usedNames.Add(fileName))
            return fileName;

        var ext = Path.GetExtension(fileName);
        var baseName = Path.GetFileNameWithoutExtension(fileName);
        for (int i = 1; ; i++)
        {
            var candidate = $"{baseName} ({i}){ext}";
            if (usedNames.Add(candidate))
                return candidate;
        }
    }

    private static string ComputeHash(string path)
    {
        using var stream = File.OpenRead(path);
        using var sha = SHA256.Create();
        return Convert.ToHexString(sha.ComputeHash(stream));
    }

    private sealed class ExistingInfo
    {
        public long Size { get; init; }
        public string? Hash { get; set; }
    }
}

public class TransferResult
{
    public int Succeeded { get; init; }
    public int Skipped { get; init; }
    public int Failed { get; init; }
    public IReadOnlyList<string> SucceededPaths { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> Errors { get; init; } = Array.Empty<string>();
}
