// ReSharper disable InconsistentNaming
// ReSharper disable PropertyCanBeMadeInitOnly.Global
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

using System.ComponentModel.DataAnnotations;

namespace ManaxApi.DTOs;

public class SerieDTO
{
    public long Id { get; set; }
    public string FolderName { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public string Path { get; set; }
}

public class SerieCreateDTO
{
    [Required] public string Title { get; set; }
    [Required] public string Description { get; set; }
    [Required] public string Path { get; set; }
}

public class SerieUpdateDTO
{
    [Required] public string Title { get; set; }
    [Required] public string Description { get; set; }
    [Required] public string Path { get; set; }
}