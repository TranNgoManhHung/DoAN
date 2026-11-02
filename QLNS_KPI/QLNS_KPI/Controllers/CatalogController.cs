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
[Route("api")]
public sealed class CatalogController(AppDbContext db) : ControllerBase
{
    [HttpGet("departments")]
    public async Task<ActionResult<IReadOnlyList<Department>>> GetDepartments(CancellationToken cancellationToken) =>
        Ok(await db.Departments.AsNoTracking().OrderBy(x => x.Id).ToListAsync(cancellationToken));

    [Authorize(Roles = RoleNames.Admin)]
    [HttpPost("departments")]
    public async Task<ActionResult<Department>> CreateDepartment(DepartmentInput input, CancellationToken cancellationToken)
    {
        if (await db.Departments.AnyAsync(x => x.Code == input.Code.Trim(), cancellationToken))
        {
            return Conflict(new { message = "Mã phòng ban đã tồn tại." });
        }

        var department = new Department
        {
            Code = input.Code.Trim(),
            Name = input.Name.Trim(),
            Description = input.Description?.Trim()
        };
        db.Departments.Add(department);
        await db.SaveChangesAsync(cancellationToken);
        return CreatedAtAction(nameof(GetDepartments), new { id = department.Id }, department);
    }

    [Authorize(Roles = RoleNames.Admin)]
    [HttpPut("departments/{id:int}")]
    public async Task<ActionResult<Department>> UpdateDepartment(int id, DepartmentInput input, CancellationToken cancellationToken)
    {
        var department = await db.Departments.FindAsync([id], cancellationToken);
        if (department is null)
        {
            return NotFound();
        }

        if (await db.Departments.AnyAsync(x => x.Id != id && x.Code == input.Code.Trim(), cancellationToken))
        {
            return Conflict(new { message = "Mã phòng ban đã tồn tại." });
        }

        department.Code = input.Code.Trim();
        department.Name = input.Name.Trim();
        department.Description = input.Description?.Trim();
        await db.SaveChangesAsync(cancellationToken);
        return Ok(department);
    }

    [Authorize(Roles = RoleNames.Admin)]
    [HttpDelete("departments/{id:int}")]
    public async Task<IActionResult> DeleteDepartment(int id, CancellationToken cancellationToken)
    {
        var department = await db.Departments.FindAsync([id], cancellationToken);
        if (department is null)
        {
            return NotFound();
        }
        if (await db.Employees.AnyAsync(x => x.DepartmentId == id, cancellationToken))
        {
            return Conflict(new { message = "Phòng ban đang có nhân viên, không thể xóa." });
        }
        db.Departments.Remove(department);
        await db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpGet("positions")]
    public async Task<ActionResult<IReadOnlyList<Position>>> GetPositions(CancellationToken cancellationToken) =>
        Ok(await db.Positions.AsNoTracking().OrderBy(x => x.Id).ToListAsync(cancellationToken));

    [Authorize(Roles = RoleNames.Admin)]
    [HttpPost("positions")]
    public async Task<ActionResult<Position>> CreatePosition(PositionInput input, CancellationToken cancellationToken)
    {
        if (await db.Positions.AnyAsync(x => x.Code == input.Code.Trim(), cancellationToken))
        {
            return Conflict(new { message = "Mã chức vụ đã tồn tại." });
        }
        var position = new Position { Code = input.Code.Trim(), Name = input.Name.Trim() };
        db.Positions.Add(position);
        await db.SaveChangesAsync(cancellationToken);
        return CreatedAtAction(nameof(GetPositions), new { id = position.Id }, position);
    }

    [Authorize(Roles = RoleNames.Admin)]
    [HttpPut("positions/{id:int}")]
    public async Task<ActionResult<Position>> UpdatePosition(int id, PositionInput input, CancellationToken cancellationToken)
    {
        var position = await db.Positions.FindAsync([id], cancellationToken);
        if (position is null)
        {
            return NotFound();
        }
        if (await db.Positions.AnyAsync(x => x.Id != id && x.Code == input.Code.Trim(), cancellationToken))
        {
            return Conflict(new { message = "Mã chức vụ đã tồn tại." });
        }
        position.Code = input.Code.Trim();
        position.Name = input.Name.Trim();
        await db.SaveChangesAsync(cancellationToken);
        return Ok(position);
    }

    [Authorize(Roles = RoleNames.Admin)]
    [HttpDelete("positions/{id:int}")]
    public async Task<IActionResult> DeletePosition(int id, CancellationToken cancellationToken)
    {
        var position = await db.Positions.FindAsync([id], cancellationToken);
        if (position is null)
        {
            return NotFound();
        }
        if (await db.Employees.AnyAsync(x => x.PositionId == id, cancellationToken))
        {
            return Conflict(new { message = "Chức vụ đang được sử dụng, không thể xóa." });
        }
        db.Positions.Remove(position);
        await db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpGet("periods")]
    public async Task<ActionResult<IReadOnlyList<EvaluationPeriod>>> GetPeriods(CancellationToken cancellationToken) =>
        Ok(await db.Periods.AsNoTracking().OrderByDescending(x => x.Start).ToListAsync(cancellationToken));

    [Authorize(Roles = RoleNames.Admin)]
    [HttpPost("periods")]
    public async Task<ActionResult<EvaluationPeriod>> CreatePeriod(PeriodInput input, CancellationToken cancellationToken)
    {
        var error = await ValidatePeriod(input, null, cancellationToken);
        if (error is not null)
        {
            return error;
        }
        var period = new EvaluationPeriod
        {
            Name = input.Name.Trim(),
            Start = input.Start!.Value,
            End = input.End!.Value,
            Status = input.Status
        };
        db.Periods.Add(period);
        await db.SaveChangesAsync(cancellationToken);
        return CreatedAtAction(nameof(GetPeriods), new { id = period.Id }, period);
    }

    [Authorize(Roles = RoleNames.Admin)]
    [HttpPut("periods/{id:int}")]
    public async Task<ActionResult<EvaluationPeriod>> UpdatePeriod(int id, PeriodInput input, CancellationToken cancellationToken)
    {
        var period = await db.Periods.FindAsync([id], cancellationToken);
        if (period is null)
        {
            return NotFound();
        }
        var error = await ValidatePeriod(input, id, cancellationToken);
        if (error is not null)
        {
            return error;
        }
        period.Name = input.Name.Trim();
        period.Start = input.Start!.Value;
        period.End = input.End!.Value;
        period.Status = input.Status;
        await db.SaveChangesAsync(cancellationToken);
        return Ok(period);
    }

    [Authorize(Roles = RoleNames.Admin)]
    [HttpDelete("periods/{id:int}")]
    public async Task<IActionResult> DeletePeriod(int id, CancellationToken cancellationToken)
    {
        var period = await db.Periods.FindAsync([id], cancellationToken);
        if (period is null)
        {
            return NotFound();
        }
        if (await db.Kpis.AnyAsync(x => x.PeriodId == id, cancellationToken))
        {
            return Conflict(new { message = "Kỳ đánh giá đang có KPI, không thể xóa." });
        }
        db.Periods.Remove(period);
        await db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpGet("criteria")]
    public async Task<ActionResult<IReadOnlyList<Criterion>>> GetCriteria(CancellationToken cancellationToken) =>
        Ok(await db.Criteria.AsNoTracking().OrderBy(x => x.Id).ToListAsync(cancellationToken));

    [Authorize(Roles = RoleNames.Admin)]
    [HttpPost("criteria")]
    public async Task<ActionResult<Criterion>> CreateCriterion(CriterionInput input, CancellationToken cancellationToken)
    {
        var criterion = new Criterion
        {
            Name = input.Name.Trim(),
            Description = input.Description?.Trim(),
            DefaultWeight = input.DefaultWeight
        };
        db.Criteria.Add(criterion);
        await db.SaveChangesAsync(cancellationToken);
        return CreatedAtAction(nameof(GetCriteria), new { id = criterion.Id }, criterion);
    }

    [Authorize(Roles = RoleNames.Admin)]
    [HttpPut("criteria/{id:int}")]
    public async Task<ActionResult<Criterion>> UpdateCriterion(int id, CriterionInput input, CancellationToken cancellationToken)
    {
        var criterion = await db.Criteria.FindAsync([id], cancellationToken);
        if (criterion is null)
        {
            return NotFound();
        }
        criterion.Name = input.Name.Trim();
        criterion.Description = input.Description?.Trim();
        criterion.DefaultWeight = input.DefaultWeight;
        await db.SaveChangesAsync(cancellationToken);
        return Ok(criterion);
    }

    [Authorize(Roles = RoleNames.Admin)]
    [HttpDelete("criteria/{id:int}")]
    public async Task<IActionResult> DeleteCriterion(int id, CancellationToken cancellationToken)
    {
        var criterion = await db.Criteria.FindAsync([id], cancellationToken);
        if (criterion is null)
        {
            return NotFound();
        }
        if (await db.Kpis.AnyAsync(x => x.CriterionId == id, cancellationToken))
        {
            return Conflict(new { message = "Tiêu chí đã được sử dụng trong KPI, không thể xóa." });
        }
        db.Criteria.Remove(criterion);
        await db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    private async Task<ActionResult?> ValidatePeriod(
        PeriodInput input,
        int? currentId,
        CancellationToken cancellationToken)
    {
        if (input.Start > input.End)
        {
            return BadRequest(new { message = "Ngày bắt đầu phải trước hoặc bằng ngày kết thúc." });
        }
        if (await db.Periods.AnyAsync(
                x => x.Id != currentId && x.Name == input.Name.Trim(),
                cancellationToken))
        {
            return Conflict(new { message = "Tên kỳ đánh giá đã tồn tại." });
        }
        return null;
    }
}
