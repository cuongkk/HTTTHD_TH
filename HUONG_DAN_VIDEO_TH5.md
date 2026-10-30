# Gợi ý tự trình bày TH5 (tối đa 5 phút)

Kịch bản nói và thao tác đầy đủ: [KICH_BAN_VIDEO_TH5.md](KICH_BAN_VIDEO_TH5.md).

Đây là dàn ý để **tự nói bằng cách hiểu của mình**, không phải lời đọc thuộc. Chỉ quay những thao tác đã chạy trên máy mình và cho thấy tiếng nói rõ.

| Thời gian | Nội dung cần cho thấy và giải thích |
| --- | --- |
| 0:00–0:35 | Nêu mục tiêu tuần 05: API kết nối PostgreSQL và đọc dữ liệu thật. Mở `QTKD.slnx` và cấu trúc project. |
| 0:35–1:35 | Mở bốn entity và `QtkdDbContext`: giải thích `Contract.CustomerId` là khóa ngoại, `PriceItem` dùng khóa ghép, `HasData` thêm dữ liệu mẫu bằng migration. |
| 1:35–2:15 | Mở `Program.cs`: chuỗi kết nối lấy từ User Secrets, `UseNpgsql`, đăng ký Repository/Service bằng DI, Swagger ở Development. Không để lộ mật khẩu trên video. |
| 2:15–3:15 | Mở `CustomersController`, `QtkdReadService`, `QtkdRepository`: nói luồng GET `/api/customers` từ HTTP đến truy vấn `ToListAsync`, rồi biến thành DTO. Với Contract, `Include` lấy Customer và Id không thấy trả 404. |
| 3:15–4:25 | Chạy API, gọi hai endpoint trong Swagger. Mở pgAdmin/psql để so sánh Customer, Contract, PriceList/PriceItem với response. Chỉ ra Contract `HD-2026-018` ở Draft. |
| 4:25–4:50 | Tóm tắt migration và giới hạn tuần 05; tuần 06 sẽ phát triển luồng tạo và gửi hợp đồng trên cùng database. |

Trước khi quay: thực hành tự giải thích vì sao dữ liệu vẫn còn sau khi tắt API, vì sao `AsNoTracking` phù hợp cho GET, và vì sao không lưu mật khẩu trong source. Nếu chưa giải thích được đoạn nào, đọc lại và tự chạy thử đoạn đó.

## Đóng gói đúng quy định lớp

Sau khi **tự quay** `23120224_th5.mp4` có tiếng và kiểm tra thời lượng không quá 5 phút, tạo ZIP tên `23120224.zip` với đúng hai mục ở cấp gốc:

```text
23120224.zip
  source/          (gồm QTKD.slnx, QTKD.Api, migration, README; bỏ bin/ và obj/)
  23120224_th5.mp4
```

Theo PDF, tạo **một Git repository cá nhân dùng xuyên suốt tuần 05–07** và cung cấp link repository có source, migration, README. Chuẩn bị thêm ảnh/video ngắn đối chiếu API với PostgreSQL và sơ đồ thành phần (có sơ đồ trong `README_PHASE2.md`). Không push mật khẩu/User Secrets.

Upload ZIP lên Google Drive, cấp quyền truy cập, tự mở link ở cửa sổ ẩn danh để kiểm tra, rồi nộp file `.txt` chứa link trên Moodle theo thông báo lớp. Giữ quyền truy cập suốt học kỳ. Không dùng video TH3/TH4 thay cho TH5. Kiểm tra hướng dẫn Moodle để đặt **link repository** đúng nơi được yêu cầu; thông báo cấu trúc ZIP/TXT bạn gửi không nói rõ link này đặt trong TXT hay ô nộp riêng.
