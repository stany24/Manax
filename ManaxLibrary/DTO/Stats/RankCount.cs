using ManaxLibrary.DTO.Rank;

namespace ManaxLibrary.DTO.Stats;

public class RankCount
{
    public RankDto Rank { get; init; } = null!;
    public int Count { get; init; }
}