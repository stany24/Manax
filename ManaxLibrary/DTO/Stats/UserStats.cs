// ReSharper disable PropertyCanBeMadeInitOnly.Global

using ManaxLibrary.DTO.Read;

namespace ManaxLibrary.DTO.Stats;

public class UserStats
{
    public long SeriesTotal { get; set; }
    public long SeriesCompleted { get; set; }
    public long SeriesInProgress { get; set; }

    public long SeriesRemaining => SeriesTotal - SeriesCompleted - SeriesInProgress;

    public long ChaptersTotal { get; set; }
    public long ChaptersRead { get; set; }
    public long ChaptersRemaining => ChaptersTotal - ChaptersRead;

    public TimeSpan ReadingTime { get; set; }

    public List<RankCount> Ranks { get; set; } = [];
    public List<ReadDto> Reads { get; set; } = [];
}