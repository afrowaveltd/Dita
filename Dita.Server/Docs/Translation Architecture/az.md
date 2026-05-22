# Əməkdaşlıq

Bu səhifə Ditanın avtomatik çeviri sisteminin modulu mimarisini izah edir, təhlükəsizliyi, testability və dayanıqlığını artırmaq üçün təklif edir.

## Dizayn məhsulları

Yeniləşdirilməsi orijinal monolithic dizaynı ilə bir neçə məlumatlaşdırdı:

- ** Şirkətlərin məlumatlaşdırılması**: Hər bir çeviri domaini (sayt, JSON dictionaries, Markdown) izole edilir.
- **Incremental persistence**: Files are saved per-language immediately after translation, reducing memory usage and providing earlier results.
- **Resilience**: Bir çox retry səviyyəsi bütün boru quraşdırmadan əvvəlliyyatları işləyir.
- **Observability**: Real-time monitorinq üçün SignalR haqqında hər hansı məsləhət bildirilir.
- **Extensibility**: New translation targets can be added by implementing a single interface.

## Xidmət dekomporativ

### BackendTranslationService (orchestrator)

**Responsibilities**:
- Lineer həyahət sürətli idarəetmə (başa, tamamilə, səyahət)
- Semaphore-based concurrency control (prevents Seniorping run)
- Yadda saxla
- Alt xidmətlərin müavini

**Does NOT contain**:
- İqtisadi
- Xüsusi formatlar üçün File I/O
- Retry sənayesi

### Avropa

**Responsibilities**:
- »
- Sinkronize ölkə adlarını default yerli sözlərinə daxil edin
- Ümumi ölkənin adları
- Müəlliflikdən sonra hər kəsmi məlumat alın

**Key davranış**:
- İnformasiya dili İngilis dili: ölkələr kimi saxlanılır -
- Yadda saxla
- Hər bir dil öz retry loop ilə öz işləyir

### YerlileştirmeTranslationService

**Responsibilities**:
- Əvvəlki snapshot ilə cari default sözləşdirilməsi ilə əlavə / izləndirilmiş anahtarlar
- Əməliyyat dilinə əlavə edib
- Haqqında silinmiş şəkillərin silinməsini
- Bir sonraki müqayisə üçün snapshot edin

**Key davranış**:
- Manual çevirilər haqqında əvvəl əvvvəllik (baharlanandan)
- Əvvəlki mövzular təsdiq edilir və hər hansı bir dərhal qəbul edilir
- Qeydiyyatdan keçməli
- Snapshot yalnız bütün dillərdən sonra tam uğurlu

### Tarix

**Responsibilities**:
- Planlaşdırılmış Markdown kökləri recursive
- SHA-256-hesapları istifadə edən məhsul faylları
- In-blok çeviri statusu
- Per-block retry ilə bloq blok-by-block
- Müqayisə sonra Markdown strategiyası
- Hər bir hər kəs dili faylını qəbul edin

**Key davranış**:
- Block-level granularity: qışlar, paragraflar, siyahısı maddələr ayrı ayrı ayrı çevrililir
- Metadata bloklar dildə / səhifə
- Təhlükəsiz bloklar re-translating uğurlu blokları olmadan birbaşa run yenilənir
- Strukturun təhlükəsizliyi, siyahısı, kod blokları, və s. kağızı

## Retry strategiyası

Sistem üç səviyyədə retries təyin edir:

### level - http://libretranslateservice

- Üstat geri dönüş ilə 5 əməkdaşlıq (1s, 2s, 3s, 4s, 5s)
- Ağ vaxtları, 5xx məlumatları və səyahət uğursuzluqları
- HTTP müştəri konfiqurasiyası daxili

### level 2 —  stage (translationretryservice)

- 30-ikinci gecikmələrlə 3 əməkdaşlıq
- HTTP-level retries sonra bütün çeviri istəyi dəstək
- Bu səviyyətdə yerləşdirici maska və reaksiya tətbiq edilir

### level - blok (documentstranslationservice)

- Metadatadatada qeyd olunmayan yalnız Markdown blokları
- Əvvəlki boru runda avtomatlaşdırılmışdır
- Ətraflı bloklar heç vaxt-translated

## Data axtarış

### J  J  J

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

### Mark

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

### Ümumi adı

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

## Dövlət davamlılığı

### Axtarış

- **JSON**: default sözləşdirilən bir faylda yerləşdirilmiş (isim saxlama təchizatçısı tərəfindən daxildir)
- **Purpose**: Əvvəlki runda təqdim edilən şeyi izləməklə dəstəkliyi

### E-poçt

- **Markdown**: mövcud fayl
- **Fallback**: Əsas yer oxuysa
- **Purpose**: gereksiz re-translation qarşısını almaq üçün məqsəd dəyişikliklərini sınaq

### Elan

- **Markdown**: `{sourceFile}.translation-meta.json`
- **Contents**:
  - Yadda saxla
- Per-dil blok statusu (bucaqlar qarışıq)
- Son yeniləmə vaxtları
- **Purpose**: Yalnız başarısız blokların ümumi re-translation

### Qalereya

- **File**: `Locales/placeholders.json`
- **Contents**: məhsullar sözləndirilməsi
- **Purpose**: proqramda yerləşdiricilər üçün default qiymətlərini verir

## Saytın xəritəsi

### Oxunub

signalR xüsusiyyətlərinin çeviri xidmətləri:

```csharp
public interface ISignalRPublisher
{
    Task PublishStageAsync<T>(Guid runId, ProcessStage stage, T data, ...);
    Task PublishMessageAsync(Guid runId, LocalizationMessageType type, ProcessStage stage, ...);
}
```

### Qeydiyyat

- Bir run daxil olmaq istehsalçılıq
- Qeydiyyat nömrələri tərəfindən unikal
- Yadda saxla

### Qeydiyyat

```csharp
app.MapHub<LocalizationHub>("/hubs/localization");
```

## Daxil ol

### Yeni bir çeviri haqqı

1. Yeni interfeys yaradır
2. Domen adı qeydiyyatdan keçirt »
3. Qablaşdırma
4. Yapışdırma
5. Müxtəlif mövzulardan sonra

### Xüsusi retry siyasəti

Override struktur parametrləri:

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

### Xüsusi yerləşdirilməsi

Yer sahibi nümayişlərini və ya saxlamaq dəyişiklikləyir:

```csharp
public class CustomPlaceholderService : IPlaceholderService
{
    // Use {{name}} syntax instead of {name}
    // Store in database instead of JSON file
    // Encrypt sensitive placeholder values
}
```

## Konfiqurasiya

### app.json

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

### Runtime avadanlıq

Setting
|---------|---------|--------|
80
10
3
30

## Test strategiyası

### Kompüter testləri

Hər bir alt xidmət bağımsız test edilir:

- Mock uğur/failure
- Qeydiyyatdan keçin
- I  for  for  for  for I/ Oxunub
- Təhlükəsizlik davranışı

### İnteqrasiya testləri

- Tam boru haqqında real (yer) LibreTranslate sənayesi
- Təhlükəsiz müştərilən müştərilərinə çatdırılmışdır
- Kompüter kompüterinq (semaphore)
- Müqayisə sonra Markdown strategiyası

### End-to-end testləri

- API və ya proqram
- Bütün hedef dil faylları yaradılmış/updated
- Metadata faylları düzgün blok statusu daxildir
- Müəlliflik hüququqları dəstəkləyir

## Performans baxışları

- **Memory**: Per-dil qorumaq bütün dictionaries tutmaq qarşısını almaq
- **Disk I/O**: Metadata faylları kiçik yüksək əlavə edir, lakin artan iş
- **Network**: Sequential processing with throttling prevents overwhelming LibreTranslate
- **CPU**: SHA-256 təhlükəsizliyi və rəsmi təhlükəsizliyi müəyyən müəyyən müəyyən müəyyəndir
- **SignalR**: Qırmızı mesajlar, tipik hesabatlar üçün pulsuz kompressiyası

## Monolithic dizayndan etibarlıq

Orijinal bir sinif bütün məhsul daxildir. Qarabağ yolu:

1. İngilis dili →
2. → JSON →
3. Markdown mantığı →
4. Çap SignalR yayımı
5. Retry →
6. Kompüterini nümayiş edir - yalnız

Bütün mövcud interfeyslər () qeyd olunub. Boru maşın avadanlıqları heç bir kvadrat dəyişiklikləri görür.
