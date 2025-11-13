// ReSharper disable PropertyCanBeMadeInitOnly.Global

using ManaxLibrary.DTO.Person;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

namespace ManaxServer.Models.Person;

public class Person
{
    public long Id { get; set; }
    public List<long> SerieIds { get; set; } = [];
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Pseudonym { get; set; }
    public Role Role { get; set; }
    
    public PersonDto ToDto()
    {
        return new PersonDto
        {
            Id = Id,
            SerieIds = SerieIds,
            FirstName = FirstName,
            LastName = LastName,
            Pseudonym = Pseudonym,
            RoleId = Role.Id
        };
    }

    public static Person Create(PersonCreateDto personCreateDto,ManaxContext context)
    {
        return new Person
        {
            FirstName = personCreateDto.FirstName,
            LastName = personCreateDto.LastName,
            Pseudonym = personCreateDto.Pseudonym,
            Role = context.Roles.Find(personCreateDto.RoleId)!
        };
    }

    public void Update(PersonUpdateDto personUpdateDto, Role role)
    {
        FirstName = personUpdateDto.FirstName;
        LastName = personUpdateDto.LastName;
        Pseudonym = personUpdateDto.Pseudonym;
        Role = role;
    }
}