namespace ManaxServer.Services.Validation;

public interface IPasswordValidationService
{
    bool IsPasswordValid(string password, out string? errorMessage);
    string GenerateValidPassword();
}