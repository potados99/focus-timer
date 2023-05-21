using focus.models;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace focus
{
    /// <summary>
    /// Interaction logic for TimerSlot.xaml
    /// </summary>
    public partial class TimerSlot : UserControl
    {
        public TimerSlot()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine(this.DataContext);
        }

        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            (this.DataContext as TimerSlotViewModel)?.FireAppClearEvent();
        }

        private void Register_Click(object sender, RoutedEventArgs e)
        {
            (this.DataContext as TimerSlotViewModel)?.FireAppRegisterEvent();
        }
    }
}
