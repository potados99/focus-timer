using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Interop;

namespace focus
{
    class FocusMonitor
    {
        public FocusMonitor() {
            FocusedWindow = new WindowInteropHelper(Application.Current.MainWindow).Handle;
        }  

        public delegate void FocusedEventHandler(IntPtr prev, IntPtr current);

        public event FocusedEventHandler? OnFocused;

        public IntPtr FocusedWindow { get; private set; }

        public void StartListening()
        {
            Automation.AddAutomationFocusChangedEventHandler(OnFocusChange);
        }

        public void StopListening()
        {
            Automation.RemoveAutomationFocusChangedEventHandler(OnFocusChange);
        }

        /// <summary>
        /// Handle the event.
        /// </summary>
        /// <param name="sender">Object that raised the event.</param>
        /// <param name="e">Event arguments.</param>
        private void OnFocusChange(object sender, AutomationFocusChangedEventArgs e)
        {
            AutomationElement? focusedElement = sender as AutomationElement;

            if (focusedElement == null)
            {
                return;
            }

            IntPtr handle = (IntPtr)focusedElement.Current.NativeWindowHandle;

            OnFocused?.Invoke(FocusedWindow, handle);

            FocusedWindow = handle;
        }
    }
}
