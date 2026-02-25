using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Messaging;
using DynamicData;
using ManaxClient.Event;
using ManaxClient.Manager;
using ManaxClient.Models.Issue;
using ManaxLibrary;
using ManaxLibrary.ApiCaller;
using ManaxLibrary.DTO.Feature;
using ManaxLibrary.DTO.Issue.Automatic;
using ManaxLibrary.DTO.Issue.Reported;
using ManaxLibrary.Logging;
using ManaxLibrary.Notifications;

namespace ManaxClient.Models.Server.Sources;

public class IssueSource
{
    private readonly Lock _issueLock = new();

    public readonly SourceCache<IssueChapterAutomatic, long> IssueChapterAutomatic =
        new(issue => issue.Chapter.Id);

    public readonly SourceCache<IssueChapterReported, long> IssueChapterReported = new(issue => issue.Id);

    public readonly SourceCache<IssueSerieAutomatic, long> IssueSerieAutomatic = new(issue => issue.Serie.Id);
    public readonly SourceCache<IssueSerieReported, long> IssueSerieReported = new(serie => serie.Id);

    public IssueSource()
    {
        FeatureManager.FeatureChanged += (_, features) =>
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

    private void OnReportedChapterIssueCreated(IssueChapterReportedDto issue)
    {
        lock (_issueLock)
        {
            IssueChapterReported.AddOrUpdate(new IssueChapterReported(issue));
        }
    }

    private void OnReportedChapterIssueDeleted(long issueId)
    {
        lock (_issueLock)
        {
            IssueChapterReported.RemoveKey(issueId);
        }
    }

    private void OnReportedSerieIssueCreated(IssueSerieReportedDto issue)
    {
        lock (_issueLock)
        {
            IssueSerieReported.AddOrUpdate(new IssueSerieReported(issue));
        }
    }

    private void OnReportedSerieIssueDeleted(long issueId)
    {
        lock (_issueLock)
        {
            IssueSerieReported.RemoveKey(issueId);
        }
    }

    private void LoadAutomaticChapterIssues()
    {
        Task.Run(async () =>
        {
            try
            {
                Optional<List<IssueChapterAutomaticDto>> responseIssueChapterAutomatic =
                    await ManaxApiIssueClient.GetAllAutomaticChapterIssuesAsync();
                if (responseIssueChapterAutomatic.Failed)
                {
                    WeakReferenceMessenger.Default.Send(new NotificationMessage(responseIssueChapterAutomatic.Error));
                }
                else
                {
                    IEnumerable<IssueChapterAutomatic> series = responseIssueChapterAutomatic.GetValue()
                        .Select(c => new IssueChapterAutomatic(c));
                    lock (_issueLock)
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

    private void LoadAutomaticSerieIssues()
    {
        Task.Run(async () =>
        {
            try
            {
                Optional<List<IssueSerieAutomaticDto>> responseIssueSerieAutomatic =
                    await ManaxApiIssueClient.GetAllAutomaticSerieIssuesAsync();
                if (responseIssueSerieAutomatic.Failed)
                {
                    WeakReferenceMessenger.Default.Send(new NotificationMessage(responseIssueSerieAutomatic.Error));
                }
                else
                {
                    IEnumerable<IssueSerieAutomatic> series = responseIssueSerieAutomatic.GetValue()
                        .Select(s => new IssueSerieAutomatic(s));
                    lock (_issueLock)
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

    private void LoadReportedChapterIssues()
    {
        Task.Run(async () =>
        {
            try
            {
                Optional<List<IssueChapterReportedDto>> responseIssueChapterReported =
                    await ManaxApiIssueClient.GetAllReportedChapterIssuesAsync();
                if (responseIssueChapterReported.Failed)
                {
                    WeakReferenceMessenger.Default.Send(new NotificationMessage(responseIssueChapterReported.Error));
                }
                else
                {
                    IEnumerable<IssueChapterReported> series = responseIssueChapterReported.GetValue()
                        .Select(c => new IssueChapterReported(c));
                    lock (_issueLock)
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

    private void LoadReportedSerieIssues()
    {
        Task.Run(async () =>
        {
            try
            {
                Optional<List<IssueSerieReportedDto>> responseIssueSerieReported =
                    await ManaxApiIssueClient.GetAllReportedSerieIssuesAsync();
                if (responseIssueSerieReported.Failed)
                {
                    WeakReferenceMessenger.Default.Send(new NotificationMessage(responseIssueSerieReported.Error));
                }
                else
                {
                    IEnumerable<IssueSerieReported> series = responseIssueSerieReported.GetValue()
                        .Select(s => new IssueSerieReported(s));
                    lock (_issueLock)
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