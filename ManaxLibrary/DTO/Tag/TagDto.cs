// ReSharper disable PropertyCanBeMadeInitOnly.Global

namespace ManaxLibrary.DTO.Tag;

public class TagDto
{
    public long Id { get; set; }
    public string Name { get; set; } = null!;
    public int ColorArgb { get; set; }
}