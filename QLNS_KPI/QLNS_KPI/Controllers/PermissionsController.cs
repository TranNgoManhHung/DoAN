using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLNS_KPI.Data;
using QLNS_KPI.Security;

namespace QLNS_KPI.Controllers;

[ApiController]
[Authorize]
[Route("api/permissions")]
public sealed class PermissionsController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var roles = await db.Roles.AsNoTracking()
            .Include(role => role.RolePermissions)
            .ThenInclude(link => link.Permission)
            .OrderBy(role => role.Id)
            .ToListAsync(cancellationToken);
        return Ok(roles.Select(role => new
        {
            role = role.Name,
            permissions = role.RolePermissions.Select(link => new
            {
                code = link.Permission!.Code,
                name = link.Permission.Name,
                group = link.Permission.Group
            }).ToArray()
        }));
    }
}
