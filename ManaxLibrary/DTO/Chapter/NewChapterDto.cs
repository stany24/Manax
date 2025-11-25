using System.ComponentModel.DataAnnotations;

namespace ManaxLibrary.DTO.Chapter;

public class NewChapterDto
{
    public int Number { get; init; }
    [Required] public byte[] Data { get; init; } = [];
    public int SerieId { get; init; }
}