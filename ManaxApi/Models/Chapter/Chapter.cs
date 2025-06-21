// ReSharper disable PropertyCanBeMadeInitOnly.Global
// ReSharper disable PropertyCanBeMadeInitOnly.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable UnusedMember.Global

namespace ManaxApi.Models.Chapter;

public class Chapter
{
    public long Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public int Number { get; set; }
    public int Pages { get; set; }
    public string Path { get; set; } = string.Empty;
}