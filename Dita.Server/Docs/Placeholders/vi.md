# Bộ giữ chỗ có tên

Dita supports **named placeholders** in localization strings, allowing dynamic values to be inserted at runtime while preserving correct grammar across languages.

## Cú pháp

Người giữ chỗ sử dụng cú pháp sắc cong trong từ điển JSON:

```json
{
  "WelcomeMessage": "Hello {userName}, you have {count} new messages",
  "DiskStatus": "Disk {diskName} is {status} with {usagePercent}% used"
}
```

Không giống như những người giữ chỗ vị trí (, ), tên người giữ chỗ là **language-aific** — dịch giả có thể sắp xếp lại họ để phù hợp với ngữ pháp của mục tiêu-laguated mà không vi phạm mã.

## Lưu trữ

Bộ giữ chỗ có tên có hai nguồn giá trị:

### 1. Giá trị thời gian chạy (dùng cho dữ liệu động)

Gửi thẳng các giá trị khi lấy chuỗi cục bộ:

```csharp
// In a Razor page or controller
@inject JsonStringLocalizer Localizer

var message = Localizer["WelcomeMessage", new Dictionary<string, string>
{
    ["userName"] = "John",
    ["count"] = "5"
}];
// Result: "Hello John, you have 5 new messages"
```

### 2. Các giá trị đã lưu (để cấu hình bán tĩnh)

Quản lý tập tin trong thư mục:

```json
{
  "WelcomeMessage": {
    "userName": "Guest",
    "count": "0"
  }
}
```

Giá trị đã lưu hành hành động như ** mặc định** và bị đè nén bởi các giá trị thời gian chạy.

## Tham chiếu ADI

### JsonStringLocalzer

```csharp
// Without placeholders (backward compatible)
LocalizedString text = localizer["SomeKey"];

// With positional formatting (backward compatible)
LocalizedString text = localizer["SomeKey", "arg1", "arg2"];

// With named placeholders (new)
LocalizedString text = localizer["SomeKey", new Dictionary<string, string>
{
    ["name"] = "value"
}];
```

### Bộ giữ dây IPaxService

```csharp
public interface IPlaceholderService
{
    // Get stored placeholders for a key
    Dictionary<string, string> GetPlaceholders(string key);
    
    // Set a stored placeholder value
    void SetPlaceholder(string key, string placeholderName, string value);
    
    // Remove all stored placeholders for a key
    void RemoveKey(string key);
    
    // Format a template with placeholders
    string Format(string template, Dictionary<string, string>? values = null);
    
    // Extract placeholder names from template
    string[] ExtractPlaceholders(string template);
    
    // Check if template contains placeholders
    bool HasPlaceholders(string template);
    
    // Prepare text for translation (mask placeholders)
    (string preparedText, Func<string, string> restore) PrepareForTranslation(string template);
    
    // Persist/load from disk
    Task SaveAsync();
    Task LoadAsync();
}
```

### Phương pháp mở rộng

Để tiện lợi khi làm việc với:

```csharp
public static class StringLocalizerExtensions
{
    public static LocalizedString WithPlaceholders(
        this IStringLocalizer localizer,
        string name,
        Dictionary<string, string> placeholders);
}
```

Sử dụng:
```csharp
var text = Localizer.WithPlaceholders("WelcomeMessage", new Dictionary<string, string>
{
    ["userName"] = Model.UserName
});
```

## Ứng xử dịch

Khi dịch vụ phiên dịch tự động gặp văn bản với tên người giữ chỗ:

1. **Trước khi dịch**: Những người giữ chỗ đeo mặt nạ với những biểu tượng an toàn () để ngăn không cho động cơ phiên dịch sửa đổi.
2. **Ding Dịch**: Động cơ phiên dịch chỉ xử lý văn bản có thể chuyển đổi.
3. **Sau khi dịch**: Tên người giữ chỗ gốc () được khôi phục đúng vị trí.

### Ví dụ

Nguồn:

Chuẩn bị để dịch:

Dịch sang tiếng Séc:

Kết quả cuối cùng:

Điều này đảm bảo rằng:
- Bộ giữ chỗ không bao giờ được dịch hay bị hỏng
- Ngữ pháp của mục tiêu có thể tự động sắp xếp văn bản xung quanh
- Cùng mẫu hoạt động đúng trong mọi ngôn ngữ

## Thực hành tốt nhất

1. ** Dùng tên mô tả**: tốt hơn hoặc
2. ** Giữ bộ giữ chỗ nhỏ nhất**: Quá nhiều người giữ chỗ làm cho việc dịch khó khăn hơn
3. **Document mong đợi loại**: Chú thích trong tập tin JSON giúp dịch thuật hiểu ngữ cảnh
4. **Prefer giá trị thời gian chạy**: Đối với dữ liệu thực sự năng động (tên người dùng, đếm, ngày), đi qua các giá trị trong lúc chạy
5. ** Dùng giá trị đã lưu cho các giá trị mặc định**: Để cấu hình mà hiếm khi thay đổi (tên mới, hỗ trợ email)
6. **Validates placeholds**: Dùng để xác minh tất cả các vị trí giữ chỗ mong đợi được cung cấp

## Hợp nhất với dịch tự động

Tự động quản lý việc bảo quản vị trí trong cuộc gọi của LibreTranslate. Không cần thêm cấu hình.

Cả hai đều sử dụng dịch vụ thử lại, do đó, tất cả các từ điển JSON dịch thuật một cách minh bạch hỗ trợ danh hiệu người giữ chỗ.

## Tương thích ngược

Mã tồn tại sử dụng các bộ giữ chỗ định vị hoặc không có bộ giữ chỗ nào tiếp tục hoạt động không thay đổi:

```csharp
// Still works exactly as before
var text = localizer["Hello"];
var formatted = localizer["Value is {0}", 42];
```

Người giữ chỗ tên là ATI thêm vào — nó không phá vỡ cách sử dụng hiện có.
