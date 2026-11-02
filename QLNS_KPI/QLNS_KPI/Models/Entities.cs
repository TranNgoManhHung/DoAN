namespace QLNS_KPI.Models;

public sealed class Employee
{
    public int Id { get; set; }
    public required string Code { get; set; }
    public required string Name { get; set; }
    public required string Email { get; set; }
    public string? Phone { get; set; }
    public int DepartmentId { get; set; }
    public int PositionId { get; set; }
    public string Status { get; set; } = "Đang làm việc";
    public Department? Department { get; set; }
    public Position? Position { get; set; }
}

public sealed class Department
{
    public int Id { get; set; }
    public required string Code { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
}

public sealed class Position
{
    public int Id { get; set; }
    public required string Code { get; set; }
    public required string Name { get; set; }
}

public sealed class Account
{
    public int Id { get; set; }
    public required string Username { get; set; }
    public required string PasswordHash { get; set; }
    public int EmployeeId { get; set; }
    public int RoleId { get; set; }
    public string Status { get; set; } = "Hoạt động";
    public bool MustChangePassword { get; set; }
    public bool TemporaryPasswordUsed { get; set; }
    public Employee? Employee { get; set; }
    public Role? Role { get; set; }
}

public sealed class Role
{
    public int Id { get; set; }
    public required string Code { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public ICollection<RolePermission> RolePermissions { get; set; } = [];
}

public sealed class Permission
{
    public int Id { get; set; }
    public required string Code { get; set; }
    public required string Name { get; set; }
    public required string Group { get; set; }
    public ICollection<RolePermission> RolePermissions { get; set; } = [];
}

public sealed class RolePermission
{
    public int RoleId { get; set; }
    public int PermissionId { get; set; }
    public Role? Role { get; set; }
    public Permission? Permission { get; set; }
}

public sealed class EvaluationPeriod
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public DateOnly Start { get; set; }
    public DateOnly End { get; set; }
    public string Status { get; set; } = "Chưa bắt đầu";
}

public sealed class Criterion
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public decimal DefaultWeight { get; set; }
}

public sealed class Kpi
{
    public int Id { get; set; }
    public int PeriodId { get; set; }
    public int EmployeeId { get; set; }
    public int CriterionId { get; set; }
    public required string Target { get; set; }
    public decimal Weight { get; set; }
    public decimal? Result { get; set; }
    public string? Note { get; set; }
    public decimal? Score { get; set; }
    public string Status { get; set; } = "Chờ cập nhật";
    public bool Published { get; set; }
    public int? EvaluatorEmployeeId { get; set; }
    public EvaluationPeriod? Period { get; set; }
    public Employee? Employee { get; set; }
    public Criterion? Criterion { get; set; }
    public Employee? Evaluator { get; set; }
}
