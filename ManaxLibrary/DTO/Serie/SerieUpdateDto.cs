// ReSharper disable InconsistentNaming
// ReSharper disable PropertyCanBeMadeInitOnly.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

using System.ComponentModel.DataAnnotations;

namespace ManaxLibrary.DTO.Serie;

public class SerieUpdateDto
{
    [Required] public string Title { get; set; }
    public string Description { get; set; }
    public long? LibraryId { get; set; }
    [Required] public Status Status { get; set; }
}