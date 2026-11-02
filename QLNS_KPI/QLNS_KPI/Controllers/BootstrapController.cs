using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLNS_KPI.Contracts;
using QLNS_KPI.Data;
using QLNS_KPI.Security;

namespace QLNS_KPI.Controllers;

[ApiController]
[Authorize]
[Route("api/bootstrap")]
public sealed class BootstrapController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<BootstrapDto>> Get(CancellationToken cancellationToken)
    {
        var isAdmin = User.IsInRole(RoleNames.Admin);
        var isManager = User.IsInRole(RoleNames.Manager);
        var employeeId = int.Parse(User.FindFirst("employeeId")!.Value);

        var employees = db.Employees.AsNoTracking();
        if (!isAdmin && !isManager)
        {
            employees = employees.Where(x => x.Id == employeeId);
        }

        var kpis = db.Kpis.AsNoTracking();
        if (!isAdmin && !isManager)
        {
            kpis = kpis.Where(x => x.EmployeeId == employeeId);
        }

        var dto = new BootstrapDto(
            await employees.OrderBy(x => x.Id).ToListAsync(cancellationToken),
            await db.Departments.AsNoTracking().OrderBy(x => x.Id).ToListAsync(cancellationToken),
            await db.Positions.AsNoTracking().OrderBy(x => x.Id).ToListAsync(cancellationToken),
            isAdmin
                ? (await db.Accounts.AsNoTracking().Include(x => x.Role).OrderBy(x => x.Id).ToListAsync(cancellationToken))
                    .Select(ApiMapper.ToDto).ToArray()
                : [],
            await db.Periods.AsNoTracking().OrderByDescending(x => x.Start).ToListAsync(cancellationToken),
            await db.Criteria.AsNoTracking().OrderBy(x => x.Id).ToListAsync(cancellationToken),
            (await kpis.OrderBy(x => x.Id).ToListAsync(cancellationToken))
                .Select(x => ApiMapper.ToDto(x, isAdmin || isManager))
                .ToArray());

        return Ok(dto);
    }
}
