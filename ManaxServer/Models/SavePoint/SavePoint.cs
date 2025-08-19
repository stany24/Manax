// ReSharper disable PropertyCanBeMadeInitOnly.Global
// ReSharper disable PropertyCanBeMadeInitOnly.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global
// ReSharper disable MemberCanBePrivate.Global

using Microsoft.EntityFrameworkCore;

namespace ManaxServer.Models.SavePoint;

[Index(nameof(Path), IsUnique = true)]
public class SavePoint
{
    public long Id { get; set; }
    public string Path { get; set; } = string.Empty;
    public DateTime Creation { get; set; }
}