# Tóm tắt những thay đổi trong dịch thuật tự động

## Toàn cảnh

Tài liệu này tóm tắt tất cả các thay đổi được thực hiện cho dịch vụ phiên dịch tự động Dita, bao gồm việc phục hồi kiến trúc, tính năng mới, cải tiến khả năng ghi lưu và cải tiến bản địa.

## Thay đổi kiến trúc

### Name

Khối đá đã bị phân hủy thành bốn dịch vụ chuyên biệt phối hợp bởi một dàn nhạc nhẹ:

- **ReendTranendTervice** — Bộ dàn nhạc ống (máy chủ hợp lệ, đại biểu sân khấu, xử lý lỗi)
- **CountriesTranseService** — Quốc gia đồng bộ hóa (ngôn ngữ đích)
- **LocalizationTrancationService** — JSON từ điển đồng bộ hoá (đã thêm/tắt)
- **DocuchtsTrangService** — Đánh dấu tài liệu hướng dẫn dịch với khả năng theo dõi mức độ chặn
- **SignalRPublister** - Tiến trình thực tế thông qua tín hiệuR
- **Transotion Reervice** - Thử lại giai đoạn với bảo tồn vị trí

### Lợi ích

- **Sự phân chia các mối quan tâm**: Mỗi dịch vụ quản lý một vùng dịch thuật
- **Maintainability**: Smaller classes are easier to understand and test
- **Extensibility**: New translation targets can be added via interface implementation
- **Reliity**: Dịch vụ độc lập cung cấp sự cô lập tốt hơn về lỗi

## Tính năng mới

### Trình theo dõi dịch trực tiếp

**Loout**:

Một trang quảng cáo mới cung cấp tầm nhìn thời gian thực vào đường ống dịch thuật:

- Hiển thị mọi tín hiệu Name
- Kiểu thông điệp đã mã hóa màu (xanh dương = khởi chạy, xanh lá cây = đầy đủ, đỏ = chống khủng bố)
- Name
- Name

### Bộ giữ chỗ có tên

Hệ thống định vị bây giờ hỗ trợ tên người giữ chỗ () để cải tiến ngữ pháp trong các ngôn ngữ khác nhau:

```csharp
// Usage in code
var message = Localizer["WelcomeMessage", new Dictionary<string, string>
{
    ["userName"] = "John",
    ["count"] = "5"
}];
// Result: "Hello John, you have 5 new messages"
```

Tính năng:
- Các giá trị giữ chỗ được cung cấp lúc chạy hay được cất giữ vào
- Comment
- Tương thích với các vị trí có sẵn

### Bản dịch gia tăng

Các tập tin đánh dấu được dịch liên tục:

- **Per-language tiết kiệm**: Mỗi ngôn ngữ đích được lưu ngay sau khi dịch, giảm áp lực bộ nhớ
- **Block cấp theo dõi**: vết dịch trạng thái mỗi khối
- **Selective retry**: Only failed blocks are re-translated on the next run
- **Metadata bendta**: Dịch sau khi ứng dụng khởi động lại

### Comment

3 mức độ bền bỉ:

1. **HTTP retry** (LibreTranslateService): 5 lần cố gắng tái sử dụng lũy thừa (1s–5)
2. **Stage retry** (TransotionReervice): 3 nỗ lực thêm với 30s trễ
3. **Block retry** (DocuchtstTrantionService): Lỗi khi gõ lại các khối

### Báo cáo tín hiệu

Tiến triển thời gian thực báo cáo cho tất cả các hoạt động đường ống:

- Mỗi giai đoạn xuất bản sự kiện
- Tiến trình mô tả được công bố là sự kiện
- Sự kiện lỗi bao gồm văn cảnh chi tiết ( cưới, mã lỗi, thông điệp)
- Thêm số dãy

## Thay đổi cấu hình

### cấu hình ứng dụng.json

Không phá vỡ thay đổi. Cấu hình tồn tại tiếp tục hoạt động:

```json
{
  "AutomaticTranslationSettings": {
    "DefaultLanguage": "en",
    "IgnoredLanguages": ["auto", "detect"],
    "MarkdownRoots": ["/Docs"],
    "AutomaticRun": true,
    "CheckingPeriod": 30,
    "WaitingTime": "00:00:00"
  }
}
```

### Dịch vụ mới

Đã đăng ký vào:

- /
- `TranslationRetryService`
- /
- /
- /
- /

Tín hiệu Trung tâm R được lập bản đồ cho kết nối khách hàng.

## Thử ra

### Trạng thái thử ra

- **243/244 thử nghiệm vượt qua**(bị bỏ qua do truy cập tập tin đồng thời trong môi trường thử nghiệm)
- Thêm kết quả kiểm tra mới:
  - Bộ giữ chỗ Chức năng dịch vụ
  - Hậu phương Comment
  - JsonStringLocazer number indexlist

### Giới hạn đã biết

- thử ra bị bỏ qua khi chạy song song vì nhiều lần thử nghiệm chia sẻ cùng một tập tin. Nó trôi qua khi chạy trong sự cô lập.

## Cấu trúc tập tin mới

### Dịch vụ vào

- - Bộ dàn nhạc đường ống
- — Quốc gia dịch tên
- - Đồng bộ hoá từ điển JSON
- — Bản dịch đánh dấu
- - Tín hiệu Xuất thông điệp R
- - Thử lại logic với mặt nạ giữ chỗ
- - Giao diện Publisher
- - Giao diện dịch vụ quốc gia
- - Giao diện dịch vụ cục bộ
- - Giao diện dịch vụ tài liệu
- - Giao diện phim ảnh (lên)
- - Siêu dữ liệu cho mỗi tập tin

### Dịch vụ đã cập nhật

- - Thêm hỗ trợ giữ chỗ tên
- - Đã cập nhật cho tham số mới
- - Quản lý bộ giữ chỗ có tên
- - Giao diện giữ chỗ

### Trang Quản trị mới trong

- - Trang kiểm tra thời gian thực
- - Kiểu trang

### Tài liệu mới vào

- - Tài liệu hướng dẫn cập nhật
- — Hướng dẫn hệ thống giữ chỗ
- - Hướng dẫn sử dụng bảng gạch
- — Xem xét kiến trúc kỹ thuật

## Tương thích ngược

Mọi thay đổi được thêm vào:

- Mã định vị sẵn () hoạt động không thay đổi
- Định dạng vị trí () không đổi
- Định dạng từ điển JSON tồn tại không thay đổi
- Comment
- Tín hiệu Comment

## Đường chuyển

Không cần phải di cư. Giải pháp là nội bộ:

1. Già được bảo quản như một tài liệu tham khảo và sau đó được thay thế
2. Đăng ký DI đã được cập nhật để sử dụng giao diện mới
3. Tất cả những người tiêu dùng hiện có đều không thấy thay đổi

## Tăng cường hiệu suất

- ** Dùng bộ nhớ đã yêu cầu**: Tập tin đã lưu trên môi miệng thay vì giữ tất cả trong bộ nhớ
- **Faster tăng dần chạy**: Chỉ những khối Đánh dấu bị thay đổi/ hư hỏng được mở lại
- ** Cách nhìn tốt hơn**: Tiến trình thời gian thực giúp chẩn đoán giai đoạn chậm

## Tăng cường tương lai

Cải tiến đã lên kế hoạch:

1. **AI fine-tuining** - Bản đánh giá dịch sau máy cho cụm từ > 5 từ
2. ** Trình xác thực giọng nói** — Giới hạn trang mandmin cho người dùng có thẩm quyền
3. ** Trình biên tập từ điển** — Mạng UI để quản lý các phím định vị
4. ** Số thống kê quan trọng** — Biểu đồ cho thấy số lượng bản dịch và tỷ lệ sai theo thời gian
5. **Custom cú pháp giữ chỗ** — Hỗ trợ định dạng vị giữ chỗ khác

## Contact & mới

Đối với các câu hỏi hoặc vấn đề với dịch vụ dịch thuật, xin vui lòng tham khảo các tài liệu chi tiết trong mỗi thư mục của mỗi mô-đun hoặc liên lạc với nhóm phát triển.
