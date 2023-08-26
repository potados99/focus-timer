namespace FocusTimer.Features.Timer;

public partial class MainViewModel
{
    public bool ShouldAskForLicense()
    {
        return _licenseService.HasValidLicenseKey() == false;
    }
}