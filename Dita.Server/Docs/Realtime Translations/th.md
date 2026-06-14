# แปลภาษาแบบเรียลไทม์

เอกสารนี้ใช้เป็นตัวทดสอบแบบสดสําหรับส่งค่าไปทดสอบสําหรับท่อแปลภาษาอัตโนมัติ การเปลี่ยนแปลงใด ๆ ของแฟ้มนี้ จะทําการแทนที่แฟ้มภาษาเป้าหมายใหม่อีกครั้ง ในครั้งต่อไป.

## ภาพรวมของสถาปัตยกรรม

ท่อส่งคําแปลได้ถูกดัดแปลงเป็นสถาปัตยกรรมสยาม โดยมีพนักงานย่อยพิเศษ 4 คน ประสานงานกันโดยวงออร์เคสตราเบา:

- **BackendTranslationService** — Orchestrates the entire pipeline, handles server validation, and delegates work to sub-services.
- **CountriesTranslationService** — Synchronizes country names from `countries.json` into per-language dictionaries.
- **LocalizationTranslationService** — Detects added/removed keys in the default JSON dictionary and translates them into target languages.
- **DocumentsTranslationService** — Translates Markdown documentation files with per-block tracking and metadata.

หน่วยงานย่อยแต่ละดําเนินการอย่างเป็นเอกเทศ และรายงานความคืบหน้าผ่านทางเครื่องส่งสัญญาณ ในเวลาจริง.

## สิ่งที่บริการทํา

บริการดําเนินการตามตารางเวลาและดําเนินการตามท่อส่งเอกสารห้าระยะ: การตรวจสอบความถูกต้องเซิร์ฟเวอร์, การปรับเทียบข้อมูลประเทศ, การปรับเทียบข้อมูลพจนานุกรม Json, การแปลแฟ้มมาร์กดาวน์, และยังคงผล แต่ละขั้นตอนปล่อยออกมา โครงสร้างของความคืบหน้าตามเวลาจริง ผ่านเครื่องส่งสัญญาณ เพื่อที่ลูกค้าที่เชื่อมต่อกัน จะสามารถติดตามได้ตามการทํางาน.

## ระยะการลาก

### ขั้น ที่ 1 — ผู้ รับ ใช้ ที่ รับ การ ตรวจ

ก่อน ที่ งาน แปล ใด ๆ จะ เริ่ม ต้น งาน นี้ ยืน ยัน ว่า มี การ ทํา ตาม เงื่อนไข ก่อน หน้า นี้ ทุก อย่าง:

- ส่วนของการปรับแต่งจะต้องอยู่และใช้งานได้.
- เซิร์ฟเวอร์ LibreTranslate ต้องตอบสนองภายในความล่าช้าที่ยอมรับได้.
- กําลังดึงรายการภาษาที่มีอยู่บนเซิร์ฟเวอร์แปลภาษา.
- ภาษาปริยายที่กําหนดเองจะต้องอยู่ในรายการดังกล่าว.
- แฟ้มที่สูญหายของ Json สําหรับภาษาที่รองรับใดๆ ถูกสร้างขึ้นโดยอัตโนมัติ.

ถ้าการตรวจสอบใด ๆ ที่ล้มเหลว ท่อส่งแก๊สหยุดทันทีและมีการปล่อยข้อความ.

### ขั้น ที่ 2 — การ แปล

ชื่อประเทศจะเก็บไว้ใน sync จากแคตตาล็อกที่อ่านได้อย่างเดียว () ไปยังพจนานุกรม Json สําหรับท้องถิ่น.

- หากภาษาปริยายของโปรแกรมเป็นภาษาอังกฤษ ชื่อประเทศแต่ละชื่อจะถูกจัดเก็บเป็นภาษาที่ไม่มีการแก้ไข.
- หากภาษาปริยายเป็นภาษาอื่น ๆ ชื่อประเทศอังกฤษจะถูกแปลครั้งแรกเป็นภาษานั้น และผลที่ได้จะกลายเป็นรายการในพจนานุกรมปริยาย.
- After the default dictionary is updated, each missing country entry in every target language dictionary is translated and saved **immediately per language**.
- รายการที่ถูกแทนที่แล้ว จะถูกเก็บไว้โดยไม่แก้ไข.
- หากการแปลล้มเหลว บริการจะปรับปรุงใหม่ถึง 3 ครั้ง ด้วยระยะเวลา 30 วินาที ก่อนที่จะย้ายไปเป็นภาษาถัดไป.

### ขั้น ที่ 3 — แปล เจ สัน

บริการจะเปรียบเทียบพจนานุกรมระบบปริยายในปัจจุบันกับภาพที่เก็บไว้จากการทํางานครั้งก่อน:

- **Added keys** — entries present in the current default but absent from the snapshot — are translated into every target language that does not already have a manual entry for that key.
- **Removed keys** — entries present in the snapshot but absent from the current default — are deleted from every target language dictionary.
- การ แปล ด้วย มือ ต้อง มา ก่อน เสมอ. หาก พจนานุกรม เป้า หมาย มี ค่า สําหรับ กุญแจ อยู่ แล้ว รายการ นั้น จะ ไม่ เปลี่ยน แปลง ไม่ ว่า แหล่ง ที่ มา นั้น จะ บอก อย่าง ไร.
- **Each target language dictionary is saved immediately after its translations complete**, rather than waiting for all languages to finish.
- หาก การ แปล ไม่ สามารถ ใช้ ภาษา เฉพาะ ได้ การ แปล ใหม่ โดย อัตโนมัติ. มีข้อผิดพลาดต่อเนื่องเท่านั้น (เช่น ภาษาที่ไม่รองรับ) ทําให้ภาษาดังกล่าวข้าม.
- หลังการประมวลผลพจนานุกรมปริยายในปัจจุบันจะถูกบันทึกเป็นภาพที่จับได้ใหม่สําหรับการเปรียบเทียบครั้งต่อไป.

พจนานุกรมทั้งหมดถูกเก็บไว้พร้อมกุญแจเรียงตัวอักษร และ Json ไม่ยอมอ่าน.

### ขั้น ตอน ที่ 4 — จง แปล แฟ้ม แบบ ย่อ

บริการนี้ จะเดินรากเอกสารที่ปรับแต่งแล้ว (ค่าปริยาย: ) และประมวลผลทุกแฟ้มต้นทาง:

1. ข้อมูลจากแฟ้มต้นฉบับถูกอ่านแล้ว และการเข้ารหัส SHA-256.
2. A `.translation-meta.json` file next to the source tracks per-language, per-block translation status, enabling **incremental re-translation** of only failed blocks.
3. กัญชา ที่ เก็บ ไว้ จาก การ วิ่ง ครั้ง ก่อน (ถูก เก็บ ไว้ ใน แฟ้ม ที่ อยู่ ถัด ไป จาก แฟ้มซอร์ส, หรือ ใน ตําแหน่ง ที่ ขาด หาย ไป ชั่ว คราว) ถูก นํา มา เทียบ กับ แฮช ปัจจุบัน.
4. สําหรับภาษาเป้าหมายแต่ละภาษา ไฟล์ที่ตรงกับนี้ ยังตรวจสอบความถูกต้องของโครงสร้าง.
5. แฟ้มเป้าหมายใด ๆ ที่ขาดหายไป, มีแฮดที่เก่าแล้ว, การตรวจสอบโครงสร้างล้มเหลว, หรือบรรจุบล็อกที่ยังไม่ถูกแปลเป็นคิวสําหรับการเปลี่ยนตําแหน่ง.
6. **Each target language is translated and saved independently** — if Czech succeeds but French fails, the Czech file is still written to disk.
7. แฟ้มที่ถูกแปลสําเร็จ ได้รับการตรวจความถูกต้องสําหรับโครงสร้างความคล้ายคลึงกับแหล่งกําเนิด (เทียบกันกับชื่อ บุคคล, รายการ, บล็อกโค้ด, บล็อกบล็อก, ลิงก์, เครื่องหมายตัวหนา/สัญลักษณ์แบบ HTML) ก่อนจะถูกเขียนไปยังดิสก์.
8. หากแฟ้มเป้าหมายทั้งหมดสําหรับแหล่งที่ประสบความสําเร็จ กัญชาใหม่จะถูกเก็บไว้ถัดจากแหล่ง ถ้าการเขียนต่อไปยังแหล่งกําเนิดล้มเหลว (ตัวอย่างเช่นในการใช้ข้อมูลอย่างเดียว) แฮชจะย้อนกลับไปยังไดเร็กทอรีชั่วคราว.
9. หากการแปลเป้าหมายใด ๆ ล้มเหลวการตรวจสอบข้อมูล ข้อมูลกํากับภาพจะทําเครื่องหมายบล็อกเหล่านั้นเป็น unstransted ดังนั้นพวกเขาจะ retrieved ในครั้งต่อไป.

### ขั้น ที่ 5 — การ ขโมย

มี การ รวบ รวม และ ตี พิมพ์. รวม เอา:

- UTC ทํางานและทําเวลาให้สมบูรณ์.
- เคานต์ของบันทึกแฟ้มท้องถิ่น Json, บันทึกแฟ้มมาร์กดาวน์, บันทึกข้อมูลแฮช, และ Fallback Hash.
- ข้อผิดพลาดที่สะสมระหว่างการทํางาน.
- สถิติการแปลภาษาต่อภาษา (จํานวนที่ลดลง, ข้ามไปนับ, นับพลาด).

## ซองข้อความ

ทุก ๆ ความคืบหน้า เหตุการณ์ที่เกิดขึ้นในสาขานี้

ช่องข้อมูล
|-------|------|-------------|
ตัวระบุการเชื่อมโยงสําหรับการทํางานท่อส่งน้ํามันในปัจจุบัน
ตัวนับโมโนโทนิคภายในการทํางาน เริ่มต้นที่ 1
ชนิดของข้อความ
ระดับท่อข้อความเป็นของ
เวลาที่ส่งข้อความ
จะให้จดหมายแสดงเงื่อนไขข้อผิดพลาดหรือไม่
สรุปที่มนุษย์อ่านได้
การโหลดค่าธรรมเนียม (report obds หรือ unk)

### ชนิดจดหมาย

ค่า
|-------|------|---------|
0
1
2
3
4
5
6

### ระยะการลาก

ค่า
|-------|------|-------------|
0
1
2
3
4
5

### ข้อความธรรมดาที่ไหลโอน

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

ถ้า ระยะ ใด ๆ ที่ ไม่ ได้ รับ การ กําหนด ระยะ ที่ เหลือ จะ ข้าม ไป จะ มี การ ส่ง ข้อ ความ และ ใน ที่ สุด ก็ มี การ ปิด ข้อความ.

## การแปลภาษาซ้ํา

ท่อ ส่ง น้ํา มี ความ ยืดหยุ่น สอง ระดับ:

### ทําซ้ําระดับขั้น (ฝึกอบรม)

- หากการร้องขอให้แปลล้มเหลว หลังจากการเปลี่ยนชื่อภายในของ LibreTransate การดําเนินการเพิ่มเติมถึง 3 รอบ ในระยะการเลื่อนระดับ 30 วินาที.
- ที่ ซ่อน: ผู้ ถือ ตําแหน่ง ที่ ชื่อ () จะ ถูก แทน ที่ ชั่ว คราว ด้วย สัญลักษณ์ ที่ ปลอด ภัย () ก่อน จะ แปล และ คืน สภาพ เดิม หลัง จาก นั้น ทํา ให้ แน่ ใจ ว่า ไวยากรณ์ ถูก ต้อง ใน ภาษา ของ เป้า หมาย.

### การตรวจสอบภาษา

- ก่อน จะ แปล เป็น ภาษา ที่ ใช้ เป็น เป้า หมาย บริการ นี้ จะ เป็น ที่ ยืน ยัน ว่า ได้ รับ การ สนับสนุน จาก เซิร์ฟเวอร์ การ แปล.
- ภาษา ที่ ไม่ ได้ รับ การ สนับสนุน จะ ถูก ข้าม ด้วย คํา เตือน ป้องกัน ความ พยายาม ครั้ง แล้ว ครั้ง เล่า ที่ ล้ม เหลว.

### เริ่มการทําเครื่องหมายใหม่

- ส่วนแปลภาษาแบบทําเครื่องหมาย จะทําการบล็อคต่อบล็อก (หัว, ย่อหน้า, รายการรายการ).
- หากบล็อกแต่ละบล็อกล้มเหลวในการแปล มันจะถูกทําเครื่องหมายว่ายังไม่ได้แปลในแฟ้มข้อมูลกํากับ และถูกแก้ไขใหม่ในการประมวลผลท่อส่งครั้งต่อไป.
- แทร็กเสียงสําหรับบริการต่อลาเล่น, จากแต่ละบล็อค ในแต่ละแฟ้ม ถัดจากแฟ้มแบบ Marcown.

## รหัสผิดพลาด

เกิดข้อผิดพลาดขึ้น โดยทําการจัดกลุ่มรวมเป็นระยะ:

ช่วง
|-------|----------|
1000–1999
2000-299
3000-399
4000-19499
5000-15599

มีข้อผิดพลาดแต่ละรายการในรายงาน จะนํามาซึ่งตัวระบุแหล่ง (รหัสย่อย, พาธของแฟ้ม หรือชื่อเวที), รหัสข้อผิดพลาด และข้อความอ่านเข้าใจของมนุษย์.

## แดชบอร์ด

โครงการเซิร์ฟเวอร์รวมถึงหน้าโฆษณา ที่เชื่อมต่อกับศูนย์ส่งสัญญาณ และแสดงเหตุการณ์ท่อส่งแก๊สทั้งหมด ในเวลาจริง.

- แสดงสถานะการเชื่อมต่อ, จํานวนจดหมาย และตารางการถ่ายทอดสดของเหตุการณ์ทั้งหมด.
- แถวที่มีรหัสสี: สีฟ้าสําหรับเริ่มเวที, สีเขียวสําหรับเสร็จสมบูรณ์, สีแดงสําหรับข้อผิดพลาด.
- การสนับสนุนการล้างแหล่งป้อนและส่งออกทุกข้อความไปยัง Json.
- เชื่อมต่ออัตโนมัติกับเอกซ์โปเนนเชียล แบ็คออฟ ถ้าการเชื่อมต่อลดลง.

## หลักการการออกแบบ

- **Modularity**: Each translation concern is isolated in its own service for maintainability and testability.
- **Incremental persistence**: Dictionaries and Markdown files are saved per-language immediately after translation, reducing memory pressure and providing earlier feedback.
- ** ความล้มเหลว **: ระดับซ้ํา (เอชทีทีพี, เวที, บล็อก) เพื่อให้แน่ใจว่าความล้มเหลวชั่วคราว จะไม่ปิดกั้นท่อส่ง.
- **State tracking**: Per-file metadata (`.translation-meta.json`) and hash files enable precise incremental work on subsequent runs.
- **Real-time visibility**: Every significant operation is reported via SignalR for monitoring and debugging.
- **Manual translations always have priority over automatic additions.**
