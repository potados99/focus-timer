using System;
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
    
    private uint ParseHex(char c)
    {
        return uint.Parse(c.ToString(), System.Globalization.NumberStyles.HexNumber);
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