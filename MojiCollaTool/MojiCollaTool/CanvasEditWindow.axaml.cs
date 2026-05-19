using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using System;

namespace MojiCollaTool;

public partial class CanvasEditWindow : Window
{
    public CanvasData CanvasData { get; set; }
    private MainWindow? _mainWindow;
    private bool _runEvent = true;

    public CanvasEditWindow()
    {
        CanvasData = new CanvasData();
        InitializeComponent();
    }

    public CanvasEditWindow(CanvasData canvasData, MainWindow mainWindow)
    {
        CanvasData = canvasData;
        _mainWindow = mainWindow;
        InitializeComponent();
        UpdateView();
    }

    private void UpdateView()
    {
        _runEvent = false;

        if (CanvasData.ImageData2.IsNullData())
        {
            MultiImageLocateGroupBox.IsEnabled = false;
            Image2Rect.IsVisible = false;
        }
        else
        {
            MultiImageLocateGroupBox.IsEnabled = true;
            Image2Rect.IsVisible = true;
            switch (CanvasData.Image2LocatePosition)
            {
                case LocatePosition.Left:
                    Grid.SetRow(Image2Rect, 1); Grid.SetColumn(Image2Rect, 0); break;
                case LocatePosition.Right:
                    Grid.SetRow(Image2Rect, 1); Grid.SetColumn(Image2Rect, 2); break;
                case LocatePosition.Top:
                    Grid.SetRow(Image2Rect, 0); Grid.SetColumn(Image2Rect, 1); break;
                case LocatePosition.Bottom:
                    Grid.SetRow(Image2Rect, 2); Grid.SetColumn(Image2Rect, 1); break;
            }
        }

        ImageWidthHeighTextBlock.Text = $"←横→:{CanvasData.ImageWidth}px\n↑縦↓:{CanvasData.ImageHeight}px";
        CanvasWidthTextBox.Text = CanvasData.CanvasWidth.ToString();
        CanvasHeightTextBox.Text = CanvasData.CanvasHeight.ToString();

        TopTextBox.SetValue(CanvasData.ImageMarginTop, false);
        BottomTextBox.SetValue(CanvasData.ImageMarginBottom, false);
        LeftTextBox.SetValue(CanvasData.ImageMarginLeft, false);
        RightTextBox.SetValue(CanvasData.ImageMarginRight, false);

        CanvasColorButton.Background = new SolidColorBrush(CanvasData.CanvasColor);

        _runEvent = true;
    }

    private void Image2LocateButton(object? sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is string tag)
        {
            CanvasData.Image2LocatePosition = Enum.Parse<LocatePosition>(tag);
            CanvasData.ModifyImageSize();
            CanvasData.UpdateCanvasSize();
            UpdateView();
            _mainWindow?.UpdateCanvas();
        }
    }

    private Color GetCanvasButtonColor() =>
        CanvasColorButton.Background is SolidColorBrush b ? b.Color : Colors.White;

    private async void CanvasColorButton_Click(object? sender, RoutedEventArgs e)
    {
        var current = GetCanvasButtonColor();
        var win = new ColorSelector.ColorSelectorWindow(current, color =>
        {
            CanvasData.CanvasColor = color;
            _mainWindow?.UpdateCanvas();
        });
        win.Topmost = Topmost;
        var result = await win.ShowDialog(this);
        if (result == true)
            CanvasColorButton.Background = new SolidColorBrush(win.NextBrush.Color);
        else
        {
            CanvasData.CanvasColor = current;
            _mainWindow?.UpdateCanvas();
        }
    }

    private void DirectionTextBox_ValueChanged(object? sender, UpDownTextBoxEvent e)
    {
        if (!_runEvent) return;
        if (sender == TopTextBox)    CanvasData.ImageMarginTop    = TopTextBox.Value;
        else if (sender == BottomTextBox) CanvasData.ImageMarginBottom = BottomTextBox.Value;
        else if (sender == LeftTextBox)   CanvasData.ImageMarginLeft   = LeftTextBox.Value;
        else if (sender == RightTextBox)  CanvasData.ImageMarginRight  = RightTextBox.Value;
        CanvasData.UpdateCanvasSize();
        UpdateView();
        _mainWindow?.UpdateCanvas();
    }

    private void ResetButton_Click(object? sender, RoutedEventArgs e)
    {
        CanvasData.InitMargin();
        CanvasData.UpdateCanvasSize();
        UpdateView();
        _mainWindow?.UpdateCanvas();
    }
}
