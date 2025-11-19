using System.ComponentModel.DataAnnotations;

namespace ManaxLibrary.DTO.Person;

public class PersonCreateDto
{
    [Required] public string FirstName { get; init; } = string.Empty;
    [Required] public string LastName { get; init; } = string.Empty;
    [Required] public string Pseudonym { get; init; } = string.Empty;
    [Required] public long RoleId { get; init; }
}