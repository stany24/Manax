// ReSharper disable PropertyCanBeMadeInitOnly.Global
// ReSharper disable PropertyCanBeMadeInitOnly.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global
// ReSharper disable MemberCanBePrivate.Global

using ManaxLibrary.DTO.SavePoint;
using Microsoft.EntityFrameworkCore;

namespace ManaxServer.Models.SavePoint;

[Index(nameof(Path), IsUnique = true)]
public class SavePoint
{
    public long Id { get; set; }
    public string Path { get; set; } = string.Empty;
    public DateTime Creation { get; set; }

    public static SavePoint Create(SavePointCreateDto savePointCreate)
    {
        return new SavePoint
        {
            Path = savePointCreate.Path,
            Creation = DateTime.UtcNow
        };
    }
}