using System.ComponentModel.DataAnnotations;

namespace ManaxLibrary.DTO.Library;

public class LibraryCreateDto
{
    [Required] [MinLength(1)] public string Name { get; init; } = string.Empty;
}