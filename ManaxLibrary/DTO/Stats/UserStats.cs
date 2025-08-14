using ManaxLibrary.DTO.Rank;

namespace ManaxLibrary.DTO.Stats;

public class UserStats
{
    public long SeriesTotal { get; set; }
    public long SeriesRead { get; set; }
    
    public long ChaptersTotal { get; set; }
    public long ChaptersRead { get; set; }
    
    public Dictionary<RankDto,int> Ranks { get; set; } = new Dictionary<RankDto, int>();
}