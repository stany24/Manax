using Microsoft.EntityFrameworkCore;

namespace ManaxServer.Models.Tag;

[Index(nameof(Name), IsUnique = true)]
public class Tag
{
    public long Id { get; set; }
    public string Name { get; set; } = null!;
    public int ColorArgb { get; set; }
}