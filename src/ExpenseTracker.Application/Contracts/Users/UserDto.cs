using System;

namespace ExpenseTracker.Application.Contracts.Users;

public class UserDto
{
    public Guid Id { get; set; }
    public required string Username { get; set; }
    public required string Email { get; set; }
    public string? FullName { get; set; }
    public DateTime CreatedAt { get; set; }
}

