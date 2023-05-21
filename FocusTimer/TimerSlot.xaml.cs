using FocusTimer.Models;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace FocusTimer
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

        private TimerSlotViewModel? ViewModel;

        #region 이벤트 핸들러

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            // 상위 윈도우에서 바인딩으로 넘겨준 데이터 컨텍스트를
            // 뷰모델로 활용합니다.
            ViewModel = (this.DataContext as TimerSlotViewModel);
        }

        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            ViewModel?.FireAppClearEvent();
        }

        private void Register_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine(ViewModel);
            ViewModel?.FireAppRegisterEvent();
        }

        #endregion
    }
}
