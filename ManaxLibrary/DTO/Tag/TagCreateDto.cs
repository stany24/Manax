// ReSharper disable UnusedAutoPropertyAccessor.Global

using System.Drawing;
using System.Text.Json.Serialization;

namespace ManaxLibrary.DTO.Tag;

public class TagCreateDto
{
    public string Name { get; set; } = null!;
    public int ColorArgb { get; set; }
    
    [JsonIgnore]
    public Color Color
    {
        get => Color.FromArgb(ColorArgb);
        set => ColorArgb = value.ToArgb();
    }
}