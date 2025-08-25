// ReSharper disable PropertyCanBeMadeInitOnly.Global

using ManaxLibrary.DTO.Serie;

namespace ManaxLibrary.DTO.Stats;

public class ServerStats
{
    public long DiskSize { get; set; }
    public long AvailableDiskSize { get; set; }
    public int Series { get; set; }
    public Dictionary<string, int> SeriesInLibraries { get; set; } = [];
    public List<SerieDto> NeverReadSeries { get; set; } = [];
    public int Chapters { get; set; }
    public int Users { get; set; }
    public int ActiveUsers { get; set; }
    public int InactiveUsers => Users - ActiveUsers;
}