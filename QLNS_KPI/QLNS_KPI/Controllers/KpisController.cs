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
[Route("api/kpis")]
public sealed class KpisController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<KpiDto>>> GetAll(CancellationToken cancellationToken)
    {
        var mayViewAll = User.IsInRole(RoleNames.Admin) || User.IsInRole(RoleNames.Manager);
        var query = db.Kpis.AsNoTracking();
        if (!mayViewAll)
        {
            var employeeId = int.Parse(User.FindFirst("employeeId")!.Value);
            query = query.Where(x => x.EmployeeId == employeeId);
        }

        var records = await query.OrderBy(x => x.Id).ToListAsync(cancellationToken);
        return Ok(records.Select(x => ApiMapper.ToDto(x, mayViewAll)).ToArray());
    }

    [Authorize(Roles = RoleNames.AdminOrManager)]
    [HttpPost]
    public async Task<ActionResult<KpiDto>> Create(KpiInput input, CancellationToken cancellationToken)
    {
        var error = await ValidateKpi(input, null, cancellationToken);
        if (error is not null)
        {
            return error;
        }

        var kpi = new Kpi
        {
            PeriodId = input.PeriodId,
            EmployeeId = input.EmployeeId,
            CriterionId = input.CriterionId,
            Target = input.Target.Trim(),
            Weight = input.Weight,
            Status = "Chờ cập nhật"
        };
        db.Kpis.Add(kpi);
        await db.SaveChangesAsync(cancellationToken);
        return CreatedAtAction(nameof(GetAll), new { id = kpi.Id }, ApiMapper.ToDto(kpi));
    }

    [Authorize(Roles = RoleNames.AdminOrManager)]
    [HttpPut("{id:int}")]
    public async Task<ActionResult<KpiDto>> Update(int id, KpiInput input, CancellationToken cancellationToken)
    {
        var kpi = await db.Kpis.FindAsync([id], cancellationToken);
        if (kpi is null)
        {
            return NotFound();
        }

        var error = await ValidateKpi(input, id, cancellationToken);
        if (error is not null)
        {
            return error;
        }

        var assignmentChanged = kpi.PeriodId != input.PeriodId
            || kpi.EmployeeId != input.EmployeeId
            || kpi.CriterionId != input.CriterionId
            || kpi.Target != input.Target.Trim()
            || kpi.Weight != input.Weight;
        kpi.PeriodId = input.PeriodId;
        kpi.EmployeeId = input.EmployeeId;
        kpi.CriterionId = input.CriterionId;
        kpi.Target = input.Target.Trim();
        kpi.Weight = input.Weight;
        if (assignmentChanged)
        {
            kpi.Result = null;
            kpi.Note = null;
            kpi.Score = null;
            kpi.Status = "Chờ cập nhật";
            kpi.Published = false;
        }

        await db.SaveChangesAsync(cancellationToken);
        return Ok(ApiMapper.ToDto(kpi));
    }

    [Authorize(Roles = RoleNames.AdminOrManager)]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var kpi = await db.Kpis.FindAsync([id], cancellationToken);
        if (kpi is null)
        {
            return NotFound();
        }

        db.Kpis.Remove(kpi);
        await db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [Authorize(Roles = RoleNames.Employee)]
    [HttpPut("{id:int}/result")]
    public async Task<ActionResult<KpiDto>> UpdateResult(
        int id,
        KpiResultInput input,
        CancellationToken cancellationToken)
    {
        var employeeId = int.Parse(User.FindFirst("employeeId")!.Value);
        var kpi = await db.Kpis.Include(x => x.Period)
            .SingleOrDefaultAsync(x => x.Id == id && x.EmployeeId == employeeId, cancellationToken);
        if (kpi is null)
        {
            return NotFound();
        }
        if (kpi.Period is null || kpi.Period.End < DateOnly.FromDateTime(DateTime.UtcNow))
        {
            return Conflict(new { message = "Kỳ đánh giá đã kết thúc, không thể cập nhật." });
        }

        kpi.Result = input.Result;
        kpi.Note = input.Note?.Trim();
        kpi.Status = "Chờ đánh giá";
        kpi.Score = null;
        kpi.Published = false;
        await db.SaveChangesAsync(cancellationToken);
        return Ok(ApiMapper.ToDto(kpi, false));
    }

    [Authorize(Roles = RoleNames.AdminOrManager)]
    [HttpPut("{id:int}/evaluation")]
    public async Task<ActionResult<KpiDto>> Evaluate(
        int id,
        KpiEvaluationInput input,
        CancellationToken cancellationToken)
    {
        var kpi = await db.Kpis.FindAsync([id], cancellationToken);
        if (kpi is null)
        {
            return NotFound();
        }
        if (!kpi.Result.HasValue)
        {
            return Conflict(new { message = "Nhân viên chưa cập nhật kết quả KPI." });
        }

        kpi.Score = input.Score;
        kpi.Status = "Đã đánh giá";
        kpi.Published = input.Published;
        kpi.EvaluatorEmployeeId = int.Parse(User.FindFirst("employeeId")!.Value);
        await db.SaveChangesAsync(cancellationToken);
        return Ok(ApiMapper.ToDto(kpi));
    }

    private async Task<ActionResult?> ValidateKpi(
        KpiInput input,
        int? currentId,
        CancellationToken cancellationToken)
    {
        if (!await db.Periods.AnyAsync(x => x.Id == input.PeriodId, cancellationToken))
        {
            return BadRequest(new { message = "Kỳ đánh giá không tồn tại." });
        }
        if (!await db.Employees.AnyAsync(
                x => x.Id == input.EmployeeId && x.Status == "Đang làm việc",
                cancellationToken))
        {
            return BadRequest(new { message = "Nhân viên không tồn tại hoặc đã ngừng hoạt động." });
        }
        if (!await db.Criteria.AnyAsync(x => x.Id == input.CriterionId, cancellationToken))
        {
            return BadRequest(new { message = "Tiêu chí KPI không tồn tại." });
        }
        if (await db.Kpis.AnyAsync(
                x => x.Id != currentId
                    && x.PeriodId == input.PeriodId
                    && x.EmployeeId == input.EmployeeId
                    && x.CriterionId == input.CriterionId,
                cancellationToken))
        {
            return Conflict(new { message = "KPI này đã được thiết lập cho nhân viên trong kỳ." });
        }
        return null;
    }
}
