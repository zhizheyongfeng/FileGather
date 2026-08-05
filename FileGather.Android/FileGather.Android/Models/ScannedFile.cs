using System;
using System.IO;

namespace FileGather.Android.Models;

public class ScannedFile
{
    public string FullPath { get; init; } = "";
    public long SizeBytes { get; init; }
    public DateTime LastModified { get; init; }

    public string FileName => Path.GetFileName(FullPath);
    public string Extension => Path.GetExtension(FullPath);
    public string SizeText => FormatSize(SizeBytes);
    public string ModifiedText => LastModified.ToString("yyyy-MM-dd HH:mm");

    private static string FormatSize(long bytes)
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
}
