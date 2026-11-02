using System.Net.Sockets;
using MailKit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLNS_KPI.Contracts;
using QLNS_KPI.Data;
using QLNS_KPI.Models;
using QLNS_KPI.Security;
using QLNS_KPI.Services;

namespace QLNS_KPI.Controllers;

[ApiController]
[Authorize(Roles = RoleNames.Admin)]
[Route("api/accounts")]
public sealed class AccountsController(
    AppDbContext db,
    IPasswordHasher passwordHasher,
    ITemporaryPasswordEmailSender emailSender,
    ILogger<AccountsController> logger) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<AccountDto>>> GetAll(CancellationToken cancellationToken) =>
        Ok((await db.Accounts.AsNoTracking().Include(x => x.Role).OrderBy(x => x.Id).ToListAsync(cancellationToken))
            .Select(ApiMapper.ToDto).ToArray());

    [HttpPost]
    public async Task<ActionResult<AccountCreationResultDto>> Create(
        AccountInput input,
        CancellationToken cancellationToken)
    {
        var employee = await db.Employees.SingleOrDefaultAsync(
            x => x.Id == input.EmployeeId && x.Status == "Đang làm việc",
            cancellationToken);
        if (employee is null)
        {
            return BadRequest(new { message = "Nhân viên liên kết không tồn tại hoặc đã ngừng hoạt động." });
        }

        //if (await db.Accounts.AnyAsync(x => x.Username == input.Username.Trim(), cancellationToken))
        //{
        //    return Conflict(new { message = "Tên đăng nhập đã tồn tại." });
        //}
        if (await db.Accounts.AnyAsync(x => x.EmployeeId == input.EmployeeId, cancellationToken))
        {
            return Conflict(new { message = "Nhân viên này đã có tài khoản." });
        }
        var role = await db.Roles.SingleOrDefaultAsync(x => x.Name == input.Role, cancellationToken);
        if (role is null)
        {
            return BadRequest(new { message = "Vai trò không tồn tại trong bảng VaiTro." });
        }

        var temporaryPassword = TemporaryPasswordGenerator.Generate();
        var account = new Account
        {
            Username = input.Username.Trim(),
            PasswordHash = passwordHasher.Hash(temporaryPassword),
            EmployeeId = input.EmployeeId,
            RoleId = role.Id,
            Role = role,
            Status = input.Status,
            MustChangePassword = true,
            TemporaryPasswordUsed = false
        };

        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
        db.Accounts.Add(account);
        await db.SaveChangesAsync(cancellationToken);

        try
        {
            await emailSender.SendAsync(employee.Email, account.Username, temporaryPassword, cancellationToken);
        }
        catch (Exception exception) when (exception is
            InvalidOperationException or
            FormatException or
            ProtocolException or
            IOException or
            SocketException)
        {
            logger.LogError(exception, "Could not send a temporary password email for account {AccountId}.", account.Id);
            await transaction.RollbackAsync(cancellationToken);
            return StatusCode(
                StatusCodes.Status503ServiceUnavailable,
                new { message = "Không gửi được email mật khẩu tạm thời; tài khoản chưa được tạo. Vui lòng kiểm tra cấu hình email và thử lại." });
        }

        await transaction.CommitAsync(cancellationToken);
        return CreatedAtAction(
            nameof(GetAll),
            new { id = account.Id },
            new AccountCreationResultDto(ApiMapper.ToDto(account), true));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<AccountDto>> Update(int id, AccountUpdateInput input, CancellationToken cancellationToken)
    {
        var account = await db.Accounts.Include(x => x.Role)
            .SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (account is null)
        {
            return NotFound();
        }

        if (await db.Accounts.AnyAsync(
                x => x.Id != id && x.Username == input.Username.Trim(),
                cancellationToken))
        {
            return Conflict(new { message = "Tên đăng nhập đã tồn tại." });
        }

        if (!await db.Employees.AnyAsync(
                x => x.Id == input.EmployeeId && x.Status == "Đang làm việc",
                cancellationToken))
        {
            return BadRequest(new { message = "Nhân viên liên kết không tồn tại hoặc đã ngừng hoạt động." });
        }
        if (await db.Accounts.AnyAsync(x => x.Id != id && x.EmployeeId == input.EmployeeId, cancellationToken))
        {
            return Conflict(new { message = "Nhân viên này đã được liên kết với tài khoản khác." });
        }
        var role = await db.Roles.SingleOrDefaultAsync(x => x.Name == input.Role, cancellationToken);
        if (role is null)
        {
            return BadRequest(new { message = "Vai trò không tồn tại trong bảng VaiTro." });
        }

        if (account.Id == User.GetAccountId() && input.Status != "Hoạt động")
        {
            return BadRequest(new { message = "Không thể khóa tài khoản đang đăng nhập." });
        }
        if (account.Role?.Name == RoleNames.Admin && account.Status == "Hoạt động"
            && (input.Role != RoleNames.Admin || input.Status != "Hoạt động")
            && !await HasOtherActiveAdmin(account.Id, cancellationToken))
        {
            return Conflict(new { message = "Không thể vô hiệu hóa quản trị viên đang hoạt động cuối cùng." });
        }

        account.Username = input.Username.Trim();
        account.EmployeeId = input.EmployeeId;
        account.RoleId = role.Id;
        account.Role = role;
        account.Status = input.Status;
        await db.SaveChangesAsync(cancellationToken);
        return Ok(ApiMapper.ToDto(account));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        if (id == User.GetAccountId())
        {
            return BadRequest(new { message = "Không thể xóa tài khoản đang đăng nhập." });
        }

        var account = await db.Accounts.Include(x => x.Role)
            .SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (account is null)
        {
            return NotFound();
        }
        if (account.Role?.Name == RoleNames.Admin && account.Status == "Hoạt động"
            && !await HasOtherActiveAdmin(account.Id, cancellationToken))
        {
            return Conflict(new { message = "Không thể xóa quản trị viên đang hoạt động cuối cùng." });
        }

        db.Accounts.Remove(account);
        await db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    private Task<bool> HasOtherActiveAdmin(int accountId, CancellationToken cancellationToken) =>
        db.Accounts.AnyAsync(
            x => x.Id != accountId && x.Role!.Name == RoleNames.Admin && x.Status == "Hoạt động",
            cancellationToken);
}
