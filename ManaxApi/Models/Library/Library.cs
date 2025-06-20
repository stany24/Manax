// ReSharper disable PropertyCanBeMadeInitOnly.Global
// ReSharper disable PropertyCanBeMadeInitOnly.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global
// ReSharper disable MemberCanBePrivate.Global

namespace ManaxApi.Models.Library;

public class Library
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<Serie.Serie> Series { get; set; } = [];
    
    public LibraryInfo GetInfo()
    {
        return new LibraryInfo
        {
            Name = Name,
            Description = Description
        };
    }
}