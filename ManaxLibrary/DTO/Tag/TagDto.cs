using System.Drawing;

namespace ManaxLibrary.DTO.Tag;

public class TagDto
{
    public long Id { get; set; }
    public string Name { get; set; } = null!;
    public Color Color { get; set; }
}