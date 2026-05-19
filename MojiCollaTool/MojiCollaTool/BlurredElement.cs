using Avalonia.Controls;
using Avalonia.Media;
using System;

namespace MojiCollaTool;

public class BlurredElement : Control
{
    private readonly Action<DrawingContext> _action;

    public BlurredElement(Action<DrawingContext> action, double radius)
    {
        _action = action;
        if (radius > 0)
            Effect = new BlurEffect { Radius = radius };
    }

    public override void Render(DrawingContext drawingContext)
    {
        _action(drawingContext);
    }
}
