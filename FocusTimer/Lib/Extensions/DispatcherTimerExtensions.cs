using System;
using System.Reflection;
using System.Windows.Threading;

namespace FocusTimer.Lib.Extensions;

public static class DispatcherTimerExtensions
{
    public static void RemoveHandlers(this DispatcherTimer dispatchTimer)
    {
        var eventField = dispatchTimer.GetType().GetField("Tick", BindingFlags.NonPublic | BindingFlags.Instance);
        if (eventField == null)
        {
            return;
        }

        if (eventField.GetValue(dispatchTimer) is not Delegate eventDelegate)
        {
            return;
        }

        var invocationList = eventDelegate.GetInvocationList();

        foreach (var handler in invocationList)
            dispatchTimer.Tick -= (EventHandler) handler;
    }
}