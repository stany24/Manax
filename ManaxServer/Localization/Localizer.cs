using ManaxServer.Localization.Languages;

namespace ManaxServer.Localization;

public static partial class Localizer
{
    private static Language _language = Language.English;
    private static Dictionary<LocalizationKey, string> _currentLocalization = null!;

    static Localizer()
    {
        LoadLanguage(_language);
    }

    private static void LoadLanguage(Language language)
    {
        _currentLocalization = language switch
        {
            Language.Français => new FrenchLocalization().GetLocalization(),
            Language.English => new EnglishLocalization().GetLocalization(),
            _ => throw new ArgumentOutOfRangeException(language.ToString())
        };
    }

    public static void SetLanguage(Language language)
    {
        List<Language> languages = Enum.GetValues<Language>().ToList();
        if (!languages.Contains(language)) return;
        _language = language;
        LoadLanguage(_language);
    }

    public static Language GetCurrentLanguage()
    {
        return _language;
    }

    public static List<Language> GetAvailableLanguages()
    {
        return Enum.GetValues<Language>().ToList();
    }
}