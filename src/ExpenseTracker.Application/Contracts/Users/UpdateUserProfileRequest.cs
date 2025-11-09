namespace ExpenseTracker.Application.Contracts.Users;

public class UpdateUserProfileRequest
{
    public string? FullName { get; set; }
    public string? Email { get; set; }
    public string? CurrentPassword { get; set; }
    public string? NewPassword { get; set; }
}

