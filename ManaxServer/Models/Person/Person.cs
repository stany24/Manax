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