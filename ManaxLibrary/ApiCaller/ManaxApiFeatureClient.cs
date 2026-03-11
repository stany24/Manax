using ManaxLibrary.DTO.Feature;

namespace ManaxLibrary.ApiCaller;

public static class ManaxApiFeatureClient
{
    public static async Task<Optional<List<Feature>>> GetEnabledFeaturesAsync()
    {
        return await ManaxApiClient.GetAsync<List<Feature>>("api/features");
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