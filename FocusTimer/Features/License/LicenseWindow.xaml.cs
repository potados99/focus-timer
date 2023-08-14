using System.Windows;
using FocusTimer.Features.Timer;
using Microsoft.Extensions.DependencyInjection;

namespace FocusTimer.Features.License;

public partial class LicenseWindow : Window
{
    private readonly LicenseViewModel _viewModel = App.Provider.GetService<LicenseViewModel>()!;

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