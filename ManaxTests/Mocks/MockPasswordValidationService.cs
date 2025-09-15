using ManaxServer.Services.Validation;

namespace ManaxTests.Mocks;

public class MockPasswordValidationService : IPasswordValidationService
{
    public bool IsPasswordValid(string password, out string? errorMessage)
    {
        if (string.IsNullOrEmpty(password))
        {
            errorMessage = "Password cannot be empty.";
            return false;
        }
        errorMessage = string.Empty;
        return true;
    }

    public string GenerateValidPassword()
    {
        return "MockPassword123!";
    }
}