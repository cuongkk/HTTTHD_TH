# Thực hành HTTTHD

## Phase 2 - QTKD tuần 05

Mở [README_PHASE2.md](README_PHASE2.md) để cấu hình PostgreSQL, chạy migration, khởi động `QTKD.slnx` và thử hai GET API. [Kịch bản video TH5](KICH_BAN_VIDEO_TH5.md) giải thích source và cách demo.

## TH3 - Course API với EF Core và PostgreSQL

Project này tiếp tục API của TH2: Controller nhận HTTP, `CourseService` xử lý Course, EF Core đọc/ghi bảng `Courses` trong PostgreSQL. Dữ liệu tồn tại sau khi tắt và chạy lại API. Các route và mã HTTP của TH2 được giữ lại; response Course có thêm `CreatedAt` (UTC).

## Chuẩn bị

- .NET SDK 10, PostgreSQL đang chạy và công cụ `dotnet ef` phiên bản 10 (`dotnet tool install --global dotnet-ef --version 10.0.9` nếu chưa có).
- Tạo một database trống, ví dụ `course_th3`, bằng pgAdmin hoặc `CREATE DATABASE course_th3;` trong `psql`. Không xóa database cũ để chạy lại migration.
- Đứng trong thư mục `source` và cấu hình chuỗi kết nối **trên máy của bạn**, không đưa mật khẩu vào `appsettings.json` hay file ZIP:

```powershell
dotnet user-secrets set 'ConnectionStrings:CourseDb' 'Host=localhost;Port=5432;Database=course_th3;Username=postgres;Password=<mat-khau-cua-ban>' --project Course.Api
```

`Course.Api/appsettings.json` có sẵn khóa `ConnectionStrings:CourseDb` để rỗng, giúp thấy rõ tên cấu hình mà không lộ mật khẩu. `Program.cs` đọc khóa này bằng `GetConnectionString("CourseDb")` rồi truyền cho `UseNpgsql`. `UserSecretsId` trong project chỉ định nơi .NET đọc bí mật cục bộ; file bí mật nằm ngoài thư mục source và giá trị User Secrets ghi đè giá trị rỗng trong `appsettings.json`. Nếu dùng biến môi trường, tên là `ConnectionStrings__CourseDb`. Không có giá trị hợp lệ, API sẽ báo rõ `Thiếu ConnectionStrings:CourseDb`.

## Tạo schema và chạy

```powershell
dotnet restore TH3.slnx
dotnet build TH3.slnx --no-restore
dotnet ef database update --project Course.Api --startup-project Course.Api
dotnet run --project Course.Api --launch-profile http
```

Swagger UI: `http://localhost:5082/swagger`. Chạy bộ kiểm tra HTTP trong terminal thứ hai:

```powershell
dotnet run --project Course.Api.Checks -- http://localhost:5082
```

Bộ kiểm tra cần ba môn mẫu C001/C002/C003 từ migration đầu. Nó tạo rồi xóa dữ liệu kiểm tra của riêng mình, nên Id tự tăng có thể vượt 4. Khi demo, dùng Id do POST trả về thay vì đoán Id.

## Hai migration và checkpoint giữ dữ liệu

`InitialCreate` tạo bảng `Courses` và chèn ba môn mẫu. `AddCourseCreatedAt` thêm cột `CreatedAt` (`timestamp with time zone`, mặc định `CURRENT_TIMESTAMP`) trên cùng bảng. File migration đã nằm trong `Course.Api/Data/Migrations`; `database update` chỉ chạy migration nào chưa có trong `__EFMigrationsHistory`.

Để tự quan sát trên **database trống mới**, áp dụng từng bước và chạy truy vấn trong pgAdmin hoặc `psql`:

```powershell
dotnet ef database update InitialCreate --project Course.Api --startup-project Course.Api
```

```sql
SELECT "Id", "Code" FROM "Courses" ORDER BY "Id";
SELECT "MigrationId" FROM "__EFMigrationsHistory" ORDER BY "MigrationId";
```

Sau khi thấy C001/C002/C003, áp dụng migration thứ hai, **không xóa database**:

```powershell
dotnet ef database update --project Course.Api --startup-project Course.Api
```

```sql
SELECT "Id", "Code", "CreatedAt" FROM "Courses" ORDER BY "Id";
SELECT "MigrationId" FROM "__EFMigrationsHistory" ORDER BY "MigrationId";
```

Cùng ba Id/Code còn đó và `CreatedAt` đã có giá trị. Nếu database của bạn đã ở migration cuối, chỉ chạy các câu `SELECT` để kiểm tra; không rollback/xóa bảng đang dùng. Trong quá trình kiểm chứng project, chính database thử đã cho C001/C002/C003 ở Id 1/2/3 trước và sau migration.

## Các API chính

| Method | Route                             | Thành công      | Lỗi xử lý            |
| ------ | --------------------------------- | --------------- | -------------------- |
| GET    | `/api/courses`                    | 200 + danh sách | -                    |
| GET    | `/api/courses/{id}`               | 200 + Course    | 404                  |
| POST   | `/api/courses`                    | 201 + Location  | 400                  |
| PUT    | `/api/courses/{id}`               | 204             | 400, 404             |
| DELETE | `/api/courses/{id}`               | 204             | 404                  |
| GET    | `/api/courses/search?keyword=...` | 200 + danh sách | 400 nếu keyword rỗng |

POST bỏ qua `Id` và `CreatedAt` do client gửi; PostgreSQL cấp hai giá trị này. PUT dùng Id từ URL, sửa Code/Name/Credits/IsActive nhưng giữ `CreatedAt`. Code không được rỗng, Credits phải từ 1 đến 6. Search tìm chuỗi con theo Code hoặc Name, bỏ khoảng trắng hai đầu, không phân biệt hoa thường; không có kết quả thì trả `[]`.

## Giải thích luồng và quan sát SQL

`Program.cs` đăng ký `CourseDbContext` và `ICourseService` theo scope của request. Controller nhận JSON, gọi interface; Service validate rồi dùng `ToListAsync`, `FirstOrDefaultAsync`, `SaveChangesAsync` để EF Core thực hiện I/O với PostgreSQL. Kết quả trở lại Controller để trả status phù hợp. `AsNoTracking` dùng cho truy vấn chỉ đọc; PUT/DELETE dùng entity được theo dõi để `SaveChangesAsync` ghi thay đổi.

Ở profile Development, log `Microsoft.EntityFrameworkCore.Database.Command` hiện câu SQL. Thử GET `/api/courses/search?keyword=c002` rồi xem terminal API: câu SELECT chứa `ILIKE`, và giá trị tham số không bị in ra. Dữ liệu trong DB có thể xem bằng SQL ở trên. Ví dụ thao tác thủ công có trong `Course.Api/Course.Api.http`.

## Video và bài nộp

Xem [HUONG_DAN_VIDEO_TH3.md](HUONG_DAN_VIDEO_TH3.md) để tự chuẩn bị phần giải thích dưới 5 phút. Sinh viên tự quay màn hình và thu tiếng, chỉ trình bày phần đã chạy và hiểu được. Khi có video, nén thành `mssv.zip` với `source/` và `mssv_th3.mp4` ở cấp gốc; bỏ các thư mục `bin/`, `obj/`, user secrets và dữ liệu PostgreSQL. Upload ZIP lên Google Drive, kiểm tra quyền xem link, rồi nộp một file `.txt` chỉ chứa link đó trên Moodle.

## TH4 – Application Architecture

API tách trách nhiệm theo luồng `CoursesController → CourseService → CourseRepository → CourseDbContext → PostgreSQL`. Controller bind request/response DTO; Entity `Course` tiếp tục là entity EF Core. Repository đóng gói truy vấn EF Core; Service xử lý validation và rule nghiệp vụ. `ApiExceptionHandler` chuyển lỗi validation, không tìm thấy và Course Inactive thành `ProblemDetails` tương ứng 400, 404 và 409. Lỗi model binding trả 400 với tiêu đề và chi tiết.

Các route CRUD/Search và status của TH3 được giữ nguyên. `POST /api/courses/{id}/registration-check` kiểm tra điều kiện đăng ký: Active trả 204, Inactive trả 409, Id không tồn tại trả 404. Nó chỉ kiểm tra, không tạo bản ghi Enrollment. Không cần migration mới; schema và dữ liệu TH3 giữ nguyên.

Xem [HUONG_DAN_VIDEO_TH4.md](HUONG_DAN_VIDEO_TH4.md) để chuẩn bị video 4:45. Hãy tự giải thích bằng lời của mình và chỉ demo những gì đã chạy trên máy.
