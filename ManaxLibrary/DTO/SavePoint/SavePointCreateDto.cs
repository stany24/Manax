using System.ComponentModel.DataAnnotations;

namespace ManaxLibrary.DTO.SavePoint;

public class SavePointCreateDto
{
    [Required] public string Path { get; init; } = string.Empty;
}