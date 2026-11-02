using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLNS_KPI.Contracts;
using QLNS_KPI.Data;
using QLNS_KPI.Security;

namespace QLNS_KPI.Controllers;

[ApiController]
[Authorize]
[Route("api")]
public sealed class DashboardController(AppDbContext db) : ControllerBase
{
    [HttpGet("dashboard")]
    public async Task<ActionResult<DashboardDto>> GetDashboard(CancellationToken cancellationToken)
    {
        var isAdmin = User.IsInRole(RoleNames.Admin);
        var isManager = User.IsInRole(RoleNames.Manager);
        var employeeId = int.Parse(User.FindFirst("employeeId")!.Value);
        var employees = db.Employees.AsNoTracking();
        var kpis = db.Kpis.AsNoTracking();
        if (!isAdmin && !isManager)
        {
            employees = employees.Where(x => x.Id == employeeId);
            kpis = kpis.Where(x => x.EmployeeId == employeeId);
        }

        var employeeRows = await employees.ToListAsync(cancellationToken);
        var kpiRows = await kpis.ToListAsync(cancellationToken);
        var departmentIds = employeeRows.Select(x => x.DepartmentId).Distinct().ToArray();
        var departments = await db.Departments.AsNoTracking()
            .Where(x => isAdmin || isManager || departmentIds.Contains(x.Id))
            .OrderBy(x => x.Id)
            .ToListAsync(cancellationToken);

        var dto = new DashboardDto(
            employeeRows.Count(x => x.Status == "Đang làm việc"),
            departments.Count,
            kpiRows.Count(x => x.Status != "Đã đánh giá"),
            kpiRows.Count(x => x.Status == "Đã đánh giá"),
            kpiRows.Count,
            departments.Select(department => new DepartmentCountDto(
                department.Id,
                department.Name,
                employeeRows.Count(x => x.DepartmentId == department.Id && x.Status == "Đang làm việc")))
                .ToArray());
        return Ok(dto);
    }
}
