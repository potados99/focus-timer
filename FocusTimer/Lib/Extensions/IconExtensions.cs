using System;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.AppCenter.Crashes;
using SkiaSharp;
using DColor = System.Drawing.Color;
using MColor = System.Windows.Media.Color;

namespace FocusTimer.Lib.Extensions;

public static class IconExtensions
{
    public static byte[] ToBytes(this Icon icon)
    {
        // Icon을 직접 Save하면 품질 저하가 생깁니다.
        // 따라서 비트맵으로 다룹니다.
        // 자세한 정보: https://stackoverflow.com/a/52689799
        var bitmap = icon.ToBitmap();

        using var ms = new MemoryStream();
        bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
        
        return ms.ToArray();
    }
    
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