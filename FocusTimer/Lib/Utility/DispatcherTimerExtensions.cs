using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace FocusTimer.Lib.Utility;

public static class DispatcherTimerExtensions
{
    public static void RemoveHandlers(this DispatcherTimer dispatchTimer)
    {
        var eventField = dispatchTimer.GetType().GetField("Tick",
            BindingFlags.NonPublic | BindingFlags.Instance);
        var eventDelegate = (Delegate)eventField.GetValue(dispatchTimer);

        if (eventDelegate == null)
        {
            return;
        }

        var invocatationList = eventDelegate.GetInvocationList();

        foreach (var handler in invocatationList)
            dispatchTimer.Tick -= ((EventHandler)handler);
    }
}