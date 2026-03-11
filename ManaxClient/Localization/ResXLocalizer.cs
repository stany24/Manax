using System.Collections.Generic;
using System.Globalization;
using ManaxClient.Localization.Localizer;

namespace ManaxClient.Localization;

public class ResXLocalizer : BaseLocalizer
{
    private readonly List<string> _languagesKeys = ["en", "fr"];

    public override void Reload()
    {
        if (_languages.Count == 0) _languages.AddRange(_languagesKeys);
        ValidateLanguage();
        Resources.Culture = new CultureInfo(_language);
        _hasLoaded = true;
        UpdateDisplayLanguages();
    }

    protected override void OnLanguageChanged()
    {
        Reload();
    }

    public override string Get(string key)
    {
        if (!_hasLoaded)
            Reload();

        string? langString = Resources.ResourceManager.GetString(key, Resources.Culture);
        return langString != null
            ? langString.Replace("\\n", "\n")
            : $"{Language}:{key}";
    }
}