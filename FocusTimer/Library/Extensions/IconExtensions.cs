// IconExtensions.cs
// 이 파일은 FocusTimer의 일부입니다.
// 
// © 2023 Potados <song@potados.com>
// 
// FocusTimer은(는) 자유 소프트웨어입니다.
// GNU General Public License v3.0을 준수하는 범위 내에서
// 자유롭게 변경, 수정하거나 배포할 수 있습니다.
// 
// 이 프로그램을 공유함으로서 다른 누군가에게 도움이 되기를 바랍니다.
// 다만 프로그램 배포와 함께 아무 것도 보증하지 않습니다. 자세한 내용은
// GNU General Public License를 참고하세요.
// 
// 라이센스 전문은 이 프로그램과 함께 제공되었을 것입니다. 만약 아니라면,
// 다음 링크에서 받아볼 수 있습니다: <https://www.gnu.org/licenses/gpl-3.0.txt>

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

namespace FocusTimer.Library.Extensions;

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