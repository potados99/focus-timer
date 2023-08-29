using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Media.Imaging;

namespace FocusTimer.Lib.Utility;

public static class BytesExtensions
{
    public static Icon ToIcon(this byte[] bytes)
    {
        // 비트맵 이미지를 불러온 다음에
        using var image = Image.FromStream(new MemoryStream(bytes)); 
        
        // 아이콘으로 변환합니다.
        return image.ToIcon(32); 
    }
}