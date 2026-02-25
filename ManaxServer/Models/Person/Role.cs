using System.ComponentModel.DataAnnotations;
using ManaxLibrary.DTO.Role;

namespace ManaxServer.Models.Person;

public class Role
{
    public long Id { get; set; }
    [MaxLength(50)] public string Name { get; set; } = string.Empty;

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