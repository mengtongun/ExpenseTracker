using AutoMapper;
using ExpenseTracker.Application.Common.Exceptions;
using ExpenseTracker.Application.Common.Interfaces.Persistence;
using ExpenseTracker.Application.Common.Interfaces.Services;
using ExpenseTracker.Application.Contracts.Auth;
using ExpenseTracker.Application.Contracts.Users;
using ExpenseTracker.Application.Options;
using ExpenseTracker.Domain.Entities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using BCryptNet = BCrypt.Net.BCrypt;

namespace ExpenseTracker.Application.Services.Auth;

public class AuthService : IAuthService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly JwtOptions _jwtOptions;

    public AuthService(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IDateTimeProvider dateTimeProvider,
        IOptions<JwtOptions> jwtOptions)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _dateTimeProvider = dateTimeProvider;
        _jwtOptions = jwtOptions.Value;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        if (await _unitOfWork.Users.EmailExistsAsync(request.Email, cancellationToken))
        {
            throw new ConflictException("Email is already in use.");
        }

        if (await _unitOfWork.Users.UsernameExistsAsync(request.Username, cancellationToken))
        {
            throw new ConflictException("Username is already taken.");
        }

        var user = new User
        {
            Username = request.Username.Trim(),
            Email = request.Email.Trim().ToLowerInvariant(),
            PasswordHash = BCryptNet.HashPassword(request.Password),
            FullName = request.FullName?.Trim(),
            IsActive = true
        };

        await _unitOfWork.Users.AddAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await CreateAuthResponseAsync(user, cancellationToken);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var identifier = request.EmailOrUsername.Trim();
        User? user = identifier.Contains('@')
            ? await _unitOfWork.Users.GetByEmailAsync(identifier.ToLowerInvariant(), cancellationToken)
            : await _unitOfWork.Users.GetByUsernameAsync(identifier, cancellationToken);

        if (user is null)
        {
            throw new UnauthorizedAppException("Invalid credentials.");
        }

        if (!user.IsActive)
        {
            throw new UnauthorizedAppException("Account is disabled. Please contact support.");
        }

        if (!BCryptNet.Verify(request.Password, user.PasswordHash))
        {
            throw new UnauthorizedAppException("Invalid credentials.");
        }

        user.LastLoginAt = _dateTimeProvider.UtcNow;
        _unitOfWork.Users.Update(user);

        return await CreateAuthResponseAsync(user, cancellationToken);
    }

    public async Task<AuthResponse> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        var tokenHash = ComputeSha256(refreshToken);
        var storedToken = await _unitOfWork.RefreshTokens.GetByHashAsync(tokenHash, cancellationToken)
            ?? throw new UnauthorizedAppException("Invalid refresh token.");

        if (!storedToken.IsActive)
        {
            throw new UnauthorizedAppException("Refresh token is no longer active.");
        }

        storedToken.RevokedAt = _dateTimeProvider.UtcNow;
        _unitOfWork.RefreshTokens.Update(storedToken);

        var user = storedToken.User;
        if (!user.IsActive)
        {
            throw new UnauthorizedAppException("Account is disabled.");
        }

        return await CreateAuthResponseAsync(user, cancellationToken);
    }

    public async Task RevokeRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        var tokenHash = ComputeSha256(refreshToken);
        var storedToken = await _unitOfWork.RefreshTokens.GetByHashAsync(tokenHash, cancellationToken)
            ?? throw new NotFoundException("Refresh token not found.");

        if (storedToken.RevokedAt is not null)
        {
            return;
        }

        storedToken.RevokedAt = _dateTimeProvider.UtcNow;
        _unitOfWork.RefreshTokens.Update(storedToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task<AuthResponse> CreateAuthResponseAsync(User user, CancellationToken cancellationToken)
    {
        var (accessToken, expiresAt) = GenerateJwtToken(user);
        var refreshToken = await IssueRefreshTokenAsync(user, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new AuthResponse
        {
            AccessToken = accessToken,
            AccessTokenExpiresAt = expiresAt,
            RefreshToken = refreshToken,
            User = _mapper.Map<UserDto>(user)
        };
    }

    private (string Token, DateTime ExpiresAt) GenerateJwtToken(User user)
    {
        var expiresAt = _dateTimeProvider.UtcNow.AddMinutes(_jwtOptions.AccessTokenMinutes);
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(ClaimTypes.Email, user.Email),
            new("username", user.Username),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        if (!string.IsNullOrWhiteSpace(user.FullName))
        {
            claims.Add(new Claim("name", user.FullName));
        }

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.SigningKey));
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = expiresAt,
            Issuer = _jwtOptions.Issuer,
            Audience = _jwtOptions.Audience,
            SigningCredentials = credentials
        };

        var handler = new JwtSecurityTokenHandler();
        var token = handler.CreateToken(tokenDescriptor);
        return (handler.WriteToken(token), expiresAt);
    }

    private async Task<string> IssueRefreshTokenAsync(User user, CancellationToken cancellationToken)
    {
        var plainToken = GenerateSecureToken();
        var refreshToken = new RefreshToken
        {
            TokenHash = ComputeSha256(plainToken),
            UserId = user.Id,
            ExpiresAt = _dateTimeProvider.UtcNow.AddDays(_jwtOptions.RefreshTokenDays)
        };

        await _unitOfWork.RefreshTokens.AddAsync(refreshToken, cancellationToken);
        return plainToken;
    }

    private static string GenerateSecureToken()
    {
        Span<byte> randomBytes = stackalloc byte[64];
        RandomNumberGenerator.Fill(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }

    private static string ComputeSha256(string input)
    {
        var bytes = Encoding.UTF8.GetBytes(input);
        var hash = SHA256.HashData(bytes);
        return Convert.ToHexString(hash);
    }
}

