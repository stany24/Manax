using System;
using System.Collections.Generic;
using System.Reflection;
using Avalonia.Threading;
using ManaxLibrary;
using ManaxLibrary.ApiCaller;
using ManaxLibrary.DTO.Feature;
using ManaxLibrary.Logging;

namespace ManaxClient.ViewModels;

public partial class MainWindowViewModel
{
    private FeaturesDto _features = new([]);

    public bool RankFeatureEnabled => _features.IsEnabled(FeatureType.Ranks);
    public bool AutomaticIssuesFeatureEnabled => _features.IsEnabled(FeatureType.AutomaticIssues);

    public bool ReportedIssuesFeatureEnabled => _features.IsEnabled(FeatureType.ReportedIssues);

    private async void LoadFeatures()
    {
        try
        {
            Optional<FeaturesDto> featureResponse = await ManaxApiFeatureClient.GetEnabledFeaturesAsync();
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
            Logger.LogError("Failed to load features",e);
        }
    }

    private void OnFeatureModified(FeatureType featureType, bool enabled)
    {
        Dispatcher.UIThread.Invoke(() =>
        {
            _features.Features.RemoveAll(f => f.Key == featureType);
            _features.Features.Add(new KeyValuePair<FeatureType, bool>(featureType, enabled));
            NotifyAllForFeatureChanged();
        });
    }

    private void NotifyAllForFeatureChanged()
    {
        PropertyInfo[] propertyInfos = GetType().GetProperties();
        foreach (PropertyInfo propertyInfo in propertyInfos)
            if (propertyInfo.PropertyType == typeof(bool) && propertyInfo.Name.EndsWith("FeatureEnabled"))
                OnPropertyChanged(propertyInfo.Name);
    }
}