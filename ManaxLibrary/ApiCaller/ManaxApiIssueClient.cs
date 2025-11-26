using ManaxLibrary.DTO.Issue.Automatic;
using ManaxLibrary.DTO.Issue.Reported;

namespace ManaxLibrary.ApiCaller;

public static class ManaxApiIssueClient
{
    public static async Task<Optional<List<IssueChapterAutomaticDto>>> GetAllAutomaticChapterIssuesAsync()
    {
        return await ManaxApiClient.GetAsync<List<IssueChapterAutomaticDto>>("api/issue/chapter/automatic");
    }

    public static async Task<Optional<List<IssueSerieAutomaticDto>>> GetAllAutomaticSerieIssuesAsync()
    {
        return await ManaxApiClient.GetAsync<List<IssueSerieAutomaticDto>>("api/issue/serie/automatic");
    }

    public static async Task<Optional<List<IssueChapterReportedDto>>> GetAllReportedChapterIssuesAsync()
    {
        return await ManaxApiClient.GetAsync<List<IssueChapterReportedDto>>("api/issue/chapter/reported");
    }

    public static async Task<Optional<List<IssueChapterReportedTypeDto>>> GetAllReportedChapterIssueTypesAsync()
    {
        return await ManaxApiClient.GetAsync<List<IssueChapterReportedTypeDto>>("api/issue/chapter/reported/types");
    }

    public static async Task<Optional<List<IssueSerieReportedDto>>> GetAllReportedSerieIssuesAsync()
    {
        return await ManaxApiClient.GetAsync<List<IssueSerieReportedDto>>("api/issue/serie/reported");
    }

    public static async Task<Optional<List<IssueSerieReportedTypeDto>>> GetAllReportedSerieIssueTypesAsync()
    {
        return await ManaxApiClient.GetAsync<List<IssueSerieReportedTypeDto>>("api/issue/serie/reported/types");
    }

    public static async Task<Optional<bool>> CreateChapterIssueAsync(IssueChapterReportedCreateDto issueChapterReported)
    {
        return await ManaxApiClient.PostSuccessAsync("api/issue/chapter", issueChapterReported);
    }

    public static async Task<Optional<bool>> CreateSerieIssueAsync(IssueSerieReportedCreateDto issueSerieReported)
    {
        return await ManaxApiClient.PostSuccessAsync("api/issue/serie", issueSerieReported);
    }

    public static async Task<Optional<bool>> CloseChapterIssueAsync(long id)
    {
        return await ManaxApiClient.PutSuccessAsync($"api/issue/chapter/{id}/close");
    }

    public static async Task<Optional<bool>> CloseSerieIssueAsync(long id)
    {
        return await ManaxApiClient.PutSuccessAsync($"api/issue/serie/{id}/close");
    }
}