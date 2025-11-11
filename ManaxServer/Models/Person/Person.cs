#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

namespace ManaxServer.Models.Person;

public class Person
{
    public long Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Pseudonym { get; set; }
    public Role Role { get; set; }
    public List<Serie.Serie> Series { get; set; }
}