using System;
using System.Collections.Generic;
using System.IO;
using FileGather.Models;

namespace FileGather.Services;

public class FileTransfer
{
    /// <summary>
    /// 将文件扁平化复制/移动到目标目录，重名时自动加 (1)、(2) 等后缀。
    /// </summary>
    public TransferResult Transfer(
        IReadOnlyList<ScannedFile> files,
        string targetDir,
        bool move,
        Action<ScannedFile>? onFileDone = null)
    {
        Directory.CreateDirectory(targetDir);

        int succeeded = 0;
        var succeededPaths = new List<string>();
        var errors = new List<string>();
        var usedNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var existing in Directory.EnumerateFiles(targetDir, "*", SearchOption.TopDirectoryOnly))
            usedNames.Add(Path.GetFileName(existing));

        foreach (var file in files)
        {
            try
            {
                var destName = GetUniqueName(Path.GetFileName(file.FullPath), usedNames);
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
            Failed = files.Count - succeeded,
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
}

public class TransferResult
{
    public int Succeeded { get; init; }
    public int Failed { get; init; }
    public IReadOnlyList<string> SucceededPaths { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> Errors { get; init; } = Array.Empty<string>();
}
