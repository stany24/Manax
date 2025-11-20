using ManaxServer.Services.Validation;

namespace ManaxTests.Server.Mocks;

public class MockPasswordValidationService : IPasswordValidationService
{
    public bool IsPasswordValid(string password)
    {
        return !string.IsNullOrEmpty(password);
    }

    public string GenerateValidPassword()
    {
        return "MockPassword123!";
    }
}