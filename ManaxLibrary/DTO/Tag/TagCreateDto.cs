using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.Text.Json.Serialization;

namespace ManaxLibrary.DTO.Tag;

public class TagCreateDto
{
    [Required] public string Name { get; init; } = null!;
    [Required] public int ColorArgb { get; init; }
}