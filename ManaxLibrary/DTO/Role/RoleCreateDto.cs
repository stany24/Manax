using System.ComponentModel.DataAnnotations;

namespace ManaxLibrary.DTO.Role;

public class RoleCreateDto
{
    [Required] public string Name { get; init; } = string.Empty;

    public bool IsValid()
    {
        return !string.IsNullOrWhiteSpace(Name);
    }
}