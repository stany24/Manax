using System.ComponentModel.DataAnnotations;

namespace ManaxLibrary.DTO.Role;

public class RoleUpdateDto
{
    [Required] public long Id { get; init; }
    [Required] public string Name { get; init; } = string.Empty;
}