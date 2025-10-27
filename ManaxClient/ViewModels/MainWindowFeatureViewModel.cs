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
    private FeaturesManager _features = new(new List<Feature>());
    public static EventHandler<FeaturesManager>? FeatureChanged { get; set; }

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
            Logger.LogError("Failed to load features",e);
        }
    }

    private void OnFeatureModified(FeatureType featureType, bool enabled)
    {
        Dispatcher.UIThread.Invoke(() =>
        {
            _features.Features.RemoveAll(f => f.Key == featureType);
            _features.Features.Add(new Feature { Key = featureType, Value = enabled });
            NotifyAllForFeatureChanged();
        });
    }

    private void NotifyAllForFeatureChanged()
    {
        FeatureChanged?.Invoke(this, _features);
        PropertyInfo[] propertyInfos = GetType().GetProperties();
        foreach (PropertyInfo propertyInfo in propertyInfos)
            if (propertyInfo.PropertyType == typeof(bool) && propertyInfo.Name.EndsWith("FeatureEnabled"))
                OnPropertyChanged(propertyInfo.Name);
    }
}