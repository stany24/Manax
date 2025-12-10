using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Messaging;
using DynamicData;
using ManaxClient.Event;
using ManaxClient.Models.Issue;
using ManaxLibrary;
using ManaxLibrary.ApiCaller;
using ManaxLibrary.DTO.Issue.Reported;
using ManaxLibrary.Logging;

namespace ManaxClient.Models.Server.Sources;

public class ProblemSource
{
    public readonly SourceCache<IssueChapterReportedType, long> ChapterProblems = new(x => x.Id);
    public readonly SourceCache<IssueSerieReportedType, long> SerieProblems = new(x => x.Id);

    public ProblemSource()
    {
        LoadProblems();
    }

    private void LoadProblems()
    {
        Task.Run(async () =>
        {
            Optional<List<IssueChapterReportedTypeDto>> chapterResponse = 
                await ManaxApiIssueClient.GetAllReportedChapterIssueTypesAsync();
            if (chapterResponse.Failed)
            {
                Logger.LogFailure(chapterResponse.Error);
                WeakReferenceMessenger.Default.Send(new NotificationMessage(chapterResponse.Error));
                return;
            }

            ChapterProblems.AddOrUpdate(chapterResponse.GetValue()
                .Select(issue => new IssueChapterReportedType(issue)));

            Optional<List<IssueSerieReportedTypeDto>> serieResponse =
                await ManaxApiIssueClient.GetAllReportedSerieIssueTypesAsync();
            if (serieResponse.Failed)
            {
                Logger.LogFailure(serieResponse.Error);
                WeakReferenceMessenger.Default.Send(new NotificationMessage(serieResponse.Error));
                return;
            }

            SerieProblems.AddOrUpdate(serieResponse.GetValue().Select(issue => new IssueSerieReportedType(issue)));
        });
    }
}