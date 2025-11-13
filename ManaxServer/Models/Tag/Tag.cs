// ReSharper disable PropertyCanBeMadeInitOnly.Global

using ManaxLibrary.DTO.Tag;
using Microsoft.EntityFrameworkCore;

namespace ManaxServer.Models.Tag;

[Index(nameof(Name), IsUnique = true)]
public class Tag
{
    public long Id { get; set; }
    public List<long> SerieIds { get; set; } = [];
    public string Name { get; set; } = null!;
    public int ColorArgb { get; set; }
    
    public TagDto ToDto()
    {
        return new TagDto
        {
            Id = Id,
            SerieIds = SerieIds,
            Name = Name,
            ColorArgb = ColorArgb
        };
    }
    
    public void Update(TagUpdateDto tagUpdate)
    {
        Name = tagUpdate.Name;
        ColorArgb = tagUpdate.ColorArgb;
    }

    public static Tag Create(TagCreateDto tagCreate)
    {
        return new Tag
        {
            Name = tagCreate.Name,
            ColorArgb = tagCreate.ColorArgb
        };
    }
}