using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLNS_KPI.Contracts;
using QLNS_KPI.Data;
using QLNS_KPI.Security;

namespace QLNS_KPI.Controllers;

[ApiController]
[Authorize(Roles = RoleNames.AdminOrManager)]
[Route("api/reports")]
public sealed class ReportsController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ReportEmployeeDto>>> Get(
        [FromQuery] int? periodId,
        [FromQuery] int? departmentId,
        CancellationToken cancellationToken)
    {
        var query = db.Kpis.AsNoTracking();
        if (periodId.HasValue)
        {
            query = query.Where(x => x.PeriodId == periodId.Value);
        }
        if (departmentId.HasValue)
        {
            query = query.Where(x => x.Employee != null && x.Employee.DepartmentId == departmentId.Value);
        }

        var kpis = await query.Include(x => x.Employee)
            .ThenInclude(x => x!.Department)
            .OrderBy(x => x.EmployeeId)
            .ToListAsync(cancellationToken);

        var rows = kpis.GroupBy(x => x.EmployeeId).Select(group =>
        {
            var employee = group.First().Employee!;
            var scored = group.Where(x => x.Score.HasValue).ToArray();
            var score = scored.Length == 0
                ? (decimal?)null
                : scored.Sum(x => x.Score!.Value * x.Weight / 100m);
            return new ReportEmployeeDto(
                employee.Id,
                employee.Name,
                employee.DepartmentId,
                employee.Department?.Name ?? "",
                group.Count(),
                scored.Length,
                score);
        }).ToArray();

        return Ok(rows);
    }
}
