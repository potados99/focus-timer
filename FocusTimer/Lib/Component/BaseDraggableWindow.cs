using System;
using System.Windows;
using System.Windows.Input;
using FocusTimer.Features.Timer;

namespace FocusTimer.Lib.Component;

public abstract class BaseDraggableWindow<T> : BaseWindow<T> where T : BaseViewModel
{
    #region 드래그 오버라이드

    Point prepoint;
    bool isDragInProgress;
    bool isMouseCaptured;
    bool isDraggedFar;

    protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        isDragInProgress = true;
        prepoint = Mouse.GetPosition(this);

        CaptureMouse();
        isMouseCaptured = true;

        base.OnMouseLeftButtonDown(e);
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        if (!isDragInProgress || !isMouseCaptured)
        {
            return;
        }

        Point newmouse = Mouse.GetPosition(this);
        double top = (double) newmouse.Y - (double) prepoint.Y;
        double left = (double) newmouse.X - (double) prepoint.X;

        if (Math.Abs(top) > 0 || Math.Abs(left) > 0)
        {
            isDraggedFar = true;
        }

        SetValue(MainWindow.TopProperty, Top + top);
        SetValue(MainWindow.LeftProperty, Left + left);

        base.OnMouseMove(e);
    }

    protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
    {
        bool wasDraggedFar = isDraggedFar;

        isDragInProgress = false;
        isDraggedFar = false;
        ReleaseMouseCapture();
        isMouseCaptured = false;

        if (!wasDraggedFar)
        {
            base.OnMouseLeftButtonUp(e);

            OnClick();
        }
    }
    
    protected virtual void OnClick() {}

    #endregion
}