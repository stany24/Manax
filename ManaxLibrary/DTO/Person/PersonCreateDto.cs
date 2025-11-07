namespace ManaxLibrary.DTO.Person;

public class PersonCreateDto
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Pseudonym { get; set; }
    public RoleDto Role { get; set; }
}