using FocusTimer.Domain.Services;
using FocusTimer.Lib.Component;

namespace FocusTimer.Features.License;

public class LicenseViewModel : BaseModel
{
    private readonly LicenseService _licenseService;

    public LicenseViewModel(LicenseService licenseService) => _licenseService = licenseService;

    public bool HasLicense()
    {
        return _licenseService.HasValidLicenseKey();
    }
}