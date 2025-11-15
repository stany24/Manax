using System.ComponentModel.DataAnnotations;

namespace ManaxLibrary.DTO.Tag;

public class TagCreateDto
{
    [Required] public string Name { get; init; } = null!;
    [Required] public int ColorArgb { get; init; }
}