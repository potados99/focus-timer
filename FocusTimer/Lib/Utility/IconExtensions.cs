using SkiaSharp;
using System.Drawing;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace FocusTimer.Lib.Utility
{
    public static class IconExtensions
    {
        public static ImageSource ToImageSource(this Icon icon)
        {
            ImageSource imageSource = Imaging.CreateBitmapSourceFromHIcon(
                icon.Handle,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());

            return imageSource;
        }

        public static System.Windows.Media.Color ToColor(this Icon icon)
        {
            var bitmap = icon.ToBitmap();
            var p = bitmap.GetPixel(bitmap.Size.Width / 2, bitmap.Size.Height / 2);

            return System.Windows.Media.Color.FromRgb(p.R, p.G, p.B);
        }

        public static SKColor ToSKColor(this Icon icon)
        {
            var bitmap = icon.ToBitmap();

            return new SKColor(0, 0, 0);
        }
    }
}
