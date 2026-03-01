using System.Collections.Generic;
using System.Linq;
using DynamicData;

namespace ManaxClient.Localization;

public static class LanguageSource
{
    public static readonly SourceCache<Language, string> Languages = new(x => x.Code);

    static LanguageSource()
    {
        Localizer.Localizer.LanguageChanged += (_, _) => UpdateLanguages();
        GenerateLanguages();
    }

    private static void GenerateLanguages()
    {
        List<string> availableLanguages = Localizer.Localizer.Languages;
        List<Language> languages = [];
        languages.AddRange(availableLanguages.Select(langCode => new Language
            { Code = langCode, DisplayName = Localizer.Localizer.Get(langCode) }));

        Languages.Edit(innerCache => { innerCache.AddOrUpdate(languages); });
    }

    private static void UpdateLanguages()
    {
        List<Language> updatedLanguages = Languages.Items.ToList();
        foreach (Language lang in updatedLanguages) lang.DisplayName = Localizer.Localizer.Get(lang.Code);
    }
}