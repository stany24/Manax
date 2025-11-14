#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

namespace ManaxLibrary.DTO.Person;

public class PersonDto
{
    public long Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Pseudonym { get; set; }
    public List<long> SerieIds { get; set; } = [];
    public long RoleId { get; set; }
}