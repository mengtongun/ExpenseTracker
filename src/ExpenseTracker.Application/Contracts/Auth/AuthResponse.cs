using ExpenseTracker.Application.Contracts.Users;

namespace ExpenseTracker.Application.Contracts.Auth;

public class AuthResponse
{
    public required string AccessToken { get; set; }
    public required DateTime AccessTokenExpiresAt { get; set; }
    public required string RefreshToken { get; set; }
    public required UserDto User { get; set; }
}

