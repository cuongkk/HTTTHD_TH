# QTKD tuần 06: API và đối chiếu US03

## Phạm vi và giả định

Tiếp tục solution và PostgreSQL tuần 05. Trong workspace không có bản US03, Acceptance Criteria hoặc danh sách tệp bắt buộc của Phase 1. Bản demo này giả định hợp đồng cần `Summary` (nội dung tóm tắt) và `DocumentReference` (mã tham chiếu tài liệu) khác rỗng trước khi gửi. Đây **chưa phải yêu cầu chính thức**; cần đối chiếu lại với kết quả Phase 1. API chỉ lưu mã tham chiếu, không nhận hay kiểm tra tệp thật.

`StartDate` và `EndDate` phải có giá trị; `EndDate >= StartDate`. Customer phải tồn tại và `Active` khi tạo, sửa hoặc gửi hợp đồng. Trạng thái mới của Contract là `Draft`; gửi trường `status` trong POST/PUT Contract bị từ chối 400 theo CTR-03. Không có endpoint đặt `Approved`. `RevisionRequested` được cho phép sửa theo CTR-01 nhưng chưa có quy trình Approval để chuyển vào trạng thái đó.

## Danh sách API

| Phương thức | Đường dẫn | Kết quả chính |
|---|---|---|
| GET | `/api/customers` | 200, danh sách Customer |
| GET | `/api/customers/{id}` | 200 hoặc 404 |
| POST | `/api/customers` | 201; tên bắt buộc, status mặc định Active |
| PUT | `/api/customers/{id}` | 200; tên bắt buộc, status Active hoặc Inactive |
| GET | `/api/contracts/{id}` | 200 hoặc 404 |
| POST | `/api/contracts` | 201, tạo Draft; Summary và DocumentReference có thể thiếu để bổ sung sau |
| PUT | `/api/contracts/{id}` | 200 khi Draft/RevisionRequested; 409 khi trạng thái khác |
| POST | `/api/contracts/{id}/submit` | 200 PendingApproval; 400 khi thiếu điều kiện; 409 khi sai trạng thái hoặc gửi lặp |
| GET | `/api/pricelists` | 200, danh sách kèm items |
| GET | `/api/pricelists/{id}` | 200 hoặc 404, kèm items |
| POST | `/api/pricelists` | 201; tên và EffectiveDate bắt buộc |
| GET | `/api/pricelists/{id}/items` | 200 hoặc 404 |
| POST | `/api/pricelists/{id}/items` | 201; ServiceCode không trùng trong bảng giá, UnitPrice dương và tối đa 2 chữ số thập phân |

JSON dùng tên thuộc tính kiểu camelCase và ngày dạng `YYYY-MM-DD`. Các lỗi nghiệp vụ có mẫu `{"code":"CTR-02","message":"..."}`; `404` của GET không có body theo cách triển khai hiện tại. `201` trả object vừa tạo, không có header Location.

## Request/response mẫu

Các ID dưới đây là ví dụ; dùng ID thực tế từ phản hồi trên máy chạy. File `QTKD.Api/QTKD.Api.http` có các request để chạy lần lượt.

```http
POST /api/customers
Content-Type: application/json

{"name":"Công ty Demo Tuần 06"}
```

```http
HTTP/1.1 201 Created

{"id":3,"name":"Công ty Demo Tuần 06","status":"Active"}
```

```http
POST /api/contracts
Content-Type: application/json

{"contractNo":"HD-2026-DEMO-06","customerId":3,"startDate":"2026-11-06","endDate":"2027-11-05"}
```

```http
HTTP/1.1 201 Created

{"id":2,"contractNo":"HD-2026-DEMO-06","customerId":3,"customerName":"Công ty Demo Tuần 06","startDate":"2026-11-06","endDate":"2027-11-05","status":"Draft","summary":null,"documentReference":null}
```

Gửi lần đầu trả `400` với `code: "CTR-02"`, liệt kê `Summary, DocumentReference`; Contract vẫn `Draft`. Bổ sung bằng `PUT /api/contracts/2` với đầy đủ các trường của request tạo và `"summary":"Cung cấp dịch vụ bảo trì","documentReference":"DOC-DEMO-06"`. Sau đó:

```http
POST /api/contracts/2/submit
```

```http
HTTP/1.1 200 OK

{"id":2,"contractNo":"HD-2026-DEMO-06","customerId":3,"customerName":"Công ty Demo Tuần 06","startDate":"2026-11-06","endDate":"2027-11-05","status":"PendingApproval","summary":"Cung cấp dịch vụ bảo trì","documentReference":"DOC-DEMO-06"}
```

Gửi lần nữa trả `409` với `code: "InvalidTransition"`. Sửa Contract ở `PendingApproval` trả `409` với `code: "CTR-01"`.

```http
POST /api/pricelists
Content-Type: application/json

{"name":"Bảng giá demo tuần 06","effectiveDate":"2026-11-06"}
```

```http
POST /api/pricelists/2/items
Content-Type: application/json

{"serviceCode":"SVC-DEMO-06","unitPrice":1250000}
```

`GET /api/pricelists/2` trả bảng giá và mảng `items` chứa `priceListId`, `serviceCode`, `unitPrice`.

## Đối chiếu quy tắc

| Yêu cầu | Vị trí xử lý | Cách chứng minh |
|---|---|---|
| US03: chỉ gửi Draft hợp lệ | `QtkdWriteService.SubmitContractAsync` gọi `ContractRules.ValidateSubmission` | Gửi thiếu dữ liệu: 400, vẫn Draft; bổ sung rồi gửi: 200, PendingApproval |
| CTR-01: chỉ sửa Draft/RevisionRequested | `ContractRules.CanEdit`, `QtkdWriteService.UpdateContractAsync` | PUT khi PendingApproval: 409, dữ liệu không đổi |
| CTR-02: Customer, ngày, nội dung/tài liệu hợp lệ | `ContractRules.ValidateSubmission` | Thiếu Summary hoặc DocumentReference, Customer Inactive, ngày sai: không gửi được |
| CTR-03: Draft không sang Approved | `ContractRules.SubmittedStatus`, `CreateContractAsync`, `UpdateContractAsync`, `SubmitContractAsync` | POST/PUT có `status: "Approved"` trả 400; submit chỉ đặt PendingApproval |
| Gửi lặp | Kiểm tra trạng thái và concurrency token `Contract.Status` | Lần hai trả 409; không tạo bản ghi xử lý mới |

Phiên bản này mô phỏng việc gửi bằng **một lần đổi trạng thái Contract sang PendingApproval**. Chưa tạo Approval, hàng đợi, thông báo hay xử lý duyệt thật. Mỗi lần gửi hợp lệ chỉ có một trạng thái đích; lệnh gửi lặp bị từ chối.

## Minh chứng demo

Chạy migration mới trước khi khởi động API. Có thể chạy `python scripts/demo_week06.py` khi API đang lắng nghe ở cổng 5095; script tạo bộ dữ liệu mới ở mỗi lần chạy và in ID cần kiểm tra. Demo theo thứ tự: tạo Customer, tạo Contract Draft thiếu thông tin, gửi lỗi 400, đọc lại thấy Draft, PUT bổ sung, gửi thành công 200, gửi lặp 409, thử sửa bị 409, đọc lại thấy PendingApproval. Cuối cùng tạo bảng giá và một mục giá, đọc lại cả hai. Trong pgAdmin kiểm tra:

```sql
SELECT "Id", "ContractNo", "CustomerId", "Status", "Summary", "DocumentReference"
FROM "Contracts" ORDER BY "Id" DESC;
SELECT "Id", "Name", "EffectiveDate" FROM "PriceLists" ORDER BY "Id" DESC;
SELECT "PriceListId", "ServiceCode", "UnitPrice" FROM "PriceItems" ORDER BY "PriceListId" DESC;
```

Quay màn hình hoặc chụp ảnh chính các bước lỗi/thành công và đối chiếu PostgreSQL để nộp cùng repository. Không coi test dùng EF InMemory là kiểm chứng tích hợp PostgreSQL.
