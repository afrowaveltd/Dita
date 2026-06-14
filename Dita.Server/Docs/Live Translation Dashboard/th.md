# แดชบอร์ด

The Life Dashboard is a admin page ซึ่งทําให้การมองเห็นภาพตามเวลาจริง ในท่อแปลอัตโนมัติ มันเชื่อมต่อกับศูนย์ส่งสัญญาณ และแสดงเหตุการณ์ท่อส่งแก๊สทั้งหมด.

## ที่อยู่ URL

`/Admin/LiveTranslation`

> Note: Authentication and authorization are not yet implemented. Future versions will restrict this page to admin users only.

## คุณสมบัติต่าง ๆ

### กระแสเหตุการณ์แบบเรียลไทม์

เหตุการณ์ที่เกิดขึ้นจากท่อส่งข้อความทั้งหมด จะถูกแสดงในตารางการถ่ายทอดสด:

- **Sequence number** — Monotonic counter within each pipeline run
- **Timestamp** — Local time when the event was received
- **Run ID** — Shortened GUID for correlation
- **Stage** — Pipeline stage badge (CheckServers, TranslateCountries, etc.)
- **Type** — Message type badge (StageStarted, Progress, StageCompleted, etc.)
- **Message** — Human-readable description
- **Details** — Full JSON payload of the event data

### การเข้ารหัสสี

สี
|-------|---------|
น้ําเงิน ()
สีเขียว ()
แดง ()
ขาว (ค่าปริยาย)

### สถานะการเชื่อมต่อ

ป้ายสถานะที่แสดงด้านบน:
- ** ทําการเชื่อมต่อ **
- **Connected** — Receiving events normally
- ** เชื่อมต่อกัน ** การเชื่อมต่อหายไป พยายามเชื่อมต่ออีกครั้ง
- ** การเชื่อมต่อ ** — การเชื่อมต่อยุติ

การเชื่อมต่อนี้ใช้การเชื่อมต่ออัตโนมัติ กับเอกซ์โปเนนเชียลแบ็คออฟ: 0, 2s, 5s, 10s, 30s.

### ควบคุม

- **Clear Feed** — Removes all displayed messages and resets the counter
- **Export JSON** — Downloads all received messages as a JSON file for analysis
- **Message counter** — Shows total number of events received in this session

## ศูนย์สัญญาณ

มุมโค้งเชื่อมต่อกับ:

```javascript
const connection = new signalr.HubConnectionBuilder()
    .withUrl("/hubs/localization")
    .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
    .build();
```

### สัญญาจดหมาย

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

### ชนิดเหตุการณ์

หน้าปัดจับทุกค่า:

ชนิด
|------|---------|
ตราสีน้ําเงิน
ตราสีเขียว
ตราแดง
ตราสีเขียว
ตราแดง
ป้ายข้อมูล
ป้ายแจ้งเตือน

## การใช้เทคนิค

### โปรแกรมเบื้องหลัง

- **LocalizationHub** (`/hubs/localization`) — SignalR hub that broadcasts messages to all connected clients
- **ISignalRPublisher** — Abstraction over the hub for use in translation services
- **SignalRPublisher** — Default implementation that increments a monotonic sequence and broadcasts

### ฟร้อนท์เอนด์

- พิมพ์ HTML/ JS ด้วยบูตสแทร็ค 5 สไตลิ่ง
- ใช้ไลบรารีไคลเอนต์ SOCKSR ของไมโครซอฟท์ (ดาวน์โหลดมาจาก CDN)
- ไม่มีความต้องการการแสดงผลของแม่ข่ายสําหรับแหล่งป้อนเหตุการณ์

### โครงสร้างหน้ากระดาษ

```
Dita.Server/Pages/Admin/
├── LiveTranslation.cshtml      — Razor page markup + JavaScript
└── LiveTranslation.cshtml.cs     — Page model (empty, data comes via SignalR)
```

## การใช้งานระหว่างการพัฒนา

1. เริ่ม ดิตะ โปรแกรมแม่ข่าย
2. นํามายัง
3. กระตุ้นให้ทําการแปลทํางาน (ทั้งรอตัวจัดตาราง หรือเรียกตัวแปล API)
4. จง เฝ้า ดู เหตุ การณ์ ต่าง ๆ ปรากฏ ใน เวลา อัน ควร
5. ใช้ปุ่มส่งออกเพื่อจับร่องเต็มรูปแบบสําหรับการดีบั๊ก

## เพิ่มในอนาคต

วางแผนการปรับปรุงหน้าปัด:

- ** การอนุมาน ** — จํากัดการเข้าถึงผู้ใช้ที่มีบทบาท
- **Filtering** — Filter events by stage, type, or run ID
- **Historical runs** — View completed runs from a database or log file
- **Statistics** — Charts showing translation counts, error rates, and latency over time
- ** จุดระเบิดแบบผู้ชาย ** — ปุ่มที่จะเริ่มใช้ท่อส่งน้ํามันแบบเฉพาะ
- **Configuration** — Edit `AutomaticTranslationSettings` directly from the dashboard
- **Language management** — View and edit supported languages
- **Dictionary preview** — Browse and search localization dictionaries

## การยิงปัญหา

### แดชบอร์ดแสดง "เชื่อมต่อแล้ว"

1. ตรวจสอบเซิร์ฟเวอร์ที่กําลังทํางานอยู่ และเข้าถึงได้
2. ตรวจสอบคอนโซลของเบราว์เซอร์สําหรับ CordS หรือข้อผิดพลาดของเครือข่าย
3. ยืนยันอยู่ใน
4. เพื่อให้แน่ใจว่าไม่มีไฟร์วอลล์ที่ถูกปิดกั้นจากการเชื่อมต่อเว็บของซ็อกเก็ต

### เหตุการณ์ที่ยังไม่ปรากฏ

1. กาเลือกที่ตําแหน่ง URL SOCKSR ที่ตรงกันระหว่างเซิร์ฟเวอร์ () กับไคลเอนต์ ()
2. ตรวจสอบตัวจัดตาราง
3. ดูที่ปูมบันทึกของแม่ข่ายสําหรับระบบส่งข้อมูลผิดพลาด
4. Checkbar Network text

### จดหมายไม่มีลําดับ

สนามรับประกันการสั่งซื้อในครั้งเดียว หาก มี การ ส่ง ข้อ ความ ตาม ลําดับ อาจ บ่ง ชี้ ว่า:
- มีหลายท่อที่ทับกัน (ไม่ควรเกิดขึ้นเนื่องจากล็อค Secmaphore)
- ปัญหาการแสดงผลของเบราว์เซอร์ (พยายามทําให้หน้าดูสดชื่น)
