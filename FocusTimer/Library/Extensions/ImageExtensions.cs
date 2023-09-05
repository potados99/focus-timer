// ImageExtensions.cs
// 이 파일은 FocusTimer의 일부입니다.
// 
// © 2023 Potados <song@potados.com>
// 
// FocusTimer은(는) 자유 소프트웨어입니다.
// GNU General Public License v3.0을 준수하는 범위 내에서
// 누구든지 자유롭게 변경, 수정하거나 배포할 수 있습니다.
// 
// 이 프로그램을 공유함으로서 다른 누군가에게 도움이 되기를 바랍니다.
// 다만 프로그램 배포와 함께 아무 것도 보증하지 않습니다. 자세한 내용은
// GNU General Public License를 참고하세요.
// 
// 라이센스 전문은 이 프로그램과 함께 제공되었을 것입니다. 만약 아니라면,
// 다음 링크에서 받아볼 수 있습니다: <https://www.gnu.org/licenses/gpl-3.0.txt>

using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace FocusTimer.Library.Extensions;

public static class ImageExtensions
{
    public static Icon ToIcon(this Image img, int size)
    {
        // https://stackoverflow.com/a/57254324
        
        using var msImg = new MemoryStream();
        using var msIco = new MemoryStream();
        img.Save(msImg, ImageFormat.Png);

        using var bw = new BinaryWriter(msIco);
        bw.Write((short)0);           //0-1 reserved
        bw.Write((short)1);           //2-3 image type, 1 = icon, 2 = cursor
        bw.Write((short)1);           //4-5 number of images
        bw.Write((byte)size);         //6 image width
        bw.Write((byte)size);         //7 image height
        bw.Write((byte)0);            //8 number of colors
        bw.Write((byte)0);            //9 reserved
        bw.Write((short)0);           //10-11 color planes
        bw.Write((short)32);          //12-13 bits per pixel
        bw.Write((int)msImg.Length);  //14-17 size of image data
        bw.Write(22);                 //18-21 offset of image data
        bw.Write(msImg.ToArray());    // write image data
        bw.Flush();
        bw.Seek(0, SeekOrigin.Begin);
        return new Icon(msIco);
    }
}