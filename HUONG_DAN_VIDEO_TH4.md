# Kịch bản video TH4 – nói theo từng yêu cầu 5.2 và 5.3

Video mục tiêu **4 phút 45 giây**, tối đa 5 phút. Mỗi mục dưới đây đi theo đúng thứ tự gạch đầu dòng của đề: **bấm vào file hoặc thao tác**, **chỉ đoạn cần nhìn**, rồi **nói ý chính**. Lời nói là mẫu để tập; khi quay hãy diễn đạt lại bằng cách hiểu của mình và chỉ nói kết quả đã chạy được trên máy.

## Chuẩn bị trước khi bấm quay

**Terminal 1 — đứng trong thư mục source và kiểm tra môi trường:**

```powershell
cd 'D:\TH2_ASPNET_Core_WebAPI_Source_va_HuongDan\source'
dotnet --version
Get-Service postgresql*
Test-NetConnection 127.0.0.1 -Port 5432 | Select-Object TcpTestSucceeded
dotnet ef --version
```

PostgreSQL cần đang chạy và `TcpTestSucceeded` phải là `True` ở **cổng thực tế của máy bạn**. Trên máy này có service `postgresql-x64-17`; nếu PostgreSQL dùng cổng khác 5432, thay cổng trong lệnh kiểm tra và connection string. Lần kiểm tra trước, User Secrets đang trỏ tới `127.0.0.1:64888` nhưng cổng đó từ chối kết nối, nên cần cập nhật tới instance PostgreSQL đang chạy trước khi demo.

**Nếu chưa cấu hình đúng kết nối, chạy một lần trước khi quay** (thay database, user, cổng và mật khẩu theo máy; **không ghi hình lệnh này**):

```powershell
dotnet user-secrets set 'ConnectionStrings:CourseDb' 'Host=127.0.0.1;Port=5432;Database=course_th3;Username=postgres;Password=<mat-khau-cua-ban>' --project Course.Api
```

**Terminal 1 — chuẩn bị database, build và giữ API chạy:**

```powershell
dotnet restore TH3.slnx
dotnet build TH3.slnx --no-restore
dotnet ef database update --project Course.Api --startup-project Course.Api
dotnet run --project Course.Api --launch-profile http
```

`database update` chỉ áp dụng migration còn thiếu; không xóa database để quay lại từ đầu. Khi thấy `Now listening on: http://localhost:5082`, giữ terminal này mở. Nếu lệnh migration lỗi kết nối, kiểm tra lại cổng, database và User Secrets rồi mới quay.

**Terminal 2 — kiểm tra API trước khi quay:**

```powershell
cd 'D:\TH2_ASPNET_Core_WebAPI_Source_va_HuongDan\source'
dotnet run --project Course.Api.Checks -- http://localhost:5082
```

Sau khi các check đạt, mở `http://localhost:5082/swagger`. GET `/api/courses` để xác nhận C002 có `isActive: true` và C003 có `isActive: false`. Chuẩn bị body POST/PUT trong [Course.Api.http](Course.Api/Course.Api.http); khi POST môn demo, dùng **Id thật** từ response. Mở sẵn các file được liên kết dưới đây thành tab, chỉnh cỡ chữ đủ đọc và giữ sẵn Swagger, terminal API, pgAdmin/psql. Không mở User Secrets hoặc mật khẩu trên màn hình quay.

## 0:00–0:12 — Mở đầu

**Bấm:** mở cây thư mục `Course.Api` trong editor.

**Nói:** “Em trình bày TH4 theo các yêu cầu refactor và business rule. Em sẽ chỉ từng lớp trong source, rồi dùng Swagger để đối chiếu với kết quả HTTP.”

## 5.2 — Yêu cầu refactor

### 0:12–0:37 — 1. Controller không truy cập DbContext trực tiếp

**Bấm:** [CoursesController.cs](Course.Api/Controllers/CoursesController.cs), chỉ constructor nhận `ICourseService` và action `Create`. Bấm tiếp [CourseService.cs](Course.Api/Services/CourseService.cs), chỉ constructor nhận `ICourseRepository`; cuối cùng [CourseRepository.cs](Course.Api/Repositories/CourseRepository.cs), chỉ constructor nhận `CourseDbContext`.

**Nói:** “Controller chỉ nhận request, gọi Service và trả response. Service gọi Repository; chỉ Repository nhận DbContext để làm việc với database. Vì vậy HTTP và thao tác dữ liệu nằm ở hai lớp khác nhau.”

### 0:37–1:03 — 2. DTO cho request và response

**Bấm:** [CourseUpsertRequest.cs](Course.Api/DTOs/CourseUpsertRequest.cs) → [CourseResponse.cs](Course.Api/DTOs/CourseResponse.cs) → `ToResponse` trong [CourseService.cs](Course.Api/Services/CourseService.cs). Nếu còn thời gian, bấm schema POST trong Swagger.

**Nói:** “Request DTO chỉ nhận Code, Name, Credits và IsActive. Response DTO có thêm Id và CreatedAt. Service chuyển Entity thành response; Id và CreatedAt do database cấp, không lấy theo hai giá trị client tự gửi.”

### 1:03–1:32 — 3. Repository đóng gói data access; lợi ích và trade-off

**Bấm:** [ICourseRepository.cs](Course.Api/Repositories/ICourseRepository.cs), rồi `SearchAsync`, `Add`, `SaveChangesAsync` trong [CourseRepository.cs](Course.Api/Repositories/CourseRepository.cs).

**Nói:** “Repository gom truy vấn EF Core và việc lưu dữ liệu. Service chỉ biết hợp đồng `ICourseRepository`, nên quy tắc nghiệp vụ không phải viết SQL hay thao tác DbContext. Đổi lại project nhỏ phải duy trì thêm interface và lớp Repository; em chọn cách này để ranh giới dữ liệu rõ ràng.”

### 1:32–2:00 — 4. Validation đặt ở đâu, client thấy lỗi gì

**Bấm:** `Validate()` trong [Course.cs](Course.Api/Models/Course.cs), rồi lời gọi `course.Validate()` trước `repository.Add` trong [CourseService.cs](Course.Api/Services/CourseService.cs). Chuyển Swagger, POST `{"code":"X","name":"Sai tín chỉ","credits":7,"isActive":true}`.

**Nói:** “Entity kiểm tra Code không rỗng và Credits từ 1 đến 6. Service gọi validation trước khi lưu. Ở request này Credits là 7 nên client nhận 400 với `title` và `detail` giải thích lỗi; không có bản ghi mới được thêm.”

### 2:00–2:28 — 5. Exception được xử lý thống nhất

**Bấm:** `TryHandleAsync` trong [ApiExceptionHandler.cs](Course.Api/Exceptions/ApiExceptionHandler.cs), rồi `AddExceptionHandler` và `UseExceptionHandler` trong [Program.cs](Course.Api/Program.cs). Bấm lại [CoursesController.cs](Course.Api/Controllers/CoursesController.cs) để thấy các action không có `try/catch` lặp lại.

**Nói:** “Một handler đổi lỗi validation thành 400, không tìm thấy thành 404 và Course Inactive thành 409, cùng cấu trúc ProblemDetails. Controller không tự bắt lại cùng một loại lỗi ở từng endpoint.”

### 2:28–2:55 — 6. DI nối Controller → Service → Repository

**Bấm:** các dòng `AddDbContext`, `AddScoped<ICourseRepository,...>` và `AddScoped<ICourseService,...>` trong [Program.cs](Course.Api/Program.cs). Đặt cạnh ba constructor vừa chỉ ở mục 1.

**Nói:** “Program đăng ký DbContext, Repository và Service theo scope. Khi có request, DI cấp Service cho Controller, Repository cho Service và DbContext cho Repository. `Scoped` giúp các đối tượng đó làm việc trong phạm vi một request.”

### 2:55–3:35 — 7. Giữ behavior của TH2/TH3

**Bấm:** Swagger và lướt các route GET, POST, PUT, DELETE, Search; có thể mở [Course.Api.http](Course.Api/Course.Api.http) để dùng body đã chuẩn bị. Chỉ response POST với Id thật và header `Location`, sau đó một response lỗi.

**Nói:** “Các route cũ vẫn giữ nguyên. GET thành công là 200; POST tạo Course trả 201 và `Location`; PUT và DELETE thành công trả 204. Dữ liệu sai trả 400, Id không có trả 404. Search vẫn tìm Code hoặc Name, không khớp trả `200 []`. Dữ liệu được Repository ghi vào PostgreSQL nên em dùng Id thực tế do POST trả về để kiểm tra tiếp.”

## 5.3 — Business Rule nhỏ

### 3:35–3:50 — 1. Requirement viết bằng lời trước khi code

**Bấm:** mục “Requirement in plain language” ở [kế hoạch TH4](docs/superpowers/plans/2026-10-23-th4-application-architecture.md), rồi trở về Swagger.

**Nói:** “Yêu cầu em viết trước khi code là: trước đăng ký mới phải tìm Course; Active thì đủ điều kiện, Inactive thì từ chối, Id không tồn tại thì báo không tìm thấy. Use case này chỉ kiểm tra điều kiện, chưa tạo Enrollment.”

### 3:50–4:08 — 2. Rule nằm ở layer nào và vì sao

**Bấm:** `EnsureCanRegisterAsync` trong [CourseService.cs](Course.Api/Services/CourseService.cs), rồi `GetByIdAsync` trong [CourseRepository.cs](Course.Api/Repositories/CourseRepository.cs).

**Nói:** “Service kiểm tra `IsActive` vì đó là quyết định nghiệp vụ. Repository chỉ lấy Course từ database; Controller chỉ gọi use case và trả kết quả HTTP.”

### 4:08–4:35 — 3. Kiểm chứng trường hợp hợp lệ và không hợp lệ

**Bấm:** Swagger `POST /api/courses/2/registration-check` → `Execute`; tiếp tục `POST /api/courses/3/registration-check` → `Execute`. Chỉ response 204 và 409. Nếu còn thời gian, thử một Id không tồn tại → 404. Có thể chỉ ba kịch bản tương ứng trong [Course.Api.Checks/Program.cs](Course.Api.Checks/Program.cs).

**Nói:** “C002 Active qua kiểm tra nên trả 204. C003 Inactive bị từ chối bằng 409 và thông báo dễ hiểu. Id không có trả 404. Các lần gọi này không tạo đăng ký hay thay đổi Course; chúng chỉ chứng minh rule.”

### 4:35–4:45 — 4. Review phần code có AI hỗ trợ

**Bấm:** giữ `EnsureCanRegisterAsync` trong Service; chỉ điều kiện `!course.IsActive`.

**Nói:** “Em đã review vị trí rule: điều kiện Inactive ở Service, còn Controller và Repository không tự quyết định nghiệp vụ. Đây là lý do em giữ cách tách lớp này.”

## Tự rà soát

- Tập nói và bấm một lần; nếu vượt 5 phút, giảm thao tác lặp và câu dẫn, vẫn giữ đủ 11 yêu cầu trên.
- Chỉ nói “đã chạy”, “trả 204/409” hoặc “dữ liệu đã lưu” khi màn hình thực sự cho thấy kết quả đó trên máy của mình.
- Trong video cần nghe được lời giải thích của sinh viên, không chỉ lướt code hoặc đọc văn bản; chữ, response và status phải đủ lớn để xem.
