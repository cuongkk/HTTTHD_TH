# TH3 - EF Core + PostgreSQL

## Mục tiêu

Nâng cấp mini Course API của TH2 để lưu dữ liệu thật trong PostgreSQL mà giữ các route, HTTP status và quy tắc tìm kiếm/validation đang có. Sinh viên có thể tự chạy, chỉ ra dữ liệu trong DB, quan sát SQL và giải thích từng checkpoint trong video tối đa 5 phút.

## Thiết kế

- `CourseDbContext` quản lý `DbSet<Course>`; Npgsql được đăng ký trong `Program.cs`. `CourseService` là scoped service, dùng các API async của EF Core. Controller tiếp tục gọi `ICourseService`, không truy cập DB trực tiếp.
- Chuỗi kết nối được lấy từ `ConnectionStrings:CourseDb`; mật khẩu do user secrets hoặc biến môi trường cung cấp, không ghi vào source. Thiếu cấu hình thì ứng dụng báo lỗi rõ ràng khi khởi động.
- Migration `InitialCreate` tạo bảng Courses và ba bản ghi mẫu để giữ ví dụ TH2. Migration `AddCourseCreatedAt` thêm cột `CreatedAt` kiểu `timestamp with time zone`, mặc định thời gian UTC phía DB. Nó không xóa bảng hoặc dữ liệu cũ.
- POST bỏ qua `Id` và `CreatedAt` do client gửi; DB tạo Id và timestamp. PUT chỉ thay Code, Name, Credits, IsActive. Validation Code không rỗng và Credits 1-6 vẫn hoạt động.
- Search trim keyword, từ chối rỗng, tìm chuỗi con theo Code hoặc Name không phân biệt hoa thường; `%`, `_`, `\` trong input được hiểu là ký tự thường, không phải wildcard SQL. Không khớp trả mảng rỗng.
- Log câu lệnh SQL EF Core ở mức Information trong Development; không log giá trị tham số nhạy cảm.

## Kiểm chứng

- Build và chạy bộ HTTP checks với PostgreSQL cục bộ.
- Áp dụng chỉ `InitialCreate`, xác nhận bản ghi mẫu; áp dụng `AddCourseCreatedAt` trên cùng DB, xác nhận bản ghi và Id vẫn còn, cột mới hiện diện.
- Khởi động lại API và xác nhận dữ liệu tạo bằng POST còn tồn tại; xem một dòng SQL SELECT trong log.
- README hướng dẫn cấu hình, migration, truy vấn psql, demo; `HUONG_DAN_VIDEO_TH3.md` là dàn ý tự trình bày, không thay thế video có tiếng của sinh viên.
