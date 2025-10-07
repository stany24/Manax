using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DynamicData;
using ManaxLibrary;
using ManaxLibrary.ApiCaller;
using ManaxLibrary.DTO.Issue.Automatic;
using ManaxLibrary.DTO.Issue.Reported;
using ManaxLibrary.Logging;
using ManaxLibrary.Notifications;

namespace ManaxClient.Models.Issue;

public class IssueSource
{
    public static readonly SourceCache<IssueChapterAutomatic,long> IssueChapterAutomatic  = new (issue => issue.Chapter.Id);
    public static readonly SourceCache<IssueSerieAutomatic,long> IssueSerieAutomatic  = new (issue => issue.Serie.Id);
    public static readonly SourceCache<IssueChapterReported,long> IssueChapterReported = new (issue => issue.Id);
    public static readonly SourceCache<IssueSerieReported,long> IssueSerieReported = new (serie => serie.Id);
    
    private static readonly object IssueLock = new ();
    
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
        lock (IssueLock)
        {
            IssueChapterReported.AddOrUpdate(new IssueChapterReported(issue));
        }
    }

    private static void OnReportedChapterIssueDeleted(long issueId)
    {
        lock (IssueLock)
        {
            IssueChapterReported.RemoveKey(issueId);
        }
    }

    private static void OnReportedSerieIssueCreated(IssueSerieReportedDto issue)
    {
        lock (IssueLock)
        {
            IssueSerieReported.AddOrUpdate(new IssueSerieReported(issue));
        }
    }

    private static void OnReportedSerieIssueDeleted(long issueId)
    {
        lock (IssueLock)
        {
            IssueSerieReported.RemoveKey(issueId);
        }
    }
    
    private static void LoadData()
    {
        Task.Run(() =>
        {
            try
            {
                Optional<List<IssueSerieAutomaticDto>> allAutomaticSerieIssuesResponse =
                    ManaxApiIssueClient.GetAllAutomaticSerieIssuesAsync().Result;
                if (allAutomaticSerieIssuesResponse.Failed)
                    ErrorEmitted?.Invoke(null, allAutomaticSerieIssuesResponse.Error);
                else
                {
                    IEnumerable<IssueSerieAutomatic> series = allAutomaticSerieIssuesResponse.GetValue()
                        .Select(s => new IssueSerieAutomatic(s));
                    lock (IssueLock)
                    {
                        IssueSerieAutomatic.AddOrUpdate(series);
                    }
                }

                Optional<List<IssueChapterAutomaticDto>> allAutomaticChapterIssuesResponse =
                    ManaxApiIssueClient.GetAllAutomaticChapterIssuesAsync().Result;
                if (allAutomaticChapterIssuesResponse.Failed)
                    ErrorEmitted?.Invoke(null, allAutomaticChapterIssuesResponse.Error);
                else
                {
                    IEnumerable<IssueChapterAutomatic> series = allAutomaticChapterIssuesResponse.GetValue()
                        .Select(c => new IssueChapterAutomatic(c));
                    lock (IssueLock)
                    {
                        IssueChapterAutomatic.AddOrUpdate(series);
                    }
                }

                Optional<List<IssueChapterReportedDto>> allReportedChapterIssuesResponse =
                    ManaxApiIssueClient.GetAllReportedChapterIssuesAsync().Result;
                if (allReportedChapterIssuesResponse.Failed)
                    ErrorEmitted?.Invoke(null, allReportedChapterIssuesResponse.Error);
                else
                {
                    IEnumerable<IssueChapterReported> series = allReportedChapterIssuesResponse.GetValue()
                        .Select(c => new IssueChapterReported(c));
                    lock (IssueLock)
                    {
                        IssueChapterReported.AddOrUpdate(series);
                    }
                }

                Optional<List<IssueSerieReportedDto>> allReportedSerieIssuesResponse =
                    ManaxApiIssueClient.GetAllReportedSerieIssuesAsync().Result;
                if (allReportedSerieIssuesResponse.Failed)
                    ErrorEmitted?.Invoke(null, allReportedSerieIssuesResponse.Error);
                else
                {
                    IEnumerable<IssueSerieReported> series = allReportedSerieIssuesResponse.GetValue()
                        .Select(s => new IssueSerieReported(s));
                    lock (IssueLock)
                    {
                        IssueSerieReported.AddOrUpdate(series);
                    }
                }
            }
            catch (Exception e)
            {
                Logger.LogError("Failed to load issues from API", e);
                throw;
            }
        });
    }
}