// ReSharper disable PropertyCanBeMadeInitOnly.Global
// ReSharper disable PropertyCanBeMadeInitOnly.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable UnusedMember.Global

using System.ComponentModel.DataAnnotations.Schema;

namespace ManaxServer.Models.Chapter;

public class Chapter
{
    public long Id { get; set; }
    public long SerieId { get; set; }

    [ForeignKey(nameof(SerieId))] public Serie.Serie Serie { get; set; } = null!;

    public string FileName { get; set; } = string.Empty;
    public int Number { get; set; }
    public int Pages { get; set; }
    public string Path { get; set; } = string.Empty;
}