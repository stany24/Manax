using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using ManaxLibrary;
using ManaxLibrary.ApiCaller;
using ManaxLibrary.DTO.Issue.Automatic;
using ManaxLibrary.DTO.Serie;

namespace ManaxClient.Models.Issue;

public class ClientAutomaticIssueSerie : ObservableObject
{
    private AutomaticIssueSerieDto _issue;

    private SerieDto? _serie;

    public ClientAutomaticIssueSerie(AutomaticIssueSerieDto issue)
    {
        _issue = issue;
        Task.Run(async () =>
        {
            Optional<SerieDto> serieInfo = await ManaxApiSerieClient.GetSerieInfoAsync(issue.SerieId);
            if (!serieInfo.Failed) Serie = serieInfo.GetValue();
        });
    }

    public AutomaticIssueSerieDto Issue
    {
        get => _issue;
        set
        {
            _issue = value;
            OnPropertyChanged();
        }
    }

    public SerieDto? Serie
    {
        get => _serie;
        set
        {
            _serie = value;
            OnPropertyChanged();
        }
    }
}