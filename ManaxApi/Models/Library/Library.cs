// ReSharper disable PropertyCanBeMadeInitOnly.Global
// ReSharper disable PropertyCanBeMadeInitOnly.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace ManaxApi.Models.Library;

public class Library
{
    public long Id { get; set; }
    public LibraryInfo Infos { get; set; } = new();
    public List<Serie.Serie> Series { get; set; } = [];
}