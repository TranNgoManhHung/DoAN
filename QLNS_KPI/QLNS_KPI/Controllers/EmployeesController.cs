using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLNS_KPI.Contracts;
using QLNS_KPI.Data;
using QLNS_KPI.Models;
using QLNS_KPI.Security;

namespace QLNS_KPI.Controllers;

[ApiController]
[Authorize]
[Route("api/employees")]
public sealed class EmployeesController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<Employee>>> GetAll(CancellationToken cancellationToken)
    {
        var employees = db.Employees.AsNoTracking();
        if (!User.IsInRole(RoleNames.Admin) && !User.IsInRole(RoleNames.Manager))
        {
            employees = employees.Where(x => x.Id == int.Parse(User.FindFirst("employeeId")!.Value));
        }

        return Ok(await employees.OrderBy(x => x.Id).ToListAsync(cancellationToken));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Employee>> GetById(int id, CancellationToken cancellationToken)
    {
        if (!CanViewEmployee(id))
        {
            return Forbid();
        }

        var employee = await db.Employees.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        return employee is null ? NotFound() : Ok(employee);
    }

    [Authorize(Roles = RoleNames.Admin)]
    [HttpPost]
    public async Task<ActionResult<Employee>> Create(EmployeeInput input, CancellationToken cancellationToken)
    {
        var error = await ValidateEmployee(input, null, cancellationToken);
        if (error is not null)
        {
            return error;
        }

        var employee = new Employee
        {
            Code = input.Code.Trim(),
            Name = input.Name.Trim(),
            Email = input.Email.Trim(),
            Phone = input.Phone.Trim(),
            DepartmentId = input.DepartmentId,
            PositionId = input.PositionId,
            Status = input.Status
        };
        db.Employees.Add(employee);
        await db.SaveChangesAsync(cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = employee.Id }, employee);
    }

    [Authorize(Roles = RoleNames.Admin)]
    [HttpPut("{id:int}")]
    public async Task<ActionResult<Employee>> Update(int id, EmployeeInput input, CancellationToken cancellationToken)
    {
        var employee = await db.Employees.FindAsync([id], cancellationToken);
        if (employee is null)
        {
            return NotFound();
        }

        var error = await ValidateEmployee(input, id, cancellationToken);
        if (error is not null)
        {
            return error;
        }

        employee.Code = input.Code.Trim();
        employee.Name = input.Name.Trim();
        employee.Email = input.Email.Trim();
        employee.Phone = input.Phone.Trim();
        employee.DepartmentId = input.DepartmentId;
        employee.PositionId = input.PositionId;
        employee.Status = input.Status;
        await db.SaveChangesAsync(cancellationToken);
        return Ok(employee);
    }

    [Authorize(Roles = RoleNames.Admin)]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var employee = await db.Employees.FindAsync([id], cancellationToken);
        if (employee is null)
        {
            return NotFound();
        }

        var hasHistory = await db.Kpis.AnyAsync(x => x.EmployeeId == id, cancellationToken);
        var hasAccount = await db.Accounts.AnyAsync(x => x.EmployeeId == id, cancellationToken);
        if (hasHistory || hasAccount)
        {
            employee.Status = "Ngừng hoạt động";
            await db.SaveChangesAsync(cancellationToken);
            return NoContent();
        }

        db.Employees.Remove(employee);
        await db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    private bool CanViewEmployee(int id) =>
        User.IsInRole(RoleNames.Admin)
        || User.IsInRole(RoleNames.Manager)
        || id == int.Parse(User.FindFirst("employeeId")!.Value);

    private async Task<ActionResult?> ValidateEmployee(
        EmployeeInput input,
        int? currentId,
        CancellationToken cancellationToken)
    {
        if (!new EmailAddressAttribute().IsValid(input.Email.Trim()))
        {
            return BadRequest(new { message = "Địa chỉ email không hợp lệ." });
        }

        if (await db.Employees.AnyAsync(
                x => x.Id != currentId && x.Code == input.Code.Trim(),
                cancellationToken))
        {
            return Conflict(new { message = "Mã nhân viên đã tồn tại." });
        }

        if (await db.Employees.AnyAsync(
                x => x.Id != currentId && x.Email == input.Email.Trim(),
                cancellationToken))
        {
            return Conflict(new { message = "Email nhân viên đã được sử dụng." });
        }

        if (!await db.Departments.AnyAsync(x => x.Id == input.DepartmentId, cancellationToken)
            || !await db.Positions.AnyAsync(x => x.Id == input.PositionId, cancellationToken))
        {
            return BadRequest(new { message = "Phòng ban hoặc chức vụ không tồn tại." });
        }

        return null;
    }
}
