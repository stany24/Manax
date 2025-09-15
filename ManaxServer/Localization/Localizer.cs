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

    public static void VerifyLocalizations()
    {
        new FrenchLocalization().VerifyLocalizationKeys();
        new EnglishLocalization().VerifyLocalizationKeys();
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
        List<Language> languages = Enum.GetValues(typeof(Language)).Cast<Language>().ToList();
        if (languages.Contains(language))
        {
            _language = language;
            LoadLanguage(_language);
        }
    }

    public static Language GetCurrentLanguage()
    {
        return _language;
    }

    public static List<Language> GetAvailableLanguages()
    {
        return Enum.GetValues(typeof(Language)).Cast<Language>().ToList();
    }
}