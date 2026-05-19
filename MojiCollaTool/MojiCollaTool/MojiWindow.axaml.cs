using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using System;
using System.IO;

namespace MojiCollaTool;

public partial class MojiWindow : Window
{
    private MojiPanel _mojiPanel;
    public bool IsHideOnly { get; set; } = true;
    private bool _runEvent = false;

    public MojiWindow(MojiPanel mojiPanel)
    {
        _mojiPanel = mojiPanel;
        InitializeComponent();

        FontFamilyComboBox.ItemsSource = FontUtil.GetFontNames();
        ShowTopMostCheckBox.IsChecked = mojiPanel.ShowTopmost;

        Opened += (_, _) =>
        {
            LoadMojiDataToWindow(_mojiPanel.MojiData);
            _runEvent = true;
        };

        Closing += (_, e) =>
        {
            if (IsHideOnly)
            {
                e.Cancel = true;
                Hide();
            }
        };
    }

    public void LoadMojiDataToWindow(MojiData mojiData)
    {
        _runEvent = false;

        Title = $"[{mojiData.Id}] {mojiData.ExampleText}";
        IDLabel.Text = $"Moji ID:{mojiData.Id}";

        TextTextBox.Text = mojiData.FullText;
        LocationXTextBox.SetValue((int)mojiData.X, false);
        LocationYTextBox.SetValue((int)mojiData.Y, false);
        DirectionComboBox.SelectedIndex = (int)mojiData.TextDirection;
        RotateTextBox.SetValue((int)mojiData.RotateAngle);
        FontSizeTextBox.SetValue(mojiData.FontSize, false);

        var fontNames = FontUtil.GetFontFamilies();
        if (fontNames.ContainsKey(mojiData.FontFamilyName))
            FontFamilyComboBox.SelectedItem = mojiData.FontFamilyName;

        BoldCheckBox.IsChecked = mojiData.IsBold;
        ItalicCheckBox.IsChecked = mojiData.IsItalic;
        CharacterMarginTextBox.SetValue((int)mojiData.CharacterMargin);
        LineMarginTextBox.SetValue((int)mojiData.LineMargin);

        ForeColorButton.Background            = new SolidColorBrush(mojiData.ForeColor);
        BorderThicknessTextBox.SetValue((int)mojiData.BorderThickness);
        BorderColorButton.Background          = new SolidColorBrush(mojiData.BorderColor);
        BorderBlurrRadiusTextBox.SetValue((int)mojiData.BorderBlurrRadius);
        SecondBorderThicknessTextBox.SetValue((int)mojiData.SecondBorderThickness);
        SecondBorderColorButton.Background    = new SolidColorBrush(mojiData.SecondBorderColor);
        SecondBorderBlurrRadiusTextBox.SetValue((int)mojiData.SecondBorderBlurrRadius);

        BackgroundBoxCheckBox.IsChecked = mojiData.IsBackgroundBoxExists;
        BackgroundBoxColorButton.Background   = new SolidColorBrush(mojiData.BackgroundBoxColor);
        BackgroundBoxPaddingTextBox.SetValue((int)mojiData.BackgroundBoxPadding);
        BackgroundBoxPaddingCornerRadiusTextBox.SetValue((int)mojiData.BackgroundBoxCornerRadius);
        BackgroundBoxBorderColorButton.Background = new SolidColorBrush(mojiData.BackgroundBoxBorderColor);
        BackgroundBoxBorderThicknessTextBox.SetValue((int)mojiData.BackgroundBoxBorderThickness);

        _runEvent = true;
    }

    public void UpdateXY(double x, double y)
    {
        _runEvent = false;
        LocationXTextBox.SetValue((int)x, false);
        LocationYTextBox.SetValue((int)y, false);
        _runEvent = true;
    }

    private void ReproductionButton_Click(object? sender, RoutedEventArgs e) => _mojiPanel.Reproduction();

    private async void DeleteButton_Click(object? sender, RoutedEventArgs e)
    {
        if (!await MainWindow.ShowOKCancelDialog("文字を削除してよろしいですか？")) return;
        _mojiPanel.Remove();
    }

    public void UpdateMojiView(bool isTextDecorationUpdated)
    {
        _mojiPanel.MojiData.FullText = TextTextBox.Text ?? "";
        Title = $"[{_mojiPanel.MojiData.Id}] {_mojiPanel.MojiData.ExampleText}";
        _mojiPanel.MojiData.FontSize = FontSizeTextBox.Value;
        _mojiPanel.MojiData.X = LocationXTextBox.Value;
        _mojiPanel.MojiData.Y = LocationYTextBox.Value;
        _mojiPanel.MojiData.TextDirection = (TextDirection)DirectionComboBox.SelectedIndex;
        _mojiPanel.MojiData.IsBold = BoldCheckBox.IsChecked == true;
        _mojiPanel.MojiData.IsItalic = ItalicCheckBox.IsChecked == true;
        _mojiPanel.MojiData.LineMargin = LineMarginTextBox.Value;
        _mojiPanel.MojiData.CharacterMargin = CharacterMarginTextBox.Value;
        _mojiPanel.MojiData.FontFamilyName = (string?)FontFamilyComboBox.SelectedItem ?? "";
        _mojiPanel.MojiData.BorderThickness = BorderThicknessTextBox.Value;
        _mojiPanel.MojiData.BorderBlurrRadius = BorderBlurrRadiusTextBox.Value;
        _mojiPanel.MojiData.SecondBorderThickness = SecondBorderThicknessTextBox.Value;
        _mojiPanel.MojiData.SecondBorderBlurrRadius = SecondBorderBlurrRadiusTextBox.Value;
        _mojiPanel.MojiData.IsBackgroundBoxExists = BackgroundBoxCheckBox.IsChecked == true;
        _mojiPanel.MojiData.BackgroundBoxPadding = BackgroundBoxPaddingTextBox.Value;
        _mojiPanel.MojiData.BackgroundBoxBorderThickness = BackgroundBoxBorderThicknessTextBox.Value;
        _mojiPanel.MojiData.BackgroundBoxCornerRadius = BackgroundBoxPaddingCornerRadiusTextBox.Value;
        _mojiPanel.MojiData.RotateAngle = RotateTextBox.Value;
        _mojiPanel.UpdateMojiView(isTextDecorationUpdated);
    }

    private void TextBox_TextChanged(object? sender, TextChangedEventArgs e)
    {
        if (!_runEvent) return;
        UpdateMojiView(false);
    }

    private void TextBox_ValueChanged(object? sender, UpDownTextBoxEvent e)
    {
        if (!_runEvent) return;
        UpdateMojiView(true);
    }

    private void ComboBox_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (!_runEvent) return;
        UpdateMojiView(true);
    }

    private void CheckBox_CheckChanged(object? sender, RoutedEventArgs e)
    {
        if (!_runEvent) return;
        UpdateMojiView(true);
    }

    private async void ColorButton_Click(Button button, Color currentColor, Action<Color> apply)
    {
        if (!_runEvent) return;
        var win = new ColorSelector.ColorSelectorWindow(currentColor, apply);
        win.Topmost = Topmost;
        var result = await win.ShowDialog(this);
        if (result != true) apply(currentColor);
    }

    private void ForeColorButton_Click(object? sender, RoutedEventArgs e) =>
        ColorButton_Click(ForeColorButton, _mojiPanel.MojiData.ForeColor, color =>
        {
            _mojiPanel.MojiData.ForeColor = color;
            ForeColorButton.Background = new SolidColorBrush(color);
            _mojiPanel.UpdateMojiView(true);
        });

    private void BorderColorButton_Click(object? sender, RoutedEventArgs e) =>
        ColorButton_Click(BorderColorButton, _mojiPanel.MojiData.BorderColor, color =>
        {
            _mojiPanel.MojiData.BorderColor = color;
            BorderColorButton.Background = new SolidColorBrush(color);
            _mojiPanel.UpdateMojiView(true);
        });

    private void SecondBorderColorButton_Click(object? sender, RoutedEventArgs e) =>
        ColorButton_Click(SecondBorderColorButton, _mojiPanel.MojiData.SecondBorderColor, color =>
        {
            _mojiPanel.MojiData.SecondBorderColor = color;
            SecondBorderColorButton.Background = new SolidColorBrush(color);
            _mojiPanel.UpdateMojiView(true);
        });

    private void BackgroundBoxColorButton_Click(object? sender, RoutedEventArgs e) =>
        ColorButton_Click(BackgroundBoxColorButton, _mojiPanel.MojiData.BackgroundBoxColor, color =>
        {
            _mojiPanel.MojiData.BackgroundBoxColor = color;
            BackgroundBoxColorButton.Background = new SolidColorBrush(color);
            _mojiPanel.UpdateMojiView(true);
        });

    private void BackgroundBoxBorderColorButton_Click(object? sender, RoutedEventArgs e) =>
        ColorButton_Click(BackgroundBoxBorderColorButton, _mojiPanel.MojiData.BackgroundBoxBorderColor, color =>
        {
            _mojiPanel.MojiData.BackgroundBoxBorderColor = color;
            BackgroundBoxBorderColorButton.Background = new SolidColorBrush(color);
            _mojiPanel.UpdateMojiView(true);
        });

    private void ShowTopMostCheckBox_IsCheckedChanged(object? sender, RoutedEventArgs e)
    {
        if (ShowTopMostCheckBox.IsChecked.HasValue == false) return;
        _mojiPanel.ShowTopmost = ShowTopMostCheckBox.IsChecked.Value;
        Topmost = _mojiPanel.ShowTopmost;
    }

    private async void SaveFormatButton_Click(object? sender, RoutedEventArgs e)
    {
        try
        {
            var topLevel = TopLevel.GetTopLevel(this)!;
            var file = await topLevel.StorageProvider.SaveFilePickerAsync(new Avalonia.Platform.Storage.FilePickerSaveOptions
            {
                Title = "文字フォーマットを保存",
                SuggestedStartLocation = await topLevel.StorageProvider.TryGetFolderFromPathAsync(new Uri(DataIO.GetMojiFormatDirPath())),
                FileTypeChoices = new[] { new Avalonia.Platform.Storage.FilePickerFileType("moji format files") { Patterns = new[] { "*.xml" } } }
            });
            if (file == null) return;

            var formatMojiData = _mojiPanel.MojiData.Clone();
            formatMojiData.FullText = Path.GetFileNameWithoutExtension(file.Path.LocalPath);
            DataIO.WriteMojiFormat(formatMojiData, file.Path.LocalPath);
        }
        catch (Exception ex) { await MainWindow.ShowError("文字フォーマット保存エラー", ex); }
    }

    private async void LoadFormatButton_Click(object? sender, RoutedEventArgs e)
    {
        try
        {
            var topLevel = TopLevel.GetTopLevel(this)!;
            var files = await topLevel.StorageProvider.OpenFilePickerAsync(new Avalonia.Platform.Storage.FilePickerOpenOptions
            {
                Title = "文字フォーマットを読み出す",
                SuggestedStartLocation = await topLevel.StorageProvider.TryGetFolderFromPathAsync(new Uri(DataIO.GetMojiFormatDirPath())),
                AllowMultiple = false,
                FileTypeFilter = new[] { new Avalonia.Platform.Storage.FilePickerFileType("moji format files") { Patterns = new[] { "*.xml" } } }
            });
            if (files.Count == 0) return;

            var formatMojiData = DataIO.ReadMojiData(files[0].Path.LocalPath);
            formatMojiData.Id = _mojiPanel.MojiData.Id;
            formatMojiData.X  = _mojiPanel.MojiData.X;
            formatMojiData.Y  = _mojiPanel.MojiData.Y;
            formatMojiData.FullText = _mojiPanel.MojiData.FullText;
            _mojiPanel.MojiData.Copy(formatMojiData);
            LoadMojiDataToWindow(_mojiPanel.MojiData);
            _mojiPanel.UpdateMojiView(true);
        }
        catch (Exception ex) { await MainWindow.ShowError("文字フォーマット読み出しエラー", ex); }
    }
}
