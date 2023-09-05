// BaseDraggableWindow.cs
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
using System.Windows;
using System.Windows.Input;

namespace FocusTimer.Library.Control.Base;

/// <summary>
/// 드래그와 클릭을 둘 다 지원하는 윈도우 컨트롤입니다.
/// </summary>
/// <typeparam name="T"></typeparam>
public abstract class BaseDraggableWindow<T> : BaseWindow<T> where T : BaseViewModel
{
    private Point _prevPoint;
    private bool _isDragInProgress;
    private bool _isMouseCaptured;
    private bool _isDraggedFar;

    protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        _isDragInProgress = true;
        _prevPoint = Mouse.GetPosition(this);

        CaptureMouse();
        _isMouseCaptured = true;

        base.OnMouseLeftButtonDown(e);
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        if (!_isDragInProgress || !_isMouseCaptured)
        {
            return;
        }

        var newMouse = Mouse.GetPosition(this);
        var top = newMouse.Y - _prevPoint.Y;
        var left = newMouse.X - _prevPoint.X;

        if (Math.Abs(top) > 0 || Math.Abs(left) > 0)
        {
            _isDraggedFar = true;
        }

        SetValue(TopProperty, Top + top);
        SetValue(LeftProperty, Left + left);

        base.OnMouseMove(e);
    }

    protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
    {
        var wasDraggedFar = _isDraggedFar;

        _isDragInProgress = false;
        _isDraggedFar = false;
        ReleaseMouseCapture();
        _isMouseCaptured = false;

        if (!wasDraggedFar)
        {
            base.OnMouseLeftButtonUp(e);

            OnClick();
        }
    }
    
    /// <summary>
    /// 윈도우의 클릭 이벤트는 이 메소드로 전달됩니다.
    /// </summary>
    protected virtual void OnClick() {}
}