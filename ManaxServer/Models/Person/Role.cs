using System.ComponentModel.DataAnnotations;
using ManaxLibrary.DTO.Role;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

namespace ManaxServer.Models.Person;

public class Role
{
    public long Id { get; set; }
    [MaxLength(50)] public string Name { get; set; }

    public RoleDto ToDto()
    {
        return new RoleDto
        {
            Id = Id,
            Name = Name
        };
    }

    public static Role Create(RoleCreateDto roleCreateDto)
    {
        return new Role
        {
            Name = roleCreateDto.Name
        };
    }

    public void Update(RoleUpdateDto roleUpdateDto)
    {
        Name = roleUpdateDto.Name;
    }
}