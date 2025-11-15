using System.ComponentModel.DataAnnotations;

namespace ManaxLibrary.DTO.Library;

public class LibraryCreateDto
{
    [Required] public string Name { get; init; } = string.Empty;
}