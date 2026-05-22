# Bảng dịch trực tiếp

Bảng dịch thuật trực tiếp là một trang quảng cáo cung cấp tầm nhìn thời gian thực vào đường ống dịch thuật tự động. Nó kết nối với trung tâm tín hiệu và hiển thị tất cả các sự kiện về đường ống.

## URL

`/Admin/LiveTranslation`

> Note: Authentication and authorization are not yet implemented. Future versions will restrict this page to admin users only.

## Tính năng

### Name

Tất cả các sự kiện tín hiệu từ đường ống dẫn dịch được hiển thị trên một bảng trực tiếp:

- **Sequence number** — Monotonic rep in every lead running
- **Thời gian đóng dấu** — Thời gian địa phương khi sự kiện được tổ chức
- ** run ID** — Giao diện người dùng đồ hoạ ngắn cho sự tương quan
- **Stage** — Biểu tượng sân khấu Đường ống (CeckServers, Dịch Countries, etc.)
- **Type** — Comment
- **Message** - Mô tả dễ đọc của con người
- ** details** — Toàn bộ dữ liệu về sự kiện

### Mã màu

Màu
|-------|---------|
Xanh dương ()
Xanh lá cây ()
Đỏ ()
Trắng (mặc định)

### Trạng thái kết nối

Một biểu ngữ trạng thái ở các chương trình đầu:
- ** Kết nối** — Thiết lập kết nối tín hiệuR
- ** Kết nối** — Nhận sự kiện bình thường
- ** Kết nối** — Kết nối bị mất, cố gắng tái kết nối
- **Disconnected** — Kết nối đóng lại

Kết nối sử dụng tự động tái kết nối với hàm mũ: 0s, 2, 5, 10, 30.

### Điều khiển

- ** Hiển nhiên nạp** — Gỡ bỏ tất cả các tin nhắn hiển thị và đặt lại quầy
- **Export JSON** — Tải về tất cả các tin nhắn đã nhận như một tập tin JSON để phân tích
- **Message đếm** — Hiển thị tổng số sự kiện nhận được trong phiên chạy này

## Trung tâm tín hiệu

Bảng điều khiển kết nối với:

```javascript
const connection = new signalr.HubConnectionBuilder()
    .withUrl("/hubs/localization")
    .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
    .build();
```

### Hợp đồng thông điệp

```typescript
interface LocalizationHubMessage {
    runId: string;        // Guid
    sequence: number;     // long
    type: LocalizationMessageType;
    stage: ProcessStage;
    timestampUtc: string; // ISO 8601
    isError: boolean;
    message: string;
    data: object | null;
}
```

### Kiểu sự kiện

Bảng điều khiển mọi giá trị:

Kiểu
|------|---------|
Sắc xanh
Sắc xanh lá cây
Đỏ
Sắc xanh lá cây
Đỏ
Comment
Cảnh báo

## Thực hiện kỹ thuật

### Hậu phương

- **LocalizationHub** () — Trung tâm tín hiệuR phát tin nhắn cho tất cả các khách hàng kết nối
- **SignalRPublister** — Trừu tượng trên trung tâm dịch thuật
- **SignalRPublister** — Mặc định thực hiện mà tăng trình đơn âm và phát thanh

### Giao diện

- Name
- Dùng thư viện trình khách JavaScript của Microsoft
- Không cần thiết vẽ cạnh máy phục vụ để nạp dữ liệu sự kiện

### Cấu trúc trang

```
Dita.Server/Pages/Admin/
├── LiveTranslation.cshtml      — Razor page markup + JavaScript
└── LiveTranslation.cshtml.cs     — Page model (empty, data comes via SignalR)
```

## Sử dụng trong quá trình phát triển

1. Khởi động Dita. Ứng dụng máy phục vụ
2. Chuyển tới
3. Gây ra việc chạy dịch (hoặc đợi bộ lập lịch hoặc gọi hệ thống API)
4. Comment
5. Dùng nút Xuất để ghi lại đầy đủ vết để gỡ lỗi

## Tăng cường tương lai

Cải tiến đã lên kế hoạch cho bảng điều khiển:

- **Authentication** — Giới hạn truy cập người dùng với vai trò
- ** Bộ lọc** — Bộ lọc sự kiện theo giai đoạn, kiểu, hoặc chạy ID
- ** Tài liệu lịch sử chạy** — Xem hoàn tất chạy từ cơ sở dữ liệu hoặc tập tin ghi lưu
- **Statisms** — Biểu đồ cho thấy số lượng bản dịch, tỷ lệ lỗi và độ trễ theo thời gian
- **Sự kích hoạt nhân cách** — Nút để bắt đầu các giai đoạn đường ống cụ thể
- ** Cấu hình** — Sửa trực tiếp từ bảng điều khiển
- ** Trình quản lý tài chính** — Xem và chỉnh sửa ngôn ngữ được hỗ trợ
- ** Xem thử từ điển** — Duyệt và tìm kiếm từ điển định vị

## Name

### Bảng gạch hiển thị "Fached to link"

1. Kiểm tra máy phục vụ đang chạy và truy cập
2. Kiểm tra bàn giao tiếp duyệt cho CORS hay lỗi mạng
3. Xác nhận có mặt tại
4. Bảo đảm không có tường lửa nào chặn kết nối WebSocket

### Sự kiện không xuất hiện

1. Kiểm tra xem địa chỉ URL trung tâm tín hiệu R tương ứng giữa máy phục vụ () và ứng dụng khách ()
2. Kiểm tra bộ lập lịch đã bật
3. Xem nhật ký máy phục vụ cho lỗi ống dẫn dịch
4. Kiểm tra thẻ mạng của trình duyệt tìm tin nhắn cắm mạng

### Comment

Thực địa đảm bảo sẽ ra lệnh trong vòng một lần. Nếu không có thông điệp, nó có thể chỉ ra:
- Nhiều đường ống chạy chồng chéo (không nên xảy ra do khóa semaphore)
- Vấn đề vẽ bộ duyệt ( thử làm tươi lại trang)
