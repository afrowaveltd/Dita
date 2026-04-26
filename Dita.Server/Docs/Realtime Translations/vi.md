# Bản dịch thời gian thực

Tài liệu này tồn tại như là một đầu vào thử nghiệm trực tiếp cho các đường ống dịch tự động.

## Những gì các dịch vụ làm

Dịch vụ này chạy theo lịch trình và xác nhận trình phục vụ dịch thuật, cấu hình và các ngôn ngữ sẵn có trước khi công việc dịch thuật bắt đầu.

Sau bước hợp lệ hóa, nó đồng bộ hoá tên đất nước từ danh mục các nước chỉ đọc vào các từ điển JSON định vị. Nếu ngôn ngữ mặc định là tiếng Anh, mục nhập quốc gia được cất giữ như phím ngang giá trị. Nếu ngôn ngữ mặc định khác, tên nước Anh được dịch lần đầu tiên sang ngôn ngữ mặc định, và chỉ sau đó được cất giữ như phím bằng giá trị trong từ điển mặc định.

Tiếp theo, dịch vụ so sánh từ điển định vị hiện thời với ảnh chụp được lưu từ lần chạy trước. Những mục nhập mới được dịch sang ngôn ngữ đích chỉ khi phím không tồn tại, vì thế các bản dịch bằng tay giữ ưu tiên. Mục nhập đã gỡ bỏ bị xoá khỏi mọi từ điển đích để giữ nguyên cả bộ đều đặn.

Cuối cùng, dịch vụ quét các tài liệu đã cấu hình rễ của cây Markdown. Mỗi thư mục chủ đề cần chứa một tập tin mã nguồn được đặt tên theo ngôn ngữ mặc định, như en.md. Dịch vụ đã ký tự tập tin mã nguồn đó, phát hiện thay đổi, dịch sự mất tích hay lỗi thời tập tin Đánh dấu, và lưu trữ trang web hiện thời bên cạnh tập tin nguồn. Nếu không thể ghi hash bên cạnh tập tin mã nguồn, nó sẽ được lưu trữ tạm thời.

## Báo cáo dịch vụ thế nào

Hậu phương phát ra thông điệp tín hiệu chung thông qua trung tâm định vị bằng một phong bì thông điệp. Mỗi tin nhắn có một kiểu thông điệp, giai đoạn quá trình hiện tại, một nhãn thời gian UTC, một bản tóm tắt văn bản, và tải trọng riêng biệt.

Những giai đoạn hiện tại là:

- máy phục vụ check
- Dịch Countries
- Dịch tập tin Json
- Dịch tập tin đánh dấu xuống
- Comment

Dòng chảy thông điệp điển hình được khởi động, giai đoạn hoàn tất và các đường ống đã hoàn tất. Nếu giai đoạn thất bại, thông điệp được đánh dấu là lỗi và bao gồm thông tin có cấu trúc lỗi với mã lỗi thống nhất.

## Nguyên tắc thiết kế

Các bản dịch được xử lý một cách thường xuyên để tránh quá tải máy chủ LibreTranslate.

Bản địa hóa từ điển JSON luôn luôn được lưu trữ với các khóa sắp xếp theo thứ tự chữ cái và định dạng JSON để bảo trì dễ dàng hơn.

Ảnh chụp từ điển mặc định trước được lưu lại một cách kiên trì để việc khởi chạy lại ứng dụng không mất khả năng theo dõi thay đổi.

**Bản dịch Đàn ông luôn luôn có ưu tiên hơn các bản bổ sung tự động.**
