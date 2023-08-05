using System.Security.Claims;

namespace ChatServer.WebApi.Extensions
{
    public static class ClaimsExtensions
    {
        public static Guid GetUserId(this ClaimsPrincipal claimsPrincipal)
        {
            var userId = claimsPrincipal?.FindFirstValue(ClaimTypes.NameIdentifier);
            return userId == null
                ? throw new Exception("UserId claim is not present")
                : Guid.Parse(userId);
        }
    }
}
