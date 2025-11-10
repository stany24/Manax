using System.Net.Http.Json;
using ManaxLibrary.DTO.Role;

namespace ManaxLibrary.ApiCaller;

public static class ManaxApiRoleClient
{
    public static async Task<Optional<List<RoleDto>>> GetRolesAsync()
    {
        return await ManaxApiClient.ExecuteWithErrorHandlingAsync(async () =>
        {
            HttpResponseMessage response = await ManaxApiClient.Client.GetAsync("api/roles");
            if (!response.IsSuccessStatusCode) return new Optional<List<RoleDto>>(response);
            List<RoleDto>? roles = await response.Content.ReadFromJsonAsync<List<RoleDto>>();
            return roles == null
                ? new Optional<List<RoleDto>>("Failed to read roles from response.")
                : new Optional<List<RoleDto>>(roles);
        });
    }

    public static async Task<Optional<bool>> CreateRoleAsync(RoleCreateDto role)
    {
        return await ManaxApiClient.ExecuteWithErrorHandlingAsync(async () =>
        {
            HttpResponseMessage response = await ManaxApiClient.Client.PostAsJsonAsync("api/role", role);
            return response.IsSuccessStatusCode
                ? new Optional<bool>(true)
                : new Optional<bool>(response);
        });
    }

    public static async Task<Optional<bool>> UpdateRoleAsync(RoleUpdateDto role)
    {
        return await ManaxApiClient.ExecuteWithErrorHandlingAsync(async () =>
        {
            HttpResponseMessage response = await ManaxApiClient.Client.PutAsJsonAsync("api/role", role);
            return response.IsSuccessStatusCode
                ? new Optional<bool>(true)
                : new Optional<bool>(response);
        });
    }

    public static async Task<Optional<bool>> DeleteRoleAsync(long id)
    {
        return await ManaxApiClient.ExecuteWithErrorHandlingAsync(async () =>
        {
            HttpResponseMessage response = await ManaxApiClient.Client.DeleteAsync($"api/role/{id}");
            return response.IsSuccessStatusCode
                ? new Optional<bool>(true)
                : new Optional<bool>(response);
        });
    }
}