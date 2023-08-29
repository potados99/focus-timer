using System;

namespace FocusTimer;

public partial class App
{
    [STAThread]
    public static void Main()
    {
        var application = new App();
        application.InitializeComponent();
        application.Run();
    }
}