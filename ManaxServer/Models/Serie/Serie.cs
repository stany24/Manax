// ReSharper disable PropertyCanBeMadeInitOnly.Global
// ReSharper disable PropertyCanBeMadeInitOnly.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global
// ReSharper disable MemberCanBePrivate.Global

using System.ComponentModel.DataAnnotations.Schema;
using ManaxLibrary.DTO.Serie;

namespace ManaxServer.Models.Serie;

public class Serie
{
    public long Id { get; set; }
    public long? LibraryId { get; set; }

    [ForeignKey(nameof(LibraryId))] public Library.Library? Library { get; set; }
    public long SavePointId { get; set; }
    [ForeignKey(nameof(SavePointId))] public SavePoint.SavePoint SavePoint { get; set; } = null!;

    public string FolderName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Status Status { get; set; }

    public DateTime Creation { get; set; }
    public DateTime LastModification { get; set; }
    
    public List<Tag.Tag> Tags { get; set; } = [];

    public string SavePath => SavePoint.Path + Path.DirectorySeparatorChar + FolderName;
}