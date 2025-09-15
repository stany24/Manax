namespace ManaxServer.Localization.Languages;

public abstract class Localization
{
    public void VerifyLocalizationKeys()
    {
        List<LocalizationKey> keys = Enum.GetValues(typeof(LocalizationKey)).Cast<LocalizationKey>().ToList();
        Dictionary<LocalizationKey, string> localization = GetLocalization();
        foreach (LocalizationKey key in keys.Where(key => !localization.ContainsKey(key)))
            throw new Exception($"Missing localization for key: {key}");

        if (localization.Count != keys.Count) throw new Exception("Localization contains extra keys");
    }

    public abstract Dictionary<LocalizationKey, string> GetLocalization();
}