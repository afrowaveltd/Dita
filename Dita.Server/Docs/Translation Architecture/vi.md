# Kiến trúc dịch thuật

Tài liệu này mô tả cấu trúc mô-đun của hệ thống phiên dịch tự động của Dita, đưa đến việc cải thiện khả năng duy trì, kiểm tra và khả năng phục hồi.

## Mục tiêu thiết kế

Giải quyết một số mối quan tâm với thiết kế khối đá nguyên thủy:

- **Sự phân chia các mối quan tâm**: Mỗi khu vực dịch thuật (thư mục, từ điển JSON, Markdown) được biệt lập.
- ** Kiên trì gia tăng**: Các tập tin được lưu theo từng ngôn ngữ ngay sau khi dịch, giảm khả năng sử dụng bộ nhớ và cung cấp kết quả sớm hơn.
- **Sự tương tác**: nhiều mức độ thử nghiệm lại xử lý lỗi tạm thời mà không chặn toàn bộ đường ống.
- **Obsvity**: Mỗi hoạt động quan trọng được báo cáo qua Tín hiệuR để theo dõi thời gian thực.
- **Extensibility**: New translation targets can be added by implementing a single interface.

## Phân cách dịch vụ

### Hậu phươngTractionService (người chụp ảnh)

**Có trách nhiệm**:
- Quản lý vòng đời ống (bắt đầu, hoàn thành, xử lý lỗi)
- Điều khiển sự nhất trí dựa trên Sêmaphore (trước khi chạy chồng chéo)
- Xác nhận máy phục vụ (thành thạo, có ngôn ngữ, cấu hình)
- Ủy nhiệm các dịch vụ phụ

** Không chứa**:
- Comment
- Tập tin I/O cho định dạng cụ thể
- Thử lại logic

### Các quốc gia phiêu lưu

**Có trách nhiệm**:
- Đọc từ thư mục
- Đồng bộ hoá tên quốc gia vào từ điển cục bộ mặc định
- Dịch tên quốc gia bị mất trên mỗi ngôn ngữ đích
- Lưu mỗi từ điển đích ngay sau khi dịch

**Key hành vi**:
- Nếu ngôn ngữ mặc định là tiếng Anh: tên nước được lưu là-is
- Nếu ngôn ngữ mặc định là khác: tên tiếng Anh được dịch sang ngôn ngữ mặc định trước
- Mỗi ngôn ngữ được xử lý độc lập với vòng lặp thử lại

### Công cụ & xoá

**Có trách nhiệm**:
- Phát hiện các phím bổ sung/ gỡ bỏ bằng cách so sánh từ điển mặc định hiện thời với ảnh chụp trước
- Dịch phím đã thêm vào mỗi ngôn ngữ đích
- Gỡ bỏ các phím đã xoá khỏi mỗi ngôn ngữ đích
- Lưu hình chụp cho lần so sánh tiếp theo

**Key hành vi**:
- Bản dịch thủ công luôn luôn ưu tiên (không bao giờ viết quá)
- Name
- Các phím đã gỡ bỏ bị xoá bỏ ngay lập tức
- Chụp ảnh chỉ sau khi mọi ngôn ngữ đã hoàn tất thành công

### Tài liệu mở rộng

**Có trách nhiệm**:
- Đi bộ lại cấu hình gốc vết mực đệ quy
- Phát hiện tập tin mã nguồn đã thay đổi sử dụng dad-256 hash
- Theo dõi trạng thái dịch trên khối
- Dịch các khối với mỗi lần thử lại
- Kiểm tra cấu trúc Đánh dấu sau khi dịch
- Lưu riêng mỗi tập tin ngôn ngữ đích

**Key hành vi**:
- Độ hạt ngăn cách: tiêu đề, đoạn, mục danh sách được dịch riêng
- Các dấu vết siêu dữ liệu mà các khối thành công/ thất bại trên mỗi ngôn ngữ
- Những khối bị lỗi được khôi phục lại trên lần chạy tiếp theo mà không chuyển đổi những khối thành công
- Xác thực cấu trúc đảm bảo tiêu điểm, danh sách, mã khối, vân vân. khớp nguồn

## Thử lại chiến lược

Hệ thống thực hiện lại ở ba cấp độ:

### Cấp 1 — HTTP (LibreTranslateService)

- Đến 5 lần thử lại số mũ (1, 2, 3, 4, 5)
- Xử lý thời hạn mạng, 5xxx lỗi, và lỗi tạm thời
- Comment

### Trình độ 2 — Giai đoạn (Sự giải thích Kinh Thánh)

- Tới 3 lần cố trì hoãn 30 giây
- Khởi động lại toàn bộ yêu cầu dịch sau khi quá trình khôi phục mức HTTP bị cạn kiệt
- Việc che giấu và phục hồi vị trí được áp dụng ở cấp độ này

### Cấp 3 — Khối (tài liệu đa chiều)

- Những khối đánh dấu bị lỗi được đánh dấu bằng siêu dữ liệu
- Tự động tái định cư trên đường ống tiếp theo chạy
- Name

## Name

### Bản dịch từ điển JSON

```
[Default Dictionary]
       ↓
[Compare with Snapshot]
       ↓
[Detect Added/Removed Keys]
       ↓
For each target language:
   ├─ Load existing dictionary
   ├─ Apply removals
   ├─ Translate additions (with retry)
   └─ Save dictionary immediately
       ↓
[Save new snapshot]
```

### Chọn

```
[Source File]
       ↓
[Compute Hash]
       ↓
[Compare with stored hash]
       ↓
[Load translation metadata]
       ↓
For each target language:
   ├─ Extract translatable blocks
   ├─ For each block:
   │   ├─ Check metadata (already translated?)
   │   ├─ Translate with retry
   │   └─ Mark success/failure in metadata
   ├─ Reconstruct Markdown
   ├─ Validate structure
   ├─ Save target file
   └─ Save metadata
```

### Bản dịch tên quốc gia

```
[countries.json]
       ↓
[Build set of country keys]
       ↓
[Update default dictionary]
       ↓
For each target language:
   ├─ Find missing country entries
   ├─ Translate each missing entry (with retry)
   └─ Save dictionary immediately
```

## Kiên trì

### Hình chụp

- **JSON**: được lưu trong tập tin bên cạnh từ điển mặc định (tên khác nhau bởi nhà cung cấp lưu trữ)
- **Purpost**: Bật đồng bộ tăng dần bằng cách theo dõi những gì đã có trong lần chạy trước

### Mở tập tin

- ** Markdown**: cạnh tập tin mã nguồn
- **Fallback**: nếu vị trí chính là chỉ đọc
- **Purpost**: phát hiện các thay đổi nguồn để tránh chuyển đổi không cần thiết

### Siêu dữ liệu dịch

- ** Markdown**:
- **Những đối thủ**:
  - Name
- Name
- Nhãn thời gian cập nhật cuối cùng
- **Purpost**: Bật lại một phần của chỉ khối bị lỗi

### Bộ giữ chỗ

- **File**:
- ** Các đối số**: Từ điển khóa cho cặp giá trị đặt tên
- **Purpost**: Cung cấp giá trị mặc định cho các vị trí có tên trên ứng dụng

## Tín hiệu R báo cáo

### Tính trừu tượng

giải quyết các dịch vụ phiên dịch từ chi tiết Tín hiệu Đỏ:

```csharp
public interface ISignalRPublisher
{
    Task PublishStageAsync<T>(Guid runId, ProcessStage stage, T data, ...);
    Task PublishMessageAsync(Guid runId, LocalizationMessageType type, ProcessStage stage, ...);
}
```

### Bảo đảm hàng loạt

- Comment
- Số dãy là duy nhất trên mỗi chạy thông qua
- Ứng dụng khách có thể phát hiện khoảng trống hay sắp xếp lại

### Sơ đồ căn cứ

```csharp
app.MapHub<LocalizationHub>("/hubs/localization");
```

## Điểm mở rộng

### Đang thêm mục tiêu phiên dịch mới

1. Tạo một giao diện mới với
2. Hoàn thiện giao diện với logic cụ thể miền
3. Name
4. Comment
5. Name

### Tự chọn chính sách thử lại

Ghi đè tham số người xây dựng:

```csharp
services.AddSingleton<TranslationRetryService>(
    sp => new TranslationRetryService(
        sp.GetRequiredService<ILibreTranslateService>(),
        sp.GetRequiredService<IPlaceholderService>(),
        sp.GetRequiredService<ILogger<TranslationRetryService>>(),
        stageMaxRetries: 5,        // More retries
        stageRetryDelaySeconds: 60  // Longer delays
    ));
```

### Quản lý vị trí riêng

Thao tác thay đổi cú pháp của người giữ chỗ hoặc lưu trữ:

```csharp
public class CustomPlaceholderService : IPlaceholderService
{
    // Use {{name}} syntax instead of {name}
    // Store in database instead of JSON file
    // Encrypt sensitive placeholder values
}
```

## Cấu hình

### cấu hình ứng dụng.json

```json
{
  "AutomaticTranslationSettings": {
    "DefaultLanguage": "en",
    "IgnoredLanguages": ["auto", "detect"],
    "MarkdownRoots": ["/Docs", "/Help"],
    "AutomaticRun": true,
    "CheckingPeriod": 30,
    "WaitingTime": "00:00:00",
    "RequestThrottleMs": 80,
    "RequestTimeoutSeconds": 10
  }
}
```

### Điều chỉnh thời gian chạy

Thiết lập
|---------|---------|--------|
80
10
3
30

## Chiến lược thử ra

### Thử đơn vị

Mỗi dịch vụ phụ có thể kiểm tra độc lập:

- Mock để mô phỏng thành công/failure
- Mock để xác minh báo cáo
- Dùng thư mục tạm thời cho tập tin I/O
- Kiểm tra hành vi tiết kiệm điện thoại

### Thử ra Hợp nhất

- Đường ống dẫn đầy đủ chạy với thực (local) tiến trình LibreTranslate
- Kiểm tra tín hiệu Name
- Kiểm tra khả năng ngăn chặn chạy song thời (semaphore)
- Kiểm tra cấu trúc Đánh dấu sau khi dịch

### Thử ra cuối đến cuối

- Khởi động dịch qua mục hay trình lên lịchName
- Kiểm tra mọi tập tin ngôn ngữ đích được tạo/ nâng cấp
- Kiểm tra siêu dữ liệu chứa trạng thái khối đúng
- Xác nhận giữ chỗ được bảo tồn qua các bản dịch

## Xem xét hiệu suất

- **Memory**: Per-language tiết kiệm ngăn chặn tất cả các từ điển trong bộ nhớ
- **Disk I/O**: Tập tin siêu dữ liệu thêm nhỏ trên đầu nhưng hiệu quả tăng dần
- **Network**: xử lý hệ thống chặt chẽ với throtling ngăn chặn quá tải LibreTranslate
- **CPU**: sch-256 hashing và regex chứng thực tương đương với dịch latency
- **SignalR**: tin nhắn nhẹ cân, không cần nén lại để báo cáo thông thường

## Name

Bản gốc bao gồm tất cả các lý luận trong một lớp học. Con đường di cư:

1. Name
2. Rút ra logic JSON
3. Name
4. Tín hiệu lấy ra Xuất bản R
5. Name
6. Đơn giản hóa dàn nhạc để chỉ phái đoàn

Mọi giao diện tồn tại () không thay đổi. Những người tiêu thụ đường ống không thấy sự thay đổi nào.
