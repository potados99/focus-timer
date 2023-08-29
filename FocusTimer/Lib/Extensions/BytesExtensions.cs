using System.Drawing;
using System.IO;

namespace FocusTimer.Lib.Extensions;

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