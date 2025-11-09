using AutoMapper;
using ExpenseTracker.Application.Common.Exceptions;
using ExpenseTracker.Application.Common.Interfaces.Persistence;
using ExpenseTracker.Application.Common.Interfaces.Services;
using ExpenseTracker.Application.Contracts.Users;
using BCryptNet = BCrypt.Net.BCrypt;

namespace ExpenseTracker.Application.Services.Users;

public class UserService : IUserService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public UserService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<UserDto> GetProfileAsync(int userId, CancellationToken cancellationToken = default)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userId, cancellationToken)
            ?? throw new NotFoundException("User not found.");

        return _mapper.Map<UserDto>(user);
    }

    public async Task<UserDto> UpdateProfileAsync(int userId, UpdateUserProfileRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userId, cancellationToken)
            ?? throw new NotFoundException("User not found.");

        if (!string.IsNullOrWhiteSpace(request.Email))
        {
            var normalizedEmail = request.Email.Trim().ToLowerInvariant();
            if (!normalizedEmail.Equals(user.Email, StringComparison.OrdinalIgnoreCase) &&
                await _unitOfWork.Users.EmailExistsAsync(normalizedEmail, cancellationToken))
            {
                throw new ConflictException("Email is already in use.");
            }

            user.Email = normalizedEmail;
        }

        if (!string.IsNullOrWhiteSpace(request.FullName))
        {
            user.FullName = request.FullName.Trim();
        }

        if (!string.IsNullOrWhiteSpace(request.NewPassword))
        {
            if (string.IsNullOrWhiteSpace(request.CurrentPassword))
            {
                throw new BadRequestException("Current password is required to set a new password.");
            }

            if (!BCryptNet.Verify(request.CurrentPassword, user.PasswordHash))
            {
                throw new UnauthorizedAppException("Current password is incorrect.");
            }

            user.PasswordHash = BCryptNet.HashPassword(request.NewPassword);
        }

        _unitOfWork.Users.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<UserDto>(user);
    }
}

