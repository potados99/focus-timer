using System.IO;
using FocusTimer.Library.Extensions;

namespace FocusTimer.Domain.Services;

public class MigrationService
{
    private const string OLD_DISTRIBUTION_UNINSTALLER = "C:\\Program Files\\World Moment\\Focus Timer\\unins000.exe";
    private const string NEW_LICENSE = "4E78-26D8-9526-5D9B-C465-E1F4"; // 아무거나

    public bool IsOldDistributionInstalled()
    {
        return File.Exists(OLD_DISTRIBUTION_UNINSTALLER);
    }

    public string BringNewLicense()
    {
        this.GetLogger().Info($"새로 이관할 라이센스 키를 가져옵니다: {NEW_LICENSE}");

        return NEW_LICENSE;
    }

    public void RemoveOldDistribution()
    {
        if (!IsOldDistributionInstalled())
        {
            return;
        }

        this.GetLogger().Info("기존 배포판의 uninstaller를 실행합니다.");

        System.Diagnostics.Process.Start(OLD_DISTRIBUTION_UNINSTALLER, "/silent");
    }
}