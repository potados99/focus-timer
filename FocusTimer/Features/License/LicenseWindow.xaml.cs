using System.Windows;
using FocusTimer.Features.Timer;

namespace FocusTimer.Features.License;

public partial class LicenseWindow : Window
{
    private readonly LicenseViewModel _viewModel = new();

    public LicenseWindow()
    {
        InitializeComponent();

        if (_viewModel.HasLicense())
        {
            new MainWindow().Show();
            Close();
        }
        
        
        
        
    }
}