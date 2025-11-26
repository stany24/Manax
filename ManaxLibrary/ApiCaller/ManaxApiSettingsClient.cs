using ManaxLibrary.DTO.Setting;

namespace ManaxLibrary.ApiCaller;

public static class ManaxApiSettingsClient
{
    public static async Task<Optional<SettingsDataDto>> GetSettingsAsync()
    {
        return await ManaxApiClient.GetAsync<SettingsDataDto>("api/settings");
    }

    public static async Task<Optional<bool>> UpdateSettingsAsync(SettingsDataDto dataDto)
    {
        return await ManaxApiClient.PutSuccessAsync("api/settings", dataDto);
    }
}