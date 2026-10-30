# QTKD tuần 05 - thiết kế

## Mục đích

Tạo nền ASP.NET Core Web API và PostgreSQL cho bài Phase 2. Người học cần chạy được, đọc được dữ liệu thật và giải thích được đường đi của một request trong video tối đa 5 phút.

## Phạm vi

- Solution `QTKD.slnx`, project `QTKD.Api` trên .NET 10, tách Controller, Service, Repository và EF Core.
- Bốn entity Customer, Contract, PriceList, PriceItem với hai quan hệ và các trường tối thiểu trong PDF.
- Migration đầu, dữ liệu mẫu cố định: hai Customer, một Contract Draft, một PriceList với hai PriceItem.
- GET `/api/customers` và GET `/api/contracts/{id}` đọc PostgreSQL; Swagger trong Development.
- README hướng dẫn cấu hình bí mật, migrate, chạy, đối chiếu bằng SQL; hướng dẫn video.

## Lựa chọn thiết kế

Thêm solution riêng cạnh bài Course hiện có để giữ nguyên bài cũ. Dùng controller/service/repository giống cấu trúc bài TH4, giúp giải thích luồng và mở rộng ở tuần 06. Dùng EF Core `HasData` để migration có dữ liệu mẫu tái lập. Không chứa mật khẩu trong source.

Phase 1 chưa có trong workspace. Status dùng chuỗi `Active`, `Draft`; các thuộc tính tối thiểu theo PDF là giả định thực hành, cần đối chiếu với thiết kế Phase 1 trước tuần 06. Tuần 05 không triển khai thao tác ghi, submit, approval, test xUnit.

## Kiểm chứng

Build solution, xem SQL migration, áp dụng trên database riêng nếu có cấu hình kết nối, gọi hai API và so sánh bản ghi qua SQL. Không xóa hoặc thay đổi database Course.
