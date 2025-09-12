using System.Drawing;
using Microsoft.EntityFrameworkCore;

namespace ManaxServer.Models.Tag;

[Index(nameof(Name), IsUnique = true)]
public class Tag
{
    public long Id { get; set; }
    public string Name { get; set; } = null!;
    public Color Color { get; set; }
}