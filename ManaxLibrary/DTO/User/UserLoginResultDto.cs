// ReSharper disable PropertyCanBeMadeInitOnly.Global

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

namespace ManaxLibrary.DTO.User;

public class UserLoginResultDto
{
    public string Token { get; set; }
    public UserDto User { get; set; }
}