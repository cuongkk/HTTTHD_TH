# Kịch bản quay TH5 - mục tiêu 4 phút 45 giây

Kịch bản này bám theo source hiện có. Hãy diễn đạt lại bằng lời của mình sau khi tự chạy được chương trình; đừng đọc nguyên văn khi chưa hiểu. Các câu trong dấu ngoặc vuông là **thao tác trên màn hình**, không cần đọc.

## Chuẩn bị trước khi bấm quay

1. Làm theo `README_PHASE2.md`: tạo database QTKD riêng, cấu hình User Secrets, chạy migration, khởi động API ở `http://localhost:5095`.
2. Mở sẵn IDE với `QTKD.slnx`, Swagger ở `http://localhost:5095/swagger`, pgAdmin/psql kết nối **đúng database QTKD**, và README có sơ đồ thành phần. Phóng to chữ để đọc được trong video.
3. Trong pgAdmin mở sẵn các câu `SELECT` ở README; chuẩn bị trang repository GitHub **sau khi đã push source, migration và README**. Không hiện lệnh `user-secrets set`, mật khẩu, hoặc giá trị chuỗi kết nối trên màn hình.
4. Thu thử 15 giây để kiểm tra tiếng. Khi quay, chuyển cửa sổ gọn; không chờ build hoặc cài package trong video.

## 0:00–0:30 - Mở đầu và cấu trúc

**[Hiện `QTKD.slnx`, cây thư mục `QTKD.Api`, sơ đồ trong README.]**

“Em là ... , mã số sinh viên 23120224. Đây là kết quả tuần 05 của Phase 2 QTKD: Web API đọc Customer và Contract từ PostgreSQL. Solution tách Controller, Service, Repository và DbContext. Sơ đồ này cho thấy request đi từ HTTP đến database rồi trả kết quả về.”

## 0:30–1:25 - Mô hình dữ liệu

**[Mở lần lượt `Models/Customer.cs`, `Contract.cs`, `PriceList.cs`, `PriceItem.cs`, rồi `Data/QtkdDbContext.cs`.]**

“Em có bốn entity. Customer có Id, Name, Status. Contract có số hợp đồng, CustomerId, ngày bắt đầu, ngày kết thúc và trạng thái. CustomerId là khóa ngoại: một khách hàng có thể có nhiều hợp đồng, nhưng mỗi hợp đồng thuộc một khách hàng. PriceList có tên và ngày hiệu lực. Mỗi PriceItem là một dịch vụ với đơn giá thuộc một PriceList. Em dùng cặp PriceListId và ServiceCode làm khóa chính để một dịch vụ không bị lặp trong cùng bảng giá. Trong DbContext, `HasOne` và `WithMany` khai báo hai quan hệ này; đơn giá có độ chính xác hai chữ số thập phân.”

## 1:25–2:10 - Migration và dữ liệu mẫu

**[Chỉ `HasData` trong DbContext và file `Data/Migrations/*_InitialCreate.cs`; chuyển qua bảng `__EFMigrationsHistory` trong PostgreSQL.]**

“`HasData` khai báo dữ liệu mẫu cố định: Minh Phát và An Bình, một hợp đồng `HD-2026-018` ở trạng thái Draft của Minh Phát, và một bảng giá có hai PriceItem. Em tạo migration `InitialCreate` từ mô hình; khi chạy `dotnet ef database update`, EF tạo bốn bảng, các khóa và thêm dữ liệu mẫu vào PostgreSQL. Bảng `__EFMigrationsHistory` ghi migration đã áp dụng, nên chạy lại lệnh không tạo lại các bản ghi này. Dữ liệu nằm trong database, không phải danh sách viết sẵn trong API.”

## 2:10–2:45 - Khởi động và cấu hình

**[Mở `Program.cs`, chỉ các dòng `GetConnectionString`, `UseNpgsql`, `AddScoped`, `UseSwagger`, `MapControllers`.]**

“Trong `Program.cs`, ứng dụng đọc cấu hình `ConnectionStrings:QtkdDb` và dùng `UseNpgsql` để kết nối PostgreSQL. Mật khẩu được giữ trong User Secrets hoặc biến môi trường, không đưa vào source hay GitHub. `AddScoped` đăng ký Repository và Service cho từng request. Swagger chỉ bật khi chạy môi trường Development; `MapControllers` đưa hai endpoint vào ứng dụng.”

## 2:45–3:35 - Giải thích luồng code

**[Mở `CustomersController.cs` → `QtkdReadService.cs` → `QtkdRepository.cs` → `ContractsController.cs`.]**

“Khi gọi `GET /api/customers`, Controller nhận request và gọi Service. Service gọi Repository, Repository dùng `db.Customers` và `ToListAsync` để truy vấn PostgreSQL theo Id. `AsNoTracking` phù hợp vì đây là thao tác đọc, EF không cần theo dõi entity để cập nhật. Service chuyển entity sang `CustomerResponse`, rồi Controller trả HTTP 200 cùng JSON. Với `GET /api/contracts/{id}`, Repository tìm theo Id và dùng `Include` lấy Customer để response có cả tên khách hàng. Nếu không tìm thấy hợp đồng, Service trả null và Controller trả 404.”

## 3:35–4:35 - Demo và đối chiếu database

**[Trong Swagger gọi `/api/customers`, `/api/contracts/1`, `/api/contracts/999`; trong pgAdmin chạy các SELECT đã chuẩn bị.]**

“Đây là hai Customer API trả về. Trong bảng `Customers` của PostgreSQL cũng là đúng hai dòng này. Gọi hợp đồng Id 1 cho thấy số `HD-2026-018`, CustomerId 1 và trạng thái Draft; truy vấn bảng `Contracts` cho cùng dữ liệu. Hai PriceItem nằm trong bảng `PriceItems` và liên kết với PriceListId 1. Thử Id 999 không có trong database thì API trả 404. Như vậy kết quả đọc qua HTTP khớp với dữ liệu đã lưu trong PostgreSQL.”

## 4:35–4:55 - Kết luận

**[Hiện sơ đồ README hoặc repository GitHub nếu đã có link.]**

“Tuần 05 em đã có migration, dữ liệu mẫu và hai API đọc. README hướng dẫn chạy lại; source, migration và README nằm trong repository dùng tiếp cho tuần 06–07. Tuần 06 em sẽ làm luồng tạo, sửa và gửi hợp đồng.”

## Nếu giảng viên hỏi thêm

- **Vì sao tắt API rồi dữ liệu vẫn còn?** PostgreSQL lưu dữ liệu độc lập với tiến trình API; migration thêm dữ liệu vào database.
- **`AsNoTracking` có tác dụng gì?** Giảm công theo dõi thay đổi cho truy vấn chỉ đọc; không dùng nó nếu cần sửa entity rồi `SaveChanges`.
- **Vì sao có `Include(x => x.Customer)`?** Để lấy Customer liên quan và đưa tên khách hàng vào `ContractResponse`.
- **Tuần 05 đã có chức năng gửi hợp đồng chưa?** Chưa. PDF giao phần tạo/sửa/submit và rule CTR-01–03 cho tuần 06; tuần 05 mới dựng schema, seed và hai GET API.
- **Vì sao không có mật khẩu trong GitHub?** `appsettings.json` để chuỗi rỗng; User Secrets nằm ngoài source hoặc dùng biến môi trường khi chạy.

## Rà soát trước khi nộp

Video có tiếng và thời lượng **không quá 5 phút**; bạn tự giải thích được từng đoạn đã nói. Repository cá nhân có source, migration, README và dùng tiếp cho tuần 06–07. ZIP `23120224.zip` chứa `source/` và `23120224_th5.mp4` ở cấp gốc; bỏ `bin/`, `obj/`, User Secrets. Upload ZIP lên Drive, kiểm tra quyền truy cập bằng cửa sổ ẩn danh, nộp file TXT chứa link Drive lên Moodle. Đối chiếu thêm hướng dẫn Moodle về nơi nộp link repository.
