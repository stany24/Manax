using System.Net.Http.Json;
using ManaxApi.Models.User;

namespace ManaxApiCaller;

public static class ManaxApiUserCaller
{
    public static async Task<List<long>?> GetUsersIdsAsync()
    {
        HttpResponseMessage response = await ManaxApiCaller.Client.GetAsync("api/users");
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<List<long>>();
    }

    public static async Task<User?> GetUserAsync(long id)
    {
        HttpResponseMessage response = await ManaxApiCaller.Client.GetAsync($"api/user/{id}");
        if (!response.IsSuccessStatusCode) return null;
        UserInfo? userInfo = await response.Content.ReadFromJsonAsync<UserInfo>();
        if (userInfo == null) return null;
        return new User
        {
            Id = id,
            Username = userInfo.Username,
            PasswordHash = string.Empty,
            Role = userInfo.Role
        };
    }

    public static async Task<UserInfo?> PostUserAsync(User user)
    {
        HttpResponseMessage response = await ManaxApiCaller.Client.PostAsJsonAsync("api/user/create", user);
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<UserInfo>();
    }

    public static async Task<bool> PutUserAsync(long id, User user)
    {
        HttpResponseMessage response = await ManaxApiCaller.Client.PutAsJsonAsync($"api/user/{id}", user);
        return response.IsSuccessStatusCode;
    }

    public static async Task<bool> DeleteUserAsync(long id)
    {
        HttpResponseMessage response = await ManaxApiCaller.Client.DeleteAsync($"api/user/{id}");
        return response.IsSuccessStatusCode;
    }
}
