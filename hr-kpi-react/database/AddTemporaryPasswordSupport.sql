IF COL_LENGTH('dbo.TaiKhoan', 'DoiMatKhauLanDau') IS NULL
BEGIN
    ALTER TABLE dbo.TaiKhoan
    ADD DoiMatKhauLanDau BIT NOT NULL
        CONSTRAINT DF_TaiKhoan_DoiMatKhauLanDau DEFAULT 0;
END;

IF COL_LENGTH('dbo.TaiKhoan', 'MatKhauTamDaDung') IS NULL
BEGIN
    ALTER TABLE dbo.TaiKhoan
    ADD MatKhauTamDaDung BIT NOT NULL
        CONSTRAINT DF_TaiKhoan_MatKhauTamDaDung DEFAULT 0;
END;
