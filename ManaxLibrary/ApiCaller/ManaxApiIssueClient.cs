using System.Net.Http.Json;
using ManaxLibrary.DTOs.Issue.Automatic;
using ManaxLibrary.DTOs.Issue.Reported;

namespace ManaxLibrary.ApiCaller;

public static class ManaxApiIssueClient
{
    public static async Task<Optional<List<AutomaticIssueChapterDTO>>> GetAllAutomaticChapterIssuesAsync()
    {
        HttpResponseMessage response = await ManaxApiClient.Client.GetAsync("/api/issue/chapter/internal");
        if (!response.IsSuccessStatusCode) return new Optional<List<AutomaticIssueChapterDTO>>(response);
        List<AutomaticIssueChapterDTO>? issues = await response.Content.ReadFromJsonAsync<List<AutomaticIssueChapterDTO>>();
        return issues == null
            ? new Optional<List<AutomaticIssueChapterDTO>>("Failed to read automatic chapter issues from response.")
            : new Optional<List<AutomaticIssueChapterDTO>>(issues);
    }
    
    public static async Task<Optional<List<AutomaticIssueSerieDTO>>> GetAllAutomaticSerieIssuesAsync()
    {
        HttpResponseMessage response = await ManaxApiClient.Client.GetAsync("/api/issue/serie/internal");
        if (!response.IsSuccessStatusCode) return new Optional<List<AutomaticIssueSerieDTO>>(response);
        List<AutomaticIssueSerieDTO>? issues = await response.Content.ReadFromJsonAsync<List<AutomaticIssueSerieDTO>>();
        return issues == null
            ? new Optional<List<AutomaticIssueSerieDTO>>("Failed to read automatic serie issues from response.")
            : new Optional<List<AutomaticIssueSerieDTO>>(issues);
    }
    
    public static async Task<Optional<List<ReportedIssueChapterDTO>>> GetAllReportedChapterIssuesAsync()
    {
        HttpResponseMessage response = await ManaxApiClient.Client.GetAsync("/api/issue/chapter/user");
        if (!response.IsSuccessStatusCode) return new Optional<List<ReportedIssueChapterDTO>>(response);
        List<ReportedIssueChapterDTO>? issues = await response.Content.ReadFromJsonAsync<List<ReportedIssueChapterDTO>>();
        return issues == null
            ? new Optional<List<ReportedIssueChapterDTO>>("Failed to read reported chapter issues from response.")
            : new Optional<List<ReportedIssueChapterDTO>>(issues);
    }
    
    public static async Task<Optional<List<ReportedIssueSerieDTO>>> GetAllReportedSerieIssuesAsync()
    {
        HttpResponseMessage response = await ManaxApiClient.Client.GetAsync("/api/issue/serie/user");
        if (!response.IsSuccessStatusCode) return new Optional<List<ReportedIssueSerieDTO>>(response);
        List<ReportedIssueSerieDTO>? issues = await response.Content.ReadFromJsonAsync<List<ReportedIssueSerieDTO>>();
        return issues == null
            ? new Optional<List<ReportedIssueSerieDTO>>("Failed to read reported serie issues from response.")
            : new Optional<List<ReportedIssueSerieDTO>>(issues);
    }
    
    public static async Task<Optional<bool>> CreateChapterIssueAsync(ReportedIssueChapterCreateDTO reportedIssueChapter)
    {
        HttpResponseMessage response = await ManaxApiClient.Client.PostAsJsonAsync("/api/issue/chapter", reportedIssueChapter);
        return response.IsSuccessStatusCode
            ? new Optional<bool>(true)
            : new Optional<bool>(response);
    }
    
    public static async Task<Optional<bool>> CreateSerieIssueAsync(ReportedIssueSerieCreateDTO reportedIssueSerie)
    {
        HttpResponseMessage response = await ManaxApiClient.Client.PostAsJsonAsync("/api/issue/serie", reportedIssueSerie);
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
