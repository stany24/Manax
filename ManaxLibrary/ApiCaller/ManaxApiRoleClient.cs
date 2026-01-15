using ManaxLibrary.DTO.Role;

namespace ManaxLibrary.ApiCaller;

public static class ManaxApiRoleClient
{
    public static async Task<Optional<List<RoleDto>>> GetRolesAsync()
    {
        return await ManaxApiClient.GetAsync<List<RoleDto>>("api/roles");
    }

    public static async Task<Optional<bool>> CreateRoleAsync(RoleCreateDto role)
    {
        return await ManaxApiClient.PostSuccessAsync("api/role", role);
    }

    public static async Task<Optional<bool>> UpdateRoleAsync(long id, RoleUpdateDto role)
    {
        return await ManaxApiClient.PutSuccessAsync($"api/role/{id}", role);
    }

    public static async Task<Optional<bool>> DeleteRoleAsync(long id)
    {
        return await ManaxApiClient.DeleteAsync($"api/role/{id}");
    }
}