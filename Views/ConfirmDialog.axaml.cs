using System.Threading.Tasks;
using Avalonia.Controls;

namespace FileGather.Views;

public partial class ConfirmDialog : Window
{
    public ConfirmDialog()
    {
        InitializeComponent();
        CancelButton.Click += (_, _) => Close(false);
        ConfirmButton.Click += (_, _) => Close(true);
    }

    /// <summary>弹出模态确认框；用户点击“确认”返回 true，取消/关闭返回 false。</summary>
    public static async Task<bool> ShowAsync(
        Window? owner, string title, string message, string confirmText)
    {
        if (owner is null)
            return false;

        var dialog = new ConfirmDialog { Title = title };
        dialog.MessageText.Text = message;
        dialog.ConfirmButton.Content = confirmText;
        return await dialog.ShowDialog<bool>(owner);
    }
}
