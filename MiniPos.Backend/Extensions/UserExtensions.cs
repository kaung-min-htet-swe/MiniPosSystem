using System.Security.Claims;

namespace MiniPos.Backend.Extensions;

public static class UserExtensions
{
    public static Guid GetUserId(this ClaimsPrincipal user)
    {
        var id = user.FindFirstValue(ClaimTypes.NameIdentifier);
        return id != null ? Guid.Parse(id) : Guid.Empty;
    }
}
