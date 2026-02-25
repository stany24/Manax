using System.ComponentModel.DataAnnotations;

namespace ManaxLibrary.DTO.Role;

public class RoleUpdateDto
{
    [Required] public string Name { get; init; } = string.Empty;

    public bool IsValid()
    {
        return !string.IsNullOrWhiteSpace(Name);
    }
}