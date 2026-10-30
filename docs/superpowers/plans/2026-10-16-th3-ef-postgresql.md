# Kế hoạch TH3

1. Bổ sung kiểm tra HTTP cho `CreatedAt` do DB sinh và giữ nguyên sau PUT; chạy với API TH2 để thấy phép kiểm tra mới thất bại đúng lý do.
2. Thêm gói Npgsql EF Core 10 và EF Core Design, `CourseDbContext`, cấu hình kết nối/DI. Tạo `InitialCreate` khi model chỉ có năm trường gốc; thêm ba mẫu bằng migration. Áp dụng trên DB thử và xác nhận có ba bản ghi.
3. Chuyển `CourseService` sang truy vấn/thao tác EF Core async; giữ semantics của CRUD/Search, validation và HTTP status. Chạy lại bộ checks trên DB thử.
4. Thêm `CreatedAt` và migration `AddCourseCreatedAt`; áp dụng vào chính DB thử, xác nhận ba dòng cũ giữ nguyên; kiểm tra POST sinh timestamp và PUT giữ timestamp.
5. Kiểm tra lưu bền qua restart, SQL log, migration script và build. Cập nhật README, ví dụ HTTP và dàn ý video trong phạm vi TH3.

Các phép kiểm tra tập trung vào hành vi thật qua API và PostgreSQL. Database thử chạy trên loopback tách biệt, không đụng DB hiện có của người dùng. Workspace không có `.git`, vì vậy không thực hiện được các bước commit/worktree của quy trình phát triển.
