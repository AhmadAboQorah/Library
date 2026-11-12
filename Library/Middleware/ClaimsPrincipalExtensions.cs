using System.Security.Claims;

namespace Library.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static int GetUserId(this ClaimsPrincipal user)
    {
        var value = user.FindFirstValue("sub");
        return int.TryParse(value, out var id)
            ? id
            : throw new UnauthorizedAccessException("Invalid token.");
    }
}