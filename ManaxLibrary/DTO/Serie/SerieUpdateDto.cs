
using System.ComponentModel.DataAnnotations;
using ManaxLibrary.DTO.Person;
using ManaxLibrary.DTO.Tag;

namespace ManaxLibrary.DTO.Serie;

public class SerieUpdateDto
{
    [Required] public string Title { get; init; } = string.Empty;
    [Required] public string Description { get; init; } = string.Empty;
    [Required] public long? LibraryId { get; init; }
    [Required] public Status Status { get; init; }
    [Required] public List<TagDto> Tags { get; init; } = [];
    [Required] public List<PersonDto> Persons { get; init; } = [];
}