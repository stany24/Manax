using System.Net.Http.Json;
using ManaxLibrary.DTO.SavePoint;

namespace ManaxLibrary.ApiCaller;

public static class ManaxApiSavePointClient
{
    public static async Task<Optional<long>> PostSavePointAsync(SavePointCreateDto savePointCreate)
    {
        return await ManaxApiClient.ExecuteWithErrorHandlingAsync(async () =>
        {
            HttpResponseMessage response =
                await ManaxApiClient.Client.PostAsJsonAsync("api/save-point/create", savePointCreate);
            if (!response.IsSuccessStatusCode) return new Optional<long>(response);
            long id = await response.Content.ReadFromJsonAsync<long>();
            return new Optional<long>(id);
        });
    }
}