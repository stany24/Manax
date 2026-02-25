namespace ManaxServer.Services.Validation;

public interface IPasswordValidationService
{
    bool IsPasswordValid(string password);
    string GenerateValidPassword();
}