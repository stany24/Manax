// ReSharper disable InconsistentNaming
// ReSharper disable PropertyCanBeMadeInitOnly.Global

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

namespace ManaxLibrary.DTO.Chapter;

public class ChapterDto
{
    public long Id { get; set; }
    public long SerieId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public int Number { get; set; }
    public int PageNumber { get; set; }

    public DateTime Creation { get; set; }
    public DateTime LastModification { get; set; }
}