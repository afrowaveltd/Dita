# สถาปัตยกรรม การ แปล

เอกสารนี้อธิบายสถาปัตยกรรมของระบบการแปลแบบอัตโนมัติของดิตะ ถูกนํามาใช้เพื่อปรับปรุงความทนทาน ความทนทาน และความยืดหยุ่น.

## ออกแบบเป้าหมาย

ความ เป็น ห่วง หลาย อย่าง เกี่ยว กับ การ ออก แบบ แบบ แบบ หิน ใหญ่ แบบ ดั้งเดิม:

- **Separation of concerns**: Each translation domain (countries, JSON dictionaries, Markdown) is isolated.
- **Incremental persistence**: Files are saved per-language immediately after translation, reducing memory usage and providing earlier results.
- ** ความไม่สงบ **: การพยายามหลายระดับ รับมือกับความล้มเหลวชั่วคราว โดยไม่ปิดกั้นท่อส่งน้ํามันทั้งหมด.
- **Observability**: Every significant operation is reported via SignalR for real-time monitoring.
- **Extensibility**: New translation targets can be added by implementing a single interface.

## การย่อยของบริการ

### โปรแกรมเบื้องหลังTranslient Serview (orchrator)

**Responsibilities**:
- ระบบจัดการระบบย่อยแบบท่อ (เริ่ม, เสร็จสิ้น, การจัดการข้อผิดพลาด)
- ควบคุมความเหลื่อมล้ําของ Semaphore (การทับซ้อนกัน)
- การตรวจสอบความถูกต้องของแม่ข่าย (ค่าน้อย, ค่าภาษา, ค่าปรับแต่ง)
- การมอบอํานาจให้แก่องค์กรย่อย

**Does NOT contain**:
- ตรรกะการแปลภาษา
- แฟ้ม I/O สําหรับรูปแบบเฉพาะ
- ลองใหม่

### ผู้ ชํานัญ พิเศษ ใน ประเทศ

**Responsibilities**:
- อ่านจากไดเรกทอรี
- ปรับเทียบชื่อประเทศเป็นพจนานุกรมท้องถิ่นปริยาย
- แปลชื่อประเทศหายต่อภาษาเป้าหมาย
- บันทึกพจนานุกรมเป้าหมายแต่ละตัวทันทีหลังจากทําการแปล

**Key behaviors**:
- หากภาษาปริยายเป็นภาษาอังกฤษ: ชื่อประเทศที่ถูกจัดเก็บเป็น
- หากภาษาปริยายเป็นภาษาอื่น: ภาษาอังกฤษแปลเป็นภาษาปริยายก่อน
- แต่ละภาษาจะถูกประมวลผลด้วยตัวเองโดยใช้วงจรของตัวมันเอง

### ตัวแบ่งเขตพื้นที่

**Responsibilities**:
- ตรวจหากุญแจที่เพิ่ม/ ถูกลบออกไป โดยเปรียบเทียบพจนานุกรมปริยายปัจจุบันกับภาพก่อนหน้า
- แปลภาษาด่วน
- ลบปุ่มพิมพ์ออกจากภาษาเป้าหมายแต่ละภาษา
- จัดเก็บภาพที่จับได้เพื่อเปรียบเทียบครั้งต่อไป

**Key behaviors**:
- การแปลด้วยตนเองจะมีความสําคัญเสมอ (ไม่เคยเขียนเกินไป)
- ปุ่มที่ถูกเพิ่มจะถูกแปลและบันทึกอัตโนมัติ
- ปุ่มลบแล้วจะถูกลบออกทันที
- การจับภาพจะถูกบันทึกไว้หลังจากภาษาทั้งหมดเสร็จสมบูรณ์แล้ว

### โปรแกรมจัดการเอกสาร

**Responsibilities**:
- ปรับตําแหน่งรากของตัวอักษรให้เดิน
- ตรวจหาแฟ้มต้นฉบับที่มีการเปลี่ยนแปลงโดยใช้ข้อมูล has-256
- สถานะการแปลของแทร็กต่อบล็อก
- แปลบล็อกต่อบล็อกด้วยตัวพิมพ์ต่อบล็อก
- ตรวจความถูกต้องของโครงสร้างตัวอักษรหลังการแปล
- จัดเก็บแฟ้มภาษาเป้าหมายแต่ละตัวเป็นอิสระ

**Key behaviors**:
- ความกว้างของบล็อก: หัว, ย่อหน้า, รายการรายการถูกแปลแยก
- ข้อมูลกํากับภาพที่บล็อคทํางาน/ ไม่สําเร็จต่อภาษา
- บล็อกที่ล้มเหลวจะถูกเรียกทํางานครั้งต่อไปโดยไม่มีการแทนที่บล็อกที่ประสบความสําเร็จอีกครั้ง
- การตรวจสอบความถูกต้องของโครงสร้าง เพื่อให้แน่ใจว่ามีการนับหัว, รายชื่อ, บล็อกโค้ด, ฯลฯ ที่เข้าคู่กัน

## กลยุทธ์ของการลองใหม่

ระบบจะเปิดใช้งานอีกครั้งที่ 3 ระดับ:

### ระดับ 1 — HTTP (Libre Translate Service)

- 2s, 3s, 4s, 5s)
- จัดการเวลาหมดเวลาของเครือข่าย, ข้อผิดพลาด 5xx และความล้มเหลวชั่วคราว
- สร้างในการปรับแต่งไคลเอนต์ HTTP

### ระดับ 2 — ขั้น ตอน (การ ทดสอบ ความ เป็น กลาง)

- หน่วงเวลาถึง 3 ครั้ง
- การส่งคําสั่งการแปลภาษาทั้งหมดใหม่อีกครั้ง หลังจากการปรับปรุงใหม่แล้ว
- ที่เก็บหน้ากากและการบูรณะถูกนํามาใช้ในระดับนี้

### ระดับ 3 — บล็อก (ศูนย์ การ ศึกษา)

- บล็อกที่ถูกทําเครื่องหมายไว้ซึ่งล้มเหลว ถูกทําเครื่องหมายไว้ในข้อมูลกํากับภาพ
- ประมวลผลอัตโนมัติเมื่อประมวลผลทางท่อต่อไป
- บล็อกที่ประสบความสําเร็จ ไม่เคยถูกปรับใหม่

## การไหลของข้อมูล

### แปลภาษาพจนานุกรม Json

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

### แปลภาษาแบบตัวอักษร

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

### แปลชื่อประเทศ

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

## แก้ไขโครงการหลัก..

### จับภาพ

- **JSON**: Stored in a file next to the default dictionary (name varies by storage provider)
- **Purpose**: Enables incremental sync by tracking what was present in the previous run

### แฟ้มแฮช

- **Markdown**: `{sourceFile}.hash.json` next to the source file
- ** ย้อนกลับ **: หากใช้ตําแหน่งหลักเท่านั้นอ่านได้
- **Purpose**: Detects source changes to avoid unnecessary re-translation

### ข้อมูลกํากับภาพการแปลภาษา

- **Markdown**: `{sourceFile}.translation-meta.json`
- **Contents**:
  - ข้อมูลต้นฉบับ:
- บล็อคต่อระเบียง (อาร์เรย์ของบูเลนส์)
- การปรับปรุงเวลา
- **Purpose**: Enables partial re-translation of only failed blocks

### คลังหลัก

- **File**: `Locales/placeholders.json`
- **Contents**: Dictionary of keys to placeholder name-value pairs
- **Purpose**: Provides default values for named placeholders across the application

## รายงานการส่งสัญญาณ

### ผู้เผยแพร่นามธรรม

บริการแปลภาษา Decouples จาก ScientR เฉพาะ:

```csharp
public interface ISignalRPublisher
{
    Task PublishStageAsync<T>(Guid runId, ProcessStage stage, T data, ...);
    Task PublishMessageAsync(Guid runId, LocalizationMessageType type, ProcessStage stage, ...);
}
```

### ความต่อเนื่องรับประกัน

- จดหมายภายในการทํางานครั้งเดียว มีลําดับเดียว
- เพิ่มเลขลําดับ
- ลูกข่ายสามารถตรวจจับช่องว่างหรือลําดับใหม่ได้

### การทําแผนที่ UB

```csharp
app.MapHub<LocalizationHub>("/hubs/localization");
```

## จุดเพิ่มเติม

### เพิ่มเป้าหมายการแปลใหม่

1. สร้างส่วนติดต่อผู้ใช้ใหม่ด้วย
2. เติมส่วนติดต่อด้วยตรรกะของโดเมน
3. ล็อกอินในบรรจุ DI
4. แก้ไขโครงการหลัก..
5. เรียกจากรายการหลังจากจบขั้นตอนที่มีแล้ว

### กําหนดข้อกําหนดเอง

พารามิเตอร์ของผู้สร้างมากกว่า:

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

### ปรับแต่งการจัดการตําแหน่งเอง

การแทนที่ของการเปลี่ยนไวยากรณ์หรือการจัดเก็บ:

```csharp
public class CustomPlaceholderService : IPlaceholderService
{
    // Use {{name}} syntax instead of {name}
    // Store in database instead of JSON file
    // Encrypt sensitive placeholder values
}
```

## ปรับแต่ง

### appletts.json

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

### หมุน & 90

ตั้งค่า
|---------|---------|--------|
80
10
3
30

## กลยุทธ์การทดสอบ

### ทดสอบหน่วย

บริการย่อยแต่ละรายการสามารถทดสอบได้ด้วยตนเอง:

- ให้จําลองความสําเร็จ/ ความสําเร็จ
- ให้จําลองเพื่อตรวจสอบรายงาน
- ใช้ไดเรกทอรีชั่วคราวสําหรับแฟ้ม I/ O
- ตรวจสอบพฤติกรรมการประหยัดต่อลา

### ทดสอบการกระตุ้น

- ประมวลผลท่อส่งแบบเต็มโดยใช้ค่าจริง (local) Libre Translate
- Comment=รายการจดหมายName
- ป้องกันการประมวลผลของการทดสอบ (Secmaphore)
- ตรวจความถูกต้องของโครงสร้างตัวอักษรหลังการแปล

### ทดสอบสุดท้าย

- แปลภาษาผ่านทาง API หรือตัวจัดตาราง
- ตรวจสอบแฟ้มภาษาเป้าหมายทั้งหมด
- ตรวจสอบแฟ้มข้อมูลกํากับภาพที่มีสถานะบล็อคที่ถูกต้อง
- ยืนยันการเก็บรักษาสถานที่ไว้

## การ พิจารณา ผล งาน

- **Memory**: Per-language saving prevents holding all dictionaries in memory
- **Disk I/O**: Metadata files add small overhead but enable incremental work
- **Network**: Sequential processing with throttling prevents overwhelming LibreTranslate
- **CPU**: SHA-256 hashing and regex validation are fast relative to translation latency
- **SignalR**: Lightweight messages, no payload compression needed for typical reports

## การ อพยพ จาก การ ออก แบบ แบบ แบบ หิน ใหญ่

ต้น ฉบับ มี เหตุ ผล ทั้ง หมด ใน ชั้น เดียว. เส้นทางการอพยพ:

1. แยก ตรรกะ ของ ประเทศ ออก มา
2. คลาย ตรรกะ ของ เจ สัน
3. คลายตรรกะของสัญลักษณ์
4. คลายการแพร่ภาพ
5. คลายการพยายามตรรกะ
6. ปรับแบนเนอร์ให้ง่าย

ส่วนติดต่อที่มีอยู่ทั้งหมด () ยังไม่เปลี่ยนแปลง ผู้ บริโภค ของ ท่อ ส่ง น้ํา ไม่ เห็น การ เปลี่ยน แปลง ใด ๆ.
