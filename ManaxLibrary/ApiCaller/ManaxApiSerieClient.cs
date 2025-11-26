using ManaxLibrary.DTO.Read;
using ManaxLibrary.DTO.Search;
using ManaxLibrary.DTO.Serie;

namespace ManaxLibrary.ApiCaller;

public static class ManaxApiSerieClient
{
    public static async Task<Optional<List<long>>> GetSeriesIdsAsync()
    {
        return await ManaxApiClient.GetAsync<List<long>>("api/series");
    }

    public static async Task<Optional<SerieDto>> GetSerieInfoAsync(long id)
    {
        return await ManaxApiClient.GetAsync<SerieDto>($"api/serie/{id}");
    }

    public static async Task<Optional<List<long>>> GetSerieChaptersAsync(long id)
    {
        return await ManaxApiClient.GetAsync<List<long>>($"api/serie/{id}/chapters");
    }

    public static async Task<Optional<List<ReadDto>>> GetSerieChaptersReadAsync(long id)
    {
        return await ManaxApiClient.GetAsync<List<ReadDto>>($"api/serie/{id}/reads");
    }

    public static async Task<Optional<long>> PostSerieAsync(SerieCreateDto serieCreate)
    {
        return await ManaxApiClient.PostAsync<long, SerieCreateDto>("api/serie", serieCreate);
    }

    public static async Task<Optional<bool>> PutSerieAsync(long id, SerieUpdateDto serieUpdate)
    {
        return await ManaxApiClient.PutSuccessAsync($"api/serie/{id}", serieUpdate);
    }

    public static async Task<Optional<bool>> DeleteSerieAsync(long id)
    {
        return await ManaxApiClient.DeleteAsync($"api/serie/{id}");
    }

    public static async Task<Optional<byte[]>> GetSeriePosterAsync(long id)
    {
        return await ManaxApiClient.GetBytesAsync($"api/serie/{id}/poster");
    }

    public static async Task<Optional<byte[]>> GetSerieBannerAsync(long id)
    {
        return await ManaxApiClient.GetBytesAsync($"api/serie/{id}/banner");
    }

    public static async Task<Optional<List<long>>> GetSearchResult(Search search)
    {
        return await ManaxApiClient.PostAsync<List<long>, Search>("api/serie/search", search);
    }
}