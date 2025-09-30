using System.Net;
using System.Net.Http.Json;
using ManaxLibrary.DTO.User;

namespace ManaxLibrary.ApiCaller;

public static class ManaxApiUserClient
{
    public static async Task<Optional<UserLoginResultDto>> LoginAsync(string username, string password)
    {
        return await ManaxApiClient.ExecuteWithErrorHandlingAsync(async () =>
        {
            HttpResponseMessage response =
                await ManaxApiClient.Client.PostAsJsonAsync("api/login", new { username, password });
            if (!response.IsSuccessStatusCode)
            {
                return response.StatusCode switch
                {
                    HttpStatusCode.Unauthorized => new Optional<UserLoginResultDto>("Invalid username or password."),
                    HttpStatusCode.BadRequest => new Optional<UserLoginResultDto>("User and password are required."),
                    _ => new Optional<UserLoginResultDto>(response)
                };
            }
            UserLoginResultDto? user = await response.Content.ReadFromJsonAsync<UserLoginResultDto>();
            return user == null
                ? new Optional<UserLoginResultDto>("Failed to read response content")
                : new Optional<UserLoginResultDto>(user);
        });
    }

    public static async Task<Optional<List<long>>> GetUsersIdsAsync()
    {
        return await ManaxApiClient.ExecuteWithErrorHandlingAsync(async () =>
        {
            HttpResponseMessage response = await ManaxApiClient.Client.GetAsync("api/users");
            if (!response.IsSuccessStatusCode) return new Optional<List<long>>(response);
            List<long>? ids = await response.Content.ReadFromJsonAsync<List<long>>();
            return ids == null
                ? new Optional<List<long>>("Failed to read user IDs from response.")
                : new Optional<List<long>>(ids);
        });
    }

    public static async Task<Optional<UserDto>> GetUserAsync(long id)
    {
        return await ManaxApiClient.ExecuteWithErrorHandlingAsync(async () =>
        {
            HttpResponseMessage response = await ManaxApiClient.Client.GetAsync($"api/user/{id}");
            if (!response.IsSuccessStatusCode) return new Optional<UserDto>(response);
            UserDto? user = await response.Content.ReadFromJsonAsync<UserDto>();
            return user == null
                ? new Optional<UserDto>($"Failed to read user with ID {id} from response.")
                : new Optional<UserDto>(user);
        });
    }

    public static async Task<Optional<bool>> PostUserAsync(UserCreateDto user)
    {
        return await ManaxApiClient.ExecuteWithErrorHandlingAsync(async () =>
        {
            HttpResponseMessage response = await ManaxApiClient.Client.PostAsJsonAsync("api/user/create", user);
            return !response.IsSuccessStatusCode ? new Optional<bool>(response) : new Optional<bool>(true);
        });
    }

    public static async Task<Optional<bool>> PutUserAsync(UserUpdateDto userUpdate)
    {
        return await ManaxApiClient.ExecuteWithErrorHandlingAsync(async () =>
        {
            HttpResponseMessage response = await ManaxApiClient.Client.PutAsJsonAsync("api/user/update", userUpdate);
            return response.IsSuccessStatusCode
                ? new Optional<bool>(true)
                : new Optional<bool>(response);
        });
    }

    public static async Task<Optional<string>> ResetPasswordAsync(long id)
    {
        return await ManaxApiClient.ExecuteWithErrorHandlingAsync(async () =>
        {
            HttpResponseMessage response = await ManaxApiClient.Client.PutAsync($"api/user/{id}/password/reset", null);
            if (!response.IsSuccessStatusCode) return new Optional<string>(response);
            string? newPassword = await response.Content.ReadFromJsonAsync<string>();
            return newPassword == null
                ? new Optional<string>("Failed to read new password from response.")
                : new Optional<string>(newPassword);
        });
    }

    public static async Task<Optional<bool>> DeleteUserAsync(long id)
    {
        return await ManaxApiClient.ExecuteWithErrorHandlingAsync(async () =>
        {
            HttpResponseMessage response = await ManaxApiClient.Client.DeleteAsync($"api/user/{id}");
            return response.IsSuccessStatusCode
                ? new Optional<bool>(true)
                : new Optional<bool>(response);
        });
    }

    public static async Task<Optional<UserLoginResultDto>> ClaimAsync(string username, string password)
    {
        return await ManaxApiClient.ExecuteWithErrorHandlingAsync(async () =>
        {
            HttpResponseMessage response =
                await ManaxApiClient.Client.PostAsJsonAsync("api/claim", new { username, password });
            if (!response.IsSuccessStatusCode) return new Optional<UserLoginResultDto>(response);
            UserLoginResultDto? user = await response.Content.ReadFromJsonAsync<UserLoginResultDto>();
            return user == null
                ? new Optional<UserLoginResultDto>("Failed to read response content")
                : new Optional<UserLoginResultDto>(user);
        });
    }

    public static async Task<Optional<bool>> LogoutAsync()
    {
        return await ManaxApiClient.ExecuteWithErrorHandlingAsync(async () =>
        {
            HttpResponseMessage response = await ManaxApiClient.Client.PostAsync("api/logout", null);
            return response.IsSuccessStatusCode
                ? new Optional<bool>(true)
                : new Optional<bool>(response);
        });
    }
}