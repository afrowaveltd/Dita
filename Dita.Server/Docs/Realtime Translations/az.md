# Real-time çeviri

Bu səhifə avtomatik çeviri boru üçün canlı test giriş kimi var. Bu faktın hər hansı bir dəyişiklik bir növbəti, növbəti planlaşdırılmış runda bütün hedef dil faktorlarının re-translation.

## Memarlıq

Translation  pipeline  has  has  has  a  The  The  The  The  The  The  The  The  The  The  The  The

- **BackendTranslationService ** — Orkestrates bütün boru, server validation, və deles sub-services iş.
- **CountriesTranslationService ** — Sinkronize ölkələrindən xüsusiyyətlərindən xüsusiyyət adları.
- **LocalizationTranslationService** - default JSON sözlərində əlavə / izləndirilməsi və onları hedef dillərdə çevirmək.
- **DocumentsTranslationService** - bloq izləmə və metadata ilə Markdown məlumat faylları.

Hər bir sub-service real vaxtda SignalR vasitəsilə özdən və hesabatların təqdim edir.

## Xidmət nədir

Xidmət bir proqram işləyir və beş səyahət borusunun icra edilməsi: server validation, ölkə senkronizasyonu, JSON sözlər senkronizasyonu, Markdown fakültəsi və həyata davam edir. Hər bir məhsul SignalR üzrə real vaxt inkişafı təqdim edir, bu da bağlı müştərilər iş haqqında davam edə bilər.

## Borular

### Stage 1 — CheckServers

Heç bir çeviri işi başlanğıcdan əvvəl, xidmət bütün prezidentlərin razı olduğunu göstərir:

- Konfiqurasiya bölməsi hazır olmalıdır və mövcud olmalıdır.
- LibreTranslate server müxtəlif latency daxil olmalıdır.
- Komponent server mövcud dillərin siyahısı alınır.
- Konfiqurasiyanın qlobal dili bu siyahıda hazır olmalıdır.
- Heç bir əlavə dili üçün yerli JSON faylları avtomatik yaradılır.

Heç bir çek qaldırırsa, boru dəyişdirir və bir mesaj yayılır.

### Stage 2 —  Translate

Ümumileştirme JSON dictionaries daxil olmaqla yalnız kataloqdan () bir oxumaq kataloqda yerləşdirilir.

- İnformasiya default dili İngilis deyil, hər bir ölkə adı çeviri olmadan saxlanılır.
- İngilis dili bir digər dil varsa, İngilis ölkə adı ilk o dili çevrilənir və nəticə default sözdə giriş olur.
- After the default dictionary is updated, each missing country entry in every target language dictionary is translated and saved **immediately per language**.
- Ətraflı girişlər dəyişdirilməsi olmadan saxlanılır.
- Bir çeviri başarısız olursa, xidmət əvvəlki dildən əvvəl 30 ikinci gecikmə ilə 3 dəfə qəbul edilir.

### Kateqoriya 3 — TranslateJsonFiles

Xidmət əvvəlki rundan saxlanılan bir snapshot ilə cari default lokalizasiya sözlərini karşılaştırır:

- ** Added keys** — mövcud default mövcuddur, lakin snapshot-dan mövcuddur - artıq o qurmaq üçün bir sifariş girişi olmayan haqqında çevrilir.
- **Removed keys** — dəyişdirilməsi həyata keçirilir, lakin cari default-dən yoxdur - hər bir hedef dil sözdən silinir.
- Manual çevirilər həyata keçirilir. Bir hedef söz əvvəl əvvəl bir dəfə dəyişdirirsə, giriş məhsulun necə deyil ki, dəyişdirilməz.
- **Each target language dictionary is saved immediately after its translations complete**, rather than waiting for all languages to finish.
- Bir çeviri xüsusi dil üçün başarısız olursa, xidmət avtomatik retries. Yalnız davamlı məlumatlar (e.g., cavabsız dil) dilin atılması üçün əvvəl.
- Başdan sonra, cari default sözlər bir sonraki müqayisə üçün yeni snapshot kimi qeyd edilir.

Bütün dictionaries həmçinin həmçinin həmçinin qeyd edilib və insan oxumaqlıq üçün indented JSON.

### Kateqoriya 4 — TranslateMarkdownFiles

Xidmət strukturu (default: ) və proseslər hər hansı bir məhsul resursiv:

1. Əsas faylları oxuyur və SHA-256 hash işlənir.
2. Yalnız başarısız bloklar yalnız başarısız bloklar **incremental re-translation** imkan verir, per-blok çeviri statusunda mövcud track.
3. Yadda saxla.
4. Hər bir hedef dil üçün, müxtəlif fayl də strukturu üçün nəzarət edilir.
5. Heç bir hedef fayl, qeyd edilmiş bir hash var, qeyd strukturu təhlükəsizliyi var, və ya untranslated bloklar re-translation üçün sıralanır.
6. ** Hər kəsmə dili təsdiq edilmiş və qüvvəl qəbul edilmişdir** - Çex uğurlu, lakin Fransız cavab, Çex fayl hala disk yazılıdır.
7. Müxtəlif təhsil faylları məhsul ilə struktur parity üçün təsdiq edilir (eşit başlıqları, siyahısı, kod blokları, blokquotes, bağlantılar, güclü/italic markerlər, və HTML tags) disk yazılı.
8. Bir məhsul üçün bütün hedef fayllar, yeni hash məhsulun yanında saxlanılır. məhsulun nömrəsindən sonra yazırsa (həmçinin yalnız yerləşdirilməsi üçün), hash müxtəlif seriyayaya geri düşür.
9. Heç bir hər hansı bir tərcümə təqdim edilməzsa, metadata, bir nömrəsində yeniləndirilmişdir.

### məhsul 5 — saxlamaq

Bir konsolide yığılmış və yayımlanmışdır. Bu daxildir:

- UTC run start və dəfə vaxtları tamamlamaq.
- Yerlie JSON faylları saxlamaq, Markdown faylları qeyd, xüsusi faylları qeyd, və geri hash yazır.
- İş zamanı toplanan həyata saxlama hataları.
- Per-dil çeviri statistikası (translated sayı, atlad sayı, səhv sayı).

## SignalR mesaj qutusu

Aşağıdakı sahələr ilə hər hansı bir inkişaf edilib:

Tarix
|-------|------|-------------|
Cari boru run üçün korrelasiya simvoliv
Bir run daxili Monotonic counter, başlanğıc 1
Saytın növü
Əməliyyat məhsulları
Oxunub:  the
Yadda saxla
Human-readable mövzu
Stage-based payload (report object or null)

### Mesaj növü

Qeydiyyat
|-------|------|---------|
Qeydiyyat
1
2
3
4
5
6

### Borular

Qeydiyyat
|-------|------|-------------|
Qeydiyyat
1
2
3
4
5

### Tipik mesaj axtarış

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

Heç bir məhsul başarısız olursa, qalın məhsulları atılır, bir mesaj yayılır, və sonunda bir mesaj işləyir.

## Kompüter resepsiyası

Boru maşın iki səviyyəsini təsdiq edir:

### Konfrans-yerasiya retry (TranslationRetryService)

- LibreTranslate'nin daxili retries-dən sonra bir əməkdaş istəyirsinizsə, 30-ikinci gecikmələrlə 3 əlavə səviyyə səviyyəti əlavə edilə bilər.
- Yerləşdirici maska: Add yerholders () məhsulda təhlükəsiz heç vasitəsilə əvvvəl əvvvəl dəyişdirilir.

### Language

- Bir qəbul dilinin çevrilməsindən əvvvəl dil çeviri serveri tərəfindən dəstəkləyir.
- İctimai dillər bir uyarı ilə atılır, müvafiq başarısız təhlükəsizliklərin qarşısını alır.

### Markdown blok-level retry

- Markdown çevirilər blok-by-block (başa, paragraflar, siyahısı maddələri).
- Bir müxtəlif blok çeviriciyə, metadata falated kimi qeyd edilir və bir sonraki boru run retried edilir.
- Xidmət, hər hansı bir mövcud Markdown faylındakı fayllarda per-blok status.

## Fayl kodları

Fayllar birləşmiş ümumi qrup istifadə edilir:

Axtarış
|-------|----------|
1000-1999
2000–2999
3000–3999
AZ1000
5000–5999

Heç bir hesabatda hər hansı bir məsləhət məqsədi (dil kodu, fayl yolu, və ya məhsul adı), səhifə kodu və insan hazırlıq mesajı.

## Domen azad deyil

Server layihəsi, SignalR hub-a bağlanır və real vaxtda bütün boru məlumatlarını göstərir.

- Bütün hadisələrin canlı-updating masasını göstərir.
- Color-coded sıralar: məhsullar üçün mavi, tamamlamaq üçün yaşıl, qırmızı.
- JSON-a bütün mesajları qiymətləndirmək və ixtisaslaşdırmaq.
- Əlaqə azaldırsa üstat backoff ilə Auto-reconnects.

## Dizayn prinsləri

- **Modularity**: Hər bir çeviri məsləhəti təhlükəsizlik və testability üçün öz xidmətdə izole edilir.
- **Incremental persistence**: Dictionaries and Markdown files are saved per-language immediately after translation, reducing memory pressure and providing earlier feedback.
- **Resilience**: Birden çox retry səviyyəsi (HTTP, məhsul, blok) səyahətin boru səviyyəsini blok etmir.
- ** Dövlət monitor**: Per-file metadata () və hash faylları aşağıdakı işlərində səhifə effektiv iş göstərir.
- **Real-time görünürlük**: monitorinq və debugging üçün SignalR haqqında hər əsas məlumat verilir.
- **Manual komponentlər avtomatik əlavələr üzərində əvvvəllik edir.**
