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
            if (!response.IsSuccessStatusCode) return Optional<FeaturesManager>.Failure(response);
            List<Feature>? permissions = await response.Content.ReadFromJsonAsync<List<Feature>>();
            return permissions == null
                ? Optional<FeaturesManager>.Failure("Failed to read permissions from response.")
                : Optional<FeaturesManager>.Success(new FeaturesManager(permissions));
        });
    }

    public static async Task<Optional<bool>> SetFeatureEnabledAsync(Feature feature)
    {
        return await ManaxApiClient.PostSuccessAsync("api/feature", feature);
    }

    public static async Task<Optional<bool>> SetFeaturesAsync(List<Feature> features)
    {
        return await ManaxApiClient.PutSuccessAsync("api/features", features);
    }
}