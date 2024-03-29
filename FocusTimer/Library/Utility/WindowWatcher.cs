﻿// WindowWatcher.cs
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
using System.Linq;
using System.Windows.Threading;

namespace FocusTimer.Library.Utility;

/// <summary>
/// 포커스를 가지고 있는 창이 바뀌는 이벤트를 감지하는 모니터입니다.
/// 시스템 UI는 포커스를 얻어도 무시합니다. 아래 <see cref="SkipList"/> 참조.
/// </summary>
public class WindowWatcher
{
    public WindowWatcher()
    {
        StartListening();
    }
    
    public delegate void FocusedEventHandler(IntPtr prev, IntPtr current);

    public event FocusedEventHandler? OnFocused;

    private readonly DispatcherTimer _timer = new();

    public static readonly string[] SkipList = {
        "TaskListThumbnailWnd", // 윈도우가 여러 개일 때 작업표시줄에 표시되는 작은 썸네일
        "ForegroundStaging", // Alt + Tab으로 보이는 UI
        "MultitaskingViewFrame", // Alt + Tab으로 보이는 UI
        "Windows.UI.Core.CoreWindow", // 시작 버튼 누르면 보이는 UI
        "Shell_TrayWnd", // 작업표시줄
        "Shell_SecondaryTrayWnd", // 다른 모니터에 떠있는 보조 작업표시줄
        "ApplicationManager_DesktopShellWindow", // 새 창 띄울 때에 개입하는 UI
        "WorkerW", // 작업표시줄 오른쪽 눌러서 나오는 바탕화면 UI
        "NotifyIconOverflowWindow", // 작업표시줄에 아이콘이 몰려있는 곳 위로가는 쉐브론 누르면 나오는 UI
    };
    
    private IntPtr _focusedWindow = IntPtr.Zero;

    private void StartListening()
    {
        _timer.Tick += (_, _) => Tick();
        _timer.Interval = new TimeSpan(0, 0, 0, 0, 1);

        _timer.Start();
    }
    
    private void Tick()
    {
        var nowFocused = APIWrapper.GetForegroundWindow();

        if (nowFocused == IntPtr.Zero)
        {
            // Null 포인터인 핸들은 취급하지 않습니다.
            return;
        }

        if (_focusedWindow == nowFocused)
        {
            // 변화가 없으면 취급하지 않습니다.
            return;
        }

        if (SkipList.Contains(APIWrapper.GetForegroundWindowClass()))
        {
            // 특정 시스템 UI는 다루지 않습니다.
            return;
        }
        
        OnFocused?.Invoke(_focusedWindow, nowFocused);
        
        _focusedWindow = nowFocused;
    }
}