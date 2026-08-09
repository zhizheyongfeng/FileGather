using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media;
using FileGather.Models;

namespace FileGather.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        ResultTree.ItemTemplate = new FuncTreeDataTemplate(
            typeof(FileTreeNode),
            static (item, _) => CreateNodeControl((FileTreeNode)item!),
            static item => ((FileTreeNode)item!).Children);
    }

    private static Control CreateNodeControl(FileTreeNode node)
    {
        if (node.IsFolder)
        {
            var folder = new TextBlock
            {
                VerticalAlignment = VerticalAlignment.Center,
                TextTrimming = TextTrimming.CharacterEllipsis,
            };
            folder.Bind(TextBlock.TextProperty, new Binding("DisplayText"));
            return folder;
        }

        var grid = new Grid { ColumnDefinitions = new ColumnDefinitions("*,Auto") };
        var name = new TextBlock
        {
            VerticalAlignment = VerticalAlignment.Center,
            TextTrimming = TextTrimming.CharacterEllipsis,
        };
        name.Bind(TextBlock.TextProperty, new Binding("DisplayText"));

        var detail = new TextBlock
        {
            VerticalAlignment = VerticalAlignment.Center,
            Foreground = Brushes.Gray,
            Margin = new Thickness(8, 0, 0, 0),
        };
        detail.Bind(TextBlock.TextProperty, new Binding("FileDetail"));
        Grid.SetColumn(detail, 1);

        grid.Children.Add(name);
        grid.Children.Add(detail);
        return grid;
    }
}
