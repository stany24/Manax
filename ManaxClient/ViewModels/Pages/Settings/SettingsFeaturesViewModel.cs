using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using Jeek.Avalonia.Localization;
using ManaxLibrary;
using ManaxLibrary.ApiCaller;
using ManaxLibrary.DTO.Feature;
using ManaxLibrary.Logging;
using Feature = ManaxClient.Models.Feature;

namespace ManaxClient.ViewModels.Pages.Settings;

public partial class SettingsFeaturesViewModel : PageViewModel
{
    [ObservableProperty] private ObservableCollection<Feature> _features = [];
    [ObservableProperty] private string _problem = string.Empty;
    [ObservableProperty] private string _success = string.Empty;

    public SettingsFeaturesViewModel()
    {
        Task.Run(LoadFeatures);
    }

    private async void LoadFeatures()
    {
        try
        {
            Logger.LogInfo("Loading features");
            Optional<FeaturesManager> featuresResponse = await ManaxApiFeatureClient.GetEnabledFeaturesAsync();
            if (featuresResponse.Failed)
            {
                Problem = featuresResponse.Error;
                Logger.LogFailure("Failed to load features");
                return;
            }

            FeaturesManager featuresManager = featuresResponse.GetValue();
            List<FeatureType> allFeatures = Enum.GetValues<FeatureType>().ToList();
            Dispatcher.UIThread.Post(() =>
            {
                Features.Clear();
                foreach (FeatureType type in allFeatures)
                    Features.Add(new Feature
                    {
                        Key = type,
                        Value = featuresManager.IsEnabled(type),
                        Name = Feature.GetFeatureName(type),
                        Description = Feature.GetFeatureDescription(type)
                    });
            });
        }
        catch (Exception e)
        {
            Logger.LogError("Error when fetching features", e);
        }
    }

    public async void SaveFeatures()
    {
        try
        {
            Problem = string.Empty;
            Success = string.Empty;
            List<ManaxLibrary.DTO.Feature.Feature> features = Features
                .Select(f => new ManaxLibrary.DTO.Feature.Feature { Key = f.Key, Value = f.Value })
                .ToList();

            Optional<bool> response = await ManaxApiFeatureClient.SetFeaturesAsync(features);
            if (response.Failed)
            {
                Problem = response.Error;
                Logger.LogFailure("Failed to update features");
                return;
            }

            Success = Localizer.Get("SettingsFeaturesPage.FeaturesUpdatedSuccess");
            Logger.LogInfo("Features updated successfully");
        }
        catch (Exception e)
        {
            Logger.LogError("Error when updating features", e);
        }
    }
}