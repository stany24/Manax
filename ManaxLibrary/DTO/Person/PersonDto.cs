namespace ManaxLibrary.DTO.Person;

public class PersonDto
{
    public long Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Pseudonym { get; set; }
    public RoleDto Role { get; set; }
}