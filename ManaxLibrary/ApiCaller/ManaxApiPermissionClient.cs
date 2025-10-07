using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using ManaxLibrary.DTO.User;

namespace ManaxLibrary.ApiCaller;

public static class ManaxApiPermissionClient
{
    public static async Task<Optional<List<Permission>>> GetMyPermissionsAsync()
    {
        return await ManaxApiClient.ExecuteWithErrorHandlingAsync(async () =>
        {
            HttpResponseMessage response = await ManaxApiClient.Client.GetAsync("api/permission/self");
            if (!response.IsSuccessStatusCode) return new Optional<List<Permission>>(response);
            List<Permission>? permissions = await response.Content.ReadFromJsonAsync<List<Permission>>();
            return permissions == null
                ? new Optional<List<Permission>>("Failed to read permissions from response.")
                : new Optional<List<Permission>>(permissions);
        });
    }

    public static async Task<Optional<List<Permission>>> GetUserPermissionsAsync(long userId)
    {
        return await ManaxApiClient.ExecuteWithErrorHandlingAsync(async () =>
        {
            HttpResponseMessage response = await ManaxApiClient.Client.GetAsync("api/permission/" + userId);
            if (!response.IsSuccessStatusCode) return new Optional<List<Permission>>(response);
            List<Permission>? permissions = await response.Content.ReadFromJsonAsync<List<Permission>>();
            return permissions == null
                ? new Optional<List<Permission>>("Failed to read permissions from response.")
                : new Optional<List<Permission>>(permissions);
        });
    }

    public static async Task<Optional<bool>> SetPermissionsAsync(long userId, List<Permission> permissions)
    {
        return await ManaxApiClient.ExecuteWithErrorHandlingAsync(async () =>
        {
            string json = JsonSerializer.Serialize(permissions);
            StringContent content = new(json, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await ManaxApiClient.Client.PostAsync($"api/permission/{userId}", content);
            return response.IsSuccessStatusCode ? new Optional<bool>(true) : new Optional<bool>(response);
        });
    }
}