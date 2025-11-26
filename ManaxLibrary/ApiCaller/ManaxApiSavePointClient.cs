using ManaxLibrary.DTO.SavePoint;

namespace ManaxLibrary.ApiCaller;

public static class ManaxApiSavePointClient
{
    public static async Task<Optional<long>> PostSavePointAsync(SavePointCreateDto savePointCreate)
    {
        return await ManaxApiClient.PostAsync<long, SavePointCreateDto>("api/save-point/create", savePointCreate);
    }
}