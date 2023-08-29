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
            MessageBox.Show("라이센스 키가 올바르지 않습니다. 입력하신 라이센스 키를 확인해 주세요.", "올바르지 않은 라이센스 키");
            return;
        }
        
        _licenseService.RegisterLicenseKey(LicenseKeyInput);
        OnLicenseAccepted?.Invoke();
    }
}