namespace QLNS_KPI.Security;

public static class RoleNames
{
    public const string Admin = "Quản trị viên";
    public const string Manager = "Người quản lý";
    public const string Employee = "Nhân viên";
    public const string AdminOrManager = Admin + "," + Manager;
}
