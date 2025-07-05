using System.Net.Http.Json;
using ManaxLibrary.DTOs.Issue.Internal;
using ManaxLibrary.DTOs.Issue.User;

namespace ManaxLibrary.ApiCaller;

public static class ManaxApiIssueClient
{
    public static async Task<List<InternalChapterIssueDTO>?> GetAllInternalChapterIssuesAsync()
    {
        HttpResponseMessage response = await ManaxApiClient.Client.GetAsync("/api/issue/chapter/internal");
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<List<InternalChapterIssueDTO>>();
    }
    
    public static async Task<List<InternalSerieIssueDTO>?> GetAllInternalSerieIssuesAsync()
    {
        HttpResponseMessage response = await ManaxApiClient.Client.GetAsync("/api/issue/serie/internal");
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<List<InternalSerieIssueDTO>>();
    }
    
    public static async Task<List<UserChapterIssueDTO>?> GetAllUserChapterIssuesAsync()
    {
        HttpResponseMessage response = await ManaxApiClient.Client.GetAsync("/api/issue/chapter/user");
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<List<UserChapterIssueDTO>>();
    }
    
    public static async Task<List<UserSerieIssueDTO>?> GetAllUserSerieIssuesAsync()
    {
        HttpResponseMessage response = await ManaxApiClient.Client.GetAsync("/api/issue/serie/user");
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<List<UserSerieIssueDTO>>();
    }
    
    public static async Task<bool> CreateChapterIssueAsync(ChapterIssueCreateDTO chapterIssue)
    {
        HttpResponseMessage response = await ManaxApiClient.Client.PostAsJsonAsync("/api/issue/chapter", chapterIssue);
        return response.IsSuccessStatusCode;
    }
    
    public static async Task<bool> CreateSerieIssueAsync(SerieIssueCreateDTO serieIssue)
    {
        HttpResponseMessage response = await ManaxApiClient.Client.PostAsJsonAsync("/api/issue/serie", serieIssue);
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
