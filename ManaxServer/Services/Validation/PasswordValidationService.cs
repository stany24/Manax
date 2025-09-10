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
}
