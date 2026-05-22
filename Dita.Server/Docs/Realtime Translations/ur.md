# حقیقی بار ترجمہ

یہ دستاویز خودکار ترجمے پائپ لائنوں کے لئے زندہ ٹیسٹ کے طور پر موجود ہے ۔ اس فائل میں کوئی بھی تبدیلی اگلے شیڈول چلانے پر تمام نشان زدہ زبان فائل کی دوبارہ منتقلی۔.

## آثارِقدیمہ پر غور کریں

ترجمہ پائپ لائن کو ایک منڈل آرکیٹیکچر میں رکھا گیا ہے جس میں چار قابل ذکر ذیلی ادارے ایک ہلکے وزن کے حساب سے بنائے گئے ہیں:

- **BackendTranslationService** — Orchestrates the entire pipeline, handles server validation, and delegates work to sub-services.
- **CountriesTranslationService** — Synchronizes country names from `countries.json` into per-language dictionaries.
- **LocalizationTranslationService** — Detects added/removed keys in the default JSON dictionary and translates them into target languages.
- **DocumentsTranslationService** — Translates Markdown documentation files with per-block tracking and metadata.

ہر ذیلی حصہ حقیقی وقت میں اشاروں کے ذریعے پیش قدمی اور رپورٹوں کا کام کرتا ہے۔.

## خدمت کیا کرتی ہے

یہ سروس ایک شیڈول پر چلتی ہے اور ایک پانچ رکنی پائپ لائن کا اجرا کرتی ہے: سرور منظوریشن، ملک شمسی نظام، جوزیو ڈکشنری سنینچرونیشن، مارک ڈاؤن فائل کا ترجمہ اور نتائج کو برقرار رکھتی ہے۔ ہر مرحلے میں اشاروں کے اوپر حقیقی وقت کی ترقی کے واقعات خارج ہوتے ہیں تاکہ کام کی آمدنی کے طور پر وابستہ گاہکوں کی پیروی کی جا سکے۔.

## پائپ لائن کے مرحلے

### پہلے پہل — عبادت‌گاہوں کا جائزہ لیں

کسی بھی ترجمے کا کام شروع کرنے سے پہلے یہ خدمت ثابت کرتی ہے کہ سب سے پہلے یہ یقین‌دہانی دستیاب ہے :

- اس حصے کو حاضر ہونا چاہئے ۔.
- آزادانہ سرور کو قابل قبول دیر کے اندر جوابی کارروائی کرنی پڑتی ہے۔.
- ترجمہ سرور پر دستیاب زبانوں کی فہرست آسان ہے۔.
- [ تصویر کا حوالہ ].
- کسی بھی معاون زبان کے لیے گم شدہ فائل کو خودکار بنایا گیا ہے۔.

اگر کوئی چیک ناکام ہو جائے تو پائپ لائن فوری طور پر رک جاتی ہے اور پیغام ختم ہو جاتا ہے۔.

### ۲ — انتقالِ‌خون

ملک کے نام کو صرف کیٹلاگ () سے لیکر مقامی طور پر جونس میں محفوظ کیا جاتا ہے۔.

- اگر اطلاقی زبان انگریزی ہے تو ہر ملک کا نام بغیر ترجمہ کے محفوظ کیا جاتا ہے۔.
- اگر کھوار زبان کوئی دوسری زبان ہو تو انگریزی زبان کا نام سب سے پہلے اس زبان میں ترجمہ کیا جاتا ہے اور اس کے نتیجے میں اقبالی لہجے میں داخلی حیثیت حاصل ہو جاتی ہے۔.
- After the default dictionary is updated, each missing country entry in every target language dictionary is translated and saved **immediately per language**.
- ترمیم کے بغیر پہلے ہی جاری کردہ اندراج شدہ ہیں۔.
- اگر کوئی ترجمہ ناکام ہو جائے تو درجہ بندی 3 مرتبہ تک جاری رہتی ہے جس میں اگلی زبان میں منتقل ہونے سے پہلے 30 جلدوں پر مشتمل ہے۔.

### تیسرا بچہ — عبرانی زبان میں

اس سروس میں حالیہ ڈیٹنگ مقامی ڈکشنری کا موازنہ سابقہ رن سے محفوظ کردہ سابقہ ریکارڈ سے کِیا گیا ہے :

- **Added keys** — entries present in the current default but absent from the snapshot — are translated into every target language that does not already have a manual entry for that key.
- **Removed keys** — entries present in the snapshot but absent from the current default — are deleted from every target language dictionary.
- دستی ترجمے ہمیشہ ترجیح دیتے ہیں۔ اگر ایک لغت میں پہلے ہی سے ایک اہم چیز پائی جاتی ہے توپھر ماخذ کی بات سے قطع‌نظر اس میں داخل ہونا باقی ہے ۔.
- **Each target language dictionary is saved immediately after its translations complete**, rather than waiting for all languages to finish.
- اگر کوئی ترجمہ کسی خاص زبان کے لیے ناکام ہو جاتا ہے تو ذاتی طور پر خدمت انجام دیتی ہے ۔ صرف مستقل غلطیوں (مثلاً غیر مستقل زبان) کی وجہ سے اس زبان کو رائج کیا جاتا ہے۔.
- دوڑنے کے بعد ، موجودہ ڈیٹنگ ڈکشنری کو اگلے مقابلے کے لئے نئے سرے سے محفوظ رکھا جاتا ہے ۔.

تمام اصناف کو ہمیشہ حروف تہجی کے ساتھ محفوظ کیا جاتا ہے اور انسانی پڑھنے کی بے ترتیبی کے لیے جِلد جِلد کی جاتی ہے ۔.

### ۴ — انتقالِ‌خون کی علامات

سروس تمام ماخذ فائل کو دوبارہ ترتیب دیتی ہے:

1. ماخذ فائل مواد پڑھا جاتا ہے اور ایک SHA-256 حاشیہ ہے۔.
2. A `.translation-meta.json` file next to the source tracks per-language, per-block translation status, enabling **incremental re-translation** of only failed blocks.
3. سابقہ دوڑ سے ذخیرہ شدہ حاشیہ (پہلے کسی فائل میں چشمہ فائل کے پاس یا عارضی طور پر گرنے والی جگہ میں) موجودہ حاشیہ سے موازنہ کیا جاتا ہے۔.
4. ہر نشانہ والی زبان کے لیے متعلقہ فائل کا جائزہ بھی لیا جاتا ہے ۔.
5. کوئی بھی ہدف فائل جو گم ہو جائے، اس کے پاس ایک قابل عمل حاشیہ ہو، اس کی ترکیب درست ہو یا پھر اس میں غیر جانبدار بلاکس کو دوبارہ جاری کرنے کے لیے استعمال کیا جاتا ہے۔.
6. **Each target language is translated and saved independently** — if Czech succeeds but French fails, the Czech file is still written to disk.
7. ترجمہ شدہ فائلوں کو کامیابی کے ساتھ ترتیب دیا جاتا ہے جس کا ماخذ (دونوں فہرستیں، فہرستیں، کوڈ بلاکس، بلاک بلاکس، لنکس، بہادر/ٹیریل مارکرز) اور ایچ ٹی ایم ایل ٹیگز کے لیے درج کیے گئے ہیں۔.
8. اگر کسی ماخذ کے لیے تمام ہدف فائل کامیاب ہو جائے تو نئے ہاس کو ماخذ کے پاس محفوظ کیا جاتا ہے۔ اگر سرسید کے پاس لکھنے سے ناکام ہو جائے (مثلاً پڑھنے میں صرف احادیث پڑھی جاتی ہیں) تو حاشیہ عارضی ڈائریکٹری میں واپس گر جاتا ہے۔.
9. اگر کوئی بھی ہدف ترجمہ درستی میں ناکام ہو جاتا ہے تو میٹاداتا ان بلاکس کی نشان دہی کرتا ہے جو غیر جانبدار ہوتے ہیں تاکہ وہ اگلے مرحلے پر دوبارہ رُک جائیں۔.

### پانچواں اُصول

ایک کتاب جمع کرکے شائع کی جاتی ہے ۔ اس میں شامل ہیں:

- یو ٹی سی شروع اور تکمیلی اوقات کا آغاز کرتی ہے۔.
- محفوظ شدہ فائلوں کی تعداد، مارک ڈاؤن فائل محفوظ کی گئی، محفوظ کردہ فائلیں، محفوظ کی گئی ہیں اور گریٹنگ ہہ لکھتا ہے۔.
- دوڑ کے دوران کوئی بھی ذخیرہ شدہ غلطیاں جمع ہو جاتی ہیں۔.
- پیر لغت ترجمہ شماریات (ترجمہ نمبر، حساب کتاب، خطاط)۔.

## اشاروں کی جِلد

ہر ترقیاتی مہم مندرجہ ذیل میدانوں کے ساتھ کے طور پر انجام دی جاتی ہے۔

فیلڈ
|-------|------|-------------|
موجودہ لیپ ٹاپ چلنے والوں کے لیے Correlation Administration
ایک دوڑ کے اندر مونوکونی کی مزاحمت 1 سے شروع ہوتی ہے۔
پیام کی اقسام
پائپ لائن اس پیغام کو حاصل ہے
یو ٹی سی وقت جب پیغام ختم ہو رہا تھا۔
خواہ پیام ایک غلطی کی حالت کی نمائندگی کرتا ہے۔
انسانی قابل تلاوت خلاصہ
سٹیج-کرنٹ ادائیگی (report object یا باطل)

### پیام قسم

قیمت
|-------|------|---------|
0
1
2
3
4
5
6

### پائپ لائن کے مرحلے

قیمت
|-------|------|-------------|
0
1
2
3
4
5

### عام پیام جاری کرتا ہے۔

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

اگر کوئی سٹیج ناکام ہو جائے تو باقی ماندہ مرحلے کو ختم کر دیا جاتا ہے ، پیغام ختم ہو جاتا ہے اور بالآخر پیغام ختم ہو جاتا ہے ۔.

## ترجمہ دوبارہ منطق

پائپ لائن دو سطحوں پر عمل کرتی ہے:

### سٹیج-سطح ری ایکٹر (Translation Reseration Reseration)

- اگر ایک ترجمہ درخواست لیبر ٹرافی کے اندرونی رد عمل کے بعد ناکام ہو جائے تو یہ 3 مزید اسٹیج سطح پر گردش کرتا ہے جس میں 30 سیکنڈ کی تاخیر ہوتی ہے۔.
- ترجمہ سے پہلے اور بعد میں درست گرائمر کو نشانہ زبانوں میں درست طور پر درست گرانے سے پہلے متن میں شامل کِیا جاتا ہے ۔.

### زبان درست

- کسی ہدف کی زبان کا ترجمہ کرنے سے قبل، سروس زبان کو ترجمان سرور کی حمایت حاصل ہے۔.
- اِن زبانوں کو ایک آگاہی دی جاتی ہے ۔.

### مارک ڈاؤن بلاک کی سطح پر دوبارہ شروع

- مارک ڈاؤن کے ترجمے بلاک بِنگ ( ہیڈنگ، پیراگراف، لسٹ ساز) ادا کیے جاتے ہیں۔.
- اگر کوئی فرد بلاک کو ناکام بنانے میں ناکام رہتا ہے تو اسے میٹاداتا فائل میں غیر جانبدار قرار دیا جاتا ہے اور اگلی پائپ لائن چلانے پر دوبارہ عمل درآمد کیا جاتا ہے۔.
- ہر ماخذ مارک ڈاؤن فائل کے پاس فائلوں میں سروس کے راستے ہر زبان میں، آن لائن حیثیت.

## خامی کوڈ

خطاطوں کو ایک متحدہ انیم گروپ کو قطروں میں استعمال کرتے ہوئے بتایا جاتا ہے:

رنگ
|-------|----------|
1000–199
2000–299
3000–3999
4000–4999
5000–5999

رپورٹ میں موجود ہر غلطی اسکرپٹ شناخت کنندہ ( لغت کوڈ، فائل راہ یا اسٹیج نام)، غلطی کوڈ اور انسانی قابل ذکر پیام شامل کرتا ہے۔.

## بائبل ترجمے کی بورڈ

سرور منصوبے میں ایک ابلاغی صفحہ شامل ہے جس میں علامہ اقبال سے متصل ہے اور تمام پائپ لائن واقعات کو حقیقی وقت میں دکھایا گیا ہے۔.

- اتصال کی حیثیت، پیغام نمبر اور تمام واقعات کی زندہ اپ ڈیٹ میز دکھا.
- رنگ کی کوڈڈ قطاریں: سٹیج شروع کرنے کے لیے نیلا، مکمل کرنے کے لیے سبز، غلطیوں کے لیے سرخ۔.
- کھانا صاف کرنے اور تمام پیغامات جون کو برآمد کرنے کی حمایت کرتا ہے۔.
- اگر اتصال گرتا ہے تو خودکار پشتے سے متعلقہ.

## ڈیزائن اصول

- **Modularity**: Each translation concern is isolated in its own service for maintainability and testability.
- **Incremental persistence**: Dictionaries and Markdown files are saved per-language immediately after translation, reducing memory pressure and providing earlier feedback.
- **Resilience**: Multiple retry levels (HTTP, stage, block) ensure transient failures do not block the pipeline.
- **State tracking**: Per-file metadata (`.translation-meta.json`) and hash files enable precise incremental work on subsequent runs.
- **Real-time visibility**: Every significant operation is reported via SignalR for monitoring and debugging.
- **Manual translations always have priority over automatic additions.**
