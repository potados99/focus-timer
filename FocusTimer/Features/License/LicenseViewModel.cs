using System.Windows;
using FocusTimer.Domain.Services;
using FocusTimer.Lib;
using FocusTimer.Lib.Component;

namespace FocusTimer.Features.License;

public class LicenseViewModel : BaseViewModel
{
    private readonly LicenseService _licenseService;
    
    public LicenseViewModel(LicenseService licenseService) => _licenseService = licenseService;
    
    public event Signal? OnLicenseAccepted;
    
    public string LicenseKeyInput { get; set; } = "";
    
    public override void OnInitialize()
    {
        if (_licenseService.HasValidLicenseKey())
        {
            OnLicenseAccepted?.Invoke();
        }
    }

    public void SubmitLicense()
    {
        if (_licenseService.ValidateLicenseKey(LicenseKeyInput) == false)
        {
            MessageBox.Show("Please check your input.", "Invalid License Key");
            return;
        }
        
        _licenseService.RegisterLicenseKey(LicenseKeyInput);
        OnLicenseAccepted?.Invoke();
    }
}