// ReSharper disable InconsistentNaming
// ReSharper disable PropertyCanBeMadeInitOnly.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

using ManaxLibrary.DTO.Tag;

namespace ManaxLibrary.DTO.Serie;

public class SerieDto
{
    public long Id { get; set; }
    public long? LibraryId { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public Status Status { get; set; }

    public DateTime Creation { get; set; }
    public DateTime LastModification { get; set; }
    public List<TagDto> Tags { get; set; } = [];
}