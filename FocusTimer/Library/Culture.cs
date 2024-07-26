// Culture.cs
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

using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Windows;
using FocusTimer.Library.Extensions;

namespace FocusTimer.Library;

public static class Culture
{
    public static readonly CultureInfo[] SupportedCultures =
    {
        CultureInfo.GetCultureInfo("ko"),
        CultureInfo.GetCultureInfo("en"),
        CultureInfo.GetCultureInfo("fr"),
        CultureInfo.GetCultureInfo("de"),
        CultureInfo.GetCultureInfo("it"),
        CultureInfo.GetCultureInfo("es"),
        CultureInfo.GetCultureInfo("zh-CN"),
        CultureInfo.GetCultureInfo("zh-TW"),
        CultureInfo.GetCultureInfo("ja"),
        CultureInfo.GetCultureInfo("ru"),
        CultureInfo.GetCultureInfo("ro")
    };

    public static void Initialize()
    {
        var storedCultureOverride = Settings.GetCultureOverride();
        var nothingOverriden = string.IsNullOrEmpty(storedCultureOverride);
        if (nothingOverriden)
        {
            return;
        }

        Initialize(storedCultureOverride);
    }

    private static void Initialize(string name)
    {
        try
        {
            var culture = CultureInfo.CreateSpecificCulture(name);
            
            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;
        }
        catch (Exception e)
        {
            e.GetLogger().Error($"주어진 이름의 Culture로 override하지 못하였습니다. 아마 [{name}]이라는 이름의 Culture가 없나봅니다.", e);
        }
    }

    public static void OverrideCulture(CultureInfo culture)
    {
        Settings.SetCultureOverride(culture.Name);

        var usersAnswer = MessageBox.Show(
            Strings.Get("prompt_language_change_restart"),
            Strings.Get("restart"),
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);
        var restartNow = usersAnswer == MessageBoxResult.Yes;
        if (restartNow)
        {
            // https://stackoverflow.com/a/5266013/11929317
            System.Windows.Forms.Application.Restart();
            Application.Current.Shutdown();
        }
    }

    public static CultureInfo GetCurrentCultureExistingInstance()
    {
        // ComboBox에 현재 선택된 item이 제대로 표시되기 위해서는,
        // 현재 선택된 item이 목록에 포함된 것이어야 합니다.
        // 따라서 현재 Culture를 가져올 때에는 정확히 목록에 들어 있는 인스턴스를 가져옵니다.
        // 이를 구현하기 위해 현재 Culture와 언어 이름이 같은 Culture를 목록에서 꺼내 오도록 합니다.

        var currentCulture = Resources.strings.Culture ?? CultureInfo.CurrentCulture;
        var currentCultureLanguage = currentCulture.TwoLetterISOLanguageName;

        return SupportedCultures.First(c => c.TwoLetterISOLanguageName == currentCultureLanguage);
    }
}