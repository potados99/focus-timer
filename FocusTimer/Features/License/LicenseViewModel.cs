using FocusTimer.Lib.Component;

namespace FocusTimer.Features.License;

public class LicenseViewModel : BaseModel
{
    private readonly LicenseService _licenseService = new();

    public bool HasLicense()
    {
        return _licenseService.HasValidLicenseKey();
    }
}