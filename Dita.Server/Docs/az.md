# Avtomatik Translation Service dəyişikliklərin məlumatları

## Overview

Bu səhifə Dita avtomatlaşdırma xidmət xidmətinin bütün dəyişikliklərini mövcuddur, yeni xüsusiyyətlər, gözəllik inkişafları və yerlileştirme inkişafları daxildir.

## Memarlıq məlumatları

### Resursed BackendTranslService

Monolithic parlaq bir rəsmi rəsmi rəsmi rəsmi xidmətləri ilə müəyyən edilmişdir:

- **BackendTranslationService** — Boru sənayesi (server validation, mərkəzi məlumat, səyahəti)
- **CountriesTranslationService** — Ümumdünya adı senkronizasyon (İngilis dili →)
- **LocalizationTranslationService ** — JSON səviyyə sinksiyası (added/removed keys)
- **DocumentsTranslationService** - Blok-düzlük monitorinq ilə Markdown məlumatları məlumat
- **SignalRPublisher** - SignalR ilə Real-time təhlükəsizlik hesabatı
- **TranslationRetryService** — mövcud saxlama ilə mövcud retry

### Benefits

- ** Şirkətlərin məlumatlaşdırılması**: Hər bir xidmət domen adı transferi
- **Maintainability**: Kiçik kurslar anlamaq və test daha asandır
- **Extensibility**: New translation targets can be added via interface implementation
- **Reliability**: Xüsusi xidmətlər daha yaxşı quruculuq təmin edir

## Yeni xüsusiyyətlər

### Proqramlar

**Location**: `/Admin/LiveTranslation`

Translation : : :  the  the

- Onlar olduğu kimi bütün SignalR hadisələri göstərir
- Color-coded mesaj növü (blue= started, yaşıl = tamamilə, qırmızı=error)
- Auto-reconnect ilə Bağlantı status banner
- JSON-a qoşulmaq

### Ad Soyad

Yerlileştirme sistemi hər hansı müxtəlif dillərin inkişaf edilməsi üçün yer tutucuları () dəstəkləyir:

```csharp
// Usage in code
var message = Localizer["WelcomeMessage", new Dictionary<string, string>
{
    ["userName"] = "John",
    ["count"] = "5"
}];
// Result: "Hello John, you have 5 new messages"
```

Xüsusiyyət:
- Runtime və ya saxlamaq üçün təhlükəsiz qiymətləri
- Yolsuzluğu qarşısını almaq üçün çeviri zamanı avtomatik maskalama/restoration
- Mövcud mövcud mərkəzi ilə uyğun

### Axtarış

Markdown faylları inkişaf edir:

- **Per-dil qəbul**: Müəlliflik hüququqları qorunur
- **Block-level monitor**: blok başına çeviri statusunu izləyir
- **Selective retry**: Only failed blocks are re-translated on the next run
- **Metadata davamlılığı**: İqtisadiyyatdan keçmişdir

### Retry Logic inkişaf

Üç dayanıq səviyyəsi:

1. **HTTP retry** (LibreTranslateService): 5 kateqoriya geri dönüş ilə çalışır (1s–5s)
2. **Stage retry** (TranslationRetryService): 30s gecikmə ilə 3 əlavə məsləhət
3. **Block retry** (DocumentsTranslationService): Birbaşa run yenilənmiş Markdown blokları

### Saytın xəritəsi

Bütün boru əməliyyatları üçün real-time inkişaf:

- Heydər Əliyev
- Müxtəlif proqramlar
- Fayl hadisələri ətraflı konfransı (source, qeyd kodu, mesaj)
- Hər bir run daxil olmaqla bağlı növlük sayı təhlükəsiz

## Konfiqurasiya dəyişiklikləri

### app.json

Çıxış dəyişikləri. mövcud konfiqurasiya işləyir:

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

### Yeni xidmətlər

Daxil ol:

- /
- `TranslationRetryService`
- /
- /
- /
- /

SignalR hub müştəri əməliyyatları üçün xidmət edilir.

## Kompüter

### Test statusu

- **243/244 testlər** (1 test texnologiyası daxil olmaqla atladı)
- Yeni test səhifə:
  - Qalereya
  - BackendTranslationService
  - JsonStringLocalizer yerləşdirici indeksers

### Xüsusiyyətlər

- test paralel çalışır zaman atılır, çünki çox test halları eyni fayl paylaşılır. Soyutma zamanı keçirilir.

## Yeni Dosya strukturu

### Xidmətlər

- - Borular
- - Ümumi adı
- — JSON sözlər sinkronizasiyası
- — Markdown çevirici
- - SignalR mesaj yayımı
- - Yerləşdirici maska ilə retry mantığı
- - Publisher interfeys
- - Country xidmət interfeys
- - Yerlileştirme xidməti interfeys
- - Document xidmət interfeys
- - Orkeor interfeys (updated)
- - Per-file çeviri metadata

### Yeniyetmə xidmətləri

- - mövcudluq əlavə
- — Yeni parametr üçün yeniləndirilmişdir
- — Add yerləşdirici idarə
- - Yerləşdirici interfeys

### Yeni Admin

- - Real-time monitor page
- —

### Yeni Sertifikatlaşdırma

- — yenilənmiş boru məlumatları
- - Qeydiyyat sistemi
- - Dashboard istifadəçisi
- — Texniki memarlıq

## Qeydiyyat

Bütün dəyişikliklər əlavə olunur:

- Yerlileştirme kodu () mövcuddur
- Rəsmi formatlama () işlənir
- Uşaq JSON səviyyə formatı dəyişikliklənir
- Markdown strukturu mövcuddur
- SignalR mesajları eyni formatdan istifadə edir

## Miqrasiya yolu

Növbət lazımdır. Refaktoring daxilidir:

1. Yaşlı bir referans kimi qəbul edilmişdir və sonra əvvəl
2. Yeni interfeys istifadə etmək üçün yeni qeydiyyatlar yeniləndi
3. Bütün mövcud müştərilər heç bir dəyişikliklər görür

## Proqramlar

- **İnformasiya istifadəsi**: Fayllar haqqında bütün saxla
- **Faster inkişafı**: Yalnız dəyişdirilmiş/failed Markdown blokları re-translated
- **Better görünürlük**: Real-time inkişaf yavaş məhsulları tanıyacaq

## İnnovasiyalar

Planned inkişaf:

1. **AI fine-tuning** — Post-machine sözlər üçün təsviri baxış > 5 söz
2. **Admin nəzarət** — Restrict admin səhifəçilərin saytında
3. **Dictionary istehsalçısı** — Yerlileştirme qorunması üçün Web UI
4. **Translation statistika** — Müəlliflik sayı və səviyyə faizlərini vasitəsilə göstərir
5. **İctimai yerləşdirici texnologiyası** — alternativ yerləşdirici formatları

## Bakı

Komponent xidməti ilə suallar və suallar üçün, hər bir modul nümayişlərini həyata keçirmək və ya inkişaf komandasına əlaqə saxlayın.
