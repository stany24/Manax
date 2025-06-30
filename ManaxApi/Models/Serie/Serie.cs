// ReSharper disable PropertyCanBeMadeInitOnly.Global
// ReSharper disable PropertyCanBeMadeInitOnly.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global
// ReSharper disable MemberCanBePrivate.Global

using System.ComponentModel.DataAnnotations.Schema;

namespace ManaxApi.Models.Serie;

public class Serie
{
    public long Id { get; set; }
    public long LibraryId { get; set; }
    [ForeignKey(nameof(LibraryId))]
    public Library.Library Library { get; set; } = null!;
    public string FolderName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public Status Status { get; set; }
}