# QLNS_KPI API

Backend ASP.NET Core API cho màn hình HR & KPI trong `hr-kpi-react`. API dùng SQL Server, JWT bearer authentication, BCrypt và kiểm tra quyền ở từng endpoint.

## Cấu hình và chạy

1. Sửa `ConnectionStrings:DefaultConnection` trong `QLNS_KPI/appsettings.json` theo SQL Server của bạn.
2. Tạo JWT key bí mật (ít nhất 32 byte) trong user-secrets:

   ```powershell
   cd QLNS_KPI
   $key = [Convert]::ToBase64String([System.Security.Cryptography.RandomNumberGenerator]::GetBytes(48))
   dotnet user-secrets set "Jwt:SigningKey" $key
   ```

   Có thể dùng biến môi trường `Jwt__SigningKey` thay thế. Không đưa key thật vào appsettings hoặc source control.
3. Chạy API:

   ```powershell
   dotnet run --project .\QLNS_KPI\QLNS_KPI.csproj
   ```

Backend ánh xạ trực tiếp các bảng/cột trong `../hr-kpi-react/database/HrKpiDb.sql` (`NhanVien`, `PhongBan`, `ChucVu`, `TaiKhoan`, `VaiTro`, `Quyen`, `VaiTroQuyen`, `KyDanhGia`, `TieuChiKpi`, `Kpi`). `TaiKhoan.VaiTroId` được nối tới `VaiTro`, còn `Kpi.KetQua` ánh xạ số thập phân. Mật khẩu SHA2-256 cũ của script được xác thực và nâng cấp sang BCrypt khi đăng nhập thành công; BCrypt vừa trong giới hạn `CHAR(64)` của `MatKhauHash`. Ứng dụng không tự tạo dữ liệu nghiệp vụ hay schema; hãy chuẩn bị schema từ script SQL và cấu hình kết nối tới `HrKpiDb`. FE tải dữ liệu và lưu thao tác qua API vào các bảng này.

### Cấu hình email và mật khẩu tạm thời

Trước khi tạo tài khoản hoặc dùng chức năng quên mật khẩu, cấu hình SMTP bằng user-secrets hoặc biến môi trường (không lưu mật khẩu SMTP trong source control). Với Gmail, bật xác minh 2 bước và tạo **App Password**; dùng địa chỉ Gmail làm username, App Password làm password, máy chủ `smtp.gmail.com`, cổng `587` (STARTTLS):

```powershell
dotnet user-secrets set "Smtp:Host" "smtp.gmail.com"
dotnet user-secrets set "Smtp:Port" "587"
dotnet user-secrets set "Smtp:ClientDomain" "gmail.com"
dotnet user-secrets set "Smtp:Username" "TranNgoManhHung@gmail.com"
dotnet user-secrets set "Smtp:Password" "<gmail-app-password>"
dotnet user-secrets set "Smtp:FromEmail" "TranNgoManhHung@gmail.com"
dotnet user-secrets set "Smtp:FromName" "HUMG HR & KPI"
dotnet user-secrets set "Smtp:EnableSsl" "true"
```

Chạy các lệnh tại thư mục project `QLNS_KPI/QLNS_KPI` để user-secrets áp dụng đúng `UserSecretsId`. Cấu hình `Smtp:ClientDomain` là hostname ASCII dùng trong lệnh EHLO; không đặt tên máy tính có dấu tiếng Việt ở đây. Với Gmail, `Smtp:Username` và `Smtp:FromEmail` phải là tài khoản Gmail của bạn. Tài khoản SMTP và mật khẩu phải được cấu hình cùng nhau; nếu máy chủ SMTP không yêu cầu xác thực, có thể bỏ cả hai. Tên đăng nhập và email đích của tài khoản lấy từ hồ sơ nhân viên đã chọn khi tạo tài khoản. Backend sinh mật khẩu ngẫu nhiên và chỉ cho phép mật khẩu này đăng nhập một lần. Phiên đăng nhập đó chỉ được phép đổi mật khẩu; nếu bị đóng trước khi đổi xong, người dùng cần yêu cầu đặt lại để nhận mật khẩu mới. Khi tạo tài khoản, bản ghi chỉ được commit sau khi SMTP chấp nhận email; nếu gửi thất bại, transaction bị rollback và API báo lỗi, không tạo tài khoản.

Với database đã được tạo trước thay đổi này, chạy `../hr-kpi-react/database/AddTemporaryPasswordSupport.sql` trước khi khởi động API mới. Script chỉ thêm các cột trạng thái mật khẩu tạm thời với mặc định `0`; **không chạy lại toàn bộ `HrKpiDb.sql` trên database đang có dữ liệu**.

Phản hồi quên mật khẩu luôn giống nhau để không tiết lộ tài khoản có tồn tại hay không. Nếu SMTP gặp lỗi, backend ghi lỗi chi tiết vào log; lần đặt lại đó không thay đổi mật khẩu hiện tại.

**Cẩn thận:** script SQL hiện có các lệnh `DROP TABLE` rồi tạo lại bảng và chèn dữ liệu demo. Chạy lại script trên database đang có dữ liệu sẽ xóa dữ liệu đó. Chỉ chạy nguyên script cho database mới; nếu muốn dữ liệu nghiệp vụ thật, bỏ các lệnh insert demo tương ứng trước lần chạy đầu. Giữ các hàng hệ thống trong `VaiTro` (ít nhất vai trò `Quản trị viên`) và `Quyen`/`VaiTroQuyen` nếu muốn dùng phân quyền hiện có. Với database không có tài khoản, tạo setup secret trong user-secrets:

```powershell
$setupSecret = [Convert]::ToBase64String([System.Security.Cryptography.RandomNumberGenerator]::GetBytes(48))
dotnet user-secrets set "InitialAdmin:SetupSecret" $setupSecret
```

Sau đó gọi `POST /api/auth/initialize` một lần để tạo quản trị viên đầu tiên. Có thể liên kết `employeeId` đang có; nếu tạo hồ sơ mới, truyền thông tin nhân viên và thông tin phòng ban/chức vụ thực tế (hoặc ID phòng ban/chức vụ đã tồn tại). API không tạo dữ liệu mẫu hay tự đặt tên/mã danh mục. Sau khi tạo xong, gỡ setup secret bằng `dotnet user-secrets remove "InitialAdmin:SetupSecret"`.

Ví dụ khởi tạo hồ sơ quản trị viên đầu tiên (thay các giá trị bằng dữ liệu thật):

```powershell
$setupSecret = "<secret-khoi-tao-da-cau-hinh>"
$body = @{
  setupSecret = $setupSecret
  username = "ten-dang-nhap-quan-tri"
  password = "<mat-khau-manh>"
  employeeCode = "MA-NHAN-VIEN"
  employeeName = "Ho va ten"
  employeeEmail = "email@donvi.edu.vn"
  departmentCode = "MA-PHONG"
  departmentName = "Ten phong ban"
  positionCode = "MA-CHUC-VU"
  positionName = "Ten chuc vu"
} | ConvertTo-Json
Invoke-RestMethod -Method Post -Uri "http://localhost:5145/api/auth/initialize" -ContentType "application/json" -Body $body
```

Vite mặc định gọi API tại `http://localhost:5145`. Nếu API chạy địa chỉ khác, tạo `../hr-kpi-react/.env.local` với `VITE_API_URL=https://localhost:<port>` (hoặc URL API tương ứng), đồng thời thêm origin FE vào `Cors:AllowedOrigins`.

OpenAPI ở `/openapi/v1.json` khi chạy Development; CORS mặc định cho Vite ở `http://localhost:5173`. Có thể cấu hình thêm bằng `Cors:AllowedOrigins`.

## API

Các API riêng tư yêu cầu xác thực JWT. Những endpoint công khai là đăng nhập, quên mật khẩu và khởi tạo quản trị viên đầu tiên.

| Method | Endpoint | Quyền | Chức năng |
|---|---|---|---|
| POST | `/api/auth/login` | Công khai | Đăng nhập, trả JWT và thông tin người dùng |
| POST | `/api/auth/forgot-password` | Công khai, giới hạn 5 yêu cầu/15 phút/IP | Xác minh tên đăng nhập và email nhân viên, gửi mật khẩu tạm thời |
| POST | `/api/auth/initialize` | Công khai, cần setup secret | Tạo tài khoản quản trị đầu tiên khi hệ thống chưa có tài khoản |
| GET | `/api/auth/me` | Đã đăng nhập | Lấy người dùng hiện tại |
| POST | `/api/auth/change-password` | Đã đăng nhập | Đổi mật khẩu |
| GET | `/api/bootstrap` | Đã đăng nhập | Dữ liệu khởi tạo dạng camelCase phù hợp `AppData` |
| GET/POST | `/api/employees` | Đọc: mọi vai trò; ghi: quản trị viên | Danh sách/hồ sơ nhân viên |
| GET/PUT/DELETE | `/api/employees/{id}` | Đọc theo vai trò; ghi: quản trị viên | Chi tiết/cập nhật/xóa mềm nhân viên |
| GET/POST/PUT/DELETE | `/api/departments`, `/api/positions`, `/api/periods`, `/api/criteria` | Đọc: mọi vai trò; ghi: quản trị viên | Danh mục và kỳ đánh giá |
| GET/POST/PUT/DELETE | `/api/accounts` | Quản trị viên | Tạo tài khoản chỉ khi SMTP chấp nhận email mật khẩu tạm thời; lỗi email sẽ rollback việc tạo tài khoản |
| GET/POST/PUT/DELETE | `/api/kpis` | Đọc theo vai trò; ghi: quản trị viên/người quản lý | Giao và quản lý KPI |
| PUT | `/api/kpis/{id}/result` | Nhân viên được giao KPI | Gửi kết quả và ghi chú |
| PUT | `/api/kpis/{id}/evaluation` | Quản trị viên/người quản lý | Chấm điểm và công bố kết quả |
| GET | `/api/dashboard` | Đã đăng nhập | Tổng quan, giới hạn theo nhân viên với vai trò nhân viên |
| GET | `/api/reports?periodId=&departmentId=` | Quản trị viên/người quản lý | Tổng hợp điểm theo nhân viên |
| GET | `/api/permissions` | Đã đăng nhập | Danh sách quyền theo vai trò |

JSON dùng tên thuộc tính camelCase (`departmentId`, `defaultWeight`, `periodId`...) trùng với model phía React. Nhân viên chỉ đọc/cập nhật dữ liệu KPI của mình; điểm KPI chỉ được trả về cho nhân viên sau khi công bố. Nhân viên/phòng ban/chức vụ/tiêu chí/kỳ đã được tham chiếu sẽ không bị xóa cứng.

Backend không tự tạo hay sửa schema. Nếu thay đổi cấu trúc SQL, cập nhật script/migration và EF Core mapping tương ứng trước khi chạy API.
