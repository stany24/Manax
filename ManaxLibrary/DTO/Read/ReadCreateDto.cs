using System.ComponentModel.DataAnnotations;

namespace ManaxLibrary.DTO.Read;

public class ReadCreateDto
{
    [Required] public long ChapterId { get; init; }
    [Required] public uint Page { get; init; }
}