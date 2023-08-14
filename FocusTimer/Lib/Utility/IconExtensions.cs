using Microsoft.AppCenter.Crashes;
using SkiaSharp;
using System;
using System.Drawing;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using DColor = System.Drawing.Color;
using MColor = System.Windows.Media.Color;

namespace FocusTimer.Lib.Utility;

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

    public static MColor ToColor(this Icon icon)
    {
        try
        {
            var bitmap = icon.ToBitmap();
            var x = bitmap.Size.Width / 2;
            var y = bitmap.Size.Height / 2;

            DColor p = bitmap.GetPixel(x, y);

            while (p.A != 255)
            {
                p = bitmap.GetPixel(++x, y);
            }

            return MColor.FromRgb(p.R, p.G, p.B);
        } catch (Exception e) {
            Crashes.TrackError(e);
            return MColor.FromRgb(50, 50, 50);
        }
    }

    public static SKColor ToSKColor(this Icon icon)
    {
        var color = icon.ToColor();

        return new SKColor(color.R, color.G, color.B);
    }
}