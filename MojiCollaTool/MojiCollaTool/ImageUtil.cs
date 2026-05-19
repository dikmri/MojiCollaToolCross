using Avalonia.Media.Imaging;

namespace MojiCollaTool;

public static class ImageUtil
{
    public static Bitmap LoadImageSource2(string filePath)
    {
        return new Bitmap(filePath);
    }
}
