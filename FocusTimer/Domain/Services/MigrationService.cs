// MigrationService.cs
// 이 파일은 FocusTimer의 일부입니다.
// 
// © 2023 Potados <song@potados.com>
// 
// FocusTimer은(는) 자유 소프트웨어입니다.
// GNU General Public License v3.0을 준수하는 범위 내에서
// 자유롭게 변경, 수정하거나 배포할 수 있습니다.
// 
// 이 프로그램을 공유함으로서 다른 누군가에게 도움이 되기를 바랍니다.
// 다만 프로그램 배포와 함께 아무 것도 보증하지 않습니다. 자세한 내용은
// GNU General Public License를 참고하세요.
// 
// 라이센스 전문은 이 프로그램과 함께 제공되었을 것입니다. 만약 아니라면,
// 다음 링크에서 받아볼 수 있습니다: <https://www.gnu.org/licenses/gpl-3.0.txt>

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