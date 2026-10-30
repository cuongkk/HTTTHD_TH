# Hướng dẫn quay video TH3 chi tiết

Video cần **không quá 5 phút**, có tiếng nói của bạn và cho thấy các checkpoint TH3 trên chính project này. File này là bản chuẩn bị: có thứ tự thao tác, ý cần giải thích và ví dụ câu nói để bạn **diễn đạt lại bằng lời của mình**. Đừng đọc nguyên văn khi quay. Mục tiêu là người xem hiểu dữ liệu đi từ HTTP request đến PostgreSQL rồi trở lại response.

## 1. Chuẩn bị trước khi bấm quay

1. Đọc [README.md](README.md), cấu hình `ConnectionStrings:CourseDb` bằng user secrets trên máy bạn, tạo database và chạy migration. Không mở màn hình có mật khẩu trong video.
2. Mở sẵn bốn cửa sổ/tab: editor ở `Program.cs`, `CourseDbContext.cs`, `CourseService.cs` và hai file migration; Swagger tại `http://localhost:5082/swagger`; pgAdmin hoặc `psql`; terminal chạy API để xem log SQL. Chỉnh cỡ chữ đủ lớn để đọc trong video.
3. Trước khi quay, chạy `dotnet build TH3.slnx`, khởi động API bằng `dotnet run --project Course.Api --launch-profile http`, rồi mở terminal thứ hai chạy `dotnet run --project Course.Api.Checks -- http://localhost:5082` để phát hiện lỗi. Giữ terminal API chạy khi quay. Bộ checks có thể làm Id tăng; không giả định môn tiếp theo mang Id 4.
4. Chuẩn bị một mã môn dễ nhận biết, ví dụ `VIDEO_TH3_01`. Nếu đã dùng mã đó, đổi hậu tố để phần demo dễ phân biệt. Chuẩn bị JSON POST và PUT bên dưới trong clipboard hoặc file `Course.Api.http` để không mất thời gian gõ.
5. Tập nói một lần và bấm giờ. Nhắm khoảng **4 phút 35 giây đến 4 phút 50 giây**, chừa vài giây cho thao tác chậm. Chỉ giữ những màn hình chứng minh yêu cầu; không chờ restore/cài package trong video.

### JSON dùng trong demo

POST hợp lệ (`POST /api/courses`):

```json
{ "code": "4", "name": "Môn demo EF Core", "credits": 3, "isActive": true }
```

POST không hợp lệ để thấy 400 (gửi riêng, không sửa bản ghi hợp lệ):

```json
{ "code": "5", "name": "Môn sai tín chỉ", "credits": 7, "isActive": true }
```

PUT hợp lệ (`PUT /api/courses/{id}`), thay `{id}` bằng **Id thật vừa nhận từ POST**:

```json
{ "code": "4", "name": "Môn demo đã cập nhật", "credits": 4, "isActive": false }
```

Nếu muốn chứng minh `CreatedAt` do DB cấp, thử thêm `"id":999` và `"createdAt":"2000-01-01T00:00:00Z"` vào body POST; response vẫn có Id mới và thời gian hiện tại. Trong video ngắn, chỉ cần nói điều này và chỉ ra code tạo đối tượng mới trong `CreateAsync`.

## 2. Kịch bản quay 4 phút 45 giây

Các câu dưới đây là **gợi ý ý chính**, không phải lời thoại bắt buộc. Khi quay, hãy chỉ chuột đúng dòng code hoặc giá trị đang nói. Nếu một thao tác chậm, bỏ bớt lời dẫn, đừng bỏ bằng chứng DB và migration.

### 0:00-0:20 — Giới thiệu mục tiêu

**Màn hình:** Swagger với nhóm endpoint `/api/courses`, sau đó lướt cây thư mục project.

**Nói bằng lời của bạn:** “Ở TH2, Course được giữ trong một List nên tắt API sẽ mất dữ liệu. TH3 giữ các endpoint cũ, nhưng dùng EF Core để lưu vào PostgreSQL. Em sẽ cho xem cấu hình, migration, một luồng request và dữ liệu thật trong DB.”

**Điểm cần chỉ:** GET/POST/PUT/DELETE và Search vẫn nằm trong `CoursesController`; Swagger chỉ là giao diện gọi thử API.

### 0:20-1:00 — Cấu hình PostgreSQL, DI và DbContext

**Màn hình:** `Course.Api.csproj` (gói `Npgsql.EntityFrameworkCore.PostgreSQL`), `appsettings.json` (khóa `ConnectionStrings:CourseDb` rỗng), `Program.cs` (các dòng `GetConnectionString`, `AddDbContext`, `AddScoped`), `Data/CourseDbContext.cs`.

**Giải thích:** Npgsql là provider để EF Core làm việc với PostgreSQL. `appsettings.json` chỉ khai báo tên khóa, còn mật khẩu trên máy nằm trong User Secrets ngoài source. `Program.cs` đọc cấu hình `CourseDb`, đăng ký `CourseDbContext` và `ICourseService` vào Dependency Injection. Mỗi HTTP request dùng một scope; DbContext không phải một danh sách dùng chung trong RAM. `DbSet<Course> Courses` đại diện bảng `Courses`; `OnModelCreating` khai báo khóa `Id`, trường bắt buộc và mặc định DB cho `CreatedAt`.

**Câu hỏi nên tự trả lời:** “Mật khẩu nằm ở đâu?” — Trong user secrets hoặc biến môi trường trên máy chạy, **không nằm trong source nộp bài**. Tránh mở nội dung user secrets trước camera.

### 1:00-1:45 — Hai migration và dữ liệu cũ

**Màn hình:** `Data/Migrations/*InitialCreate.cs`, `*AddCourseCreatedAt.cs`, bảng `__EFMigrationsHistory` và kết quả truy vấn `Courses` trong pgAdmin/psql.

**Giải thích:** `InitialCreate.Up()` tạo bảng và chèn ba dòng C001/C002/C003. Migration thứ hai dùng `AddColumn`, thêm `CreatedAt` dạng `timestamp with time zone` với mặc định `CURRENT_TIMESTAMP`; nó không drop bảng. Bảng `__EFMigrationsHistory` ghi hai migration đã chạy, nên `database update` chỉ áp dụng phần còn thiếu. Chỉ vào các Id/Code 1-C001, 2-C002, 3-C003 vẫn còn và `CreatedAt` đã có giá trị.

**Cách chứng minh trước/sau thật:** Trên **database demo trống riêng** (không phải DB đang giữ bài làm), chạy `dotnet ef database update InitialCreate --project Course.Api --startup-project Course.Api`, rồi lưu kết quả `SELECT "Id", "Code" FROM "Courses" ORDER BY "Id";`. Sau đó chạy `dotnet ef database update --project Course.Api --startup-project Course.Api` trên **chính DB đó**, và cho xem `SELECT "Id", "Code", "CreatedAt" FROM "Courses" ORDER BY "Id";`. Có thể quay hai thao tác này thành clip ngắn để ghép vào video. Không chạy API phiên bản cuối khi DB còn ở migration đầu, vì model hiện tại đã có `CreatedAt`. Nếu database của bạn đã ở migration cuối, xem hướng dẫn chứng minh an toàn ở mục 4, không rollback/xóa dữ liệu chỉ để quay lại.

### 1:45-2:30 — Giải thích một request đi qua code

**Màn hình:** `Controllers/CoursesController.cs`, chuyển sang `Services/CourseService.cs`, rồi `Models/Course.cs`.

**Lấy POST làm ví dụ:** `[HttpPost]` nhận JSON thành đối tượng Course. Controller gọi `_service.CreateAsync(course)`. Service gọi `course.Validate()` để từ chối Code rỗng hoặc Credits ngoài 1–6, tạo Course mới chỉ từ bốn trường cho phép, `db.Courses.Add(created)` rồi `await db.SaveChangesAsync()`. EF Core gửi `INSERT` sang PostgreSQL; DB cấp Id và `CreatedAt`. Controller trả `CreatedAtAction`, tức **201 Created**, body chứa Course và header `Location` dẫn đến GET theo Id.

**Nói thêm một câu về GET:** `AsNoTracking()` dùng khi chỉ đọc; `ToListAsync()`/`FirstOrDefaultAsync()` chờ I/O database. Với PUT/DELETE, Service dùng `FindAsync` để lấy entity được EF theo dõi rồi `SaveChangesAsync` để ghi thay đổi.

### 2:30-4:00 — Demo API và kiểm tra DB

**Màn hình:** Swagger; sau POST chuyển ngắn sang pgAdmin/psql, rồi quay lại Swagger. Thực hiện theo thứ tự này để đỡ đổi tab:

1. `GET /api/courses` → **200**, thấy C001/C002/C003 từ database. Nói đây là dữ liệu đã được migration đầu chèn, không tạo lại mỗi lần chạy API.
2. `POST /api/courses` với JSON hợp lệ → **201**. Chỉ ra Id thực tế, `createdAt` có giá trị UTC và header `Location`. Ghi nhớ Id này.
3. Trong pgAdmin/psql chạy `SELECT "Id", "Code", "Name", "Credits", "CreatedAt" FROM "Courses" WHERE "Code" = 'VIDEO_TH3_01';` → thấy dòng vừa POST. Đây là bằng chứng dữ liệu đã vào PostgreSQL.
4. `PUT /api/courses/{id}` → **204**. `GET /api/courses/{id}` → **200**, Name/Credits/IsActive đổi, `CreatedAt` giữ nguyên. Nếu thiếu thời gian, chỉ cần chỉ ra response GET.
5. `GET /api/courses/search?keyword=video_th3` → **200** và thấy môn demo. `SearchAsync` trim keyword, dùng `EF.Functions.ILike` để tìm trong Code hoặc Name không phân biệt hoa thường. Keyword rỗng trả **400**; từ khóa không khớp trả **200** với `[]`.
6. `POST` với `credits:7` → **400** và thông báo tín chỉ phải từ 1 đến 6. Đây là validation của `Course.Validate()` được Controller đổi thành HTTP response.

**Nếu còn thời gian:** `DELETE /api/courses/{id}` → **204**, rồi GET lại Id đó → **404**. Nếu đang dùng bản ghi này để chứng minh dữ liệu sau restart, hãy quay phần restart **trước khi DELETE**.

### 4:00-4:35 — SQL log và async

**Màn hình:** Terminal API sau khi gọi Search. Tìm dòng `Microsoft.EntityFrameworkCore.Database.Command` và câu `SELECT ... FROM "Courses" ... ILIKE @pattern`.

**Giải thích:** LINQ trong `SearchAsync` được provider dịch thành SQL PostgreSQL. `@pattern` là tham số truy vấn; log che giá trị bằng `?`. `await` ở `SaveChangesAsync` hoặc `ToListAsync` có ích vì chương trình phải chờ mạng/database, khác `Task.FromResult` của List trong TH2. Chỉ vào đúng câu SQL của request vừa gọi, không cần đọc toàn bộ log.

### 4:35-4:45 — Kết thúc

**Màn hình:** Kết quả DB hoặc Swagger. Chốt bằng 1–2 câu tự nhiên: endpoint vẫn trả đúng status; migration đã đổi schema mà giữ dữ liệu; PostgreSQL giữ bản ghi qua các request và sau khi chạy lại API. Không tuyên bố đã thử restart nếu bạn chưa thực sự thử trên máy mình.

## 3. Giải thích sâu để ôn trước khi quay

### EF Core, DbContext và Service có vai trò gì?

EF Core ánh xạ class `Course` sang bảng và biến các truy vấn LINQ thành lệnh SQL. `CourseDbContext` là phiên làm việc với DB: giữ `DbSet<Course>` và theo dõi entity khi cần cập nhật. `CourseService` giữ logic nghiệp vụ và thao tác dữ liệu; Controller chỉ nhận request, gọi Service rồi chọn HTTP status. Vì DbContext gắn với một scope và không thiết kế để dùng đồng thời từ nhiều request, Service cũng được đăng ký `AddScoped`, thay cho singleton chứa List ở TH2.

### Migration khác `database update` ở điểm nào?

File migration ghi **cần thay đổi schema ra sao**. `dotnet ef database update` **thi hành** những migration chưa được ghi trong `__EFMigrationsHistory`. `InitialCreate` tạo bảng; `AddCourseCreatedAt` chạy `ALTER TABLE ... ADD "CreatedAt" ...`, nên các dòng đang có vẫn tồn tại. Cột mới bắt buộc có `CURRENT_TIMESTAMP` để các dòng cũ nhận một giá trị hợp lệ. `CreatedAt` của dòng cũ là thời điểm migration thêm cột, **không phải thời điểm dòng đó được tạo từ trước**.

### Tại sao POST trả Id và thời gian dù body không gửi?

Trong `CreateAsync`, Service tạo đối tượng mới với Id 0 và không sao chép `CreatedAt` do client gửi. `SaveChangesAsync` gửi INSERT; cột Id dùng identity của PostgreSQL, `CreatedAt` dùng mặc định DB. EF đọc lại hai giá trị từ `RETURNING "Id", "CreatedAt"`. PUT chỉ gán Code, Name, Credits và IsActive nên không đổi thời gian tạo.

### Tại sao có 200, 201, 204, 400 và 404?

GET thành công trả **200**. POST tạo tài nguyên mới trả **201** và `Location`. PUT/DELETE thành công trả **204** không có body. Dữ liệu Course sai hoặc keyword Search rỗng trả **400**. GET/PUT/DELETE theo Id không tồn tại trả **404**. Search không có kết quả vẫn là truy vấn hợp lệ, nên trả **200** với mảng rỗng.

### Search có phải đọc toàn bộ bảng về RAM rồi lọc không?

Không. `Where(... EF.Functions.ILike(...))` được EF Core/Npgsql dịch thành SQL `ILIKE` và chạy ở PostgreSQL. `ToListAsync()` mới thực thi truy vấn và lấy kết quả về. Code escape `%`, `_` và `\` để các ký tự đó trong keyword được hiểu là ký tự cần tìm, không biến thành wildcard SQL. Query dùng tham số `@pattern`; log mặc định không hiện giá trị tham số.

### Chứng minh dữ liệu bền vững thế nào?

Tạo một Course qua POST, ghi lại Id/Code, dừng API bằng Ctrl+C, chạy API lại rồi GET đúng Id hoặc SELECT trong DB. Bản ghi vẫn còn vì được lưu ở PostgreSQL. Chỉ refresh Swagger không phải kiểm chứng restart. Sau khi chứng minh xong, có thể DELETE bản ghi demo để dọn dữ liệu.

## 4. Lệnh chuẩn bị bằng chứng migration trên database demo

Phần này để **chuẩn bị và kiểm tra**, không cần đọc hết trong video. Dùng database demo mới khi muốn ghi hình cả hai trạng thái schema. Cấu hình chuỗi kết nối tới database demo bằng user secrets hoặc biến môi trường; đảm bảo terminal đang trỏ đúng database trước khi chạy lệnh.

```powershell
dotnet ef database update InitialCreate --project Course.Api --startup-project Course.Api
```

```sql
SELECT "Id", "Code" FROM "Courses" ORDER BY "Id";
SELECT "MigrationId" FROM "__EFMigrationsHistory" ORDER BY "MigrationId";
```

Sau đó, trên **cùng database demo**:

```powershell
dotnet ef database update --project Course.Api --startup-project Course.Api
```

```sql
SELECT "Id", "Code", "CreatedAt" FROM "Courses" ORDER BY "Id";
SELECT "MigrationId" FROM "__EFMigrationsHistory" ORDER BY "MigrationId";
```

Kết quả mong đợi: trước có ba dòng `1|C001`, `2|C002`, `3|C003`; sau vẫn ba Id/Code đó, có thêm `CreatedAt`, và history có hai migration. Nếu DB của bạn đã chạy cả hai migration, không rollback chỉ để tạo cảnh “trước”. Bạn có thể cho xem hai file migration, hai dòng trong history và ba dòng hiện tại; hãy nói rõ đó là **giải thích dựa trên migration và trạng thái hiện tại**, không gọi là ảnh chụp trước/sau nếu chưa ghi lại trạng thái trước.

## 5. Tự rà soát trước khi nộp

- Video có tiếng của bạn, xem được, dài dưới 5 phút; chữ trên màn hình đọc được.
- Bạn nói được mỗi checkpoint: provider PostgreSQL, cấu hình connection, DbContext/DbSet, hai migration, dữ liệu cũ, EF Core async, CRUD Swagger, dữ liệu thật, SQL log.
- Bạn phân biệt được `Course.Validate()` với lỗi database, và giải thích được một request cụ thể từ Controller đến PostgreSQL rồi trở về HTTP response.
- Không quay lộ mật khẩu; không nói “đã thử” cho thao tác chưa thực hiện. Dùng Id thực tế từ POST.
- Sau khi tự quay, đặt tên `mssv_th3.mp4`. ZIP phải chứa `source/` và video ở cấp gốc; tải ZIP lên Google Drive và nộp file `.txt` chứa link Drive theo yêu cầu.
