using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Mail;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using QLNS_KPI.Contracts;
using QLNS_KPI.Data;
using QLNS_KPI.Models;
using QLNS_KPI.Security;
using QLNS_KPI.Services;

namespace QLNS_KPI.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(
    AppDbContext db,
    IPasswordHasher passwordHasher,
    IConfiguration configuration,
    ITemporaryPasswordEmailSender emailSender,
    ILogger<AuthController> logger) : ControllerBase
{
    [AllowAnonymous]
    [HttpPost("initialize")]
    public async Task<ActionResult<AccountDto>> InitializeAdmin(
        InitializeAdminRequest request,
        CancellationToken cancellationToken)
    {
        var setupSecret = configuration["InitialAdmin:SetupSecret"];
        if (string.IsNullOrWhiteSpace(setupSecret)
            || Encoding.UTF8.GetByteCount(setupSecret) < 32
            || !CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(setupSecret),
                Encoding.UTF8.GetBytes(request.SetupSecret)))
        {
            return NotFound();
        }

        await using var transaction = await db.Database.BeginTransactionAsync(
            IsolationLevel.Serializable,
            cancellationToken);
        if (await db.Accounts.AnyAsync(cancellationToken))
        {
            return Conflict(new { message = "Hệ thống đã có tài khoản; không thể khởi tạo quản trị viên." });
        }
        var employee = request.EmployeeId.HasValue
            ? await db.Employees.SingleOrDefaultAsync(x => x.Id == request.EmployeeId.Value, cancellationToken)
            : null;
        if (request.EmployeeId.HasValue && employee is null)
        {
            return BadRequest(new { message = "Nhân viên liên kết không tồn tại." });
        }
        if (employee is not null && employee.Status != "Đang làm việc")
        {
            return BadRequest(new { message = "Nhân viên liên kết đã ngừng hoạt động." });
        }
        if (employee is null)
        {
            if (string.IsNullOrWhiteSpace(request.EmployeeCode)
                || string.IsNullOrWhiteSpace(request.EmployeeName)
                || string.IsNullOrWhiteSpace(request.EmployeeEmail)
                || (!request.DepartmentId.HasValue
                    && (string.IsNullOrWhiteSpace(request.DepartmentCode) || string.IsNullOrWhiteSpace(request.DepartmentName)))
                || (!request.PositionId.HasValue
                    && (string.IsNullOrWhiteSpace(request.PositionCode) || string.IsNullOrWhiteSpace(request.PositionName))))
            {
                return BadRequest(new
                {
                    message = "Cần nhập thông tin nhân viên và chọn hoặc tạo phòng ban, chức vụ cho quản trị viên đầu tiên."
                });
            }
            if (await db.Employees.AnyAsync(
                    x => x.Code == request.EmployeeCode.Trim() || x.Email == request.EmployeeEmail.Trim(),
                    cancellationToken))
            {
                return Conflict(new { message = "Mã nhân viên hoặc email đã tồn tại." });
            }

            var department = request.DepartmentId.HasValue
                ? await db.Departments.SingleOrDefaultAsync(x => x.Id == request.DepartmentId.Value, cancellationToken)
                : null;
            if (request.DepartmentId.HasValue && department is null)
            {
                return BadRequest(new { message = "Phòng ban liên kết không tồn tại." });
            }
            if (department is null)
            {
                if (await db.Departments.AnyAsync(x => x.Code == request.DepartmentCode.Trim(), cancellationToken))
                {
                    return Conflict(new { message = "Mã phòng ban đã tồn tại." });
                }
                department = new Department
                {
                    Code = request.DepartmentCode.Trim(),
                    Name = request.DepartmentName.Trim()
                };
                db.Departments.Add(department);
            }

            var position = request.PositionId.HasValue
                ? await db.Positions.SingleOrDefaultAsync(x => x.Id == request.PositionId.Value, cancellationToken)
                : null;
            if (request.PositionId.HasValue && position is null)
            {
                return BadRequest(new { message = "Chức vụ liên kết không tồn tại." });
            }
            if (position is null)
            {
                if (await db.Positions.AnyAsync(x => x.Code == request.PositionCode.Trim(), cancellationToken))
                {
                    return Conflict(new { message = "Mã chức vụ đã tồn tại." });
                }
                position = new Position
                {
                    Code = request.PositionCode.Trim(),
                    Name = request.PositionName.Trim()
                };
                db.Positions.Add(position);
            }
            await db.SaveChangesAsync(cancellationToken);

            employee = new Employee
            {
                Code = request.EmployeeCode.Trim(),
                Name = request.EmployeeName.Trim(),
                Email = request.EmployeeEmail.Trim(),
                DepartmentId = department.Id,
                PositionId = position.Id
            };
            db.Employees.Add(employee);
            await db.SaveChangesAsync(cancellationToken);
        }

        var adminRole = await db.Roles.SingleOrDefaultAsync(x => x.Name == RoleNames.Admin, cancellationToken);
        if (adminRole is null)
        {
            return Conflict(new { message = "Chưa có vai trò quản trị viên trong bảng VaiTro." });
        }

        var account = new Account
        {
            Username = request.Username.Trim(),
            PasswordHash = passwordHasher.Hash(request.Password),
            EmployeeId = employee.Id,
            RoleId = adminRole.Id,
            Role = adminRole,
            Status = "Hoạt động"
        };
        db.Accounts.Add(account);
        await db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return StatusCode(StatusCodes.Status201Created, ApiMapper.ToDto(account));
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login(LoginRequest request, CancellationToken cancellationToken)
    {
        var account = await db.Accounts
            .Include(x => x.Employee)
            .Include(x => x.Role)
            .SingleOrDefaultAsync(x => x.Username == request.Username.Trim(), cancellationToken);

        if (account is null || account.Status != "Hoạt động"
            || !passwordHasher.Verify(request.Password, account.PasswordHash)
            || (account.MustChangePassword && account.TemporaryPasswordUsed)
            || account.Employee is null
            || account.Employee.Status != "Đang làm việc"
            || account.Role is null)
        {
            return Unauthorized(new { message = "Tên đăng nhập hoặc mật khẩu không chính xác." });
        }

        if (account.MustChangePassword)
        {
            var consumed = await db.Accounts
                .Where(x => x.Id == account.Id
                    && x.MustChangePassword
                    && !x.TemporaryPasswordUsed
                    && x.PasswordHash == account.PasswordHash)
                .ExecuteUpdateAsync(
                    updates => updates.SetProperty(x => x.TemporaryPasswordUsed, true),
                    cancellationToken);
            if (consumed != 1)
            {
                return Unauthorized(new { message = "Tên đăng nhập hoặc mật khẩu không chính xác." });
            }
        }
        else if (passwordHasher.NeedsRehash(account.PasswordHash))
        {
            account.PasswordHash = passwordHasher.Hash(request.Password);
            await db.SaveChangesAsync(cancellationToken);
        }

        var issuedAt = DateTime.UtcNow;
        var expiresAt = issuedAt.AddHours(8);
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, account.Id.ToString()),
            new Claim(ClaimTypes.Name, account.Username),
            new Claim(ClaimTypes.Role, account.Role.Name),
            new Claim("employeeId", account.EmployeeId.ToString()),
            new Claim("displayName", account.Employee.Name)
        };
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:SigningKey"]!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: configuration["Jwt:Issuer"] ?? "QLNS_KPI",
            audience: configuration["Jwt:Audience"] ?? "QLNS_KPI_Frontend",
            claims: claims,
            notBefore: issuedAt,
            expires: expiresAt,
            signingCredentials: credentials);

        return Ok(new LoginResponse(
            new JwtSecurityTokenHandler().WriteToken(token),
            expiresAt,
            new SessionUserDto(
                account.Username,
                account.Role.Name,
                account.EmployeeId,
                account.Employee.Name,
                account.MustChangePassword)));
    }

    [AllowAnonymous]
    [EnableRateLimiting("password-recovery")]
    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword(
        ForgotPasswordRequest request,
        CancellationToken cancellationToken)
    {
        const string responseMessage =
            "Nếu thông tin khớp với tài khoản đang hoạt động, hướng dẫn đăng nhập sẽ được gửi tới email đã đăng ký.";
        var username = request.Username.Trim();
        var email = request.Email.Trim();
        var account = await db.Accounts
            .Include(x => x.Employee)
            .SingleOrDefaultAsync(
                x => x.Username == username
                    && x.Employee!.Email == email
                    && x.Status == "Hoạt động"
                    && x.Employee.Status == "Đang làm việc",
                cancellationToken);
        if (account?.Employee is null)
        {
            return Ok(new { message = responseMessage });
        }

        var temporaryPassword = TemporaryPasswordGenerator.Generate();
        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
        account.PasswordHash = passwordHasher.Hash(temporaryPassword);
        account.MustChangePassword = true;
        account.TemporaryPasswordUsed = false;
        await db.SaveChangesAsync(cancellationToken);

        try
        {
            await emailSender.SendAsync(email, account.Username, temporaryPassword, cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch (Exception exception) when (
            exception is SmtpException or InvalidOperationException or FormatException)
        {
            await transaction.RollbackAsync(cancellationToken);
            logger.LogError(exception, "Could not send a password recovery email for account {AccountId}.", account.Id);
            return Ok(new { message = responseMessage });
        }

        return Ok(new { message = responseMessage });
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<SessionUserDto>> Me(CancellationToken cancellationToken)
    {
        var account = await db.Accounts
            .Include(x => x.Employee)
            .Include(x => x.Role)
            .SingleOrDefaultAsync(x => x.Id == User.GetAccountId(), cancellationToken);
        if (account is null || account.Status != "Hoạt động" || account.Employee is null || account.Role is null)
        {
            return Unauthorized();
        }

        return Ok(new SessionUserDto(
            account.Username,
            account.Role.Name,
            account.EmployeeId,
            account.Employee.Name,
            account.MustChangePassword));
    }

    [Authorize]
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword(ChangePasswordRequest request, CancellationToken cancellationToken)
    {
        var account = await db.Accounts.SingleOrDefaultAsync(
            x => x.Id == User.GetAccountId(),
            cancellationToken);
        if (account is null || account.Status != "Hoạt động")
        {
            return Unauthorized();
        }

        if (!passwordHasher.Verify(request.CurrentPassword, account.PasswordHash)
            || (account.MustChangePassword && !account.TemporaryPasswordUsed))
        {
            return BadRequest(new { message = "Mật khẩu hiện tại không chính xác." });
        }

        if (passwordHasher.Verify(request.NewPassword, account.PasswordHash))
        {
            return BadRequest(new { message = "Mật khẩu mới phải khác mật khẩu hiện tại." });
        }

        account.PasswordHash = passwordHasher.Hash(request.NewPassword);
        account.MustChangePassword = false;
        account.TemporaryPasswordUsed = false;
        await db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }
}
