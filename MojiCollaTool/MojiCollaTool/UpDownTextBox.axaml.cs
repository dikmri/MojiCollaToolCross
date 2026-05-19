using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using System;

namespace MojiCollaTool;

public partial class UpDownTextBox : UserControl
{
    public int Value { get; set; } = 0;
    public bool RunEvent { get; set; } = true;
    public int ValueMinLimit { get; set; } = 0;
    public int ValueMaxLimit { get; set; } = int.MaxValue;
    public int Step { get; set; } = 1;

    public event EventHandler<UpDownTextBoxEvent>? ValueChanged;

    public UpDownTextBox()
    {
        InitializeComponent();
        ValueTextBox.AddHandler(KeyDownEvent, ValueTextBox_KeyDown, RoutingStrategies.Tunnel);
    }

    public void SetValue(int value, bool runEvent = true)
    {
        Value = value;
        RunEvent = runEvent;
        ValueTextBox.Text = value.ToString();
        RunEvent = true;
    }

    public void RunDownButton() => DownButton_Click(this, new RoutedEventArgs());
    public void RunUpButton() => UpButton_Click(this, new RoutedEventArgs());

    private void DownButton_Click(object? sender, RoutedEventArgs e)
    {
        Value -= Step;
        LimitValueMinimum();
        ValueTextBox.Text = Value.ToString();
    }

    private void UpButton_Click(object? sender, RoutedEventArgs e)
    {
        Value += Step;
        LimitValueMaximum();
        ValueTextBox.Text = Value.ToString();
    }

    private void ValueTextBox_TextChanged(object? sender, TextChangedEventArgs e)
    {
        if (!RunEvent) return;
        if (int.TryParse(ValueTextBox.Text, out int tempValue))
        {
            Value = tempValue;
            LimitValueMinimum();
            LimitValueMaximum();
            ValueChanged?.Invoke(this, new UpDownTextBoxEvent(Value));
        }
    }

    private void ValueTextBox_KeyDown(object? sender, KeyEventArgs e)
    {
        if (!RunEvent) return;
        if (e.Key == Key.Up) UpButton_Click(this, new RoutedEventArgs());
        else if (e.Key == Key.Down) DownButton_Click(this, new RoutedEventArgs());
    }

    private void Common_PointerWheelChanged(object? sender, PointerWheelEventArgs e)
    {
        if (e.Delta.Y > 0) UpButton_Click(this, new RoutedEventArgs());
        if (e.Delta.Y < 0) DownButton_Click(this, new RoutedEventArgs());
        e.Handled = true;
    }

    private void LimitValueMinimum() { if (Value < ValueMinLimit) Value = ValueMinLimit; }
    private void LimitValueMaximum() { if (Value > ValueMaxLimit) Value = ValueMaxLimit; }
}

public class UpDownTextBoxEvent : EventArgs
{
    public int Value { get; set; }
    public UpDownTextBoxEvent(int value) { Value = value; }
}
