using CommunityToolkit.Mvvm.ComponentModel;
using Jeek.Avalonia.Localization;
using ManaxLibrary.DTO.Feature;

namespace ManaxClient.Models;

public partial class Feature : ObservableObject
{
    [ObservableProperty] private string _description = string.Empty;
    [ObservableProperty] private FeatureType _key;
    [ObservableProperty] private string _name = string.Empty;
    [ObservableProperty] private bool _value;

    public static string GetFeatureName(FeatureType featureType)
    {
        return featureType switch
        {
            FeatureType.Ranks => Localizer.Get("SettingsFeaturesPage.Feature.Ranks"),
            FeatureType.AutomaticIssues => Localizer.Get("SettingsFeaturesPage.Feature.AutomaticIssues"),
            FeatureType.ReportedIssues => Localizer.Get("SettingsFeaturesPage.Feature.ReportedIssues"),
            _ => featureType.ToString()
        };
    }

    public static string GetFeatureDescription(FeatureType featureType)
    {
        return featureType switch
        {
            FeatureType.Ranks => Localizer.Get("SettingsFeaturesPage.Description.Ranks"),
            FeatureType.AutomaticIssues => Localizer.Get("SettingsFeaturesPage.Description.AutomaticIssues"),
            FeatureType.ReportedIssues => Localizer.Get("SettingsFeaturesPage.Description.ReportedIssues"),
            _ => ""
        };
    }
}