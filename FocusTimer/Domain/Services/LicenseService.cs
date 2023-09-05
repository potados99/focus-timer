// LicenseService.cs
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

using System;
using System.Globalization;
using System.Linq;
using Meziantou.Framework.Win32;

namespace FocusTimer.Domain.Services;

public class LicenseService
{
    public bool ValidateLicenseKey(string key)
    {
        var normalized = key.Replace("-", "");

        return ValidatePart(normalized);
    }

    private bool ValidatePart(string part)
    {
        var sum = 3 + part.Take(part.Length - 1).Sum(digit => (ParseHex(digit) ^ 3) + 1);

        return sum % 16 == ParseHex(part.Last());
    }

    private int ParseHex(char c)
    {
        var succeeded = int.TryParse(
            c.ToString(),
            System.Globalization.NumberStyles.HexNumber,
            NumberFormatInfo.CurrentInfo,
            out var result
        );

        if (!succeeded)
        {
            return -1;
        }

        return result;
    }

    public void RegisterLicenseKey(string key)
    {
        if (ValidateLicenseKey(key) == false)
        {
            throw new Exception($"Invalid license key: {key}");
        }

        CredentialManager.WriteCredential(
            "FocusTimer",
            "LicenseService",
            key,
            CredentialPersistence.LocalMachine
        );
    }

    public bool HasValidLicenseKey()
    {
        var credential = CredentialManager.ReadCredential("FocusTimer");
        if (credential == null)
        {
            return false;
        }

        return credential.UserName == "LicenseService" && ValidateLicenseKey(credential.Password ?? "");
    }
}