# Bản dịch thời gian thực

Tài liệu này tồn tại như là một đầu vào thử nghiệm trực tiếp cho các đường ống dịch tự động. Bất kỳ thay đổi nào trong tập tin này gây ra việc chuyển đổi tất cả các tập tin ngôn ngữ đích trong tiến trình tiếp theo.

## Xem xét kiến trúc

Đường ống dịch thuật đã được xây dựng lại thành một cấu trúc mô-đun với bốn dịch vụ phụ chuyên biệt phối hợp bởi một dàn nhạc nhẹ:

- **ReaendTranendTervice** - Tổ chức chặt chẽ toàn bộ đường ống, quản lý việc xác nhận máy chủ, và các đại biểu làm việc dưới quyền.
- **CountriesTranseService** — đồng bộ hóa tên quốc gia từ sang từ điển per-language.
- **LocalizationTractionService** — Phát hiện các phím bổ sung/ gỡ bỏ trong từ điển JSON mặc định và dịch chúng sang ngôn ngữ đích.
- **DocuchtsTrangService** — Dịch các tập tin tài liệu đánh dấu bằng cách theo dõi hàng loạt và siêu dữ liệu.

Mỗi dịch vụ phụ hoạt động độc lập và báo cáo tiến triển qua Tín hiệu Đỏ trong thời gian thực.

## Những gì các dịch vụ làm

Dịch vụ này chạy trên lịch trình và thực hiện một đường ống 5 giai đoạn: hợp lệ hoá máy phục vụ, đồng bộ hoá quốc gia, đồng bộ hoá JSON, dịch thuật tập tin Markdown, và bền bỉ kết quả. Mỗi giai đoạn phát ra các sự kiện tiến triển thời gian thực qua Tín hiệu Đỏ để khách hàng kết nối có thể tiếp tục làm việc.

## Giai đoạn ống

### Giai đoạn 1 — Kiểm tra lại

Trước khi bất cứ công việc dịch thuật nào bắt đầu, dịch vụ xác nhận rằng tất cả các nguyên tắc trước khi điều chỉnh được thỏa mãn:

- Phần cấu hình phải có mặt và hợp lệ.
- Máy chủ LibreTranslate phải đáp ứng trong một hoạt động được chấp nhận.
- Có thể tìm thấy danh sách các ngôn ngữ có trong máy phục vụ dịch thuật.
- Ngôn ngữ mặc định phải có trong danh sách đó.
- Thiếu tập tin locale JSON cho bất kỳ ngôn ngữ được hỗ trợ nào được tạo tự động.

Nếu ngân phiếu thất bại, đường ống sẽ dừng ngay lập tức và một thông điệp sẽ được phát tán.

### Giai đoạn 2 — Dịch Countries

Tên quốc gia được giữ đồng bộ từ danh mục chỉ đọc () vào từ điển JSON cục bộ.

- Nếu ngôn ngữ mặc định là tiếng Anh, mỗi tên nước được cất giữ như không có bản dịch.
- Nếu ngôn ngữ mặc định là bất kỳ ngôn ngữ khác, tên quốc gia tiếng Anh được dịch lần đầu tiên trong ngôn ngữ đó, và kết quả sẽ trở thành mục nhập trong từ điển mặc định.
- Sau khi cập nhật từ điển mặc định, mỗi mục nhập quốc gia bị mất trong mỗi từ điển tiếng địa phương được dịch ra và lưu lại ** - ngay lập tức trên mỗi ngôn ngữ**.
- Các mục nhập đã được mã hóa đã được bảo quản mà không cần sửa đổi.
- Nếu một bản dịch không thành công, công việc dịch thuật sẽ kéo dài đến 3 lần với 30 giây trước khi dịch sang ngôn ngữ kế tiếp.

### Giai đoạn 3 — Dịch tập tin

Dịch vụ so sánh từ điển cục bộ mặc định hiện thời với ảnh chụp được cất giữ từ lần chạy trước:

- ** Các phím đóng gói** — những mục nhập hiện có trong mặc định hiện tại nhưng không có trong hình chụp — được dịch sang mỗi ngôn ngữ đích mà chưa có mục nhập hướng dẫn cho chìa khóa đó.
- ** Các phím chuyển động** — mục nhập có trong hình chụp nhưng không có trong mặc định hiện thời — bị xoá khỏi mọi từ điển ngôn ngữ đích.
- Bản dịch thủ công luôn đặt ưu tiên hàng đầu. Nếu từ điển đích đã chứa giá trị cho một phím, mục đó không thay đổi bất kể nguồn nói gì.
- ** Mỗi từ điển nhắm vào ngôn ngữ được lưu ngay sau khi bản dịch hoàn tất**, thay vì đợi mọi ngôn ngữ kết thúc.
- Nếu một bản dịch không được dịch cho một ngôn ngữ cụ thể, thì dịch vụ sẽ tự động lặp lại. Chỉ những lỗi dai dẳng (v. d., ngôn ngữ không được hỗ trợ) làm cho ngôn ngữ đó bị bỏ qua.
- Sau khi chạy, từ điển mặc định hiện thời được lưu dạng ảnh chụp mới cho lần so sánh tiếp theo.

Tất cả các từ điển luôn luôn được lưu trữ với các phím sắp xếp theo thứ tự chữ cái và được in trùng JSON để con người đọc được.

### Giai đoạn 4 — Dịch tập tin đánh dấu

Dịch vụ đi theo nguồn tài liệu đã cấu hình (mặc định:) và xử lý mỗi tập tin nguồn đệ quy:

1. Nội dung tập tin nguồn được đọc và một harch-256 hash được tính toán.
2. Một tập tin bên cạnh các mã nguồn trên địa chỉ dịch mỗi khối, cho phép **inal re- re-plation**s chỉ thất bại.
3. Hash từ lần chạy trước (giữ trong tập tin bên cạnh tập tin mã nguồn, hoặc trong vị trí lùi tạm thời) được so sánh với hash hiện thời.
4. Đối với mỗi ngôn ngữ đích, tập tin tương ứng cũng được kiểm tra để có sự toàn vẹn về cấu trúc.
5. Bất kỳ tập tin mục tiêu nào còn thiếu, có một hash lỗi thời, lỗi cấu trúc hợp lệ hóa, hoặc chứa các khối chưa được mã hóa được xếp hàng để chuyển đổi lại.
6. ** Mỗi ngôn ngữ đích được dịch và lưu độc lập** — nếu Séc thành công nhưng Pháp thất bại, tập tin Czech vẫn còn được viết vào đĩa.
7. Các tập tin được dịch thành công được xác nhận cho tính chất cấu trúc với mã nguồn (các tiêu đề ngang nhau, danh sách mục, hộp mã, chặnquot, liên kết, dấu hiệu táo bạo/talic, và thẻ HTML) trước khi được ghi vào đĩa.
8. Nếu mọi tập tin đích cho một nguồn thành công, nó sẽ được cất giữ bên cạnh nguồn. Nếu việc ghi bên cạnh nguồn bị lỗi (v. d. trong việc triển khai chỉ đọc), thì h sẽ trở lại thư mục tạm thời.
9. Nếu bản dịch đích nào bị lỗi, các vật liệu siêu dữ liệu đánh dấu những khối này không được thay thế nên chúng được chuyển lại vào lần chạy tiếp theo.

### Giai đoạn 5 — Những cơn bão

Một sự hợp nhất được thu thập và xuất bản. Nó bao gồm:

- UTC chạy bắt đầu và hoàn thành nhãn thời gian.
- Số lượng đếm của lưu tập tin locale JSON, lưu tập tin Markdown, lưu tập tin Hash, và fallback hah viết.
- Bất kỳ lỗi lưu trữ được thu thập trong khi chạy.
- Thống kê phiên dịch Per-language (số đếm được, đếm chậm, đếm lỗi).

## Phong bì thông điệp tín hiệu

Mỗi sự kiện tiến hành được trình bày như một trong những lĩnh vực sau:

Tìm kiếm
|-------|------|-------------|
Bộ nhận diện tương quan cho việc chạy đường ống
Bắt đầu lúc 1
Comment
Name
Name
Comment
Tóm tắt khả năng đọc của con người
Trọng tải sân khấu đặc trưng (vật thể hay vô giá trị)

### Kiểu thông điệp

Giá trị
|-------|------|---------|
0
1
2
3
4
5
6

### Giai đoạn ống

Giá trị
|-------|------|-------------|
0
1
2
3
4
5

### Name

```text
StageStarted  / CheckServers
Progress / CheckServers — Server latency: 42ms
StageCompleted / CheckServers
StageStarted  / TranslateCountries
Progress / TranslateCountries — Found 195 country names
Progress / TranslateCountries — Starting translations for 'cs'...
Progress / TranslateCountries — Saved dictionary for 'cs' (198 entries)
StageCompleted / TranslateCountries
StageStarted  / TranslateJsonFiles
Progress / TranslateJsonFiles — Detected 3 added and 0 removed keys
Progress / TranslateJsonFiles — Starting JSON translations for 'cs'...
Progress / TranslateJsonFiles — Saved dictionary for 'cs' (201 entries)
StageCompleted / TranslateJsonFiles
StageStarted  / TranslateMarkdownFiles
Progress / TranslateMarkdownFiles — Scanning 2 source files in '/Docs'
Progress / TranslateMarkdownFiles — File 'en.md' has 12 translatable blocks
Progress / TranslateMarkdownFiles — Translating 'en.md' to 'cs'...
Progress / TranslateMarkdownFiles — Saved 'cs' translation for 'en.md' (12/12 blocks)
StageCompleted / TranslateMarkdownFiles
StageCompleted / StoringResults
PipelineCompleted / StoringResults
```

Nếu bất kỳ giai đoạn nào thất bại, những giai đoạn còn lại sẽ bị bỏ qua, một thông điệp được phát ra, và cuối cùng một thông điệp đóng lại cuộc chạy.

## Comment

Đường ống này thực hiện hai mức độ bền bỉ:

### Thử lại cấp sân khấu

- Nếu một yêu cầu dịch không thành công sau khi chương trình phục hồi nội bộ của LibreTranslate, các màn trình diễn lên đến 3 vòng lặp cấp độ sân khấu với 30 giây chậm trễ.
- Mặt nạ giữ chỗ: Các bộ giữ chỗ có tên () trong văn bản được thay thế tạm thời bằng những dấu hiệu an toàn () trước khi dịch và phục hồi sau đó, bảo đảm ngữ pháp chính xác trong ngôn ngữ đích.

### Comment

- Trước khi dịch sang ngôn ngữ đích, dịch vụ xác nhận ngôn ngữ này được máy phục vụ dịch thuật hỗ trợ.
- Các ngôn ngữ không được hỗ trợ bị bỏ qua bằng một lời cảnh báo, ngăn chặn những nỗ lực thất bại lặp đi lặp lại.

### Thử lại cấp khối đánh dấu

- Các bản dịch đánh dấu được thực hiện từ chặn lại (đầu, đoạn, mục danh sách).
- Nếu một khối bị lỗi dịch, nó được đánh dấu trong tập tin siêu dữ liệu và được lưu lại trên đường ống tiếp theo.
- Dấu vết dịch vụ trên trang web, trạng thái một khối trong tập tin bên cạnh mỗi tập tin đánh dấu nguồn.

## Mã lỗi

Các lỗi được báo cáo bằng cách hợp nhất nhóm lại thành nhóm:

Phạm vi
|-------|----------|
1000–199
2000–299
3000–399
4000–499
5000–599

Mỗi lỗi trong báo cáo chứa mã nhận diện nguồn (tiếng địa phương, đường dẫn tập tin hoặc tên giai đoạn), mã lỗi, và một thông điệp có thể đọc được của con người.

## Bảng dịch trực tiếp

Dự án máy chủ bao gồm một trang quảng cáo ở đó kết nối với trung tâm tín hiệu và hiển thị tất cả các sự kiện đường ống trong thời gian thực.

- Hiển thị trạng thái kết nối, đếm thông điệp, và một bảng chọn lọc của mọi sự kiện.
- Hàng có màu: xanh cho khởi động sân khấu, xanh lá cây cho hoàn thành, đỏ cho lỗi.
- Hỗ trợ dọn sạch dữ liệu và xuất mọi tin nhắn cho JSON.
- Tự động kết nối với lại cấp số nhân nếu kết nối rơi.

## Nguyên tắc thiết kế

- ** Sự khác thường**: mỗi sự quan tâm của bản dịch được tách biệt trong dịch vụ riêng của nó cho khả năng duy trì và kiểm tra.
- ** Kiên trì gia tăng**: Nhật ký và tập tin đánh dấu được lưu sau khi dịch, giảm áp lực trí nhớ và đưa ra phản hồi trước đó.
- ** Sự chấp nhận**: mức độ thử lại nhiều (HTTP, giai đoạn, khối) bảo đảm thất bại tạm thời không chặn đường ống.
- **Stete Theo dõi**: per- file siêu dữ liệu () và tập tin hash hiệu lực việc tăng dần chính xác trong lần chạy tiếp theo.
- **Real-time visibility**: Every significant operation is reported via SignalR for monitoring and debugging.
- **Bản dịch Đàn ông luôn luôn có ưu tiên hơn các bản bổ sung tự động.**
