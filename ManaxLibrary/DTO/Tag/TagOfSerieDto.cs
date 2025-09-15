// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable CollectionNeverUpdated.Global

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

namespace ManaxLibrary.DTO.Tag;

public class TagOfSerieDto
{
    public List<TagDto> Tags { get; set; }
    public long SerieId { get; set; }
}