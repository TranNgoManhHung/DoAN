using System.Security.Claims;

namespace QLNS_KPI.Security;

public static class ClaimsPrincipalExtensions
{
    public static int GetAccountId(this ClaimsPrincipal user)
    {
        var value = user.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(value, out var id)
            ? id
            : throw new InvalidOperationException("The authenticated token does not contain a valid account id.");
    }
}
