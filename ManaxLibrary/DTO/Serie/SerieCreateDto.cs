using System.ComponentModel.DataAnnotations;

namespace ManaxLibrary.DTO.Serie;

public class SerieCreateDto
{
    [Required] public string Title { get; init; } = string.Empty;
}