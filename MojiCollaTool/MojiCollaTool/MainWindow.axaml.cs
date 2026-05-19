using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MojiCollaTool;

public partial class MainWindow : Window
{
    private List<MojiPanel> _mojiPanels = new();
    private ObservableCollection<MojiPanel> _viewMojiPanels = new();
    public CanvasData CanvasData { get; set; } = new CanvasData();
    private CanvasEditWindow? _canvasEditWindow;
    private string? _lastUsedDirectory;
    private bool _runEvent = false;
    private bool _forceClose = false;

    public MainWindow()
    {
        InitializeComponent();
        _lastUsedDirectory = DataIO.GetExeDirPath();
        Title = $"MojiCollaTool ver{System.Reflection.Assembly.GetExecutingAssembly().GetName().Version}";
        MojiListView.ItemsSource = _viewMojiPanels;
        ThemeComboBox.SelectedIndex = ThemeManager.CurrentSetting switch
        {
            AppTheme.Light => 1,
            AppTheme.Dark  => 2,
            _              => 0
        };

        CanvasGrid.AddHandler(DragDrop.DropEvent, CanvasGrid_Drop);
        CanvasGrid.AddHandler(DragDrop.DragOverEvent, CanvasGrid_DragOver);
        DragDrop.SetAllowDrop(CanvasGrid, true);

        CanvasGrid.PointerWheelChanged += CanvasGrid_PointerWheelChanged;
        MojiListView.PointerReleased += MojiListView_PointerReleased;

        Activated += (_, _) => UpdateMojiList();
        Closing += async (_, e) =>
        {
            if (_forceClose) return;
            if (_mojiPanels.Count > 0)
            {
                e.Cancel = true;
                if (await ShowOKCancelDialog("文字データが存在しています。終了しても問題ありませんか？"))
                {
                    _forceClose = true;
                    Close();
                }
            }
        };

        ResetScale();
    }

    private void UpdateMojiList()
    {
        _viewMojiPanels.Clear();
        foreach (var p in _mojiPanels) _viewMojiPanels.Add(p);
    }

    private async void InitButton_Click(object? sender, RoutedEventArgs e)
    {
        if (_mojiPanels.Count > 0)
        {
            if (!await ShowOKCancelDialog("文字データが存在しています。削除しても問題ありませんか？")) return;
            RemoveAllMojiPanel();
        }

        var files = await PickImageFileAsync();
        if (files == null) return;

        try
        {
            _canvasEditWindow?.Close();
            CanvasData.Init();
            DataIO.InitWorkingDirectory();
            UnloadImage(ImageControl1); UnloadImage(ImageControl2);
            CanvasData.ImageData1 = LoadImage(ImageControl1, files);
            CanvasData.UpdateCanvasSize();
            UpdateCanvas();
            DataIO.CopyImageToWorkingDirectory(1, files);
            _lastUsedDirectory = Path.GetDirectoryName(files);
        }
        catch (Exception ex) { await ShowError("初期化エラー", ex); }
    }

    private async void SwapImageButton_Click(object? sender, RoutedEventArgs e)
    {
        var files = await PickImageFileAsync();
        if (files == null) return;
        await SwapImageAsync(files);
    }

    private async Task SwapImageAsync(string filePath)
    {
        try
        {
            _canvasEditWindow?.Close();
            CanvasData.Init();
            DataIO.DeleteAllWorkingDirImage(filePath);
            UnloadImage(ImageControl1); UnloadImage(ImageControl2);
            CanvasData.ImageData1 = LoadImage(ImageControl1, filePath);
            CanvasData.UpdateCanvasSize();
            UpdateCanvas();
            DataIO.CopyImageToWorkingDirectory(1, filePath);
            _lastUsedDirectory = Path.GetDirectoryName(filePath);
        }
        catch (Exception ex) { await ShowError("画像入れ替えエラー", ex); }
    }

    private async void MultiImageButton_Click(object? sender, RoutedEventArgs e)
    {
        var filePath = await PickImageFileAsync();
        if (filePath == null) return;

        if (CanvasData.ImageData1.IsNullData()) { await SwapImageAsync(filePath); return; }

        try
        {
            _canvasEditWindow?.Close();
            CanvasData.ImageData2.Init();
            DataIO.DeleteWorkingDirImage(2, filePath);
            UnloadImage(ImageControl2);
            CanvasData.ImageData2 = LoadImage(ImageControl2, filePath);
            CanvasData.ModifyImageSize();
            CanvasData.UpdateCanvasSize();
            UpdateCanvas();
            DataIO.CopyImageToWorkingDirectory(2, filePath);
            _lastUsedDirectory = Path.GetDirectoryName(filePath);
        }
        catch (Exception ex) { await ShowError("画像追加エラー", ex); }
    }

    private ImageData LoadImage(Image image, string filePath)
    {
        var bmp = ImageUtil.LoadImageSource2(filePath);
        image.Source = bmp;
        return new ImageData(bmp.PixelSize.Width, bmp.PixelSize.Height);
    }

    private static void UnloadImage(Image image) => image.Source = null;

    private async void LoadProjectButton_Click(object? sender, RoutedEventArgs e)
    {
        if (_mojiPanels.Count > 0)
        {
            if (!await ShowOKCancelDialog("文字データが存在しています。置き換えても問題ありませんか？")) return;
        }

        var topLevel = TopLevel.GetTopLevel(this)!;
        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "プロジェクトを開く",
            SuggestedStartLocation = await TryGetFolder(_lastUsedDirectory),
            FileTypeFilter = new[] { new FilePickerFileType("mctzip project file") { Patterns = new[] { "*.mctzip" } } }
        });
        if (files.Count == 0) return;
        await LoadProjectAsync(files[0].Path.LocalPath);
    }

    private async Task LoadProjectAsync(string filePath)
    {
        try
        {
            _canvasEditWindow?.Close();
            RemoveAllMojiPanel();
            UnloadImage(ImageControl1); UnloadImage(ImageControl2);
            DataIO.InitWorkingDirectory();
            DataIO.ReadProjectDataToWorkingDir(filePath);

            var img1 = DataIO.GetWorkingDirImagePath(1);
            if (!string.IsNullOrEmpty(img1)) LoadImage(ImageControl1, img1);
            var img2 = DataIO.GetWorkingDirImagePath(2);
            if (!string.IsNullOrEmpty(img2)) LoadImage(ImageControl2, img2);

            CanvasData = DataIO.ReadCanvasDataFromWorkingDir();
            UpdateCanvas();

            foreach (var md in DataIO.ReadMojiDatasFromWorkingDir())
                AddMojiPanel(new MojiPanel(md, this));

            _lastUsedDirectory = Path.GetDirectoryName(filePath);
        }
        catch (Exception ex) { await ShowError($"{filePath} プロジェクト読み出しエラー", ex); }
    }

    private async void SaveProjectButton_Click(object? sender, RoutedEventArgs e)
    {
        var topLevel = TopLevel.GetTopLevel(this)!;
        var file = await topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "プロジェクトを保存",
            SuggestedStartLocation = await TryGetFolder(_lastUsedDirectory),
            SuggestedFileName = $"MCToolProject{DateTime.Now:yyyyMMdd-HHmmss}.mctzip",
            FileTypeChoices = new[] { new FilePickerFileType("mctzip project file") { Patterns = new[] { "*.mctzip" } } }
        });
        if (file == null) return;

        try
        {
            DataIO.WriteWorkingDirToProjectDataFile(file.Path.LocalPath, _mojiPanels.Select(x => x.MojiData), CanvasData);
            _lastUsedDirectory = Path.GetDirectoryName(file.Path.LocalPath);
            await ShowInfo($"{file.Path.LocalPath} プロジェクト保存完了");
        }
        catch (Exception ex) { await ShowError("プロジェクト保存エラー", ex); }
    }

    private async void OutputImageButton_Click(object? sender, RoutedEventArgs e)
    {
        var topLevel = TopLevel.GetTopLevel(this)!;
        var file = await topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "画像を出力",
            SuggestedStartLocation = await TryGetFolder(_lastUsedDirectory),
            SuggestedFileName = $"MojiColla{DateTime.Now:yyyyMMdd-HHmmss}.png",
            FileTypeChoices = new[]
            {
                new FilePickerFileType("png file") { Patterns = new[] { "*.png" } },
                new FilePickerFileType("jpg file") { Patterns = new[] { "*.jpg" } }
            }
        });
        if (file == null) return;

        double preScale = ScalingTextBox.Value;
        try
        {
            UpdateScale(100);
            var w = (int)MainCanvas.Bounds.Width;
            var h = (int)MainCanvas.Bounds.Height;
            if (w <= 0 || h <= 0) { await ShowError("キャンバスサイズが無効です"); return; }

            using var bmp = new RenderTargetBitmap(new Avalonia.PixelSize(w, h), new Avalonia.Vector(96, 96));
            bmp.Render(MainCanvas);
            bmp.Save(file.Path.LocalPath);

            _lastUsedDirectory = Path.GetDirectoryName(file.Path.LocalPath);
            await ShowInfo($"{file.Path.LocalPath} 画像出力完了");
        }
        catch (Exception ex) { await ShowError("画像出力エラー", ex); }
        finally { UpdateScale(preScale); }
    }

    private void ThemeComboBox_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (!_runEvent) return;
        ThemeManager.SetTheme(ThemeComboBox.SelectedIndex switch
        {
            1 => AppTheme.Light,
            2 => AppTheme.Dark,
            _ => AppTheme.System
        });
    }

    private void ScalingTextBox_ValueChanged(object? sender, UpDownTextBoxEvent e)
    {
        if (!_runEvent) return;
        UpdateScale(e.Value);
    }

    private void ResetScale()
    {
        _runEvent = false;
        UpdateScale(100);
        ScalingTextBox.SetValue(100, false);
        _runEvent = true;
    }

    private void UpdateScale(double pct)
    {
        var scale = pct / 100.0;
        CanvasLTC.LayoutTransform = new ScaleTransform(scale, scale);
    }

    private void CanvasGrid_PointerWheelChanged(object? sender, PointerWheelEventArgs e)
    {
        if (e.Delta.Y > 0) ScalingTextBox.RunUpButton();
        if (e.Delta.Y < 0) ScalingTextBox.RunDownButton();
    }

    private void MojiListView_PointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (MojiListView.SelectedItem is MojiPanel p) p.ShowMojiWindow();
    }

    private void AddTextButton_Click(object? sender, RoutedEventArgs e)
    {
        AddMojiPanel(new MojiPanel(GetNextMojiId(), this));
    }

    public void ReproductionMoji(MojiPanel mojiPanel)
    {
        AddMojiPanel(new MojiPanel(mojiPanel.MojiData.Reproduct(GetNextMojiId()), this));
    }

    private int GetNextMojiId() =>
        _mojiPanels.Count <= 0 ? 1 : _mojiPanels.Max(x => x.Id) + 1;

    private void AddMojiPanel(MojiPanel mojiPanel)
    {
        _mojiPanels.Add(mojiPanel);
        UpdateMojiList();
        MainCanvas.Children.Add(mojiPanel);
    }

    public void RemoveMojiPanel(MojiPanel mojiPanel)
    {
        _mojiPanels.Remove(mojiPanel);
        UpdateMojiList();
        MainCanvas.Children.Remove(mojiPanel);
    }

    public void RemoveAllMojiPanel()
    {
        while (_mojiPanels.Count > 0) RemoveMojiPanel(_mojiPanels.First());
    }

    private void CanvasEditButton_Click(object? sender, RoutedEventArgs e)
    {
        if (_canvasEditWindow == null || !_canvasEditWindow.IsVisible)
        {
            _canvasEditWindow = new CanvasEditWindow(CanvasData, this);
            _canvasEditWindow.Show(this);
        }
    }

    public void UpdateCanvas()
    {
        if (!CanvasData.ImageData1.IsNullData())
        {
            ImageControl1.Width  = CanvasData.ImageData1.ModifiedWidth;
            ImageControl1.Height = CanvasData.ImageData1.ModifiedHeight;
        }
        if (!CanvasData.ImageData2.IsNullData())
        {
            ImageControl2.Width  = CanvasData.ImageData2.ModifiedWidth;
            ImageControl2.Height = CanvasData.ImageData2.ModifiedHeight;
        }

        MainCanvas.Width  = CanvasData.CanvasWidth;
        MainCanvas.Height = CanvasData.CanvasHeight;

        CanvasBackgroundRect.Fill   = new SolidColorBrush(CanvasData.CanvasColor);
        CanvasBackgroundRect.Width  = CanvasData.CanvasWidth;
        CanvasBackgroundRect.Height = CanvasData.CanvasHeight;

        var m1 = CanvasData.GetImage1Margin();
        Canvas.SetLeft(ImageControl1, m1.Left);
        Canvas.SetTop(ImageControl1,  m1.Top);

        var m2 = CanvasData.GetImage2Margin();
        Canvas.SetLeft(ImageControl2, m2.Left);
        Canvas.SetTop(ImageControl2,  m2.Top);
    }

    private async void CanvasGrid_Drop(object? sender, DragEventArgs e)
    {
        var asyncTransfer = e.DataTransfer as IAsyncDataTransfer;
        var items = asyncTransfer == null ? null : (await asyncTransfer.TryGetFilesAsync())?.ToArray();
        if (items == null || items.Length == 0) return;
        var filePath = items[0].Path.LocalPath;

        switch (Path.GetExtension(filePath).ToLower())
        {
            case ".jpg": case ".png":
                await SwapImageAsync(filePath); break;
            case ".mctzip":
                if (_mojiPanels.Count > 0)
                    if (!await ShowOKCancelDialog("文字データが存在しています。置き換えても問題ありませんか？")) return;
                await LoadProjectAsync(filePath); break;
        }
    }

    private void CanvasGrid_DragOver(object? sender, DragEventArgs e)
    {
        e.DragEffects = DragDropEffects.Copy;
        e.Handled = true;
    }

    // ─── ダイアログヘルパー ───────────────────────────────
    public static async Task<bool> ShowOKCancelDialog(string message)
    {
        var owner = GetCurrentWindow();
        if (owner == null) return false;
        return await AppMessageBox.ShowOKCancel(owner, message);
    }

    public static async Task ShowError(string message, Exception? ex = null)
    {
        var sb = new StringBuilder();
        sb.AppendLine(message);
        if (ex != null) sb.Append(ex.Message);
        DataIO.WriteErrorLog(sb.ToString());
        var owner = GetCurrentWindow();
        if (owner != null) await AppMessageBox.ShowError(owner, sb.ToString());
    }

    public static async Task ShowInfo(string message)
    {
        var owner = GetCurrentWindow();
        if (owner != null) await AppMessageBox.ShowInfo(owner, message);
    }

    private static Window? GetCurrentWindow() =>
        (Avalonia.Application.Current?.ApplicationLifetime
            as Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime)
            ?.MainWindow;

    // ─── ファイル選択ヘルパー ─────────────────────────────
    private async Task<string?> PickImageFileAsync()
    {
        var topLevel = TopLevel.GetTopLevel(this)!;
        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "画像を選択",
            SuggestedStartLocation = await TryGetFolder(_lastUsedDirectory),
            AllowMultiple = false,
            FileTypeFilter = new[] { new FilePickerFileType("image files") { Patterns = new[] { "*.jpg", "*.png" } } }
        });
        return files.Count > 0 ? files[0].Path.LocalPath : null;
    }

    private async Task<IStorageFolder?> TryGetFolder(string? path)
    {
        if (string.IsNullOrEmpty(path)) return null;
        try { return await TopLevel.GetTopLevel(this)!.StorageProvider.TryGetFolderFromPathAsync(new Uri(path)); }
        catch { return null; }
    }
}
