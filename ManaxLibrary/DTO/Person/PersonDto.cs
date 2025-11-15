namespace ManaxLibrary.DTO.Person;

public class PersonDto
{
    public long Id { get; init; }
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string Pseudonym { get; init; } = string.Empty;
    public List<long> SerieIds { get; init; } = [];
    public long RoleId { get; init; }
}