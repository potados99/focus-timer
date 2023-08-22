using System.Drawing;
using System.IO;

namespace FocusTimer.Lib.Utility;

public static class BytesExtensions
{
    public static Icon ToIcon(this byte[] bytes)
    {
        using var ms = new MemoryStream(bytes);
        return new Icon(ms);
    }
}