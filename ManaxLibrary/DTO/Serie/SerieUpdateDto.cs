
using System.ComponentModel.DataAnnotations;

namespace ManaxLibrary.DTO.Serie;

public class SerieUpdateDto
{
    [Required] public string Title { get; init; } = string.Empty;
    [Required] public string Description { get; init; } = string.Empty;
    [Required] public long? LibraryId { get; init; }
    [Required] public Status Status { get; init; }
    [Required] public List<long> TagIds { get; init; } = [];
    [Required] public List<long> PersonIds { get; init; } = [];
}