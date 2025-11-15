using System.ComponentModel.DataAnnotations;
using ManaxLibrary.DTO.Serie;

namespace ManaxLibrary.DTO.Search;

public class Search
{
    [Required] public List<long> IncludedLibraries { get; init; } = [];
    [Required] public List<long> ExcludedLibraries { get; init; } = [];

    [Required] public List<Status> IncludedStatuses { get; init; } =
        [Status.Cancelled, Status.Ongoing, Status.Completed, Status.Hiatus];

    [Required] public List<Status> ExcludedStatuses { get; init; } = [];
    [Required] public string RegexSearch { get; init; } = @"[\s\S]*";
    [Required] public int MinChapters { get; init; } = 0;
    [Required] public int MaxChapters { get; init; } = int.MaxValue;
}