using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DynamicData;
using ManaxLibrary;
using ManaxLibrary.ApiCaller;
using ManaxLibrary.DTO.Issue.Automatic;
using ManaxLibrary.DTO.Issue.Reported;
using ManaxLibrary.Notifications;

namespace ManaxClient.Models.Issue;

public class IssueSource
{
    public static SourceCache<IssueChapterAutomatic,long> IssueChapterAutomatic  = new (issue => issue.Chapter.Id);
    public static SourceCache<IssueSerieAutomatic,long> IssueSerieAutomatic  = new (issue => issue.Serie.Id);
    public static SourceCache<IssueChapterReported,long> IssueChapterReported = new (issue => issue.Id);
    public static SourceCache<IssueSerieReported,long> IssueSerieReported = new (serie => serie.Id);
    
    public static EventHandler<string>? ErrorEmitted { get; set; }

    static IssueSource()
    {
        LoadData();
        ServerNotification.OnReportedChapterIssueCreated += OnReportedChapterIssueCreated;
        ServerNotification.OnReportedChapterIssueDeleted += OnReportedChapterIssueDeleted;
        ServerNotification.OnReportedSerieIssueCreated += OnReportedSerieIssueCreated;
        ServerNotification.OnReportedSerieIssueDeleted += OnReportedSerieIssueDeleted;
    }
    
    private static void OnReportedChapterIssueCreated(IssueChapterReportedDto issue)
    {
        IssueChapterReported.AddOrUpdate(new IssueChapterReported(issue));
    }

    private static void OnReportedChapterIssueDeleted(long issueId)
    {
        IssueChapterReported.RemoveKey(issueId);
    }

    private static void OnReportedSerieIssueCreated(IssueSerieReportedDto issue)
    {
        IssueSerieReported.AddOrUpdate(new IssueSerieReported(issue));
    }

    private static void OnReportedSerieIssueDeleted(long issueId)
    {
        IssueSerieReported.RemoveKey(issueId);
    }
    
    private static void LoadData()
    {
        Task.Run(async () =>
        {
            Optional<List<IssueSerieAutomaticDto>> allAutomaticSerieIssuesResponse =
                await ManaxApiIssueClient.GetAllAutomaticSerieIssuesAsync();
            if (allAutomaticSerieIssuesResponse.Failed)
                ErrorEmitted?.Invoke(null, allAutomaticSerieIssuesResponse.Error);
            else
            {
                IEnumerable<IssueSerieAutomatic> series = allAutomaticSerieIssuesResponse.GetValue()
                    .Select(s => new IssueSerieAutomatic(s));
                IssueSerieAutomatic.AddOrUpdate(series);
            }
                    

            Optional<List<IssueChapterAutomaticDto>> allAutomaticChapterIssuesResponse =
                await ManaxApiIssueClient.GetAllAutomaticChapterIssuesAsync();
            if (allAutomaticChapterIssuesResponse.Failed)
                ErrorEmitted?.Invoke(null, allAutomaticChapterIssuesResponse.Error);
            else
            {
                IEnumerable<IssueChapterAutomatic> series = allAutomaticChapterIssuesResponse.GetValue()
                    .Select(c => new IssueChapterAutomatic(c));
                IssueChapterAutomatic.AddOrUpdate(series);
            }

            Optional<List<IssueChapterReportedDto>> allReportedChapterIssuesResponse =
                await ManaxApiIssueClient.GetAllReportedChapterIssuesAsync();
            if (allReportedChapterIssuesResponse.Failed)
                ErrorEmitted?.Invoke(null, allReportedChapterIssuesResponse.Error);
            else
            {
                IEnumerable<IssueChapterReported> series = allReportedChapterIssuesResponse.GetValue()
                    .Select(c => new IssueChapterReported(c));
                IssueChapterReported.AddOrUpdate(series);
            }

            Optional<List<IssueSerieReportedDto>> allReportedSerieIssuesResponse =
                await ManaxApiIssueClient.GetAllReportedSerieIssuesAsync();
            if (allReportedSerieIssuesResponse.Failed)
                ErrorEmitted?.Invoke(null, allReportedSerieIssuesResponse.Error);
            else
            {
                IEnumerable<IssueSerieReported> series = allReportedSerieIssuesResponse.GetValue()
                    .Select(s => new IssueSerieReported(s));
                IssueSerieReported.AddOrUpdate(series);
            }
        });
    }
}