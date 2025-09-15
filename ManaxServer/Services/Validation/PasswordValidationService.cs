using System.Text.RegularExpressions;
using ManaxServer.Localization;

namespace ManaxServer.Services.Validation;

public partial class PasswordValidationService(bool isProduction) : IPasswordValidationService
{
    [GeneratedRegex("[a-z]")]
    private static partial Regex HasLowercase();
    
    [GeneratedRegex("[A-Z]")]
    private static partial Regex HasUppercase();
    
    [GeneratedRegex(@"[!@#$%^&*()_+\-=\[\]{};':""\\|,.<>\/?]")]
    private static partial Regex HasSpecialCharacter();
    
    [GeneratedRegex("[0-9]")]
    private static partial Regex HasDigit();

    public bool IsPasswordValid(string password, out string? errorMessage)
    {
        errorMessage = null;

        if (string.IsNullOrEmpty(password))
        {
            errorMessage = Localizer.PasswordEmpty();
            return false;
        }

        if (!isProduction)
        {
            return true;
        }

        if (password.Length < 14)
        {
            errorMessage = Localizer.PasswordTooShort();
            return false;
        }

        if (!HasLowercase().IsMatch(password))
        {
            errorMessage = Localizer.PasswordNoLowercase();
            return false;
        }

        if (!HasUppercase().IsMatch(password))
        {
            errorMessage = Localizer.PasswordNoUppercase();
            return false;
        }

        if (HasSpecialCharacter().IsMatch(password) || HasDigit().IsMatch(password)) return true;
        errorMessage = Localizer.PasswordNoSpecialCharacterOrDigit();
        return false;
    }

    public string GenerateValidPassword()
    {
        const string lower = "abcdefghijklmnopqrstuvwxyz";
        const string upper = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        const string digits = "0123456789";
        const string special = "!@#$%^&*()_+-=[]{};':\"\\|,.<>/?";
        const string all = lower + upper + digits + special;

        Random random = new();
        char[] passwordChars = new char[14];

        passwordChars[0] = lower[random.Next(lower.Length)];
        passwordChars[1] = upper[random.Next(upper.Length)];
        passwordChars[2] = digits[random.Next(digits.Length)];
        passwordChars[3] = special[random.Next(special.Length)];

        for (int i = 4; i < passwordChars.Length; i++)
        {
            passwordChars[i] = all[random.Next(all.Length)];
        }

        return new string(passwordChars.OrderBy(x => random.Next()).ToArray());
    }
}
