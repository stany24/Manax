using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DynamicData;
using ManaxClient.Models.Issue;
using ManaxClient.ViewModels;
using ManaxLibrary;
using ManaxLibrary.ApiCaller;
using ManaxLibrary.DTO.Feature;
using ManaxLibrary.DTO.Issue.Automatic;
using ManaxLibrary.DTO.Issue.Reported;
using ManaxLibrary.Logging;
using ManaxLibrary.Notifications;

namespace ManaxClient.Models.Server.Sources;

public static class IssueSource
{
    public static readonly SourceCache<IssueChapterAutomatic, long> IssueChapterAutomatic =
        new(issue => issue.Chapter.Id);

    public static readonly SourceCache<IssueSerieAutomatic, long> IssueSerieAutomatic = new(issue => issue.Serie.Id);
    public static readonly SourceCache<IssueChapterReported, long> IssueChapterReported = new(issue => issue.Id);
    public static readonly SourceCache<IssueSerieReported, long> IssueSerieReported = new(serie => serie.Id);

    private static readonly Lock IssueLock = new();

    static IssueSource()
    {
        MainWindowViewModel.FeatureChanged += (_, features) =>
        {
            if (features is { Key: FeatureType.AutomaticIssues, Value: true })
            {
                LoadAutomaticChapterIssues();
                LoadAutomaticSerieIssues();
            }
            else
            {
                IssueChapterAutomatic.Clear();
                IssueSerieAutomatic.Clear();
            }

            if (features is { Key: FeatureType.ReportedIssues, Value: true })
            {
                LoadReportedChapterIssues();
                LoadReportedSerieIssues();
            }
            else
            {
                IssueChapterReported.Clear();
                IssueSerieReported.Clear();
            }
        };
        NotificationReceiver.OnReportedChapterIssueCreated += OnReportedChapterIssueCreated;
        NotificationReceiver.OnReportedChapterIssueDeleted += OnReportedChapterIssueDeleted;
        NotificationReceiver.OnReportedSerieIssueCreated += OnReportedSerieIssueCreated;
        NotificationReceiver.OnReportedSerieIssueDeleted += OnReportedSerieIssueDeleted;
    }

    public static EventHandler<string>? ErrorEmitted { get; set; }

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

    private static void LoadAutomaticChapterIssues()
    {
        Task.Run(async () =>
        {
            try
            {
                Optional<List<IssueChapterAutomaticDto>> responseIssueChapterAutomatic =
                    await ManaxApiIssueClient.GetAllAutomaticChapterIssuesAsync();
                if (responseIssueChapterAutomatic.Failed)
                {
                    ErrorEmitted?.Invoke(null, responseIssueChapterAutomatic.Error);
                }
                else
                {
                    IEnumerable<IssueChapterAutomatic> series = responseIssueChapterAutomatic.GetValue()
                        .Select(c => new IssueChapterAutomatic(c));
                    lock (IssueLock)
                    {
                        IssueChapterAutomatic.AddOrUpdate(series);
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

    private static void LoadAutomaticSerieIssues()
    {
        Task.Run(async () =>
        {
            try
            {
                Optional<List<IssueSerieAutomaticDto>> responseIssueSerieAutomatic =
                    await ManaxApiIssueClient.GetAllAutomaticSerieIssuesAsync();
                if (responseIssueSerieAutomatic.Failed)
                {
                    ErrorEmitted?.Invoke(null, responseIssueSerieAutomatic.Error);
                }
                else
                {
                    IEnumerable<IssueSerieAutomatic> series = responseIssueSerieAutomatic.GetValue()
                        .Select(s => new IssueSerieAutomatic(s));
                    lock (IssueLock)
                    {
                        IssueSerieAutomatic.AddOrUpdate(series);
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

    private static void LoadReportedChapterIssues()
    {
        Task.Run(async () =>
        {
            try
            {
                Optional<List<IssueChapterReportedDto>> responseIssueChapterReported =
                    await ManaxApiIssueClient.GetAllReportedChapterIssuesAsync();
                if (responseIssueChapterReported.Failed)
                {
                    ErrorEmitted?.Invoke(null, responseIssueChapterReported.Error);
                }
                else
                {
                    IEnumerable<IssueChapterReported> series = responseIssueChapterReported.GetValue()
                        .Select(c => new IssueChapterReported(c));
                    lock (IssueLock)
                    {
                        IssueChapterReported.AddOrUpdate(series);
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

    private static void LoadReportedSerieIssues()
    {
        Task.Run(async () =>
        {
            try
            {
                Optional<List<IssueSerieReportedDto>> responseIssueSerieReported =
                    await ManaxApiIssueClient.GetAllReportedSerieIssuesAsync();
                if (responseIssueSerieReported.Failed)
                {
                    ErrorEmitted?.Invoke(null, responseIssueSerieReported.Error);
                }
                else
                {
                    IEnumerable<IssueSerieReported> series = responseIssueSerieReported.GetValue()
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