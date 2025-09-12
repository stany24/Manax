using System.Drawing;

namespace ManaxLibrary.DTO.Tag;

public class TagCreateDto
{
    public string Name { get; set; } = null!;
    public Color Color { get; set; }
}