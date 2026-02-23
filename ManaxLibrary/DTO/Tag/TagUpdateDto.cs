using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.Text.Json.Serialization;

namespace ManaxLibrary.DTO.Tag;

public class TagUpdateDto
{
    [Required] public long Id { get; init; }
    [Required] public string Name { get; init; } = null!;
    [Required] public int ColorArgb { get; init; }

    [JsonIgnore]
    public Color Color
    {
        get => Color.FromArgb(ColorArgb);
        init => ColorArgb = value.ToArgb();
    }

    public bool IsValid()
    {
        return !string.IsNullOrWhiteSpace(Name);
    }
}