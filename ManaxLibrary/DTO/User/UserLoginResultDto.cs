namespace ManaxLibrary.DTO.User;

public class UserLoginResultDto
{
    public string Token { get; init; } = string.Empty;
    public UserDto User { get; init; } = new();
}