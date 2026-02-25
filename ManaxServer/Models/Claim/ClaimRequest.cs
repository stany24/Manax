// ReSharper disable PropertyCanBeMadeInitOnly.Global

namespace ManaxServer.Models.Claim;

public class ClaimRequest
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}