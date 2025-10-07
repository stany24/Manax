// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable PropertyCanBeMadeInitOnly.Global

using System.Drawing;
using System.Text.Json.Serialization;

namespace ManaxLibrary.DTO.Tag;

public class TagUpdateDto
{
    public long Id { get; set; }
    public string Name { get; set; } = null!;
    public int ColorArgb { get; set; }

    [JsonIgnore]
    public Color Color
    {
        get => Color.FromArgb(ColorArgb);
        set => ColorArgb = value.ToArgb();
    }
}