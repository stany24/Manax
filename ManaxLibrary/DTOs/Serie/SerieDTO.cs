// ReSharper disable InconsistentNaming
// ReSharper disable PropertyCanBeMadeInitOnly.Global

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

namespace ManaxLibrary.DTOs.Serie;

public class SerieDTO
{
    public long Id { get; set; }
    public long LibraryId { get; set; }
    public string FolderName { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public Status Status { get; set; }
}