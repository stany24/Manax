namespace ManaxLibrary.DTO.Stats;

public class UserStats
{
    public long SeriesTotal { get; set; }
    public long SeriesCompleted { get; set; }
    public long SeriesInProgress { get; set; }

    public long ChaptersTotal { get; set; }
    public long ChaptersRead { get; set; }
    public long ChaptersRemaining { get; set; }
    
    public TimeSpan ReadingTime { get; set; }

    public List<RankCount> Ranks { get; set; } = [];
}