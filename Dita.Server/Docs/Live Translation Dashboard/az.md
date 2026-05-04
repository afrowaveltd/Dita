# Domen adı qeydiyyatdan keçir

Live Translation Dashboard avtomatik çeviri maşınında real-time görünürlük təklif edir admin səhifədir. Bu SignalR hub bağlanır və onların meydana gətirdi kimi bütün boru sərgiləri göstərir.

## URL

`/Admin/LiveTranslation`

> Note: Authentication and authorization are not yet implemented. Future versions will restrict this page to admin users only.

## Features

### Real-time event stream

All Müəlliflik şəhərindən R hadisələr canlı-updating masasında göstərilir:

- **Sequence nömrə** — Her boru daxilində Monotonic counter
- **Timestamp** — Mövcud olduqda yerli vaxt
- **Run ID** — korporasiya üçün qarşılı GUID
- **Stage** — Boru mərkəzi badge (CheckServers, TranslateCountries, və s.)
- **Type** — Mesaj növü badge (StageStarted, Progress, StageCompleted, və s.)
- **Message** — Human-readable definition
- **Details** — Tam JSON məlumatların ödənişi

### Yadda saxla

Color
|-------|---------|
Mavi ()
Yaşıl ()
Qızıl ()
Ağ (default)

### Qeydiyyat

Üst göstəricilərdə bir status banner:
- **Connecting** — SignalR bağlantısı
- **Connected** — Daxil ol
- **Reconnecting** — Bağlantı itirdi, yeniləmək üçün çalışır
- **Disəfəli** - Bağlantı dəyişik

Əməliyyatdan keçirilir: 0s, 2s, 5s, 10s, 30s.

### Elanlar

- **Clear Feed** — Bütün göstərilən mesajları qaldırır və qazanır
- **Export JSON** - Analiz üçün JSON faylları kimi alınan bütün mesajları yükləyin
- **Message counter** — Bu sessiyada alınan ümumi sayı göstərir

## Signal Qalereya

Panel birləşdirir:

```javascript
const connection = new signalr.HubConnectionBuilder()
    .withUrl("/hubs/localization")
    .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
    .build();
```

### E-poçt

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

### Kompüter növü

Panel bütün qiymətləri işləyir:

Axtarış
|------|---------|
Qalereya
Yadda saxla
Yuxarı
Yadda saxla
Yuxarı
Elanlar
Qeydiyyat

## Texniki proqram

### Oxunub

- **Localization Hub** () — Bütün bağlı müştərilər üçün mesaj yayımlayan SignalR hub
- **ISignalRPublisher** — Mühəndislik xidmətlərinin istifadəsi üçün maşın üzrə bilik
- **SignalRPublisher** - Bir monoton səviyyəsi və yayımları artıq inkişaf edir

### Ad Soyad

- Pure HTML/JS with Bootstrap 5 stil
- Yadda saxla
- Hesabat yeməyi üçün tələb olunan server-side render

### Page  structure

```
Dita.Server/Pages/Admin/
├── LiveTranslation.cshtml      — Razor page markup + JavaScript
└── LiveTranslation.cshtml.cs     — Page model (empty, data comes via SignalR)
```

## Inkişaf zamanı istifadə

1. Dita başlayır. Proqram
2. Qeydiyyat
3. Texniki axtarış axtarış (həmçi üçün gözləyin və ya API çağırır)
4. Watch hadisələri real vaxtda görünür
5. Debugging üçün tam iz almaq üçün Export düyməsini istifadə edin

## İnnovasiyalar

Panel üçün planlaşdırılmış inkişaflar:

- **Authentication** — rolu ilə istifadəçilərin qoşulması
- ** Filtering** — Sənaye, nömrə, nömrə, və ya nömrəli işləyir
- ** Historical çalışır** — Bir bazadan və ya log faylından tamamlanmış işləyir
- **Statistics** — Müəlliflik göstərir, məlumat məlumatları və vaxt üzərində gecikmə
- **Manual tetikler** — Elmi boru mərkəzlərinin qarşılaşdırılması
- **Konfiguration** — Direktordan dəyişdirin
- ** Dil idarəetmə ** — Xüsusi idarə etmək və qazanmaq
- **Dictionary önizleme** — Qeyd və axtarış yerlileştirme dictionaries

## Qeydiyyat

### Dashboard "Səstəkləşdirilmiş"

1. Server çalışan və erişilebilir
2. KORS və ya şəbəkələr üçün browser konsolu
3. Qeydiyyat
4. WebSocket əməliyyatlarının bloklaşdırılması

### Mövzular görünməz

1. Server () və müştəri () arasında SignalR hub URL oyuncaqlarını baxın
2. Müəlliflik hüquququ
3. Kompüter boru faktorları üçün server loglarına baxın
4. Daxil ol WebSocket mesaj üçün Ağ sekmesi

### Mesajlar sifariş edir

Alan bir run daxili sifariş edir. Və mesaj sifariş görünür, göstərir:
- Multimaphore kilidi tərəfindən çox boru aparmaq çalışır (tərmafore lock düyməyə lazımdır)
- Browser tətbiqi məsləhətləri (səhifənin yeniləndirilməsi)
