# 文件收集器 (FileGather)

一个简单的桌面应用，用于把散落在电脑各处的文件按条件收集到统一位置，方便打包带走。

使用企业微信、微信等工具下载的文件常常分布在不同的目录和层级里，离职或换电脑时容易丢失。FileGather 可以按文件扩展名和文件名关键词检索指定目录（含子目录）下的文件，然后统一复制或移动到目标文件夹，方便整理和带走。

## 功能特性

- **按扩展名筛选**：支持 Word / PDF / Excel / PPT 等任意后缀（逗号分隔，可自定义）
- **按文件名关键词筛选**：可选，只检索文件名包含指定关键词的文件
- **递归检索**：包含子文件夹（可关闭）
- **复制 / 移动**：复制保留源文件，移动则移除源文件
- **扁平化收集**：所有文件直接放入目标文件夹，不保留原目录层级
- **重名自动改名**：目标位置存在同名文件时自动生成 `文件名 (1).ext`、`文件名 (2).ext`，不覆盖已有文件
- **实时进度**：执行时显示进度条和完成统计

## 使用说明

1. 双击运行 `FileGather.exe`
2. **选择源文件夹**：要检索的目录
3. **设置筛选条件**：
   - 文件扩展名（如 `docx,doc,pdf`）
   - 文件名关键词（可选，留空表示不过滤）
   - 是否包含子文件夹
4. 点击 **开始检索**，预览符合条件的结果列表
5. **选择目标文件夹**：收集文件存放的位置
6. 选择操作方式：**复制**（保留源文件）或 **移动**（移除源文件）
7. 点击 **开始执行**，完成后查看底部统计

> 提示：源文件夹和目标文件夹不能相同。

## 技术栈

- [Avalonia UI](https://avaloniaui.net/) 12.x（跨平台桌面框架，XAML + C#）
- [.NET 10](https://dotnet.microsoft.com/)
- [CommunityToolkit.Mvvm](https://github.com/CommunityToolkit/dotnet)

## 构建与发布

> **在 Windows 上打包 exe ？** 请参考 [BUILD_ON_WINDOWS.md](BUILD_ON_WINDOWS.md)，含详细的 .NET SDK 安装、克隆代码和打包步骤。

```bash
# 本地运行（Linux 下可直接运行）
dotnet run

# 发布 Windows 单文件自包含应用（无需安装 .NET 运行时）
dotnet publish -c Release -r win-x64 --self-contained true \
  -p:PublishSingleFile=true \
  -p:IncludeNativeLibrariesForSelfExtract=true \
  -p:DebugType=None -p:DebugSymbols=false
```

产物位于 `bin/Release/net10.0/win-x64/publish/FileGather.exe`，拷贝到 Windows 电脑双击即可运行。

## 项目结构

```
FileGather/
├── Models/              # 数据模型
│   └── ScannedFile.cs   # 检索到的文件信息
├── Services/            # 核心逻辑
│   ├── FileScanner.cs   # 递归扫描 + 扩展名/关键词过滤
│   └── FileTransfer.cs  # 扁平化复制/移动 + 重名自动改名
├── ViewModels/          # MVVM ViewModel
├── Views/               # XAML 界面
└── tests/               # 核心逻辑测试（独立于主项目）
```

## 许可证

本项目基于 [MIT License](LICENSE) 开源。
