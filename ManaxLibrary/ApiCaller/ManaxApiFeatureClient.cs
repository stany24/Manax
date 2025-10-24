using System.Net.Http.Json;
using ManaxLibrary.DTO.Feature;

namespace ManaxLibrary.ApiCaller;

public static class ManaxApiFeatureClient
{
    public static async Task<Optional<FeaturesDto>> GetEnabledFeaturesAsync()
    {
        return await ManaxApiClient.ExecuteWithErrorHandlingAsync(async () =>
        {
            HttpResponseMessage response = await ManaxApiClient.Client.GetAsync("api/features");
            if (!response.IsSuccessStatusCode) return new Optional<FeaturesDto>(response);
            FeaturesDto? permissions = await response.Content.ReadFromJsonAsync<FeaturesDto>();
            return permissions == null
                ? new Optional<FeaturesDto>("Failed to read permissions from response.")
                : new Optional<FeaturesDto>(permissions);
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
    
    public static async Task<Optional<bool>> SetFeaturesAsync(FeaturesDto features)
    {
        return await ManaxApiClient.ExecuteWithErrorHandlingAsync(async () =>
        {
            HttpResponseMessage response = await ManaxApiClient.Client.PutAsJsonAsync("api/features", features);
            return !response.IsSuccessStatusCode ? new Optional<bool>(response) : new Optional<bool>(true);
        });
    }
}