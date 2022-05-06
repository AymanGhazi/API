using System.Globalization;
using System.Security.Claims;

namespace API.extensions
{
    public static class ClaimsPrincipleExtension
    {
        public static string GetuserName(this ClaimsPrincipal user)
        {
            return user.FindFirst(ClaimTypes.Name)?.Value;

        }
        public static int GetuserID(this ClaimsPrincipal user)
        {
            return int.Parse(user.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        }
    }
}