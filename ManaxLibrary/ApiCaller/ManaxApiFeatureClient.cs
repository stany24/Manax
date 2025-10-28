using System.Net.Http.Json;
using ManaxLibrary.DTO.Feature;

namespace ManaxLibrary.ApiCaller;

public static class ManaxApiFeatureClient
{
    public static async Task<Optional<FeaturesManager>> GetEnabledFeaturesAsync()
    {
        return await ManaxApiClient.ExecuteWithErrorHandlingAsync(async () =>
        {
            HttpResponseMessage response = await ManaxApiClient.Client.GetAsync("api/features");
            if (!response.IsSuccessStatusCode) return new Optional<FeaturesManager>(response);
            List<Feature>? permissions = await response.Content.ReadFromJsonAsync<List<Feature>>();
            return permissions == null
                ? new Optional<FeaturesManager>("Failed to read permissions from response.")
                : new Optional<FeaturesManager>(new FeaturesManager(permissions));
        });
    }
    
    public static async Task<Optional<bool>> SetFeatureEnabledAsync(FeatureType featureType, bool enabled)
    {
        return await ManaxApiClient.ExecuteWithErrorHandlingAsync(async () =>
        {
            HttpResponseMessage response = await ManaxApiClient.Client.PostAsync($"api/feature/{featureType}/{enabled}", null);
            return !response.IsSuccessStatusCode ? new Optional<bool>(response) : new Optional<bool>(true);
        });
    }
    
    public static async Task<Optional<bool>> SetFeatureEnabledAsync(string featureName, bool enabled)
    {
        return await ManaxApiClient.ExecuteWithErrorHandlingAsync(async () =>
        {
            HttpResponseMessage response = await ManaxApiClient.Client.PostAsync($"api/feature/{featureName}/{enabled}", null);
            return !response.IsSuccessStatusCode ? new Optional<bool>(response) : new Optional<bool>(true);
        });
    }
    
    public static async Task<Optional<bool>> SetFeaturesAsync(List<Feature> features)
    {
        return await ManaxApiClient.ExecuteWithErrorHandlingAsync(async () =>
        {
            HttpResponseMessage response = await ManaxApiClient.Client.PutAsJsonAsync("api/features", features);
            return !response.IsSuccessStatusCode ? new Optional<bool>(response) : new Optional<bool>(true);
        });
    }
}