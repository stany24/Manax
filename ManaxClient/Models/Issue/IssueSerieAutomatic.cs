using System;
using CommunityToolkit.Mvvm.ComponentModel;
using ManaxLibrary.DTO.Issue.Automatic;

namespace ManaxClient.Models.Issue;

public partial class IssueSerieAutomatic : ObservableObject
{
    [ObservableProperty] private DateTime _createdAt;
    [ObservableProperty] private long _serieId;

    [ObservableProperty] private IssueSerieAutomaticType _problem;

    public IssueSerieAutomatic(IssueSerieAutomaticDto dto)
    {
        FromDto(dto);
    }

    private void FromDto(IssueSerieAutomaticDto dto)
    {
        CreatedAt = dto.CreatedAt;
        SerieId = dto.SerieId;
        Problem = dto.Problem;
    }
}