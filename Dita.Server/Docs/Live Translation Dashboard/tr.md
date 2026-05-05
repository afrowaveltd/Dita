# Canlı Çeviri Dashboard

Live Translation Dashboard, otomatik çeviri hattına gerçek zamanlı görünürlük sağlayan bir yönetici sayfasıdır. SignalR merkezine bağlanır ve meydana geldikleri gibi tüm boru hatları olayları gösterir.

## URL

`/Admin/LiveTranslation`

> Note: Authentication and authorization are not yet implemented. Future versions will restrict this page to admin users only.

## Özellikler

### Gerçek zamanlı etkinlik akışı

Tüm Sinyaller Çeviri hattından R olayları canlı bir masada gösteriliyor:

- **Sequence number** – Monotonik sayacı her boru hattının içinde
- **Timestamp** - Olay alındığı zaman
- **Run ID** - korelasyon için Kısalanmış GUID
- **Stage** – Boru aşaması kötüge (CheckServers, TranslateCountries, vs.)
- **Type** - Mesaj tipi kötüge (StageStarted, Progress, StageCompleted, vs.)
- **Message** - İnsan hazırlanabilir açıklama
- **Details ** – Full JSON olayın veri yükünü öder

### Renkli kodlama

Renkli Renk
|-------|---------|
mavi ()
Yeşil ()
kırmızı ()
Beyaz (default)

### Bağlantı durumu

En üst şovlarda bir durum bayrağı:
- **Connecting** - SignalR bağlantı kurmak
- **Connected** - Normalde olayları yeniden algılama
- **Reconnecting** – Bağlantı kaybetti, yeniden bağlantı kurmaya çalıştı
- **Dis bağlantılı** - Bağlantı kapalı

Bağlantı, üst üste otomatik yeniden bağlantı kullanır: 0s, 2s, 5s, 10s, 30s.

### Kontroller

- **Clear Feed** - Tüm görüntülenen mesajları kaldırır ve karşıtlığı sıfırlar
- **Export JSON ** - Tüm mesajları analiz için bir JSON dosyası olarak indirin
- **Message counter** - Bu oturumda alınan toplam olayları göster

## Signal Signal R hub

Panel birbirine bağlanır:

```javascript
const connection = new signalr.HubConnectionBuilder()
    .withUrl("/hubs/localization")
    .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
    .build();
```

### Mesaj sözleşmesi

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

### Event türleri

Panel tüm değerleri ele alır:

Tipi
|------|---------|
mavi badge
yeşil badge
Red badge
yeşil badge
Red badge
Bilgi kötü
Uyarı

## Teknik uygulama

### Backend

- **Localization Hub** () - SignalR tüm bağlantılı müşterilere mesajları yayınlayan merkezi
- **ISignalRPublisher** - Çeviri hizmetlerinde kullanılmak için merkezin üzerinden
- **SignalRPublisher** - monoton bir sırayı artıran ve yayınları artıran Varsayılan uygulama

### Frontend

- Pure HTML/JS with Bootstrap 5 stil
- Microsoft SignalR JavaScript müşteri kütüphanesini kullanın ( CDN'den yükleniyor)
- Etkinlik beslemesi için gerekli olan hiçbir sunucu-side oluşturma

### sayfa yapısı

```
Dita.Server/Pages/Admin/
├── LiveTranslation.cshtml      — Razor page markup + JavaScript
└── LiveTranslation.cshtml.cs     — Page model (empty, data comes via SignalR)
```

## Geliştirme sırasında kullanım

1. Dita'ya başlayın. Server uygulaması
2. gezinmek için
3. Bir çeviri koşmak (programcı için ne bekleyin veya API'yi arayın)
4. Watch events gerçek zamanlı görünüyor
5. Debugging için tam bir iz yakalamak için ihracat düğmesine kullanın

## Future geliştirmeler

Panel için planlanmış gelişmeler:

- **Authentication** - Rolt access to users with the role
- **Filtering** - Sahneye göre filtre olayları, tipi veya ID koşmak
- **Hetorical çalışır** - Tamamlanan bir veritabanı veya log dosyasından çalışır
- **Statistics ** - Çeviri sayılarını gösteren grafikler, hata oranları ve zamanla gecikmeler
- **Manual triggers** - Belirli boru hatları aşamalarına manuel olarak başlamak için Düğmeler
- **Konfigurasyon** - Doğrudan panodan analiz
- ** Dil Yönetimi** - View and edit supported languages
- **Diksiyoner önizleme** - Yerelleşme sözlükleri ara ve arama

## Sorun Giderme

### Dashboard, "Bağlantılı" gösterir

1. Sunucuyu doğrulayın ve erişilebilir
2. KURUMSALS veya ağ hataları için tarayıcı konsolu
3. Onay şu anda mevcut
4. Hiçbir güvenlik duvarının WebSocket bağlantılarını engellemesi

### Olaylar görünmüyor

1. SignalR merkez URL'nin sunucu () ve müşteri arasındaki maçları kontrol edin ()
2. Programcıyı teyit etmek, etkinleştirilir
3. Çeviri hatları hataları için sunucu loglarına bakın
4. Check browser WebSocket mesajları için ağ sekme

### Mesajlar sipariş dışında

Alan tek bir koşu içinde sipariş etmeyi garanti eder. Eğer mesajlar sırayla ortaya çıkarsa, işaret edebilir:
- Birden çok boru hattı çakılıyor ( semaphore kilit nedeniyle gerçekleşmelidir)
- Tarayıcı oluşturma sorunları (sayfayı ferahlatıcı)
