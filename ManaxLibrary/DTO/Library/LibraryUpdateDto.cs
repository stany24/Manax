using System.ComponentModel.DataAnnotations;

namespace ManaxLibrary.DTO.Library;

public class LibraryUpdateDto
{
    [Required] public string Name { get; init; } = string.Empty;
}