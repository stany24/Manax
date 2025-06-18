// ReSharper disable PropertyCanBeMadeInitOnly.Global
// ReSharper disable PropertyCanBeMadeInitOnly.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace ManaxApi.Models.Serie;

public class Serie
{
    public long Id { get; set; }
    public SerieInfo Infos { get; set; } = new();
    public List<Chapter.Chapter> Chapters { get; set; }
}