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
                return response.StatusCode switch
                {
                    HttpStatusCode.Unauthorized => Optional<UserLoginResultDto>.Failure("Invalid username or password."),
                    HttpStatusCode.BadRequest => Optional<UserLoginResultDto>.Failure("User and password are required."),
                    _ => Optional<UserLoginResultDto>.Failure(response)
                };
            UserLoginResultDto? user = await response.Content.ReadFromJsonAsync<UserLoginResultDto>();
            return user == null
                ? Optional<UserLoginResultDto>.Failure("Failed to read response content")
                : Optional<UserLoginResultDto>.Success(user);
        });
    }

    public static async Task<Optional<List<long>>> GetUsersIdsAsync()
    {
        return await ManaxApiClient.GetAsync<List<long>>("api/users");
    }

    public static async Task<Optional<UserDto>> GetUserAsync(long id)
    {
        return await ManaxApiClient.GetAsync<UserDto>($"api/user/{id}");
    }

    public static async Task<Optional<bool>> PostUserAsync(UserCreateDto user)
    {
        return await ManaxApiClient.PostSuccessAsync("api/user/create", user);
    }

    public static async Task<Optional<bool>> PutUserAsync(UserUpdateDto userUpdate)
    {
        return await ManaxApiClient.PutSuccessAsync("api/user/update", userUpdate);
    }

    public static async Task<Optional<string>> ResetPasswordAsync(long id)
    {
        return await ManaxApiClient.PutAsync<string, object>($"api/user/{id}/password/reset", null!);
    }

    public static async Task<Optional<bool>> DeleteUserAsync(long id)
    {
        return await ManaxApiClient.DeleteAsync($"api/user/{id}");
    }

    public static async Task<Optional<UserLoginResultDto>> ClaimAsync(string username, string password)
    {
        return await ManaxApiClient.PostAsync<UserLoginResultDto, object>("api/claim", new { username, password });
    }

    public static async Task<Optional<bool>> LogoutAsync()
    {
        return await ManaxApiClient.PostSuccessAsync("api/logout");
    }
}