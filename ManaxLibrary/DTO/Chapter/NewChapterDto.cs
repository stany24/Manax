using System.ComponentModel.DataAnnotations;

namespace ManaxLibrary.DTO.Chapter;

public class NewChapterDto
{
    public int Number { get; set; }
    [Required] public byte[] Data { get; set; } = [];
    public int SerieId { get; set; }
}