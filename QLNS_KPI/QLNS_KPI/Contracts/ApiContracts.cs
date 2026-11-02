using System.ComponentModel.DataAnnotations;
using QLNS_KPI.Models;

namespace QLNS_KPI.Contracts;

public sealed class LoginRequest
{
    [Required, StringLength(50)]
    public string Username { get; set; } = "";

    [Required, StringLength(200)]
    public string Password { get; set; } = "";
}

public sealed class ForgotPasswordRequest
{
    [Required, StringLength(50)]
    public string Username { get; set; } = "";

    [Required, EmailAddress, StringLength(150)]
    public string Email { get; set; } = "";
}

public sealed class InitializeAdminRequest
{
    [Required, MinLength(32), StringLength(256)]
    public string SetupSecret { get; set; } = "";

    [Required, StringLength(50)]
    public string Username { get; set; } = "";

    [Required, MinLength(6), StringLength(200)]
    public string Password { get; set; } = "";

    public int? EmployeeId { get; set; }

    [StringLength(20)]
    public string EmployeeCode { get; set; } = "";

    [StringLength(150)]
    public string EmployeeName { get; set; } = "";

    [EmailAddress, StringLength(150)]
    public string EmployeeEmail { get; set; } = "";

    public int? DepartmentId { get; set; }

    [StringLength(20)]
    public string DepartmentCode { get; set; } = "";

    [StringLength(150)]
    public string DepartmentName { get; set; } = "";

    public int? PositionId { get; set; }

    [StringLength(30)]
    public string PositionCode { get; set; } = "";

    [StringLength(100)]
    public string PositionName { get; set; } = "";
}

public sealed class ChangePasswordRequest
{
    [Required]
    public string CurrentPassword { get; set; } = "";

    [Required, MinLength(6), StringLength(200)]
    public string NewPassword { get; set; } = "";
}

public sealed class EmployeeInput
{
    [Required, StringLength(20)]
    public string Code { get; set; } = "";

    [Required, StringLength(150)]
    public string Name { get; set; } = "";

    [Required, EmailAddress, StringLength(150)]
    public string Email { get; set; } = "";

    [StringLength(20)]
    public string Phone { get; set; } = "";

    [Range(1, int.MaxValue)]
    public int DepartmentId { get; set; }

    [Range(1, int.MaxValue)]
    public int PositionId { get; set; }

    [Required, RegularExpression("^(Đang làm việc|Ngừng hoạt động)$")]
    public string Status { get; set; } = "Đang làm việc";
}

public sealed class DepartmentInput
{
    [Required, StringLength(20)]
    public string Code { get; set; } = "";

    [Required, StringLength(150)]
    public string Name { get; set; } = "";

    [StringLength(500)]
    public string? Description { get; set; }
}

public sealed class PositionInput
{
    [Required, StringLength(30)]
    public string Code { get; set; } = "";

    [Required, StringLength(100)]
    public string Name { get; set; } = "";
}

public sealed class PeriodInput
{
    [Required, StringLength(100)]
    public string Name { get; set; } = "";

    [Required]
    public DateOnly? Start { get; set; }

    [Required]
    public DateOnly? End { get; set; }

    [Required, RegularExpression("^(Chưa bắt đầu|Đang diễn ra|Đã kết thúc)$")]
    public string Status { get; set; } = "Chưa bắt đầu";
}

public sealed class CriterionInput
{
    [Required, StringLength(150)]
    public string Name { get; set; } = "";

    [StringLength(500)]
    public string? Description { get; set; }

    [Range(typeof(decimal), "0", "100")]
    public decimal DefaultWeight { get; set; }
}

public sealed class AccountInput
{
    [Required, StringLength(50)]
    public string Username { get; set; } = "";

    [Range(1, int.MaxValue)]
    public int EmployeeId { get; set; }

    [Required, RegularExpression("^(Quản trị viên|Người quản lý|Nhân viên)$")]
    public string Role { get; set; } = "Nhân viên";

    [Required, RegularExpression("^(Hoạt động|Bị khóa)$")]
    public string Status { get; set; } = "Hoạt động";
}

public sealed class AccountUpdateInput
{
    [Required, StringLength(50)]
    public string Username { get; set; } = "";

    [Range(1, int.MaxValue)]
    public int EmployeeId { get; set; }

    [Required, RegularExpression("^(Quản trị viên|Người quản lý|Nhân viên)$")]
    public string Role { get; set; } = "Nhân viên";

    [Required, RegularExpression("^(Hoạt động|Bị khóa)$")]
    public string Status { get; set; } = "Hoạt động";
}

public sealed class KpiInput
{
    [Range(1, int.MaxValue)]
    public int PeriodId { get; set; }

    [Range(1, int.MaxValue)]
    public int EmployeeId { get; set; }

    [Range(1, int.MaxValue)]
    public int CriterionId { get; set; }

    [Required, StringLength(500)]
    public string Target { get; set; } = "";

    [Range(typeof(decimal), "1", "100")]
    public decimal Weight { get; set; }
}

public sealed class KpiResultInput
{
    [Required]
    [Range(typeof(decimal), "0", "99999999.99")]
    public decimal Result { get; set; }

    [StringLength(500)]
    public string? Note { get; set; }
}

public sealed class KpiEvaluationInput
{
    [Required]
    [Range(typeof(decimal), "0", "100")]
    public decimal Score { get; set; }

    public bool Published { get; set; }
}

public sealed record SessionUserDto(
    string Username,
    string Role,
    int EmployeeId,
    string Name,
    bool MustChangePassword = false);
public sealed record LoginResponse(string Token, DateTime ExpiresAt, SessionUserDto User);
public sealed record AccountDto(int Id, string Username, int EmployeeId, string Role, string Status);
public sealed record AccountCreationResultDto(AccountDto Account, bool EmailSent);
public sealed record KpiDto(
    int Id,
    int PeriodId,
    int EmployeeId,
    int CriterionId,
    string Target,
    decimal Weight,
    decimal? Result,
    string? Note,
    decimal? Score,
    string Status,
    bool Published);

public sealed record BootstrapDto(
    IReadOnlyList<Employee> Employees,
    IReadOnlyList<Department> Departments,
    IReadOnlyList<Position> Positions,
    IReadOnlyList<AccountDto> Accounts,
    IReadOnlyList<EvaluationPeriod> Periods,
    IReadOnlyList<Criterion> Criteria,
    IReadOnlyList<KpiDto> Kpis);

public sealed record ReportEmployeeDto(
    int EmployeeId,
    string Name,
    int DepartmentId,
    string DepartmentName,
    int Count,
    int Done,
    decimal? Score);

public sealed record DashboardDto(
    int ActiveEmployees,
    int DepartmentCount,
    int PendingKpis,
    int EvaluatedKpis,
    int TotalKpis,
    IReadOnlyList<DepartmentCountDto> EmployeesByDepartment);

public sealed record DepartmentCountDto(int DepartmentId, string DepartmentName, int EmployeeCount);

public static class ApiMapper
{
    public static AccountDto ToDto(Account account) =>
        new(account.Id, account.Username, account.EmployeeId, account.Role?.Name ?? "", account.Status);

    public static KpiDto ToDto(Kpi kpi, bool mayViewUnpublishedScore = true) =>
        new(
            kpi.Id,
            kpi.PeriodId,
            kpi.EmployeeId,
            kpi.CriterionId,
            kpi.Target,
            kpi.Weight,
            kpi.Result,
            kpi.Note,
            mayViewUnpublishedScore || kpi.Published ? kpi.Score : null,
            kpi.Status,
            kpi.Published);
}
