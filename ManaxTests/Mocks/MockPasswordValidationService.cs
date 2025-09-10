using ManaxServer.Services.Validation;

namespace ManaxTests.Mocks;

public class MockPasswordValidationService : IPasswordValidationService
{
    public bool IsPasswordValid(string password, out string? errorMessage)
    {
        errorMessage = string.Empty;
        return true;
    }
}