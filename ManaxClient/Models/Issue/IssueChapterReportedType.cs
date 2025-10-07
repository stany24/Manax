// ReSharper disable UnusedAutoPropertyAccessor.Global

using CommunityToolkit.Mvvm.ComponentModel;
using ManaxLibrary.DTO.Issue.Reported;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

namespace ManaxClient.Models.Issue;

public class IssueChapterReportedType : ObservableObject
{
    public IssueChapterReportedType(IssueChapterReportedTypeDto dto)
    {
        FromDto(dto);
    }

    public long Id { get; set; }
    public string Name { get; set; }

    public void FromDto(IssueChapterReportedTypeDto dto)
    {
        Id = dto.Id;
        Name = dto.Name;
    }
}