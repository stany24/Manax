using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using ManaxClient.Event;
using ManaxLibrary;
using ManaxLibrary.ApiCaller;
using ManaxLibrary.DTO.Feature;
using ManaxLibrary.Logging;
using ManaxLibrary.Notifications;

namespace ManaxClient.Manager;

public class FeatureManager : ObservableObject
{
    public FeatureManager()
    {
        WeakReferenceMessenger.Default.Register<LoggedInMessage>(this, (_, _) => { Task.Run(LoadFeatures); });
        NotificationReceiver.OnFeatureModified += OnFeatureModified;
    }

    private List<Feature> Features { get; set; } = [];

    public static EventHandler<Feature>? FeatureChanged { get; set; }

    public bool RankFeatureEnabled => IsEnabled(FeatureType.Ranks);
    public bool AutomaticIssuesFeatureEnabled => IsEnabled(FeatureType.AutomaticIssues);

    public bool ReportedIssuesFeatureEnabled => IsEnabled(FeatureType.ReportedIssues);

    ~FeatureManager()
    {
        NotificationReceiver.OnFeatureModified -= OnFeatureModified;
    }

    public bool IsEnabled(FeatureType feature)
    {
        return Features.FirstOrDefault(f => f.Key == feature)?.Value ?? false;
    }

    private async Task<List<Feature>> LoadFeatures()
    {
        try
        {
            Optional<List<Feature>> featureResponse = await ManaxApiFeatureClient.GetEnabledFeaturesAsync();
            if (featureResponse.Failed)
            {
                WeakReferenceMessenger.Default.Send(new NotificationMessage(new Notification(featureResponse.Error)));
                return Features;
            }

            Features = featureResponse.GetValue();
            foreach (Feature feature in Features) OnFeatureModified(feature);
        }
        catch (Exception e)
        {
            Logger.LogError("Failed to load features", e);
        }

        return Features;
    }

    private void OnFeatureModified(Feature feature)
    {
        Dispatcher.UIThread.Post(() =>
        {
            Features.RemoveAll(f => f.Key == feature.Key);
            Features.Add(feature);

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