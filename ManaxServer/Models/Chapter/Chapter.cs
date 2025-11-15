// ReSharper disable PropertyCanBeMadeInitOnly.Global
// ReSharper disable PropertyCanBeMadeInitOnly.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable UnusedMember.Global

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ManaxLibrary.DTO.Chapter;

namespace ManaxServer.Models.Chapter;

public class Chapter
{
    public long Id { get; set; }
    public long SerieId { get; set; }

    [ForeignKey(nameof(SerieId))] public Serie.Serie Serie { get; set; } = null!;

    [MaxLength(255)]public string FileName { get; set; } = string.Empty;
    public int Number { get; set; }
    public int PageNumber { get; set; }
    [MaxLength(4096)]public string Path { get; set; } = string.Empty;
    public DateTime Creation { get; set; }
    public DateTime LastModification { get; set; }

    public ChapterDto ToDto()
    {
        return new ChapterDto
        {
            Id = Id,
            SerieId = SerieId,
            FileName = FileName,
            Number = Number,
            PageNumber = PageNumber,
            Creation = Creation,
            LastModification = LastModification
        };
    }
}