# Kết quả kiểm chứng QTKD tuần 06

Ngày 06/11/2026, trên database cục bộ `qtkd_week05`, đã áp dụng migration `20261106074745_Week06ContractRequirements` và chạy `python scripts/demo_week06.py` với API tại `http://localhost:5095`.

`dotnet build QTKD.slnx --verbosity quiet` hoàn thành với 0 warning, 0 error; `dotnet test QTKD.slnx --verbosity quiet` có 18/18 test pass.

| Bước | HTTP | Kết quả quan sát |
|---|---:|---|
| Tạo và đọc Customer | 201, 200 | ID 4 |
| Gửi `status: Approved` khi tạo Contract | 400 | Bị từ chối |
| Tạo Contract Draft thiếu nội dung/tài liệu | 201 | ID 3, Draft |
| Gửi khi thiếu điều kiện | 400 | `CTR-02`, đọc lại vẫn Draft |
| Bổ sung dữ liệu bằng PUT | 200 | Summary và DocumentReference có giá trị |
| Gửi hợp lệ | 200 | PendingApproval |
| Gửi lặp và thử sửa sau khi gửi | 409, 409 | Đọc lại vẫn PendingApproval |
| Tạo PriceList và PriceItem | 201, 201 | PriceList ID 3; đọc lại một item |

Truy vấn PostgreSQL trực tiếp sau demo trả:

```text
3|HD-DEMO-BAB04081|PendingApproval|Cung cap dich vu bao tri|DOC-BAB04081
3|SVC-BAB04081|1250000.00
20261106074745_Week06ContractRequirements
```

Bộ test xUnit dùng EF InMemory để kiểm tra logic, còn kết quả trên là luồng API chạy với PostgreSQL. Người nộp cần tự quay màn hình hoặc chụp ảnh luồng API và bảng dữ liệu trong pgAdmin theo mục 4.5 của đề.
