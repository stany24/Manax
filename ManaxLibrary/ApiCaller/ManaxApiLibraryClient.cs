using ManaxLibrary.DTO.Library;

namespace ManaxLibrary.ApiCaller;

public static class ManaxApiLibraryClient
{
    public static async Task<Optional<List<long>>> GetLibraryIdsAsync()
    {
        return await ManaxApiClient.GetAsync<List<long>>("api/libraries");
    }

    public static async Task<Optional<LibraryDto>> GetLibraryAsync(long id)
    {
        return await ManaxApiClient.GetAsync<LibraryDto>($"api/library/{id}");
    }

    public static async Task<Optional<long>> PostLibraryAsync(LibraryCreateDto library)
    {
        return await ManaxApiClient.PostAsync<long, LibraryCreateDto>("api/library/create", library);
    }

    public static async Task<Optional<bool>> PutLibraryAsync(long id, LibraryUpdateDto library)
    {
        return await ManaxApiClient.PutSuccessAsync($"api/library/{id}", library);
    }

    public static async Task<Optional<bool>> DeleteLibraryAsync(long id)
    {
        return await ManaxApiClient.DeleteAsync($"api/library/{id}");
    }
}