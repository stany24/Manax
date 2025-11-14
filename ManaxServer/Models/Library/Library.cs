// ReSharper disable PropertyCanBeMadeInitOnly.Global
// ReSharper disable PropertyCanBeMadeInitOnly.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global
// ReSharper disable MemberCanBePrivate.Global

using ManaxLibrary.DTO.Library;
using Microsoft.EntityFrameworkCore;

namespace ManaxServer.Models.Library;

[Index(nameof(Name), IsUnique = true)]
public class Library
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime Creation { get; set; }
    
    public LibraryDto ToDto()
    {
        return new LibraryDto
        {
            Id = Id,
            Name = Name,
            Creation = Creation
        };
    }

    public void Update(LibraryUpdateDto libraryUpdate)
    {
        Name = libraryUpdate.Name;
    }

    public static Library Create(LibraryCreateDto libraryCreate)
    {
        return new Library
        {
            Name = libraryCreate.Name,
            Creation = DateTime.UtcNow
        };
    }
}