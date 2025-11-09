using ExpenseTracker.Application.Contracts.Users;

namespace ExpenseTracker.Application.Common.Interfaces.Services;

public interface IUserService
{
    Task<UserDto> GetProfileAsync(int userId, CancellationToken cancellationToken = default);
    Task<UserDto> UpdateProfileAsync(int userId, UpdateUserProfileRequest request, CancellationToken cancellationToken = default);
}

