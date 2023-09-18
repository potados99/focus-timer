// Settings.cs
// 이 파일은 FocusTimer의 일부입니다.
// 
// © 2023 Potados <song@potados.com>
// 
// FocusTimer은(는) 자유 소프트웨어입니다.
// GNU General Public License v3.0을 준수하는 범위 내에서
// 누구든지 자유롭게 변경, 수정하거나 배포할 수 있습니다.
// 
// 이 프로그램을 공유함으로서 다른 누군가에게 도움이 되기를 바랍니다.
// 다만 프로그램 배포와 함께 아무 것도 보증하지 않습니다. 자세한 내용은
// GNU General Public License를 참고하세요.
// 
// 라이센스 전문은 이 프로그램과 함께 제공되었을 것입니다. 만약 아니라면,
// 다음 링크에서 받아볼 수 있습니다: <https://www.gnu.org/licenses/gpl-3.0.txt>

namespace FocusTimer.Library;

/// <summary>
/// 애플리케이션의 설정에 접근할 수 있는 인터페이스를 제공합니다.
/// </summary>
public static class Settings
{
    public static void UpgradeIfNeeded()
    {
        if (Properties.Settings.Default.UpgradeRequired)
        {
            Properties.Settings.Default.Upgrade();
            Properties.Settings.Default.UpgradeRequired = false;
            Properties.Settings.Default.Save();
        }
    }

    public static int GetFocusLockHoldDuration()
    {
        var got = Properties.Settings.Default.FocusLockHoldDuration;
        const int fallback = 10;

        return got <= 0 ? fallback : got;
    }

    public static void SetFocusLockHoldDuration(int duration)
    {
        Properties.Settings.Default.FocusLockHoldDuration = duration;

        Properties.Settings.Default.Save();
    }

    public static int GetActivityTimeout()
    {
        var got = Properties.Settings.Default.ActivityTimeout;
        const int fallback = 10;

        return got <= 0 ? fallback : got;
    }

    public static void SetActivityTimeout(int timeout)
    {
        Properties.Settings.Default.ActivityTimeout = timeout;

        Properties.Settings.Default.Save();
    }

    public static bool GetShowConcentration()
    {
        return Properties.Settings.Default.ShowConcentration;
    }

    public static void SetShowConcentration(bool show)
    {
        Properties.Settings.Default.ShowConcentration = show;

        Properties.Settings.Default.Save();
    }

    public static string GetCultureOverride()
    {
        return Properties.Settings.Default.CultureOverride;
    }

    public static void SetCultureOverride(string culture)
    {
        Properties.Settings.Default.CultureOverride = culture;

        Properties.Settings.Default.Save();
    }
}