using ManaxLibrary.DTO.User;

namespace ManaxLibrary.ApiCaller;

public static class ManaxApiPermissionClient
{
    public static async Task<Optional<List<Permission>>> GetMyPermissionsAsync()
    {
        return await ManaxApiClient.GetAsync<List<Permission>>("api/permission/self");
    }

    public static async Task<Optional<List<Permission>>> GetUserPermissionsAsync(long userId)
    {
        return await ManaxApiClient.GetAsync<List<Permission>>("api/permission/" + userId);
    }

    public static async Task<Optional<bool>> SetPermissionsAsync(long userId, List<Permission> permissions)
    {
        return await ManaxApiClient.PostSuccessAsync($"api/permission/{userId}", permissions);
    }
}