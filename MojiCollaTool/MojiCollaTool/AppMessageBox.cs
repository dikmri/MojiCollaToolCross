using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using System.Threading.Tasks;

namespace MojiCollaTool;

public static class AppMessageBox
{
    public static Task ShowInfo(Window owner, string message) =>
        Show(owner, "インフォメーション", message, false);

    public static Task ShowError(Window owner, string message) =>
        Show(owner, "エラー", message, false);

    public static async Task<bool> ShowOKCancel(Window owner, string message)
    {
        var result = await ShowInternal(owner, "確認", message, true);
        return result == true;
    }

    private static Task Show(Window owner, string title, string message, bool hasCancel) =>
        ShowInternal(owner, title, message, hasCancel);

    private static Task<bool?> ShowInternal(Window owner, string title, string message, bool hasCancel)
    {
        var okButton = new Button { Content = "OK", Width = 80, HorizontalAlignment = HorizontalAlignment.Center };
        var buttonPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 10,
            HorizontalAlignment = HorizontalAlignment.Center
        };
        buttonPanel.Children.Add(okButton);

        Button? cancelButton = null;
        if (hasCancel)
        {
            cancelButton = new Button { Content = "キャンセル", Width = 100, HorizontalAlignment = HorizontalAlignment.Center };
            buttonPanel.Children.Add(cancelButton);
        }

        var content = new StackPanel { Spacing = 16, Margin = new Thickness(20) };
        content.Children.Add(new TextBlock
        {
            Text = message,
            TextWrapping = TextWrapping.Wrap,
            MaxWidth = 360
        });
        content.Children.Add(buttonPanel);

        var dialog = new Window
        {
            Title = title,
            Content = content,
            SizeToContent = SizeToContent.WidthAndHeight,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            CanResize = false,
            MinWidth = 300
        };

        okButton.Click += (_, _) => dialog.Close(true);
        if (cancelButton != null) cancelButton.Click += (_, _) => dialog.Close(false);

        return dialog.ShowDialog<bool?>(owner);
    }
}
