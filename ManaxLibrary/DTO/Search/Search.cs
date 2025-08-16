// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global
// ReSharper disable PropertyCanBeMadeInitOnly.Global

using ManaxLibrary.DTO.Serie;

namespace ManaxLibrary.DTO.Search;

public class Search
{
    public List<long> IncludedLibraries { get; set; } = [];
    public List<long> ExcludedLibraries { get; set; } = [];

    public List<Status> IncludedStatuses { get; set; } =
        [Status.Cancelled, Status.Ongoing, Status.Completed, Status.Hiatus];

    public List<Status> ExcludedStatuses { get; set; } = [];
    public string RegexSearch { get; set; } = @"[\s\S]*";
    public int MinChapters { get; set; } = 0;
    public int MaxChapters { get; set; } = int.MaxValue;
}