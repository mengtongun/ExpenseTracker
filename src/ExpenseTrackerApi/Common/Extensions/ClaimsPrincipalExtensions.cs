using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace ExpenseTrackerApi.Common.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static int GetUserId(this ClaimsPrincipal user)
    {
        var identifier = user.FindFirstValue(ClaimTypes.NameIdentifier) ?? user.FindFirstValue(JwtRegisteredClaimNames.Sub);

        if (identifier is null || !int.TryParse(identifier, out var userId) || userId <= 0)
        {
            throw new UnauthorizedAccessException("Unable to determine the current user.");
        }

        return userId;
    }
}

