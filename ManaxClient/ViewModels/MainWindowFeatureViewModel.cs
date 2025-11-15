using System;
using System.Reflection;
using Avalonia.Threading;
using ManaxLibrary;
using ManaxLibrary.ApiCaller;
using ManaxLibrary.DTO.Feature;
using ManaxLibrary.Logging;

namespace ManaxClient.ViewModels;

public partial class MainWindowViewModel
{
    private FeaturesManager _features = new([]);
    public static EventHandler<Feature>? FeatureChanged { get; set; }

    public bool RankFeatureEnabled => _features.IsEnabled(FeatureType.Ranks);
    public bool AutomaticIssuesFeatureEnabled => _features.IsEnabled(FeatureType.AutomaticIssues);

    public bool ReportedIssuesFeatureEnabled => _features.IsEnabled(FeatureType.ReportedIssues);

    private async void LoadFeatures()
    {
        try
        {
            Optional<FeaturesManager> featureResponse = await ManaxApiFeatureClient.GetEnabledFeaturesAsync();
            if (featureResponse.Failed)
            {
                ShowInfo(featureResponse.Error);
                return;
            }

            _features = featureResponse.GetValue();
            NotifyAllForFeatureChanged();
        }
        catch (Exception e)
        {
            Logger.LogError("Failed to load features", e);
        }
    }

    private void OnFeatureModified(Feature feature)
    {
        Dispatcher.UIThread.Invoke(() =>
        {
            _features.Features.RemoveAll(f => f.Key == feature.Key);
            _features.Features.Add(feature);

            FeatureChanged?.Invoke(this, feature);
            NotifyAllForFeatureChanged();
        });
    }

    private void NotifyAllForFeatureChanged()
    {
        PropertyInfo[] propertyInfos = GetType().GetProperties();
        foreach (PropertyInfo propertyInfo in propertyInfos)
            if (propertyInfo.PropertyType == typeof(bool) &&
                propertyInfo.Name.EndsWith("FeatureEnabled", StringComparison.InvariantCulture))
                OnPropertyChanged(propertyInfo.Name);
    }
}