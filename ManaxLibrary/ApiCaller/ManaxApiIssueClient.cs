using System.Net.Http.Json;
using ManaxLibrary.DTO.Issue.Automatic;
using ManaxLibrary.DTO.Issue.Reported;

namespace ManaxLibrary.ApiCaller;

public static class ManaxApiIssueClient
{
    public static async Task<Optional<List<AutomaticIssueChapterDto>>> GetAllAutomaticChapterIssuesAsync()
    {
        HttpResponseMessage response = await ManaxApiClient.Client.GetAsync("/api/issue/chapter/internal");
        if (!response.IsSuccessStatusCode) return new Optional<List<AutomaticIssueChapterDto>>(response);
        List<AutomaticIssueChapterDto>? issues =
            await response.Content.ReadFromJsonAsync<List<AutomaticIssueChapterDto>>();
        return issues == null
            ? new Optional<List<AutomaticIssueChapterDto>>("Failed to read automatic chapter issues from response.")
            : new Optional<List<AutomaticIssueChapterDto>>(issues);
    }

    public static async Task<Optional<List<AutomaticIssueSerieDto>>> GetAllAutomaticSerieIssuesAsync()
    {
        HttpResponseMessage response = await ManaxApiClient.Client.GetAsync("/api/issue/serie/internal");
        if (!response.IsSuccessStatusCode) return new Optional<List<AutomaticIssueSerieDto>>(response);
        List<AutomaticIssueSerieDto>? issues = await response.Content.ReadFromJsonAsync<List<AutomaticIssueSerieDto>>();
        return issues == null
            ? new Optional<List<AutomaticIssueSerieDto>>("Failed to read automatic serie issues from response.")
            : new Optional<List<AutomaticIssueSerieDto>>(issues);
    }

    public static async Task<Optional<List<ReportedIssueChapterDto>>> GetAllReportedChapterIssuesAsync()
    {
        HttpResponseMessage response = await ManaxApiClient.Client.GetAsync("/api/issue/chapter/reported");
        if (!response.IsSuccessStatusCode) return new Optional<List<ReportedIssueChapterDto>>(response);
        List<ReportedIssueChapterDto>? issues =
            await response.Content.ReadFromJsonAsync<List<ReportedIssueChapterDto>>();
        return issues == null
            ? new Optional<List<ReportedIssueChapterDto>>("Failed to read reported chapter issues from response.")
            : new Optional<List<ReportedIssueChapterDto>>(issues);
    }
    
    public static async Task<Optional<List<ReportedIssueChapterTypeDto>>> GetAllReportedChapterIssueTypesAsync()
    {
        HttpResponseMessage response = await ManaxApiClient.Client.GetAsync("/api/issue/chapter/reported/types");
        if (!response.IsSuccessStatusCode) return new Optional<List<ReportedIssueChapterTypeDto>>(response);
        List<ReportedIssueChapterTypeDto>? issues =
            await response.Content.ReadFromJsonAsync<List<ReportedIssueChapterTypeDto>>();
        return issues == null
            ? new Optional<List<ReportedIssueChapterTypeDto>>("Failed to read reported chapter issue type from response.")
            : new Optional<List<ReportedIssueChapterTypeDto>>(issues);
    }

    public static async Task<Optional<List<ReportedIssueSerieDto>>> GetAllReportedSerieIssuesAsync()
    {
        HttpResponseMessage response = await ManaxApiClient.Client.GetAsync("/api/issue/serie/reported");
        if (!response.IsSuccessStatusCode) return new Optional<List<ReportedIssueSerieDto>>(response);
        List<ReportedIssueSerieDto>? issues = await response.Content.ReadFromJsonAsync<List<ReportedIssueSerieDto>>();
        return issues == null
            ? new Optional<List<ReportedIssueSerieDto>>("Failed to read reported serie issues from response.")
            : new Optional<List<ReportedIssueSerieDto>>(issues);
    }
    
    public static async Task<Optional<List<ReportedIssueSerieTypeDto>>> GetAllReportedSerieIssueTypesAsync()
    {
        HttpResponseMessage response = await ManaxApiClient.Client.GetAsync("/api/issue/chapter/reported/types");
        if (!response.IsSuccessStatusCode) return new Optional<List<ReportedIssueSerieTypeDto>>(response);
        List<ReportedIssueSerieTypeDto>? issues =
            await response.Content.ReadFromJsonAsync<List<ReportedIssueSerieTypeDto>>();
        return issues == null
            ? new Optional<List<ReportedIssueSerieTypeDto>>("Failed to read reported chapter issue type from response.")
            : new Optional<List<ReportedIssueSerieTypeDto>>(issues);
    }

    public static async Task<Optional<bool>> CreateChapterIssueAsync(ReportedIssueChapterCreateDto reportedIssueChapter)
    {
        HttpResponseMessage response =
            await ManaxApiClient.Client.PostAsJsonAsync("/api/issue/chapter", reportedIssueChapter);
        return response.IsSuccessStatusCode
            ? new Optional<bool>(true)
            : new Optional<bool>(response);
    }

    public static async Task<Optional<bool>> CreateSerieIssueAsync(ReportedIssueSerieCreateDto reportedIssueSerie)
    {
        HttpResponseMessage response =
            await ManaxApiClient.Client.PostAsJsonAsync("/api/issue/serie", reportedIssueSerie);
        return response.IsSuccessStatusCode
            ? new Optional<bool>(true)
            : new Optional<bool>(response);
    }

    public static async Task<Optional<bool>> CloseChapterIssueAsync(long id)
    {
        HttpResponseMessage response = await ManaxApiClient.Client.PutAsync($"/api/issue/chapter/{id}/close", null);
        return response.IsSuccessStatusCode
            ? new Optional<bool>(true)
            : new Optional<bool>(response);
    }

    public static async Task<Optional<bool>> CloseSerieIssueAsync(long id)
    {
        HttpResponseMessage response = await ManaxApiClient.Client.PutAsync($"/api/issue/serie/{id}/close", null);
        return response.IsSuccessStatusCode
            ? new Optional<bool>(true)
            : new Optional<bool>(response);
    }
}