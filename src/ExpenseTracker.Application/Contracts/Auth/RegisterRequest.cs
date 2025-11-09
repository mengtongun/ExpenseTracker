namespace ExpenseTracker.Application.Contracts.Auth;

public class RegisterRequest
{
    public required string Username { get; set; }
    public required string Email { get; set; }
    public required string Password { get; set; }
    public string? FullName { get; set; }
}

