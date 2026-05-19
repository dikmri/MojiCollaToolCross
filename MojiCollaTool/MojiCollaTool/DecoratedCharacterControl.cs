using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using System.Globalization;

namespace MojiCollaTool;

// 縦書き対応のために90°右回転させる必要のある文字（主にかっこなど）
file static class TategakiChars
{
    public const string Rotate90 = " 　‥…:：;；-=＝≒ー～─━|（）()｟｠⦅⦆❨❩❪❫⸨⸩⦕⦖⦇⦈⦓⦔﴾﴿⸦⸧⎛⎞⎜⎟⎝⎠╭╮┃┃╰╯⁽⁾₍₎︶⁐「」『』⌜⌟⌞⌝﹂［﹄［〈〉⟨⟩《》⟪⟫‹›«»❮❯❬❭❰❱⦉⦊⦑⦒⦓⦔⦖⦕⧼⧽﹀︾｛｝{}❴❵⦃⦄⎧⎫⎨⎬⎩⎭︸［］[]〚〛⟦⟧⦋⦌⦍⦎⦏⦐⁅⁆⎡⎤⎢⎥⎣⎦⸢⸣⸠⸡⸤⸥﹈⎵【】〖〗︼︘〔〕❲❳〘〙⟬⟭⦗⦘︺''''\"\"\"\"❛❜❝❞‚‚„„〝　＜＞<>≪≫≦≧≤≥⩽⩾≲≳⪍⪎⪅⪆⋜⋝⪙⪚≶≷⋚⋛⪋⪌";
    public const string ShiftRightUp = "｡。､、.．,，";
    public const string Small = "ぁぃぅぇぉっゃゅょゎゕゖァィゥェォヵㇰヶㇱㇲッㇳㇴㇵㇶㇷㇷ゚ㇸㇹㇺャュョㇻㇼㇽㇾㇿヮ";
}

/// <summary>
/// 1文字分の装飾レンダリングを行うコントロール。
/// Canvas を継承し、縁取り・ぼかし・文字塗りのレイヤーを重ねて描画する。
/// </summary>
public class DecoratedCharacterControl : Canvas
{
    public char Character { get; }

    public DecoratedCharacterControl(char character, MojiData mojiData)
    {
        Character = character;

        var typeface = new Typeface(
            new FontFamily(mojiData.FontFamilyName),
            mojiData.IsItalic ? FontStyle.Italic : FontStyle.Normal,
            mojiData.IsBold ? FontWeight.Bold : FontWeight.Regular
        );

        var ft = new FormattedText(
            character.ToString(),
            CultureInfo.GetCultureInfo("ja-JP"),
            FlowDirection.LeftToRight,
            typeface,
            mojiData.FontSize,
            Brushes.White
        );

        double charWidth = ft.WidthIncludingTrailingWhitespace;
        double charHeight = ft.Height;
        Width = charWidth;
        Height = charHeight;

        var geometry = ft.BuildGeometry(new Point(0, 0));

        // 縁取り2（後面・最下層）
        if (mojiData.IsSecondBorderExists)
        {
            var pen = new Pen(new SolidColorBrush(mojiData.SecondBorderColor),
                mojiData.SecondBorderThickness, lineJoin: PenLineJoin.Round);
            Children.Add(CreateLayer(geometry, null, pen, charWidth, charHeight, mojiData.SecondBorderBlurrRadius));
        }

        // 縁取り1（前面）
        if (mojiData.IsBorderExists)
        {
            var pen = new Pen(new SolidColorBrush(mojiData.BorderColor),
                mojiData.BorderThickness, lineJoin: PenLineJoin.Round);
            Children.Add(CreateLayer(geometry, null, pen, charWidth, charHeight, mojiData.BorderBlurrRadius));
        }

        // 文字塗り（最上層）
        Children.Add(CreateLayer(geometry, new SolidColorBrush(mojiData.ForeColor), null, charWidth, charHeight, 0));

        // 縦書き/横書きの配置設定
        switch (mojiData.TextDirection)
        {
            case TextDirection.Yokogaki:
                VerticalAlignment = VerticalAlignment.Bottom;
                HorizontalAlignment = HorizontalAlignment.Center;
                Margin = new Thickness(0, 0, mojiData.CharacterMargin, 0);
                break;

            case TextDirection.Tategaki:
                VerticalAlignment = VerticalAlignment.Center;
                HorizontalAlignment = HorizontalAlignment.Center;
                Margin = new Thickness(0, 0, 0, mojiData.CharacterMargin);

                if (TategakiChars.ShiftRightUp.Contains(character))
                    RenderTransform = new TranslateTransform(charWidth / 2, -charHeight / 2);
                else if (TategakiChars.Small.Contains(character))
                    RenderTransform = new TranslateTransform(charWidth / 8, -charHeight / 14);
                else if (TategakiChars.Rotate90.Contains(character))
                {
                    RenderTransformOrigin = RelativePoint.Center;
                    RenderTransform = new RotateTransform(90);
                }
                break;
        }
    }

    private static Control CreateLayer(Geometry? geometry, IBrush? brush, IPen? pen,
        double width, double height, double blurRadius)
    {
        var layer = new CharacterLayerControl(geometry, brush, pen, width, height);
        if (blurRadius > 0)
            layer.Effect = new BlurEffect { Radius = blurRadius };
        return layer;
    }
}

/// <summary>
/// 文字ジオメトリを1レイヤー分描画するコントロール
/// </summary>
file sealed class CharacterLayerControl : Control
{
    private readonly Geometry? _geometry;
    private readonly IBrush? _brush;
    private readonly IPen? _pen;

    public CharacterLayerControl(Geometry? geometry, IBrush? brush, IPen? pen, double width, double height)
    {
        _geometry = geometry;
        _brush = brush;
        _pen = pen;
        Width = width;
        Height = height;
    }

    public override void Render(DrawingContext context)
    {
        if (_geometry != null)
            context.DrawGeometry(_brush, _pen, _geometry);
    }
}
