using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DynamicData;
using ManaxClient.Models.Issue;
using ManaxLibrary;
using ManaxLibrary.ApiCaller;
using ManaxLibrary.DTO.Issue.Reported;
using ManaxLibrary.Logging;

namespace ManaxClient.Models.Sources;

public static class ProblemSource
{
    public static readonly SourceCache<IssueChapterReportedType, long> ChapterProblems = new(x => x.Id);
    public static readonly SourceCache<IssueSerieReportedType, long> SerieProblems = new(x => x.Id);

    static ProblemSource()
    {
        LoadProblems();
    }

    public static EventHandler<string>? ErrorEmitted { get; set; }

    private static void LoadProblems()
    {
        Task.Run(() =>
        {
            Optional<List<IssueChapterReportedTypeDto>> chapterResponse =
                ManaxApiIssueClient.GetAllReportedChapterIssueTypesAsync().Result;
            if (chapterResponse.Failed)
            {
                Logger.LogFailure(chapterResponse.Error);
                ErrorEmitted?.Invoke(null, chapterResponse.Error);
                return;
            }

            ChapterProblems.AddOrUpdate(chapterResponse.GetValue()
                .Select(issue => new IssueChapterReportedType(issue)));

            Optional<List<IssueSerieReportedTypeDto>> serieResponse =
                ManaxApiIssueClient.GetAllReportedSerieIssueTypesAsync().Result;
            if (serieResponse.Failed)
            {
                Logger.LogFailure(serieResponse.Error);
                ErrorEmitted?.Invoke(null, serieResponse.Error);
                return;
            }

            SerieProblems.AddOrUpdate(serieResponse.GetValue().Select(issue => new IssueSerieReportedType(issue)));
        });
    }
}