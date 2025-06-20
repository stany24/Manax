// ReSharper disable PropertyCanBeMadeInitOnly.Global
// ReSharper disable PropertyCanBeMadeInitOnly.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace ManaxApi.Models.Serie;

public class Serie
{
    public long Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<Chapter.Chapter> Chapters { get; set; } = [];

    public SerieInfo GetInfo()
    {
        return new SerieInfo
        {
            Title = Title,
            Description = Description
        };
    }
}