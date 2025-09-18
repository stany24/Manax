namespace ManaxServer.Localization.Languages;

public interface ILocalization
{
    public Dictionary<LocalizationKey, string> GetLocalization();
}