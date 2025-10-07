// ReSharper disable PropertyCanBeMadeInitOnly.Global

using ManaxLibrary.DTO.User;

namespace ManaxServer.Models.User;

public class UserPermission
{
    public long Id { get; set; }
    public long UserId { get; set; }
    public User User { get; set; } = null!;
    public Permission Permission { get; set; }
}