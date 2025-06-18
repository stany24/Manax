// ReSharper disable PropertyCanBeMadeInitOnly.Global
// ReSharper disable PropertyCanBeMadeInitOnly.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace ManaxApi.Models.Chapter;

public class Chapter
{
    public long Id { get; set; }
    public string Name { get; set; }
    public int Number { get; set; }
    public string Path { get; set; } = string.Empty;
}