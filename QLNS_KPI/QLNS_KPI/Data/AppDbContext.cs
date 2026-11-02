using Microsoft.EntityFrameworkCore;
using QLNS_KPI.Models;

namespace QLNS_KPI.Data;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<Position> Positions => Set<Position>();
    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<EvaluationPeriod> Periods => Set<EvaluationPeriod>();
    public DbSet<Criterion> Criteria => Set<Criterion>();
    public DbSet<Kpi> Kpis => Set<Kpi>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Employee>(entity =>
        {
            entity.ToTable("NhanVien", "dbo");
            entity.HasIndex(x => x.Code).IsUnique();
            entity.HasIndex(x => x.Email).IsUnique();
            entity.HasIndex(x => x.DepartmentId);
            entity.HasIndex(x => x.PositionId);
            entity.Property(x => x.Code).HasColumnName("Ma").HasMaxLength(20);
            entity.Property(x => x.Name).HasColumnName("HoTen").HasMaxLength(150);
            entity.Property(x => x.Email).HasMaxLength(150);
            entity.Property(x => x.Phone).HasColumnName("DienThoai").HasMaxLength(20);
            entity.Property(x => x.DepartmentId).HasColumnName("PhongBanId");
            entity.Property(x => x.PositionId).HasColumnName("ChucVuId");
            entity.Property(x => x.Status).HasColumnName("TrangThai").HasMaxLength(30);
            entity.HasOne(x => x.Department).WithMany().HasForeignKey(x => x.DepartmentId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Position).WithMany().HasForeignKey(x => x.PositionId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Department>(entity =>
        {
            entity.ToTable("PhongBan", "dbo");
            entity.HasIndex(x => x.Code).IsUnique();
            entity.Property(x => x.Code).HasColumnName("Ma").HasMaxLength(20);
            entity.Property(x => x.Name).HasColumnName("Ten").HasMaxLength(150);
            entity.Property(x => x.Description).HasColumnName("MoTa").HasMaxLength(500);
        });

        modelBuilder.Entity<Position>(entity =>
        {
            entity.ToTable("ChucVu", "dbo");
            entity.HasIndex(x => x.Code).IsUnique();
            entity.Property(x => x.Code).HasColumnName("Ma").HasMaxLength(30);
            entity.Property(x => x.Name).HasColumnName("Ten").HasMaxLength(100);
        });

        modelBuilder.Entity<Account>(entity =>
        {
            entity.ToTable("TaiKhoan", "dbo");
            entity.HasIndex(x => x.Username).IsUnique();
            entity.HasIndex(x => x.EmployeeId).IsUnique();
            entity.Property(x => x.Username).HasColumnName("TenDangNhap").HasMaxLength(50);
            entity.Property(x => x.PasswordHash).HasColumnName("MatKhauHash").HasColumnType("char(64)").IsFixedLength();
            entity.Property(x => x.EmployeeId).HasColumnName("NhanVienId");
            entity.Property(x => x.RoleId).HasColumnName("VaiTroId");
            entity.Property(x => x.Status).HasColumnName("TrangThai").HasMaxLength(20);
            entity.Property(x => x.MustChangePassword).HasColumnName("DoiMatKhauLanDau").HasDefaultValue(false);
            entity.Property(x => x.TemporaryPasswordUsed).HasColumnName("MatKhauTamDaDung").HasDefaultValue(false);
            entity.HasOne(x => x.Employee).WithMany().HasForeignKey(x => x.EmployeeId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Role).WithMany().HasForeignKey(x => x.RoleId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.ToTable("VaiTro", "dbo");
            entity.HasIndex(x => x.Code).IsUnique();
            entity.Property(x => x.Code).HasColumnName("Ma").HasMaxLength(30);
            entity.Property(x => x.Name).HasColumnName("Ten").HasMaxLength(80);
            entity.Property(x => x.Description).HasColumnName("MoTa").HasMaxLength(400);
        });

        modelBuilder.Entity<Permission>(entity =>
        {
            entity.ToTable("Quyen", "dbo");
            entity.HasIndex(x => x.Code).IsUnique();
            entity.Property(x => x.Code).HasColumnName("Ma").HasMaxLength(50);
            entity.Property(x => x.Name).HasColumnName("Ten").HasMaxLength(150);
            entity.Property(x => x.Group).HasColumnName("Nhom").HasMaxLength(80);
        });

        modelBuilder.Entity<RolePermission>(entity =>
        {
            entity.ToTable("VaiTroQuyen", "dbo");
            entity.HasKey(x => new { x.RoleId, x.PermissionId });
            entity.Property(x => x.RoleId).HasColumnName("VaiTroId");
            entity.Property(x => x.PermissionId).HasColumnName("QuyenId");
            entity.HasOne(x => x.Role).WithMany(x => x.RolePermissions).HasForeignKey(x => x.RoleId);
            entity.HasOne(x => x.Permission).WithMany(x => x.RolePermissions).HasForeignKey(x => x.PermissionId);
        });

        modelBuilder.Entity<EvaluationPeriod>(entity =>
        {
            entity.ToTable("KyDanhGia", "dbo");
            entity.HasIndex(x => x.Name).IsUnique();
            entity.Property(x => x.Name).HasColumnName("Ten").HasMaxLength(100);
            entity.Property(x => x.Start).HasColumnName("NgayBatDau");
            entity.Property(x => x.End).HasColumnName("NgayKetThuc");
            entity.Property(x => x.Status).HasColumnName("TrangThai").HasMaxLength(30);
        });

        modelBuilder.Entity<Criterion>(entity =>
        {
            entity.ToTable("TieuChiKpi", "dbo");
            entity.HasIndex(x => x.Name).IsUnique();
            entity.Property(x => x.Name).HasColumnName("Ten").HasMaxLength(150);
            entity.Property(x => x.Description).HasColumnName("MoTa").HasMaxLength(500);
            entity.Property(x => x.DefaultWeight).HasColumnName("TrongSoMacDinh").HasPrecision(5, 2);
        });

        modelBuilder.Entity<Kpi>(entity =>
        {
            entity.ToTable("Kpi", "dbo");
            entity.HasIndex(x => new { x.PeriodId, x.EmployeeId, x.CriterionId }).IsUnique();
            entity.HasIndex(x => x.PeriodId);
            entity.HasIndex(x => x.EmployeeId);
            entity.Property(x => x.PeriodId).HasColumnName("KyDanhGiaId");
            entity.Property(x => x.EmployeeId).HasColumnName("NhanVienId");
            entity.Property(x => x.CriterionId).HasColumnName("TieuChiId");
            entity.Property(x => x.Target).HasColumnName("MucTieu").HasMaxLength(500);
            entity.Property(x => x.Weight).HasColumnName("TrongSo").HasPrecision(5, 2);
            entity.Property(x => x.Result).HasColumnName("KetQua").HasPrecision(10, 2);
            entity.Property(x => x.Note).HasColumnName("GhiChu").HasMaxLength(500);
            entity.Property(x => x.Score).HasColumnName("Diem").HasPrecision(5, 2);
            entity.Property(x => x.Status).HasColumnName("TrangThai").HasMaxLength(30);
            entity.Property(x => x.Published).HasColumnName("DaCongBo");
            entity.Property(x => x.EvaluatorEmployeeId).HasColumnName("NguoiDanhGiaId");
            entity.HasOne(x => x.Period).WithMany().HasForeignKey(x => x.PeriodId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Employee).WithMany().HasForeignKey(x => x.EmployeeId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Criterion).WithMany().HasForeignKey(x => x.CriterionId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Evaluator).WithMany().HasForeignKey(x => x.EvaluatorEmployeeId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
