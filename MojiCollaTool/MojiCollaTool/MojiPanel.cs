using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.VisualTree;

namespace MojiCollaTool;

/// <summary>
/// 1つの文字要素をキャンバス上に配置・レンダリングするパネル。
/// Grid を継承し、背景ボックスと文字レイアウトを重ねて描画する。
/// ドラッグで位置変更、ダブルクリックで設定ウィンドウを開く。
/// </summary>
public class MojiPanel : Grid
{
    public MojiData MojiData { get; private set; }
    public int Id => MojiData.Id;
    public string ExampleText => MojiData.ExampleText;
    public bool ShowTopmost { get; set; }

    private readonly MainWindow _mainWindow;
    private MojiWindow? _mojiWindow;

    private Point _dragStartPointerPos;
    private double _dragStartX, _dragStartY;
    private bool _isDragging;

    public MojiPanel(int id, MainWindow mainWindow) : this(new MojiData(id), mainWindow) { }

    public MojiPanel(MojiData mojiData, MainWindow mainWindow)
    {
        MojiData = mojiData;
        _mainWindow = mainWindow;

        DoubleTapped += (_, _) => ShowMojiWindow();

        UpdateMojiView(true);
    }

    public void ShowMojiWindow()
    {
        if (_mojiWindow == null || !_mojiWindow.IsVisible)
        {
            _mojiWindow = new MojiWindow(this);
            _mojiWindow.Show(_mainWindow);
        }
        else
        {
            _mojiWindow.Activate();
        }
    }

    public void UpdateMojiView(bool isDecorChanged)
    {
        // キャンバス上の位置を設定
        Canvas.SetLeft(this, MojiData.X);
        Canvas.SetTop(this, MojiData.Y);

        // 回転変換
        if (MojiData.IsRotateActive)
        {
            RenderTransformOrigin = RelativePoint.Center;
            RenderTransform = new RotateTransform(MojiData.RotateAngle);
        }
        else
        {
            RenderTransform = null;
        }

        // 子要素を再構築
        Children.Clear();
        BuildChildren();
    }

    private void BuildChildren()
    {
        var padding = MojiData.IsBackgroundBoxExists ? MojiData.BackgroundBoxPadding : 0;

        // 背景ボックス（下レイヤー）
        if (MojiData.IsBackgroundBoxExists)
        {
            var bgBorder = new Border
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                Background = new SolidColorBrush(MojiData.BackgroundBoxColor),
                BorderBrush = new SolidColorBrush(MojiData.BackgroundBoxBorderColor),
                BorderThickness = new Thickness(MojiData.BackgroundBoxBorderThickness),
                CornerRadius = new CornerRadius(MojiData.BackgroundBoxCornerRadius)
            };
            Children.Add(bgBorder);
        }

        // テキストレイアウト（上レイヤー）
        var textContainer = new Border { Padding = new Thickness(padding) };
        textContainer.Child = BuildTextLayout();
        Children.Add(textContainer);
    }

    private Control BuildTextLayout()
    {
        var lines = MojiData.GetTextLines();

        if (MojiData.TextDirection == TextDirection.Yokogaki)
        {
            var outer = new StackPanel { Orientation = Avalonia.Layout.Orientation.Vertical };
            for (int i = 0; i < lines.Length; i++)
            {
                var linePanel = new StackPanel { Orientation = Avalonia.Layout.Orientation.Horizontal };
                if (i > 0) linePanel.Margin = new Thickness(0, MojiData.LineMargin, 0, 0);
                foreach (char c in lines[i])
                    linePanel.Children.Add(new DecoratedCharacterControl(c, MojiData));
                outer.Children.Add(linePanel);
            }
            return outer;
        }
        else // Tategaki
        {
            var outer = new StackPanel { Orientation = Avalonia.Layout.Orientation.Horizontal };
            for (int i = 0; i < lines.Length; i++)
            {
                var colPanel = new StackPanel { Orientation = Avalonia.Layout.Orientation.Vertical };
                if (i > 0) colPanel.Margin = new Thickness(MojiData.LineMargin, 0, 0, 0);
                foreach (char c in lines[i])
                    colPanel.Children.Add(new DecoratedCharacterControl(c, MojiData));
                outer.Children.Add(colPanel);
            }
            return outer;
        }
    }

    public void Reproduction() => _mainWindow.ReproductionMoji(this);
    public void Remove() => _mainWindow.RemoveMojiPanel(this);

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        _dragStartPointerPos = e.GetPosition(this.GetVisualParent());
        _dragStartX = MojiData.X;
        _dragStartY = MojiData.Y;
        _isDragging = true;
        e.Pointer.Capture(this);
        e.Handled = true;
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);
        if (!_isDragging) return;
        var pos = e.GetPosition(this.GetVisualParent());
        MojiData.X = _dragStartX + (pos.X - _dragStartPointerPos.X);
        MojiData.Y = _dragStartY + (pos.Y - _dragStartPointerPos.Y);
        Canvas.SetLeft(this, MojiData.X);
        Canvas.SetTop(this, MojiData.Y);
        _mojiWindow?.UpdateXY(MojiData.X, MojiData.Y);
        e.Handled = true;
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        _isDragging = false;
        e.Pointer.Capture(null);
        e.Handled = true;
    }
}
