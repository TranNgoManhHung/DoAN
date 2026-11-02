# HR & KPI Management — ReactJS

Frontend prototype bám theo Chương 3 và Chương 4 của tài liệu ĐATN: quản lý nhân sự, đơn vị/phòng ban, chức vụ, tài khoản và phân quyền; kỳ đánh giá, tiêu chí KPI, thiết lập/giao KPI, cập nhật kết quả, đánh giá, dashboard và báo cáo.

## Chạy dự án
1. Cài Node.js LTS (khuyến nghị Node 20+).
2. Mở terminal tại thư mục chứa `package.json`. Nếu terminal đang ở thư mục cha `hr-kpi-react`, chạy `cd .\hr-kpi-react` để vào thư mục dự án bên trong.
3. Trong PowerShell, chạy `npm.cmd install` rồi `npm.cmd run dev`. Dùng `npm.cmd` để tránh lỗi Execution Policy chặn script `npm.ps1`; trong Command Prompt có thể dùng `npm install` và `npm run dev`.
4. Mở URL Vite hiển thị trong terminal.

## Tài khoản demo
- Quản trị viên: `admin` / `123456`
- Người quản lý: `manager` / `123456`
- Nhân viên: `employee` / `123456`

Dữ liệu hiện đang là dữ liệu mẫu lưu trong `localStorage` để có thể chạy giao diện độc lập. Để kết nối backend thật, cấu hình `VITE_API_BASE_URL` và thay lớp mock/service trong `src/data.js` bằng các request tới ASP.NET Core Web API (.NET 8). JWT ở bản demo chỉ mô phỏng phiên đăng nhập, không phải cơ chế bảo mật sản xuất.

## Các module
- Đăng nhập/đăng xuất, đổi mật khẩu (mô phỏng), điều hướng theo vai trò.
- Dashboard tổng quan.
- Nhân viên: tìm kiếm, lọc, thêm, sửa, ngừng hoạt động.
- Phòng ban và chức vụ: thêm/sửa/xóa có kiểm tra dữ liệu liên quan.
- Tài khoản, vai trò và quyền: giao diện quản trị demo, khóa/mở tài khoản.
- Kỳ đánh giá KPI và tiêu chí KPI.
- Thiết lập/giao KPI cho nhân viên.
- Nhân viên xem KPI được giao và cập nhật kết quả.
- Quản lý đánh giá KPI, công bố kết quả.
- Báo cáo, tra cứu theo kỳ/phòng ban/nhân viên và xuất CSV.

## Lưu ý
Đây là frontend demo, không bao gồm backend .NET 8 và SQL Server. Quyền và dữ liệu mẫu phía client không thay thế xác thực/phân quyền tại backend.
