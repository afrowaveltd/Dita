# สรุป การ เปลี่ยน แปลง งาน แปล โดย อัตโนมัติ

## ภาพรวม

เอกสาร ฉบับ นี้ สรุป การ เปลี่ยน แปลง ทุก อย่าง ที่ เกิด ขึ้น กับ งาน แปล แบบ อัตโนมัติ ของ ดิ ทา รวม ทั้ง การ ปรับ ปรุง สถาปัตยกรรม, ลักษณะ ใหม่, การ ปรับ ปรุง แก้ไข ความ เป็น ไป ได้, และ การ ปรับ ปรุง ระบบ ท้อง ถิ่น.

## สถาปัตยกรรมเปลี่ยนแปลง

### โปรแกรมเบื้องหลังที่สนับสนุน

หินปูนได้รับการย่อยสลายเป็นสี่บริการพิเศษ ประสานงานโดยวงออร์เคสตราเบา:

- **BackendTranslationService** — Pipeline orchestrator (server validation, stage delegation, error handling)
- **CountriesTranslationService** — Country name synchronization (English → target language)
- **LocalizationTranslationService** — JSON dictionary synchronization (added/removed keys)
- **DocumentsTranslationService** — Markdown documentation translation with block-level tracking
- **SignalRPublisher** — Real-time progress reporting via SignalR
- **TranslationRetryService** — Stage-level retry with placeholder preservation

### ผล ประโยชน์

- **Separation of concerns**: Each service handles a single translation domain
- **Maintainability**: Smaller classes are easier to understand and test
- **Extensibility**: New translation targets can be added via interface implementation
- **Reliability**: Independent services provide better fault isolation

## คุณสมบัติใหม่

### โปรแกรมติดตามการแปลภาษาสด

**Location**: `/Admin/LiveTranslation`

หน้าโฆษณาใหม่ซึ่งให้มุมมองตามเวลาจริง ในท่อแปล:

- แสดงเหตุการณ์ต่าง ๆ ของ ScienterR ขณะเกิด
- สี- เข้ารหัสข้อความประเภท (Ctrl= started, สีเขียว = เสร็จสมบูรณ์, สีแดง = profile)
- แสดงสถานะการเชื่อมต่อด้วยการเชื่อมต่ออัตโนมัติ
- ส่งข้อความไปยัง Json

### ผู้ถือตําแหน่งที่กําหนดชื่อ

ปัจจุบัน ระบบ การ ทํา ให้ ภาษา ท้อง ถิ่น มี ชื่อ เสียง ใน เรื่อง การ ปรับ ปรุง ไวยากรณ์ ภาษา ต่าง ๆ:

```csharp
// Usage in code
var message = Localizer["WelcomeMessage", new Dictionary<string, string>
{
    ["userName"] = "John",
    ["count"] = "5"
}];
// Result: "Hello John, you have 5 new messages"
```

คุณสมบัติ:
- ที่เก็บค่าของเจ้าภาพต่าง ๆ ที่กําหนดให้ในเวลาทํางาน หรือเก็บไว้ใน
- การซ่อน/ การแก้ไขอัตโนมัติระหว่างการแปลภาษาเพื่อป้องกันการทุจริต
- เข้ากันได้กับผู้ถือตําแหน่งเดิม

### การ แปล ที่ สําคัญ

แฟ้มจํานวนตัวอักษรจะถูกแปลเป็นเพิ่ม:

- **Per-language saving**: Each target language is saved immediately after translation, reducing memory pressure
- ** การติดตามระดับ block **: สถานะการแปลเพลงต่อบล็อก
- **Selective retry**: Only failed blocks are re-translated on the next run
- **Metadata persistence**: Translation state survives application restarts

### แก้ไขค่าตรรกะเพิ่มเติม

ความยืดหยุ่น 3 ระดับ:

1. **HTTP retry** (LibreTranslateService): 5 attempts with exponential backoff (1s–5s)
2. ** ทดสอบอีกครั้ง ** (สัญญา): 3 ความพยายามเพิ่มเติมด้วยเวลา 30 วินาที
3. **Block retry** (DocumentsTranslationService): Failed Markdown blocks retried on next run

### รายงานการส่งสัญญาณ

รายงานความคืบหน้าของเวลาจริง สําหรับการดําเนินงานท่อส่งน้ํามันทั้งหมด:

- เหตุการณ์ที่ตีพิมพ์ทุกขั้นตอน
- ความคืบหน้าต่อภาษาที่ถูกตีพิมพ์เป็นเหตุการณ์
- เกิดข้อผิดพลาดขึ้น รวมถึงรายละเอียดในบริบทต่าง ๆ (ทรัพยากร, รหัสข้อผิดพลาด, ข้อความ)
- เพิ่มเลขลําดับ

## การปรับแต่งเปลี่ยนแปลง

### appletts.json

ไม่ทําลายการเปลี่ยนแปลง การปรับแต่งที่มีอยู่ยังคงใช้ได้:

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

### บริการใหม่

ลงทะเบียนไว้ใน:

- /
- `TranslationRetryService`
- /
- /
- /
- /

ศูนย์ส่งสัญญาณ ถูกโยงหาการเชื่อมต่อของลูกค้า.

## ทดสอบ

### สถานะการทดสอบ

- **243/244 ทดสอบผ่าน ** (1ข้ามเนื่องจากเข้าถึงแฟ้มซ้ําในสภาพแวดล้อมการทดสอบ)
- เพิ่มข้อมูลการทดสอบใหม่
  - การจัดตําแหน่ง
  - วง ออร์ เคส ตรา สําหรับ ผู้ จัด การ โปรแกรม
  - ตัวทําดัชนีของ JsonString

### ข้อจํากัดที่รู้จัก

- การทดสอบจะข้ามเมื่อทํางานขนานเนื่องจากหลายกรณีการทดสอบร่วมกันไฟล์เดียวกัน มันผ่านเมื่อทํางานในความโดดเดี่ยว.

## สร้างโครงสร้างแฟ้มใหม่

### บริการ

- - ขลุ่ยออเคสตร้า
- — ฉบับ แปลชื่อประเทศ
- -Json พจนานุกรมการประสาน
- — ฉบับ แปล มาระ โก
- — ข่าวสาร ที่ ส่ง ออก
- -ลองเหตุผลใหม่ด้วยการปกปิดตําแหน่ง
- — ผู้ จัด พิมพ์
- — ส่วน ประกอบ ของ บริการ ใน ประเทศ
- — ส่วนติดต่อผู้ใช้ท้องถิ่น
- — ส่วนติดต่อผู้ใช้ของเอกสาร
- -อินเทอร์เฟซออร์เคสตร้า (อัพเดต)
- — ข้อมูลการแปลภาษาต่อแฟ้ม

### ปรับปรุงบริการใน

- เพิ่มชื่อตัวแทน
- - ปรับปรุงพารามิเตอร์ใหม่
- -ผู้จัดการสถานที่ชื่อ
- — ส่วน ประกอบ หลัก ฐาน

### สร้างหน้าโฆษณาใหม่

- — หน้าสังเกตการณ์เรียลไทม์
- -โมเดลหน้า

### เอกสารใหม่ใน

- -ปรับปรุงเอกสารทางท่อส่งน้ํามัน
- — เครื่อง นํา ทาง ระบบ
- -คู่มือการใช้แดชบอร์ด
- สถาปัตยกรรมเชิงเทคนิค

## ความเข้ากันได้แบบย้อนกลับ

เปลี่ยนแปลงทั้งหมด ถูกเพิ่ม:

- รหัสภาษาท้องถิ่นที่มีอยู่ () ทํางานได้ไม่เปลี่ยนแปลง
- รูปแบบตําแหน่ง () ทํางานได้ไม่เปลี่ยนแปลง
- รูปแบบพจนานุกรม Jonson ที่มีอยู่ไม่เปลี่ยนแปลง
- โครงสร้างการขีดเส้นใต้ที่มีอยู่ ไม่เปลี่ยนแปลง
- ข้อความ SECR ใช้รูปแบบเดียวกัน

## พาธการย้ายภาพ

ไม่จําเป็นต้องย้ายถิ่นฐาน การชดเชยอยู่ภายใน:

1. เก่าถูกเก็บรักษาไว้เพื่ออ้างอิง และต่อมาก็ถูกแทนที่
2. มีการปรับปรุงการลงทะเบียน DI เพื่อใช้ส่วนติดต่อใหม่
3. ผู้ใช้ทั้งหมดที่มีอยู่แล้ว ไม่มีการเปลี่ยนแปลง

## การ ปรับปรุง ผล งาน

- **Reduced memory usage**: Files saved per-language immediately instead of holding all in memory
- **Faster incremental runs**: Only changed/failed Markdown blocks are re-translated
- **Better visibility**: Real-time progress helps diagnose slow stages

## อนาคต

ปรับปรุงแบบวางแผน:

1. **AI fine-tuning** — Post-machine translation review for phrases > 5 words
2. **Admin authentication** — Restrict admin pages to authorized users
3. **Dictionary editor** — Web UI for managing localization keys
4. **Translation statistics** — Charts showing translation counts and error rates over time
5. **Custom placeholder syntax** — Support for alternate placeholder formats

## ติดต่อ

สําหรับคําถามหรือปัญหาในการแปล โปรดอ้างอิงรายละเอียดเอกสาร ในไดเรกทอรีของแต่ละโมดูล หรือติดต่อทีมพัฒนา.
