using System.Net.Http.Json;
using ManaxLibrary.DTOs.Issue.Automatic;
using ManaxLibrary.DTOs.Issue.Reported;

namespace ManaxLibrary.ApiCaller;

public static class ManaxApiIssueClient
{
    public static async Task<List<AutomaticIssueChapterDTO>?> GetAllAutomaticChapterIssuesAsync()
    {
        HttpResponseMessage response = await ManaxApiClient.Client.GetAsync("/api/issue/chapter/internal");
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<List<AutomaticIssueChapterDTO>>();
    }
    
    public static async Task<List<AutomaticIssueSerieDTO>?> GetAllAutomaticSerieIssuesAsync()
    {
        HttpResponseMessage response = await ManaxApiClient.Client.GetAsync("/api/issue/serie/internal");
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<List<AutomaticIssueSerieDTO>>();
    }
    
    public static async Task<List<ReportedIssueChapterDTO>?> GetAllReportedChapterIssuesAsync()
    {
        HttpResponseMessage response = await ManaxApiClient.Client.GetAsync("/api/issue/chapter/user");
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<List<ReportedIssueChapterDTO>>();
    }
    
    public static async Task<List<ReportedIssueSerieDTO>?> GetAllReportedSerieIssuesAsync()
    {
        HttpResponseMessage response = await ManaxApiClient.Client.GetAsync("/api/issue/serie/user");
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<List<ReportedIssueSerieDTO>>();
    }
    
    public static async Task<bool> CreateChapterIssueAsync(ReportedIssueChapterCreateDTO reportedIssueChapter)
    {
        HttpResponseMessage response = await ManaxApiClient.Client.PostAsJsonAsync("/api/issue/chapter", reportedIssueChapter);
        return response.IsSuccessStatusCode;
    }
    
    public static async Task<bool> CreateSerieIssueAsync(ReportedIssueSerieCreateDTO reportedIssueSerie)
    {
        HttpResponseMessage response = await ManaxApiClient.Client.PostAsJsonAsync("/api/issue/serie", reportedIssueSerie);
        return response.IsSuccessStatusCode;
    }
    
    public static async Task<bool> CloseChapterIssueAsync(long id)
    {
        HttpResponseMessage response = await ManaxApiClient.Client.PutAsync($"/api/issue/chapter/{id}/close", null);
        return response.IsSuccessStatusCode;
    }
    
    public static async Task<bool> CloseSerieIssueAsync(long id)
    {
        HttpResponseMessage response = await ManaxApiClient.Client.PutAsync($"/api/issue/serie/{id}/close", null);
        return response.IsSuccessStatusCode;
    }
}
