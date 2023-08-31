using System.Windows;
using FocusTimer.Domain.Services;
using FocusTimer.Library;
using FocusTimer.Library.Control.Base;
using FocusTimer.Library.Extensions;

namespace FocusTimer.Features.License;

public class LicenseViewModel : BaseViewModel
{
    private readonly LicenseService _licenseService;
    private readonly MigrationService _migrationService;

    public LicenseViewModel(LicenseService licenseService, MigrationService migrationService)
    {
        _licenseService = licenseService;
        _migrationService = migrationService;
    }

    public event Signal? OnLicenseAccepted;

    public string LicenseKeyInput { get; set; } = "";

    public override void OnLoaded() // Initialized 시점이 아닌 Loaded 시점에서 실행해야 Window가 show된 다음에 아래 코드가 실행됩니다.
    {
        if (_licenseService.HasValidLicenseKey())
        {
            this.GetLogger().Info("유효한 라이센스가 확인되었습니다. 다음으로 넘어갑니다.");

            OnLicenseAccepted?.Invoke();
        }
        else if (_migrationService.IsOldDistributionInstalled())
        {
            this.GetLogger().Info("라이센스는 없지만 기존 배포판이 설치되어 있습니다. 라이센스를 자동으로 등록합니다.");

            _migrationService.RemoveOldDistribution();
            LicenseKeyInput = _migrationService.BringNewLicense();
            SubmitLicense();
        }
        else
        {
            this.GetLogger().Info("라이센스는 없고 기존 배포판도 없습니다. 사용자에게 라이센스 등록을 요청합니다.");
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