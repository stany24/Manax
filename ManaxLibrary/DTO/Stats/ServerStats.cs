using ManaxLibrary.DTO.Serie;

namespace ManaxLibrary.DTO.Stats;

public class ServerStats
{
    public long DiskSize { get; init; }
    public long AvailableDiskSize { get; init; }
    public int Series { get; init; }
    public Dictionary<string, int> SeriesInLibraries { get; init; } = [];
    public List<SerieDto> NeverReadSeries { get; init; } = [];
    public int Chapters { get; init; }
    public int Users { get; init; }
    public int ActiveUsers { get; init; }
    public int InactiveUsers => Users - ActiveUsers;
}