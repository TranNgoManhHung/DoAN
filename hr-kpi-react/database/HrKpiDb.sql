/*
  HrKpiDb — CSDL cho hệ thống quản lý nhân sự & đánh giá KPI (HUMG)
  Tương ứng frontend React: employees, departments, positions, accounts,
  periods, criteria, kpis + vai trò/quyền.

  Chạy trong SSMS hoặc:
    sqlcmd -S localhost -E -i database\HrKpiDb.sql
*/
SET NOCOUNT ON;
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

IF DB_ID(N'HrKpiDb') IS NULL
BEGIN
    CREATE DATABASE HrKpiDb COLLATE Vietnamese_CI_AS;
END
GO

USE HrKpiDb;
GO

/* Xóa theo thứ tự phụ thuộc để chạy lại script an toàn */
IF OBJECT_ID(N'dbo.vw_BaoCaoKpiNhanVien', N'V') IS NOT NULL DROP VIEW dbo.vw_BaoCaoKpiNhanVien;
IF OBJECT_ID(N'dbo.Kpi', N'U') IS NOT NULL DROP TABLE dbo.Kpi;
IF OBJECT_ID(N'dbo.TaiKhoan', N'U') IS NOT NULL DROP TABLE dbo.TaiKhoan;
IF OBJECT_ID(N'dbo.VaiTroQuyen', N'U') IS NOT NULL DROP TABLE dbo.VaiTroQuyen;
IF OBJECT_ID(N'dbo.Quyen', N'U') IS NOT NULL DROP TABLE dbo.Quyen;
IF OBJECT_ID(N'dbo.VaiTro', N'U') IS NOT NULL DROP TABLE dbo.VaiTro;
IF OBJECT_ID(N'dbo.TieuChiKpi', N'U') IS NOT NULL DROP TABLE dbo.TieuChiKpi;
IF OBJECT_ID(N'dbo.KyDanhGia', N'U') IS NOT NULL DROP TABLE dbo.KyDanhGia;
IF OBJECT_ID(N'dbo.NhanVien', N'U') IS NOT NULL DROP TABLE dbo.NhanVien;
IF OBJECT_ID(N'dbo.ChucVu', N'U') IS NOT NULL DROP TABLE dbo.ChucVu;
IF OBJECT_ID(N'dbo.PhongBan', N'U') IS NOT NULL DROP TABLE dbo.PhongBan;
GO

CREATE TABLE dbo.PhongBan (
    Id          INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_PhongBan PRIMARY KEY,
    Ma          NVARCHAR(20)  NOT NULL,
    Ten         NVARCHAR(150) NOT NULL,
    MoTa        NVARCHAR(500) NULL,
    NgayTao     DATETIME2(0)  NOT NULL CONSTRAINT DF_PhongBan_NgayTao DEFAULT SYSUTCDATETIME(),
    CONSTRAINT UQ_PhongBan_Ma UNIQUE (Ma)
);

CREATE TABLE dbo.ChucVu (
    Id          INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_ChucVu PRIMARY KEY,
    Ma          NVARCHAR(30)  NOT NULL,
    Ten         NVARCHAR(100) NOT NULL,
    NgayTao     DATETIME2(0)  NOT NULL CONSTRAINT DF_ChucVu_NgayTao DEFAULT SYSUTCDATETIME(),
    CONSTRAINT UQ_ChucVu_Ma UNIQUE (Ma)
);

CREATE TABLE dbo.NhanVien (
    Id           INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_NhanVien PRIMARY KEY,
    Ma           NVARCHAR(20)  NOT NULL,
    HoTen        NVARCHAR(150) NOT NULL,
    Email        NVARCHAR(150) NOT NULL,
    DienThoai    NVARCHAR(20)  NULL,
    PhongBanId   INT           NOT NULL,
    ChucVuId     INT           NOT NULL,
    TrangThai    NVARCHAR(30)  NOT NULL CONSTRAINT DF_NhanVien_TrangThai DEFAULT N'Đang làm việc',
    NgayTao      DATETIME2(0)  NOT NULL CONSTRAINT DF_NhanVien_NgayTao DEFAULT SYSUTCDATETIME(),
    NgayCapNhat  DATETIME2(0)  NULL,
    CONSTRAINT UQ_NhanVien_Ma UNIQUE (Ma),
    CONSTRAINT UQ_NhanVien_Email UNIQUE (Email),
    CONSTRAINT CK_NhanVien_TrangThai CHECK (TrangThai IN (N'Đang làm việc', N'Ngừng hoạt động')),
    CONSTRAINT FK_NhanVien_PhongBan FOREIGN KEY (PhongBanId) REFERENCES dbo.PhongBan (Id),
    CONSTRAINT FK_NhanVien_ChucVu   FOREIGN KEY (ChucVuId)   REFERENCES dbo.ChucVu (Id)
);

CREATE TABLE dbo.VaiTro (
    Id      INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_VaiTro PRIMARY KEY,
    Ma      NVARCHAR(30)  NOT NULL,
    Ten     NVARCHAR(80)  NOT NULL,
    MoTa    NVARCHAR(400) NULL,
    CONSTRAINT UQ_VaiTro_Ma UNIQUE (Ma)
);

CREATE TABLE dbo.Quyen (
    Id      INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Quyen PRIMARY KEY,
    Ma      NVARCHAR(50)  NOT NULL,
    Ten     NVARCHAR(150) NOT NULL,
    Nhom    NVARCHAR(80)  NOT NULL,
    CONSTRAINT UQ_Quyen_Ma UNIQUE (Ma)
);

CREATE TABLE dbo.VaiTroQuyen (
    VaiTroId INT NOT NULL,
    QuyenId  INT NOT NULL,
    CONSTRAINT PK_VaiTroQuyen PRIMARY KEY (VaiTroId, QuyenId),
    CONSTRAINT FK_VaiTroQuyen_VaiTro FOREIGN KEY (VaiTroId) REFERENCES dbo.VaiTro (Id) ON DELETE CASCADE,
    CONSTRAINT FK_VaiTroQuyen_Quyen  FOREIGN KEY (QuyenId)  REFERENCES dbo.Quyen (Id)  ON DELETE CASCADE
);

CREATE TABLE dbo.TaiKhoan (
    Id              INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_TaiKhoan PRIMARY KEY,
    TenDangNhap     NVARCHAR(50)  NOT NULL,
    MatKhauHash     CHAR(64)      NOT NULL, -- SHA2-256 hex; khi triển khai .NET nên dùng Identity/BCrypt
    NhanVienId      INT           NOT NULL,
    VaiTroId        INT           NOT NULL,
    TrangThai       NVARCHAR(20)  NOT NULL CONSTRAINT DF_TaiKhoan_TrangThai DEFAULT N'Hoạt động',
    DoiMatKhauLanDau BIT           NOT NULL CONSTRAINT DF_TaiKhoan_DoiMatKhauLanDau DEFAULT 0,
    MatKhauTamDaDung BIT           NOT NULL CONSTRAINT DF_TaiKhoan_MatKhauTamDaDung DEFAULT 0,
    LanDangNhapCuoi DATETIME2(0)  NULL,
    NgayTao         DATETIME2(0)  NOT NULL CONSTRAINT DF_TaiKhoan_NgayTao DEFAULT SYSUTCDATETIME(),
    CONSTRAINT UQ_TaiKhoan_TenDangNhap UNIQUE (TenDangNhap),
    CONSTRAINT UQ_TaiKhoan_NhanVien UNIQUE (NhanVienId),
    CONSTRAINT CK_TaiKhoan_TrangThai CHECK (TrangThai IN (N'Hoạt động', N'Bị khóa')),
    CONSTRAINT FK_TaiKhoan_NhanVien FOREIGN KEY (NhanVienId) REFERENCES dbo.NhanVien (Id),
    CONSTRAINT FK_TaiKhoan_VaiTro   FOREIGN KEY (VaiTroId)   REFERENCES dbo.VaiTro (Id)
);

CREATE TABLE dbo.KyDanhGia (
    Id          INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_KyDanhGia PRIMARY KEY,
    Ten         NVARCHAR(100) NOT NULL,
    NgayBatDau  DATE          NOT NULL,
    NgayKetThuc DATE          NOT NULL,
    TrangThai   NVARCHAR(30)  NOT NULL CONSTRAINT DF_KyDanhGia_TrangThai DEFAULT N'Đang diễn ra',
    NgayTao     DATETIME2(0)  NOT NULL CONSTRAINT DF_KyDanhGia_NgayTao DEFAULT SYSUTCDATETIME(),
    CONSTRAINT CK_KyDanhGia_ThoiGian CHECK (NgayBatDau <= NgayKetThuc),
    CONSTRAINT CK_KyDanhGia_TrangThai CHECK (TrangThai IN (N'Chưa bắt đầu', N'Đang diễn ra', N'Đã kết thúc')),
    CONSTRAINT UQ_KyDanhGia_Ten UNIQUE (Ten)
);

CREATE TABLE dbo.TieuChiKpi (
    Id              INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_TieuChiKpi PRIMARY KEY,
    Ten             NVARCHAR(150) NOT NULL,
    MoTa            NVARCHAR(500) NULL,
    TrongSoMacDinh  DECIMAL(5,2)  NOT NULL CONSTRAINT DF_TieuChiKpi_TrongSo DEFAULT 0,
    NgayTao         DATETIME2(0)  NOT NULL CONSTRAINT DF_TieuChiKpi_NgayTao DEFAULT SYSUTCDATETIME(),
    CONSTRAINT CK_TieuChiKpi_TrongSo CHECK (TrongSoMacDinh >= 0 AND TrongSoMacDinh <= 100),
    CONSTRAINT UQ_TieuChiKpi_Ten UNIQUE (Ten)
);

CREATE TABLE dbo.Kpi (
    Id                 INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Kpi PRIMARY KEY,
    KyDanhGiaId        INT            NOT NULL,
    NhanVienId         INT            NOT NULL,
    TieuChiId          INT            NOT NULL,
    MucTieu            NVARCHAR(500)  NOT NULL,
    TrongSo            DECIMAL(5,2)   NOT NULL,
    KetQua             DECIMAL(10,2)  NULL,
    GhiChu             NVARCHAR(500)  NULL,
    Diem               DECIMAL(5,2)   NULL,
    TrangThai          NVARCHAR(30)   NOT NULL CONSTRAINT DF_Kpi_TrangThai DEFAULT N'Chờ cập nhật',
    DaCongBo           BIT            NOT NULL CONSTRAINT DF_Kpi_DaCongBo DEFAULT 0,
    NguoiDanhGiaId     INT            NULL,
    NgayCapNhatKetQua  DATETIME2(0)   NULL,
    NgayDanhGia        DATETIME2(0)   NULL,
    NgayTao            DATETIME2(0)   NOT NULL CONSTRAINT DF_Kpi_NgayTao DEFAULT SYSUTCDATETIME(),
    CONSTRAINT CK_Kpi_TrongSo CHECK (TrongSo > 0 AND TrongSo <= 100),
    CONSTRAINT CK_Kpi_Diem CHECK (Diem IS NULL OR (Diem >= 0 AND Diem <= 100)),
    CONSTRAINT CK_Kpi_KetQua CHECK (KetQua IS NULL OR KetQua >= 0),
    CONSTRAINT CK_Kpi_TrangThai CHECK (TrangThai IN (N'Chờ cập nhật', N'Chờ đánh giá', N'Đã đánh giá')),
    CONSTRAINT UQ_Kpi_Ky_NhanVien_TieuChi UNIQUE (KyDanhGiaId, NhanVienId, TieuChiId),
    CONSTRAINT FK_Kpi_KyDanhGia    FOREIGN KEY (KyDanhGiaId)    REFERENCES dbo.KyDanhGia (Id),
    CONSTRAINT FK_Kpi_NhanVien     FOREIGN KEY (NhanVienId)     REFERENCES dbo.NhanVien (Id),
    CONSTRAINT FK_Kpi_TieuChi      FOREIGN KEY (TieuChiId)      REFERENCES dbo.TieuChiKpi (Id),
    CONSTRAINT FK_Kpi_NguoiDanhGia FOREIGN KEY (NguoiDanhGiaId) REFERENCES dbo.NhanVien (Id)
);
GO

CREATE INDEX IX_NhanVien_PhongBan ON dbo.NhanVien (PhongBanId);
CREATE INDEX IX_NhanVien_ChucVu   ON dbo.NhanVien (ChucVuId);
CREATE INDEX IX_Kpi_KyDanhGia     ON dbo.Kpi (KyDanhGiaId);
CREATE INDEX IX_Kpi_NhanVien      ON dbo.Kpi (NhanVienId);
GO

CREATE VIEW dbo.vw_BaoCaoKpiNhanVien
AS
SELECT
    nv.Id AS NhanVienId,
    nv.Ma AS MaNhanVien,
    nv.HoTen,
    pb.Ten AS PhongBan,
    cv.Ten AS ChucVu,
    ky.Id AS KyDanhGiaId,
    ky.Ten AS KyDanhGia,
    COUNT(*) AS SoKpi,
    SUM(CASE WHEN k.Diem IS NOT NULL THEN 1 ELSE 0 END) AS SoDaDanhGia,
    CAST(SUM(CASE WHEN k.Diem IS NOT NULL THEN k.Diem * k.TrongSo / 100.0 END) AS DECIMAL(6,2)) AS DiemTongHop
FROM dbo.Kpi k
JOIN dbo.NhanVien nv ON nv.Id = k.NhanVienId
JOIN dbo.PhongBan pb ON pb.Id = nv.PhongBanId
JOIN dbo.ChucVu cv ON cv.Id = nv.ChucVuId
JOIN dbo.KyDanhGia ky ON ky.Id = k.KyDanhGiaId
GROUP BY nv.Id, nv.Ma, nv.HoTen, pb.Ten, cv.Ten, ky.Id, ky.Ten;
GO

/* ===== Dữ liệu mẫu (trùng data.js / tài khoản demo) ===== */
SET IDENTITY_INSERT dbo.PhongBan ON;
INSERT INTO dbo.PhongBan (Id, Ma, Ten, MoTa) VALUES
(1, N'CNTT', N'Khoa Công nghệ thông tin', N'Đào tạo và nghiên cứu CNTT'),
(2, N'KT',   N'Phòng Kế hoạch - Tài chính', N'Công tác kế hoạch và tài chính'),
(3, N'TCHC', N'Phòng Tổ chức - Hành chính', N'Quản lý tổ chức và nhân sự');
SET IDENTITY_INSERT dbo.PhongBan OFF;

SET IDENTITY_INSERT dbo.ChucVu ON;
INSERT INTO dbo.ChucVu (Id, Ma, Ten) VALUES
(1, N'TRUONG_KHOA',  N'Trưởng khoa'),
(2, N'TRUONG_PHONG', N'Trưởng phòng'),
(3, N'CHUYEN_VIEN',  N'Chuyên viên');
SET IDENTITY_INSERT dbo.ChucVu OFF;

SET IDENTITY_INSERT dbo.NhanVien ON;
INSERT INTO dbo.NhanVien (Id, Ma, HoTen, Email, DienThoai, PhongBanId, ChucVuId, TrangThai) VALUES
(1, N'CB001', N'Nguyễn Minh Anh', N'anh.nguyen@humg.edu.vn',  N'0912345678', 1, 1, N'Đang làm việc'),
(2, N'CB002', N'Trần Quốc Bảo',   N'bao.tran@humg.edu.vn',    N'0987654321', 2, 2, N'Đang làm việc'),
(3, N'CB003', N'Lê Thu Hà',       N'ha.le@humg.edu.vn',       N'0909123456', 1, 3, N'Đang làm việc'),
(4, N'CB004', N'Phạm Đức Long',   N'long.pham@humg.edu.vn',   N'0933555777', 3, 3, N'Ngừng hoạt động'),
(5, N'CB005', N'Đỗ Mai Linh',     N'linh.do@humg.edu.vn',     N'0977111222', 2, 3, N'Đang làm việc');
SET IDENTITY_INSERT dbo.NhanVien OFF;

SET IDENTITY_INSERT dbo.VaiTro ON;
INSERT INTO dbo.VaiTro (Id, Ma, Ten, MoTa) VALUES
(1, N'ADMIN',    N'Quản trị viên', N'Quản lý tài khoản, vai trò, quyền, nhân viên, phòng ban và chức vụ.'),
(2, N'MANAGER',  N'Người quản lý', N'Theo dõi nhân sự, thiết lập và đánh giá KPI.'),
(3, N'EMPLOYEE', N'Nhân viên',     N'Theo dõi thông tin cá nhân và mục tiêu KPI được giao.');
SET IDENTITY_INSERT dbo.VaiTro OFF;

SET IDENTITY_INSERT dbo.Quyen ON;
INSERT INTO dbo.Quyen (Id, Ma, Ten, Nhom) VALUES
(1,  N'dashboard',    N'Xem tổng quan',              N'Tổng quan'),
(2,  N'employees',    N'Quản lý nhân viên',          N'Nhân sự'),
(3,  N'departments',  N'Quản lý phòng ban',          N'Nhân sự'),
(4,  N'positions',    N'Quản lý chức vụ',            N'Nhân sự'),
(5,  N'accounts',     N'Quản lý tài khoản',          N'Hệ thống'),
(6,  N'permissions',  N'Quản lý vai trò và quyền',   N'Hệ thống'),
(7,  N'periods',      N'Quản lý kỳ đánh giá',        N'KPI'),
(8,  N'criteria',     N'Quản lý tiêu chí KPI',       N'KPI'),
(9,  N'setup',        N'Thiết lập và giao KPI',      N'KPI'),
(10, N'mykpi',        N'Xem và cập nhật KPI của tôi',N'KPI'),
(11, N'evaluations',  N'Đánh giá KPI',               N'KPI'),
(12, N'reports',      N'Thống kê và báo cáo',        N'KPI');
SET IDENTITY_INSERT dbo.Quyen OFF;

INSERT INTO dbo.VaiTroQuyen (VaiTroId, QuyenId) VALUES
(1,1),(1,2),(1,3),(1,4),(1,5),(1,6),(1,7),(1,8),(1,9),(1,10),(1,11),(1,12),
(2,1),(2,2),(2,7),(2,8),(2,9),(2,10),(2,11),(2,12),
(3,1),(3,10);

DECLARE @MatKhauDemo CHAR(64) =
    CONVERT(CHAR(64), CONVERT(VARCHAR(64), HASHBYTES('SHA2_256', CONVERT(VARBINARY(4000), N'123456')), 2));

SET IDENTITY_INSERT dbo.TaiKhoan ON;
INSERT INTO dbo.TaiKhoan (Id, TenDangNhap, MatKhauHash, NhanVienId, VaiTroId, TrangThai) VALUES
(1, N'admin',    @MatKhauDemo, 1, 1, N'Hoạt động'),
(2, N'manager',  @MatKhauDemo, 2, 2, N'Hoạt động'),
(3, N'employee', @MatKhauDemo, 3, 3, N'Hoạt động');
SET IDENTITY_INSERT dbo.TaiKhoan OFF;

SET IDENTITY_INSERT dbo.KyDanhGia ON;
INSERT INTO dbo.KyDanhGia (Id, Ten, NgayBatDau, NgayKetThuc, TrangThai) VALUES
(1, N'Quý IV/2026',  '2026-10-01', '2026-12-31', N'Đang diễn ra'),
(2, N'Quý III/2026', '2026-07-01', '2026-09-30', N'Đã kết thúc');
SET IDENTITY_INSERT dbo.KyDanhGia OFF;

SET IDENTITY_INSERT dbo.TieuChiKpi ON;
INSERT INTO dbo.TieuChiKpi (Id, Ten, MoTa, TrongSoMacDinh) VALUES
(1, N'Hoàn thành nhiệm vụ',  N'Mức độ hoàn thành công việc được giao', 40),
(2, N'Chất lượng công việc', N'Độ chính xác và chất lượng đầu ra',     30),
(3, N'Tiến độ thực hiện',    N'Đảm bảo tiến độ công việc',             20),
(4, N'Phối hợp và kỷ luật',  N'Phối hợp nhóm, tuân thủ quy định',      10);
SET IDENTITY_INSERT dbo.TieuChiKpi OFF;

SET IDENTITY_INSERT dbo.Kpi ON;
INSERT INTO dbo.Kpi (Id, KyDanhGiaId, NhanVienId, TieuChiId, MucTieu, TrongSo, KetQua, GhiChu, Diem, TrangThai, DaCongBo, NguoiDanhGiaId, NgayDanhGia) VALUES
(1, 1, 3, 1, N'Hoàn thành 100% nhiệm vụ',   40, 90,  N'Đã hoàn thành phần lớn nhiệm vụ', 90, N'Chờ đánh giá', 0, NULL, NULL),
(2, 1, 3, 2, N'Không có lỗi nghiêm trọng',  30, 95,  N'',                                 95, N'Chờ đánh giá', 0, NULL, NULL),
(3, 1, 2, 1, N'Hoàn thành 100% nhiệm vụ',   40, 100, N'',                                100, N'Đã đánh giá',  1, 1,    SYSUTCDATETIME()),
(4, 1, 5, 3, N'Hoàn thành đúng hạn',        20, 80,  N'Một nhiệm vụ chậm tiến độ',        80, N'Chờ đánh giá', 0, NULL, NULL),
(5, 2, 3, 3, N'Hoàn thành đúng hạn',        20, 90,  N'',                                 90, N'Đã đánh giá',  1, 2,    SYSUTCDATETIME());
SET IDENTITY_INSERT dbo.Kpi OFF;
GO

PRINT N'Đã tạo CSDL HrKpiDb và nạp dữ liệu mẫu.';
GO
