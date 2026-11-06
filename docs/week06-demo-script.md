# Kịch bản video QTKD tuần 06 — 4 phút 55 giây

**Trọng tâm:** giải thích code đã làm gì ở mục **4.2** (Customer, Contract, Pricing) và **4.3** (US03/submit), sau đó dùng **Swagger** chạy luồng chính. Cuối video mở pgAdmin **hoặc terminal `psql`** khoảng 20 giây để chứng minh trạng thái thực sự nằm trong PostgreSQL như bước 13 yêu cầu. Không cần chạy script tự động trong lúc quay.

## Chuẩn bị trước khi bấm quay

1. Áp dụng migration và khởi động API trước; giữ terminal chạy:

   ```powershell
   dotnet ef database update --project QTKD.Api --startup-project QTKD.Api
   dotnet run --project QTKD.Api --launch-profile http
   ```

2. Mở sẵn các tab code theo thứ tự: `Program.cs`, `CustomersController.cs`, `ContractsController.cs`, `PriceListsController.cs`, `QtkdWriteService.cs`, `ContractRules.cs`. Phóng to chữ và thu gọn vùng không liên quan.
3. Mở `http://localhost:5095/swagger` và phần **JSON để dán** ở cuối file này. Sau POST Customer, ghi lại `id` để thay `customerId`; sau POST Contract, ghi `id` để điền vào URL những request tiếp theo.
4. Dùng một `contractNo` mới, ví dụ `HD-SWAGGER-0611-A`; đổi chữ cuối nếu đã tập với số đó. `GET /api/pricelists/1` dùng bảng giá mẫu có hai PriceItem để minh họa phần Pricing.
   Mở sẵn pgAdmin Query Tool, kết nối đúng database QTKD và dán truy vấn sau; sau khi tạo Contract, thay `<contract-id>` bằng ID Swagger vừa trả rồi bấm Execute ở cảnh cuối:

   ```sql
   SELECT "Id", "ContractNo", "CustomerId", "Status", "Summary", "DocumentReference"
   FROM "Contracts" WHERE "Id" = 6;
   ```

   **Nếu dùng terminal thay pgAdmin:** trong PowerShell, chuẩn bị UTF-8 và kết nối **trước khi quay**:

   ```powershell
   chcp 65001
   $env:PGCLIENTENCODING = 'UTF8'
   psql -h localhost -p 5432 -U postgres -d qtkd_week05
   ```

   Nhập mật khẩu PostgreSQL khi được hỏi (ký tự không hiện trên màn hình). Trong dấu nhắc `qtkd_week05=#`, chạy `SHOW client_encoding;` (phải là `UTF8`) và `\pset pager off`. Ở cảnh cuối, dán **nguyên một dòng có dấu `;` ở cuối** rồi Enter; đối chiếu `Id` với response Swagger:

   ```sql
   SELECT "Id", "ContractNo", "Status", "Summary", "DocumentReference" FROM "Contracts" ORDER BY "Id" DESC LIMIT 1;
   ```

   Nếu thấy `qtkd_week05-#`, câu SQL cũ đang nhập dở: bấm `Ctrl+C` để về `qtkd_week05=#` rồi dán lại dòng trên. Khi lọc ID cụ thể, viết `WHERE "Id" = 6;` (**không** viết `"6"` vì PostgreSQL hiểu đó là tên cột). Cảnh báo code page có thể vẫn xuất hiện khi khởi động `psql`; `SHOW client_encoding;` và kết quả truy vấn mới là phần cần kiểm tra.

5. Kiểm tra mã nguồn, migration và tài liệu đã được đưa lên cùng repository tuần 05 trước khi nộp. Giữ mật khẩu/User Secrets ngoài màn hình quay.

## Chỉ đúng file khi giải thích điều kiện 4.2

**Customer — nói trong cảnh 0:20–0:55:**

| Điều kiện/chức năng                                                        | Mở file và chỉ vào đâu                                                                                                                                                |
| -------------------------------------------------------------------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Route tạo, đọc danh sách/chi tiết, cập nhật                                | `QTKD.Api/Controllers/CustomersController.cs`: các hàm `GetAll`, `GetById`, `Create`, `Update`. Controller chuyển request sang service, không chứa quy tắc nghiệp vụ. |
| Tên bắt buộc, không quá 200 ký tự                                          | `QTKD.Api/Services/QtkdWriteService.cs`: `CreateCustomerAsync` dòng 14 và `UpdateCustomerAsync` dòng 28 gọi helper `ValidText` dòng 167; lỗi trả 400 `InvalidName`.   |
| Customer mới mặc định `Active`; chỉ cập nhật sang `Active` hoặc `Inactive` | Cùng file: dòng 17 gán `Status = "Active"`; dòng 30 từ chối status khác; `FindAsync` dòng 26 trả 404 khi không có ID.                                                 |
| Ràng buộc schema                                                           | `QTKD.Api/Data/QtkdDbContext.cs` dòng 17–19: khóa chính `Id`, `Name` bắt buộc/tối đa 200, `Status` bắt buộc/tối đa 32.                                                |
| GET đọc từ database                                                        | `QTKD.Api/Services/QtkdReadService.cs` gọi `QTKD.Api/Repositories/QtkdRepository.cs`; repository dùng `AsNoTracking` cho truy vấn đọc.                                |

**PriceList/PriceItem — nói trong cảnh 0:55–1:20:**

| Điều kiện/chức năng                                                                                 | Mở file và chỉ vào đâu                                                                                                                                                             |
| --------------------------------------------------------------------------------------------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Route tạo/xem PriceList, thêm/xem PriceItem                                                         | `QTKD.Api/Controllers/PriceListsController.cs`: `GetAll`, `GetById`, `GetItems`, `Create`, `AddItem`.                                                                              |
| Tên PriceList bắt buộc/tối đa 200; `EffectiveDate` phải có giá trị                                  | `QTKD.Api/Services/QtkdWriteService.cs`: `CreatePriceListAsync` dòng 135–142; lỗi 400 `InvalidPriceList`.                                                                          |
| PriceItem phải thuộc PriceList tồn tại                                                              | Cùng file: `AddPriceItemAsync` dòng 149 tìm PriceList; không có thì 404.                                                                                                           |
| `ServiceCode` bắt buộc/tối đa 50; `UnitPrice` dương, tối đa 2 số thập phân, vừa cột `numeric(18,2)` | Cùng file: `AddPriceItemAsync` dòng 151–153; lỗi 400 `InvalidPriceItem`.                                                                                                           |
| Không trùng ServiceCode trong **cùng** PriceList                                                    | Cùng file: dòng 155 kiểm tra trước khi thêm, trả 409 `DuplicatePriceItem`.                                                                                                         |
| Ràng buộc bảng và quan hệ                                                                           | `QTKD.Api/Data/QtkdDbContext.cs` dòng 48–64: khóa chính PriceList; khóa ghép `(PriceListId, ServiceCode)` của PriceItem; `UnitPrice` precision `(18,2)`; khóa ngoại tới PriceList. |
| GET danh sách/chi tiết và items                                                                     | `QTKD.Api/Services/QtkdReadService.cs` gọi `QTKD.Api/Repositories/QtkdRepository.cs`, truy vấn có `Include(x => x.PriceItems)`.                                                    |

Nói ngắn trên video: **Controller định tuyến → `QtkdWriteService` kiểm tra và lưu → `QtkdDbContext` ràng buộc bảng; GET đi qua `QtkdReadService`/repository.** Không cần đọc từng dòng trong hai bảng này; chỉ dừng ở dòng kiểm tra quan trọng rồi nói kết quả.

## Quay theo mốc thời gian

| Thời gian     | Thao tác trên màn hình                                                                                                                                                     | Lời thoại gợi ý                                                                                                                                                                                                                                                                                                                  | Yêu cầu                                          |
| ------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ------------------------------------------------ |
| **0:00–0:20** | Mở `Program.cs`; chỉ `AddDbContext`, đăng ký repository/read/write service và Swagger.                                                                                     | “Em tiếp tục solution tuần 05. Controller nhận HTTP, service kiểm tra nghiệp vụ, EF Core lưu PostgreSQL. Swagger dùng để thử endpoint.”                                                                                                                                                                                          | Nền tuần 05                                      |
| **0:20–0:55** | Chỉ `CustomersController`, `ContractsController`, rồi `CreateCustomerAsync`, `UpdateCustomerAsync`, `CreateContractAsync`, `UpdateContractAsync` trong `QtkdWriteService`. | “Mục 4.2: Customer có tạo, xem và cập nhật; tên bắt buộc. Contract có tạo, xem, sửa; Customer phải Active, ngày kết thúc không trước ngày bắt đầu, số hợp đồng không trùng. Hợp đồng mới luôn là Draft.”                                                                                                                         | 4.2 Customer/Contract                            |
| **0:55–1:20** | Chỉ `PriceListsController`, `CreatePriceListAsync`, `AddPriceItemAsync`.                                                                                                   | “Pricing có tạo/xem bảng giá và thêm/xem mục giá. PriceItem phải thuộc PriceList có thật; ServiceCode không trùng trong bảng và UnitPrice phải dương, tối đa hai số thập phân.”                                                                                                                                                  | 4.2 Pricing                                      |
| **1:20–2:05** | Mở `ContractRules.cs`, rồi `SubmitContractAsync` trong `QtkdWriteService`. Chỉ `Draft`, `ValidateSubmission`, `PendingApproval`, `SaveChangesAsync` và nhánh lỗi.          | “Mục 4.3: POST `/api/contracts/{id}/submit` chỉ nhận Draft. CTR-02 kiểm tra Customer Active, ngày, Summary và DocumentReference; thiếu thì 400 trước khi lưu. Hợp lệ mới đổi sang PendingApproval. Gửi lặp trả 409. CTR-01 chỉ cho sửa Draft hoặc RevisionRequested; CTR-03 chặn đặt Approved trực tiếp. Chưa có Approval thật.” | 8–12; CTR-01–03                                  |
| **2:05–2:15** | Chuyển sang Swagger.                                                                                                                                                       | “Sau đây em thử luồng Customer → Contract → Submit bằng Swagger.”                                                                                                                                                                                                                                                                | Bắt đầu bước 13                                  |
| **2:15–2:45** | `POST /api/customers` với JSON A, ghi `id`; `POST /api/contracts` với JSON B, thay `customerId`, ghi `id`.                                                                 | “Customer tạo 201. Contract tạo 201 ở Draft; em cố ý để thiếu nội dung và tài liệu.”                                                                                                                                                                                                                                             | Tạo Customer, Contract Draft                     |
| **2:45–3:15** | `POST /api/contracts/{id}/submit`: 400 `CTR-02`. `GET /api/contracts/{id}`: Draft. `PUT /api/contracts/{id}` với JSON C: 400 `CTR-03`.                                     | “Gửi thiếu điều kiện bị từ chối, dữ liệu vẫn Draft. Thử chuyển thẳng Draft sang Approved cũng bị từ chối.”                                                                                                                                                                                                                       | 10–11; CTR-02/03                                 |
| **3:15–3:45** | `PUT /api/contracts/{id}` với JSON D: 200. `POST /api/contracts/{id}/submit`: 200, PendingApproval.                                                                        | “Em bổ sung dữ liệu khi còn Draft. Lúc này submit hợp lệ, chỉ chuyển sang PendingApproval.”                                                                                                                                                                                                                                      | CTR-01 nhánh sửa được; submit thành công         |
| **3:45–4:10** | Gọi lại `POST /api/contracts/{id}/submit`: 409. Thử `PUT /api/contracts/{id}` bằng JSON D: 409 `CTR-01`.                                                                   | “Gửi lặp không tạo thêm kết quả xử lý; sau khi PendingApproval thì không được sửa.”                                                                                                                                                                                                                                              | 12; CTR-01; gửi lặp                              |
| **4:10–4:20** | `GET /api/contracts/{id}`: PendingApproval; nếu còn thời gian, `GET /api/pricelists/1` chỉ hai items mẫu.                                                                  | “Đọc lại qua API, trạng thái vẫn PendingApproval.”                                                                                                                                                                                                                                                                               | Đọc lại; 4.2 Pricing                             |
| **4:20–4:45** | Chuyển pgAdmin đã mở sẵn, điền Contract ID vào truy vấn SQL, Execute và chỉ hàng kết quả.                                                                                  | “Trong bảng Contracts của PostgreSQL, chính hợp đồng này có trạng thái PendingApproval, Summary và DocumentReference đã lưu. Lần gửi lỗi trước đó không đổi Draft.”                                                                                                                                                              | Bước 13: xem trạng thái trực tiếp trong database |
| **4:45–4:55** | Mở bảng đối chiếu trong `docs/week06-api.md`.                                                                                                                              | “Tài liệu ghi request/response và đối chiếu US03/CTR-01–03. Summary và DocumentReference là giả định demo, cần xác nhận với Phase 1.”                                                                                                                                                                                            | Bước 8–9; hồ sơ nộp 4.5                          |

**Mẹo giữ dưới 5 phút:** tập một lượt thao tác Swagger; dán JSON đã chuẩn bị, chỉ đọc to mã HTTP, `code` lỗi và `status`, không đọc toàn bộ response.

## JSON để dán vào Swagger

**A — `POST /api/customers`**

```json
{ "name": "Công ty Demo Swagger" }
```

**B — `POST /api/contracts`** (đổi `customerId` theo response A; đổi `contractNo` nếu đã dùng)

```json
{ "contractNo": "HD-SWAGGER-0611-A", "customerId": 7, "startDate": "2026-11-06", "endDate": "2027-11-05" }
```

**C — `PUT /api/contracts/{id}` để thử CTR-03** (đổi `customerId`, giữ cùng `contractNo`)

```json
{ "contractNo": "HD-SWAGGER-0611-A", "customerId": 7, "startDate": "2026-11-06", "endDate": "2027-11-05", "status": "Approved" }
```

**D — `PUT /api/contracts/{id}` để bổ sung và sau đó thử CTR-01** (đổi `customerId`, giữ cùng `contractNo`)

```json
{ "contractNo": "HD-SWAGGER-0611-A", "customerId": 7, "startDate": "2026-11-06", "endDate": "2027-11-05", "summary": "Cung cấp dịch vụ bảo trì", "documentReference": "DOC-SWAGGER-0611-A" }
```

Swagger không tự thay ID trong JSON: dùng đúng `customerId` và Contract `{id}` của lượt quay. `POST /api/contracts/{id}/submit` **không có JSON body**; chỉ điền ID trên URL. Các JSON A–D không thay thế tài liệu API, code kiểm tra nghiệp vụ hoặc cảnh pgAdmin chứng minh dữ liệu trong PostgreSQL.

## JSON và các bước 8–13: phần nào chứng minh điều gì?

| Bước                                                 | Bằng chứng cần có                                                                                                                                                                                        | JSON A–D đóng vai trò gì?                                                                                                                                                                     |
| ---------------------------------------------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| **8 — US03/Acceptance Criteria**                     | Nêu điều kiện gửi, Draft → PendingApproval và các trường hợp từ chối; đối chiếu với bản Phase 1 thật.                                                                                                    | B để thiếu nội dung/tài liệu; D bổ sung. **Workspace chưa có US03/Acceptance Criteria Phase 1**, nên `Summary` và `DocumentReference` mới là giả định demo, chưa thể khẳng định khớp Phase 1. |
| **9 — endpoint và mẫu phản hồi**                     | Chỉ `ContractsController.Submit`, Swagger `POST /api/contracts/{id}/submit`, và `docs/week06-api.md` có mẫu 200/400/409.                                                                                 | Submit **không gửi JSON**. Đáp ứng bước này bằng URL, response và tài liệu API.                                                                                                               |
| **10 — service kiểm tra trước khi lưu**              | Chỉ `QtkdWriteService.SubmitContractAsync` và `ContractRules.ValidateSubmission`: Draft, Customer Active, ngày hợp lệ, Summary, DocumentReference; sau đó mới gán PendingApproval và `SaveChangesAsync`. | B dùng Customer mới với ngày hợp lệ nhưng thiếu hai trường; D điền đủ hai trường để thử nhánh thành công.                                                                                     |
| **11 — lỗi không đổi dữ liệu; cấm Draft → Approved** | Submit hợp đồng tạo từ B trả 400 `CTR-02`; GET ngay sau vẫn Draft. PUT C trả 400 `CTR-03`; PUT D tiếp đó vẫn sửa được vì Contract còn Draft.                                                             | C là request cố đặt `status: "Approved"`; không có thao tác chuyển thẳng trạng thái.                                                                                                          |
| **12 — gửi lặp và giới hạn mô phỏng**                | Sau PUT D, submit lần đầu trả 200 PendingApproval; lần hai trả 409 `InvalidTransition`. Nói rõ bản này chỉ đổi `Contract.Status`, **chưa tạo Approval record/quy trình duyệt**.                          | Cùng một Contract ID được gọi submit hai lần; không có JSON mới.                                                                                                                              |
| **13 — luồng và database**                           | A → B → submit lỗi → D → submit thành công → chạy SQL trong pgAdmin thấy hàng Contract là PendingApproval.                                                                                               | JSON tạo/bổ sung dữ liệu; **Swagger GET không thay thế truy vấn trực tiếp PostgreSQL**.                                                                                                       |
