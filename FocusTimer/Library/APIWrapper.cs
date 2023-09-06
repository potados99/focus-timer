// APIWrapper.cs
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

using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using FocusTimer.Library.Extensions;
using Microsoft.AppCenter.Crashes;

namespace FocusTimer.Library;

/// <summary>
/// 저수준의 API 호출을 감싸는 wrapper입니다.
/// </summary>
// ReSharper disable once InconsistentNaming
public static class APIWrapper
{
    private static readonly log4net.ILog s_logger = log4net.LogManager.GetLogger(typeof(APIWrapper));

    #region 창

    public static string GetClassName(IntPtr windowHandle)
    {
        StringBuilder builder = new StringBuilder(256);

        API.GetClassName(windowHandle, builder, builder.Capacity);

        return builder.ToString();
    }

    public static IntPtr GetForegroundWindow()
    {
        return API.GetForegroundWindow();
    }

    public static string GetForegroundWindowClass()
    {
        return GetClassName(GetForegroundWindow());
    }

    public static void SetForegroundWindow(IntPtr windowHandle)
    {
        // ALT 키를 눌러주지 않으면, 창이 전면으로 오지는 않고
        // 작업 표시줄에서 반짝이기만 합니다.
        // 이 증상은 Visual Studio와 attach되었을 때에는
        // 나타나지 않습니다.
        // 해결책: https://stackoverflow.com/questions/10740346/setforegroundwindow-only-working-while-visual-studio-is-open

        // ALT를 눌러 줍니다.
        API.keybd_event((byte) API.ALT, 0x45, API.EXTENDEDKEY | 0, 0);

        // 누른 ALT를 다시 뗍니다.
        API.keybd_event((byte) API.ALT, 0x45, API.EXTENDEDKEY | API.KEYUP, 0);

        API.SetForegroundWindow(windowHandle);
    }

    public static void MinimizeWindow(IntPtr windowHandle)
    {
        API.ShowWindow(windowHandle, API.SW_MINIMIZE);
    }

    #endregion

    #region 입력

    public static long GetElapsedFromLastInput()
    {
        API.LASTINPUTINFO lastInputInfo = new API.LASTINPUTINFO();
        lastInputInfo.cbSize = Marshal.SizeOf(lastInputInfo);
        lastInputInfo.dwTime = 0;

        API.GetLastInputInfo(out lastInputInfo);

        int lastInputTick = lastInputInfo.dwTime;

        return Environment.TickCount - lastInputTick;
    }

    #endregion

    #region 프로세스

    public static Process? GetProcessByWindowHandle(IntPtr windowHandle)
    {
        var result = API.GetWindowThreadProcessId(windowHandle, out var processId);
        if (result == 0)
        {
            // https://learn.microsoft.com/ko-kr/windows/win32/api/winuser/nf-winuser-getwindowthreadprocessid#return-value
            s_logger.Error($"주어진 Window Handle({windowHandle})이 잘못되었습니다.");
            return null;
        }

        try
        {
            return Process.GetProcessById((int) processId);
        }
        catch (Exception e)
        {
            s_logger.Error($"주어진 프로세스 ID({processId})에 해당하는 프로세스를 찾을 수 없습니다: {e}");
            return null;
        }
    }

    public static Process? GetForegroundProcess()
    {
        return GetProcessByWindowHandle(GetForegroundWindow());
    }

    public static bool IsThisProcessForeground()
    {
        return GetForegroundProcess()?.Id == Environment.ProcessId;
    }

    public static string GetProcessFilename(Process p)
    {
        var capacity = 2000;
        StringBuilder builder = new(capacity);

        // Process.MainModule은 OpenProcess 호출에
        // access 인자로 0x0410을 넘깁니다.
        // 이는 PROCESS_QUERY_INFORMATION 과 PROCESS_VM_READ 이 합쳐진 것인데, (https://learn.microsoft.com/en-us/windows/win32/procthread/process-security-and-access-rights)
        // 대상 프로세스가 상승된 권한으로 실행되고 있을 때에 문제를 일으킵니다. (https://stackoverflow.com/questions/9501771/how-to-avoid-a-win32-exception-when-accessing-process-mainmodule-filename-in-c#comment96065027_34991822)
        // 따라서 PROCESS_QUERY_LIMITED_INFORMATION access를 사용합니다.
        var processPtr = API.OpenProcess(API.ProcessAccessFlags.QueryLimitedInformation, false, p.Id);
        var gotName = API.QueryFullProcessImageName(processPtr, 0, builder, ref capacity);
        
        return gotName ? builder.ToString() : string.Empty;
    }

    #endregion

    #region 아이콘

    /// <summary>
    /// 기존의 <see cref="Icon.ExtractAssociatedIcon(string)"/> 메소드는
    /// 인자가 UNC이면 실행을 거부하는 문제가 있습니다.
    /// 이 메소드는 그 문제를 해결합니다.
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    public static Icon ExtractAssociatedIcon(string filePath)
    {
        if (!File.Exists(filePath))
        {
            s_logger.Warn($"Executable not found: {filePath}. Use C:\\Windows\\System32\\cmd.exe for fallback.");
            return ExtractAssociatedIcon("C:\\Windows\\System32\\cmd.exe");
        }

        StringBuilder strB = new StringBuilder(260); // Allocate MAX_PATH chars
        strB.Append(filePath);
        IntPtr handle = API.ExtractAssociatedIcon(IntPtr.Zero, strB, out var uicon);
        Icon ico = Icon.FromHandle(handle);

        return ico;
    }

    #endregion
}