// ReSharper disable InconsistentNaming
// ReSharper disable PropertyCanBeMadeInitOnly.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

using System.ComponentModel.DataAnnotations;

namespace ManaxLibrary.DTOs.Serie;

public class SerieUpdateDTO
{
    [Required] public string Title { get; set; }
    [Required] public string Description { get; set; }
    public Status Status { get; set; }
}