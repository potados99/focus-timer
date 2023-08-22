namespace FocusTimer.Features.Timer;

public partial class MainViewModel
{
    #region 라이센스

    public bool ShouldAskForLicense()
    {
        return _licenseService.HasValidLicenseKey() == false;
    }

    #endregion
}