using System.Net.Http.Json;
using ManaxLibrary.DTO.Setting;

namespace ManaxLibrary.ApiCaller;

public static class ManaxApiSettingsClient
{
    public static async Task<Optional<SettingsDataDto>> GetSettingsAsync()
    {
        return await ManaxApiClient.ExecuteWithErrorHandlingAsync(async () =>
        {
            HttpResponseMessage response = await ManaxApiClient.Client.GetAsync("api/settings");
            if (!response.IsSuccessStatusCode) return new Optional<SettingsDataDto>(response);
            SettingsDataDto? data = await response.Content.ReadFromJsonAsync<SettingsDataDto>();
            return data == null
                ? new Optional<SettingsDataDto>("Failed to read settings from response.")
                : new Optional<SettingsDataDto>(data);
        });
    }

    public static async Task<Optional<bool>> UpdateSettingsAsync(SettingsDataDto dataDto)
    {
        return await ManaxApiClient.ExecuteWithErrorHandlingAsync(async () =>
        {
            HttpResponseMessage response = await ManaxApiClient.Client.PutAsJsonAsync("api/settings", dataDto);
            return response.IsSuccessStatusCode
                ? new Optional<bool>(true)
                : new Optional<bool>(response);
        });
    }
}