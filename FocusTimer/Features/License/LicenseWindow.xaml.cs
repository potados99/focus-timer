using System.Windows;
using FocusTimer.Features.Timer;
using FocusTimer.Lib.Component;

namespace FocusTimer.Features.License;

public partial class LicenseWindow : LicenseViewModelWindow
{
    protected override void OnInitialize()
    {
        InitializeComponent();

        // stop execution and open Main when valid license exists
        ViewModel.OnLicenseAccepted += () =>
        {
            new MainWindow().Show();
            Close();
        };
    }

    private void SubmitButton_OnClick(object sender, RoutedEventArgs e)
    {
        ViewModel.SubmitLicense();
    }
}

public abstract class LicenseViewModelWindow : BaseWindow<LicenseViewModel>
{
}