using System.Net.Http.Json;
using System.Text.Json;
using ManaxLibrary.DTOs;

namespace ManaxLibrary.ApiCaller;

public static class ManaxApiUserClient
{
    public static async Task<string?> LoginAsync(string username, string password)
    {
        HttpResponseMessage response = await ManaxApiClient.Client.PostAsJsonAsync("/api/login", new { username, password });
        if (!response.IsSuccessStatusCode) return null;
        string json = await response.Content.ReadAsStringAsync();
        using JsonDocument doc = JsonDocument.Parse(json);
        return doc.RootElement.GetProperty("token").GetString();
    }

    public static async Task<List<long>?> GetUsersIdsAsync()
    {
        HttpResponseMessage response = await ManaxApiClient.Client.GetAsync("api/users");
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<List<long>>();
    }

    public static async Task<UserDTO?> GetUserAsync(long id)
    {
        HttpResponseMessage response = await ManaxApiClient.Client.GetAsync($"api/user/{id}");
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<UserDTO>();
    }

    public static async Task<long?> PostUserAsync(UserCreateDTO user)
    {
        HttpResponseMessage response = await ManaxApiClient.Client.PostAsJsonAsync("api/user/create", user);
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<long>();
    }

    public static async Task<bool> PutUserAsync(long id, UserDTO user)
    {
        HttpResponseMessage response = await ManaxApiClient.Client.PutAsJsonAsync($"api/user/{id}", user);
        return response.IsSuccessStatusCode;
    }

    public static async Task<bool> DeleteUserAsync(long id)
    {
        HttpResponseMessage response = await ManaxApiClient.Client.DeleteAsync($"api/user/{id}");
        return response.IsSuccessStatusCode;
    }

    public static async Task<UserDTO?> GetSelf()
    {
        HttpResponseMessage response = await ManaxApiClient.Client.GetAsync("api/user/current");
        if (!response.IsSuccessStatusCode) return null;
        UserDTO? userInfo = await response.Content.ReadFromJsonAsync<UserDTO>();
        if (userInfo == null) return null;
        return new UserDTO
        {
            Id = userInfo.Id,
            Username = userInfo.Username,
            Role = userInfo.Role
        };
    }

    public static async Task<string?> ClaimAsync(string username, string password)
    {
        HttpResponseMessage response = await ManaxApiClient.Client.PostAsJsonAsync("/api/claim", new { username, password });
        if (!response.IsSuccessStatusCode) return null;
        string json = await response.Content.ReadAsStringAsync();
        using JsonDocument doc = JsonDocument.Parse(json);
        return doc.RootElement.GetProperty("token").GetString();
    }
}