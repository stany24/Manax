using ManaxLibrary.DTO.Read;

namespace ManaxLibrary.DTO.Stats;

public class UserStats
{
    public long SeriesTotal { get; init; }
    public long SeriesCompleted { get; init; }
    public long SeriesInProgress { get; init; }

    public long SeriesRemaining => SeriesTotal - SeriesCompleted - SeriesInProgress;

    public long ChaptersTotal { get; init; }
    public long ChaptersRead { get; init; }
    public long ChaptersRemaining => ChaptersTotal - ChaptersRead;

    public TimeSpan ReadingTime { get; init; }

    public List<RankCount> Ranks { get; init; } = [];
    public List<ReadDto> Reads { get; init; } = [];
}