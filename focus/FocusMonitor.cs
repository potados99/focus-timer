using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Automation;

namespace focus
{
    class FocusMonitor
    {

        public delegate void FocusedEventHandler(string processName);

        public event FocusedEventHandler? OnFocused;

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

            int processId = focusedElement.Current.ProcessId;
            using Process process = Process.GetProcessById(processId);

            OnFocused?.Invoke(process.ProcessName);
        }
    }
}
