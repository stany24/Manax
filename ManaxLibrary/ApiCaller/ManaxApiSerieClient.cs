using System.Net.Http.Json;
using ManaxLibrary.DTO.Read;
using ManaxLibrary.DTO.Search;
using ManaxLibrary.DTO.Serie;

namespace ManaxLibrary.ApiCaller;

public static class ManaxApiSerieClient
{
    public static async Task<Optional<List<long>>> GetSeriesIdsAsync()
    {
        HttpResponseMessage response = await ManaxApiClient.Client.GetAsync("api/series");
        if (!response.IsSuccessStatusCode) return new Optional<List<long>>(response);
        List<long>? ids = await response.Content.ReadFromJsonAsync<List<long>>();
        return ids == null
            ? new Optional<List<long>>("Failed to read series IDs from response.")
            : new Optional<List<long>>(ids);
    }

    public static async Task<Optional<SerieDto>> GetSerieInfoAsync(long id)
    {
        HttpResponseMessage response = await ManaxApiClient.Client.GetAsync($"api/serie/{id}");
        if (!response.IsSuccessStatusCode) return new Optional<SerieDto>(response);
        SerieDto? serie = await response.Content.ReadFromJsonAsync<SerieDto>();
        return serie == null
            ? new Optional<SerieDto>($"Failed to read serie info for ID {id} from response.")
            : new Optional<SerieDto>(serie);
    }

    public static async Task<Optional<List<long>>> GetSerieChaptersAsync(long id)
    {
        HttpResponseMessage response = await ManaxApiClient.Client.GetAsync($"api/series/{id}/chapters");
        if (!response.IsSuccessStatusCode) return new Optional<List<long>>(response);
        List<long>? ids = await response.Content.ReadFromJsonAsync<List<long>>();
        return ids == null
            ? new Optional<List<long>>("Failed to read chapter IDs from response.")
            : new Optional<List<long>>(ids);
    }
    
    public static async Task<Optional<List<ReadDto>>> GetSerieChaptersReadAsync(long id)
    {
        HttpResponseMessage response = await ManaxApiClient.Client.GetAsync($"api/series/{id}/reads");
        if (!response.IsSuccessStatusCode) return new Optional<List<ReadDto>>(response);
        List<ReadDto>? ids = await response.Content.ReadFromJsonAsync<List<ReadDto>>();
        return ids == null
            ? new Optional<List<ReadDto>>("Failed to read chapter IDs from response.")
            : new Optional<List<ReadDto>>(ids);
    }

    public static async Task<Optional<long>> PostSerieAsync(SerieCreateDto serieCreate)
    {
        HttpResponseMessage response = await ManaxApiClient.Client.PostAsJsonAsync("/api/serie", serieCreate);
        if (!response.IsSuccessStatusCode) return new Optional<long>(response);
        long id = await response.Content.ReadFromJsonAsync<long>();
        return new Optional<long>(id);
    }

    public static async Task<Optional<bool>> PutSerieAsync(long id, SerieUpdateDto serieUpdate)
    {
        HttpResponseMessage response = await ManaxApiClient.Client.PutAsJsonAsync($"api/serie/{id}", serieUpdate);
        return response.IsSuccessStatusCode
            ? new Optional<bool>(true)
            : new Optional<bool>(response);
    }

    public static async Task<Optional<bool>> DeleteSerieAsync(long id)
    {
        HttpResponseMessage response = await ManaxApiClient.Client.DeleteAsync($"api/serie/{id}");
        return response.IsSuccessStatusCode
            ? new Optional<bool>(true)
            : new Optional<bool>(response);
    }

    public static async Task<Optional<byte[]>> GetSeriePosterAsync(long id)
    {
        HttpResponseMessage response = await ManaxApiClient.Client.GetAsync($"api/serie/{id}/poster");
        if (!response.IsSuccessStatusCode) return new Optional<byte[]>(response);
        byte[] data = await response.Content.ReadAsByteArrayAsync();
        return data.Length == 0
            ? new Optional<byte[]>($"Empty poster data received for serie ID {id}.")
            : new Optional<byte[]>(data);
    }

    public static async Task<Optional<List<long>>> GetSearchResult(Search search)
    {
        HttpResponseMessage response = await ManaxApiClient.Client.PostAsJsonAsync("api/serie/search", search);
        if (!response.IsSuccessStatusCode) return new Optional<List<long>>(response);
        List<long>? results = await response.Content.ReadFromJsonAsync<List<long>>();
        return results == null
            ? new Optional<List<long>>("Failed to read search results from response.")
            : new Optional<List<long>>(results);
    }
}