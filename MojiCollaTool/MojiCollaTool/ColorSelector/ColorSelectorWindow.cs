using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MojiCollaTool.ColorSelector;

/// <summary>
/// Avalonia 版カラーピッカーウィンドウ。
/// マテリアルデザインパレット・色履歴・RGBA スライダー・Hex 入力に対応。
/// </summary>
public class ColorSelectorWindow : Window
{
    private const int HistoryMax = 20;
    private static readonly List<Color> _colorHistory = Enumerable.Repeat(Colors.White, HistoryMax).ToList();

    private readonly Color _initialColor;
    private readonly Action<Color>? _callback;

    private byte _r, _g, _b, _a;
    private bool _updating;

    private Border _afterPreview = null!;
    private Slider _rSlider = null!, _gSlider = null!, _bSlider = null!, _aSlider = null!;
    private TextBox _rBox = null!, _gBox = null!, _bBox = null!, _aBox = null!;
    private TextBox _hexBox = null!;
    private readonly List<Button> _historyButtons = new();

    public SolidColorBrush NextBrush => new SolidColorBrush(Color.FromArgb(_a, _r, _g, _b));

    public ColorSelectorWindow(Color currentColor, Action<Color>? callback = null)
    {
        _initialColor = currentColor;
        _callback = callback;
        _r = currentColor.R;
        _g = currentColor.G;
        _b = currentColor.B;
        _a = currentColor.A;

        Title = "色の選択";
        Width = 450;
        CanResize = false;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;

        Content = BuildContent();
    }

    public new Task<bool?> ShowDialog(Window owner) => ShowDialog<bool?>(owner);

    private Control BuildContent()
    {
        var outer = new StackPanel { Margin = new Thickness(8), Spacing = 6 };

        // Color history row
        var historyWrap = new WrapPanel();
        for (int i = 0; i < HistoryMax; i++)
        {
            var btn = CreateColorButton(_colorHistory[i], 20, 20);
            int idx = i;
            btn.Click += (_, _) => ApplyColor(((SolidColorBrush)_historyButtons[idx].Background!).Color);
            _historyButtons.Add(btn);
            historyWrap.Children.Add(btn);
        }
        outer.Children.Add(historyWrap);

        // Material Design palette grid
        outer.Children.Add(BuildPaletteGrid());

        // Before / After preview
        var previewRow = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8 };
        var beforePreview = new Border { Width = 80, Height = 36, Background = new SolidColorBrush(_initialColor), BorderBrush = Brushes.Gray, BorderThickness = new Thickness(1) };
        _afterPreview = new Border { Width = 80, Height = 36, Background = new SolidColorBrush(_initialColor), BorderBrush = Brushes.Gray, BorderThickness = new Thickness(1) };
        previewRow.Children.Add(new TextBlock { Text = "変更前", VerticalAlignment = VerticalAlignment.Center });
        previewRow.Children.Add(beforePreview);
        previewRow.Children.Add(new TextBlock { Text = "変更後", VerticalAlignment = VerticalAlignment.Center });
        previewRow.Children.Add(_afterPreview);
        outer.Children.Add(previewRow);

        // RGBA sliders
        var slidersGrid = new Grid();
        slidersGrid.ColumnDefinitions.Add(new ColumnDefinition(24, GridUnitType.Pixel));
        slidersGrid.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));
        slidersGrid.ColumnDefinitions.Add(new ColumnDefinition(52, GridUnitType.Pixel));
        for (int i = 0; i < 4; i++) slidersGrid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));

        (_rSlider, _rBox) = AddSliderRow(slidersGrid, "R", _r, 0);
        (_gSlider, _gBox) = AddSliderRow(slidersGrid, "G", _g, 1);
        (_bSlider, _bBox) = AddSliderRow(slidersGrid, "B", _b, 2);
        (_aSlider, _aBox) = AddSliderRow(slidersGrid, "A", _a, 3);

        _rSlider.ValueChanged += OnSliderChanged;
        _gSlider.ValueChanged += OnSliderChanged;
        _bSlider.ValueChanged += OnSliderChanged;
        _aSlider.ValueChanged += OnSliderChanged;

        outer.Children.Add(slidersGrid);

        // Hex input row
        var hexRow = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 4 };
        hexRow.Children.Add(new TextBlock { Text = "Hex:", VerticalAlignment = VerticalAlignment.Center, Width = 36 });
        _hexBox = new TextBox { Text = ToHex(), Width = 120 };
        _hexBox.LostFocus += HexBox_LostFocus;
        _hexBox.KeyDown += (_, e) => { if (e.Key == Avalonia.Input.Key.Enter) ParseHexBox(); };
        hexRow.Children.Add(_hexBox);
        outer.Children.Add(hexRow);

        // OK / Cancel
        var btnRow = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right, Spacing = 8 };
        var okBtn = new Button { Content = "OK", Width = 80 };
        okBtn.Click += (_, _) => { AddToHistory(Color.FromArgb(_a, _r, _g, _b)); Close(true); };
        var cancelBtn = new Button { Content = "キャンセル", Width = 80 };
        cancelBtn.Click += (_, _) => Close(false);
        btnRow.Children.Add(okBtn);
        btnRow.Children.Add(cancelBtn);
        outer.Children.Add(btnRow);

        return new ScrollViewer { Content = outer };
    }

    private Control BuildPaletteGrid()
    {
        int cols = ColorPalette.ColorList.GetLength(0);
        int rows = ColorPalette.ColorList.GetLength(1);
        var grid = new Grid();
        for (int c = 0; c < cols; c++) grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
        for (int r = 0; r < rows; r++) grid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));

        for (int col = 0; col < cols; col++)
        {
            for (int row = 0; row < rows; row++)
            {
                var color = ColorPalette.GetColorFromUInt(ColorPalette.ColorList[col, row]);
                var btn = CreateColorButton(color, 20, 20);
                var c = color;
                btn.Click += (_, _) => ApplyColor(c);
                Grid.SetColumn(btn, col);
                Grid.SetRow(btn, row);
                grid.Children.Add(btn);
            }
        }
        return grid;
    }

    private static Button CreateColorButton(Color color, double w, double h) => new Button
    {
        Width = w, Height = h,
        Padding = new Thickness(0),
        Margin = new Thickness(1),
        Background = new SolidColorBrush(color)
    };

    private (Slider slider, TextBox box) AddSliderRow(Grid grid, string label, byte value, int row)
    {
        var lbl = new TextBlock { Text = label, VerticalAlignment = VerticalAlignment.Center };
        var slider = new Slider { Minimum = 0, Maximum = 255, Value = value };
        var box = new TextBox { Text = value.ToString(), Margin = new Thickness(4, 0, 0, 0) };
        box.LostFocus += (s, _) => OnTextBoxChanged(s as TextBox);
        box.KeyDown += (s, e) => { if (e.Key == Avalonia.Input.Key.Enter) OnTextBoxChanged(s as TextBox); };

        Grid.SetColumn(lbl, 0); Grid.SetRow(lbl, row);
        Grid.SetColumn(slider, 1); Grid.SetRow(slider, row);
        Grid.SetColumn(box, 2); Grid.SetRow(box, row);
        grid.Children.Add(lbl);
        grid.Children.Add(slider);
        grid.Children.Add(box);
        return (slider, box);
    }

    private void OnSliderChanged(object? sender, RangeBaseValueChangedEventArgs e)
    {
        if (_updating) return;
        _updating = true;
        _r = (byte)_rSlider.Value;
        _g = (byte)_gSlider.Value;
        _b = (byte)_bSlider.Value;
        _a = (byte)_aSlider.Value;
        UpdateBoxes();
        UpdateHex();
        UpdateAfterPreview();
        _callback?.Invoke(Color.FromArgb(_a, _r, _g, _b));
        _updating = false;
    }

    private void OnTextBoxChanged(TextBox? box)
    {
        if (_updating || box == null) return;
        if (!byte.TryParse(box.Text, out byte val)) return;
        _updating = true;
        if (box == _rBox) { _r = val; _rSlider.Value = val; }
        else if (box == _gBox) { _g = val; _gSlider.Value = val; }
        else if (box == _bBox) { _b = val; _bSlider.Value = val; }
        else if (box == _aBox) { _a = val; _aSlider.Value = val; }
        UpdateHex();
        UpdateAfterPreview();
        _callback?.Invoke(Color.FromArgb(_a, _r, _g, _b));
        _updating = false;
    }

    private void HexBox_LostFocus(object? sender, Avalonia.Interactivity.RoutedEventArgs e) => ParseHexBox();

    private void ParseHexBox()
    {
        var text = _hexBox.Text?.TrimStart('#') ?? "";
        if (text.Length == 6 && TryParseHexByte(text, 0, out byte r) && TryParseHexByte(text, 2, out byte g) && TryParseHexByte(text, 4, out byte b))
            ApplyColor(Color.FromArgb(_a, r, g, b));
        else if (text.Length == 8 && TryParseHexByte(text, 0, out byte a2) && TryParseHexByte(text, 2, out byte r2) && TryParseHexByte(text, 4, out byte g2) && TryParseHexByte(text, 6, out byte b2))
            ApplyColor(Color.FromArgb(a2, r2, g2, b2));
    }

    private static bool TryParseHexByte(string s, int start, out byte result) =>
        byte.TryParse(s.Substring(start, 2), System.Globalization.NumberStyles.HexNumber, null, out result);

    private void ApplyColor(Color color)
    {
        _updating = true;
        _r = color.R; _g = color.G; _b = color.B; _a = color.A;
        _rSlider.Value = _r; _gSlider.Value = _g; _bSlider.Value = _b; _aSlider.Value = _a;
        UpdateBoxes();
        UpdateHex();
        UpdateAfterPreview();
        _callback?.Invoke(Color.FromArgb(_a, _r, _g, _b));
        _updating = false;
    }

    private void UpdateBoxes()
    {
        _rBox.Text = _r.ToString();
        _gBox.Text = _g.ToString();
        _bBox.Text = _b.ToString();
        _aBox.Text = _a.ToString();
    }

    private void UpdateHex() => _hexBox.Text = ToHex();

    private void UpdateAfterPreview() => _afterPreview.Background = new SolidColorBrush(Color.FromArgb(_a, _r, _g, _b));

    private string ToHex() => $"#{_a:X2}{_r:X2}{_g:X2}{_b:X2}";

    private void AddToHistory(Color color)
    {
        if (_colorHistory[0] == color) return;
        _colorHistory.Insert(0, color);
        _colorHistory.RemoveAt(HistoryMax);
        for (int i = 0; i < HistoryMax; i++)
            _historyButtons[i].Background = new SolidColorBrush(_colorHistory[i]);
    }
}
