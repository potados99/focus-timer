using System;
using System.Windows;
using System.Windows.Input;

namespace FocusTimer.Lib.Control.Base;

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