using System.Net.Http.Json;
using ManaxLibrary.DTOs.Library;

namespace ManaxLibrary.ApiCaller;

public static class ManaxApiLibraryClient
{
    public static async Task<Optional<List<long>>> GetLibraryIdsAsync()
    {
        HttpResponseMessage response = await ManaxApiClient.Client.GetAsync("api/libraries");
        if (!response.IsSuccessStatusCode) return new Optional<List<long>>(response);
        List<long>? ids = await response.Content.ReadFromJsonAsync<List<long>>();
        return ids == null
            ? new Optional<List<long>>("Failed to read library IDs from response.")
            : new Optional<List<long>>(ids);
    }

    public static async Task<Optional<LibraryDTO>> GetLibraryAsync(long id)
    {
        HttpResponseMessage response = await ManaxApiClient.Client.GetAsync($"api/library/{id}");
        if (!response.IsSuccessStatusCode) return new Optional<LibraryDTO>(response);
        LibraryDTO? library = await response.Content.ReadFromJsonAsync<LibraryDTO>();
        return library == null
            ? new Optional<LibraryDTO>($"Failed to read library with ID {id} from response.")
            : new Optional<LibraryDTO>(library);
    }

    public static async Task<Optional<long>> PostLibraryAsync(LibraryCreateDTO library)
    {
        HttpResponseMessage response = await ManaxApiClient.Client.PostAsJsonAsync("api/library/create", library);
        if (!response.IsSuccessStatusCode) return new Optional<long>(response);
        long? id = await response.Content.ReadFromJsonAsync<long>();
        return id == null
            ? new Optional<long>("Failed to read created library ID from response.")
            : new Optional<long>(id.Value);
    }

    public static async Task<Optional<bool>> PutLibraryAsync(long id, LibraryCreateDTO library)
    {
        HttpResponseMessage response = await ManaxApiClient.Client.PutAsJsonAsync($"api/library/{id}", library);
        return response.IsSuccessStatusCode
            ? new Optional<bool>(true)
            : new Optional<bool>(response);
    }

    public static async Task<Optional<bool>> DeleteLibraryAsync(long id)
    {
        HttpResponseMessage response = await ManaxApiClient.Client.DeleteAsync($"api/library/{id}");
        return response.IsSuccessStatusCode
            ? new Optional<bool>(true)
            : new Optional<bool>(response);
    }
}