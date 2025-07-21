using System.Net.Http.Json;
using System.Text.Json;
using ManaxLibrary.DTOs.User;

namespace ManaxLibrary.ApiCaller;

public static class ManaxApiUserClient
{
    public static async Task<Optional<string>> LoginAsync(string username, string password)
    {
        HttpResponseMessage response =
            await ManaxApiClient.Client.PostAsJsonAsync("/api/login", new { username, password });
        if (!response.IsSuccessStatusCode) return new Optional<string>(response);
        string json = await response.Content.ReadAsStringAsync();
        using JsonDocument doc = JsonDocument.Parse(json);
        string? token = doc.RootElement.GetProperty("token").GetString();
        return token == null
            ? new Optional<string>("Failed to read token from response.")
            : new Optional<string>(token,false);
    }

    public static async Task<Optional<List<long>>> GetUsersIdsAsync()
    {
        HttpResponseMessage response = await ManaxApiClient.Client.GetAsync("api/users");
        if (!response.IsSuccessStatusCode) return new Optional<List<long>>(response);
        List<long>? ids = await response.Content.ReadFromJsonAsync<List<long>>();
        return ids == null
            ? new Optional<List<long>>("Failed to read user IDs from response.")
            : new Optional<List<long>>(ids);
    }

    public static async Task<Optional<UserDTO>> GetUserAsync(long id)
    {
        HttpResponseMessage response = await ManaxApiClient.Client.GetAsync($"api/user/{id}");
        if (!response.IsSuccessStatusCode) return new Optional<UserDTO>(response);
        UserDTO? user = await response.Content.ReadFromJsonAsync<UserDTO>();
        return user == null
            ? new Optional<UserDTO>($"Failed to read user with ID {id} from response.")
            : new Optional<UserDTO>(user);
    }

    public static async Task<Optional<long>> PostUserAsync(UserCreateDTO user)
    {
        HttpResponseMessage response = await ManaxApiClient.Client.PostAsJsonAsync("api/user/create", user);
        if (!response.IsSuccessStatusCode) return new Optional<long>(response);
        long? id = await response.Content.ReadFromJsonAsync<long>();
        return id == null
            ? new Optional<long>("Failed to read created user ID from response.")
            : new Optional<long>(id.Value);
    }

    public static async Task<Optional<bool>> PutUserAsync(long id, UserDTO user)
    {
        HttpResponseMessage response = await ManaxApiClient.Client.PutAsJsonAsync($"api/user/{id}", user);
        return response.IsSuccessStatusCode
            ? new Optional<bool>(true)
            : new Optional<bool>(response);
    }

    public static async Task<Optional<bool>> DeleteUserAsync(long id)
    {
        HttpResponseMessage response = await ManaxApiClient.Client.DeleteAsync($"api/user/{id}");
        return response.IsSuccessStatusCode
            ? new Optional<bool>(true)
            : new Optional<bool>(response);
    }

    public static async Task<Optional<UserDTO>> GetSelf()
    {
        HttpResponseMessage response = await ManaxApiClient.Client.GetAsync("api/user/current");
        if (!response.IsSuccessStatusCode) return new Optional<UserDTO>(response);
        UserDTO? userInfo = await response.Content.ReadFromJsonAsync<UserDTO>();
        if (userInfo == null) return new Optional<UserDTO>("Failed to read current user info from response.");
        
        return new Optional<UserDTO>(new UserDTO
        {
            Id = userInfo.Id,
            Username = userInfo.Username,
            Role = userInfo.Role
        });
    }

    public static async Task<Optional<string>> ClaimAsync(string username, string password)
    {
        HttpResponseMessage response =
            await ManaxApiClient.Client.PostAsJsonAsync("/api/claim", new { username, password });
        if (!response.IsSuccessStatusCode) return new Optional<string>(response);
        string json = await response.Content.ReadAsStringAsync();
        using JsonDocument doc = JsonDocument.Parse(json);
        string? token = doc.RootElement.GetProperty("token").GetString();
        return token == null
            ? new Optional<string>("Failed to read token from response.")
            : new Optional<string>(token,false);
    }
}