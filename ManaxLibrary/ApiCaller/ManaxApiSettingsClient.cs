using System.Net.Http.Json;
using ManaxLibrary.DTO.Setting;

namespace ManaxLibrary.ApiCaller;

public static class ManaxApiSettingsClient
{
    public static async Task<Optional<SettingsData>> GetSettingsAsync()
    {
        HttpResponseMessage response = await ManaxApiClient.Client.GetAsync("api/settings");
        if (!response.IsSuccessStatusCode) return new Optional<SettingsData>(response);
        SettingsData? data = await response.Content.ReadFromJsonAsync<SettingsData>();
        return data == null
            ? new Optional<SettingsData>("Failed to read settings from response.")
            : new Optional<SettingsData>(data);
    }

    public static async Task<Optional<bool>> UpdateSettingsAsync(SettingsData data)
    {
        HttpResponseMessage response = await ManaxApiClient.Client.PutAsJsonAsync("api/settings", data);
        return response.IsSuccessStatusCode
            ? new Optional<bool>(true)
            : new Optional<bool>(response);
    }
}