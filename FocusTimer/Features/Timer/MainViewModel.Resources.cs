using System.Windows;
using System.Windows.Media.Animation;

namespace FocusTimer.Features.Timer;

public partial class MainViewModel
{
    private void StartAnimation(string name)
    {
        var sb = Application.Current.MainWindow?.Resources[name] as Storyboard;
        sb?.Begin();
    }
}