# Tercüme Mimarisi

Bu belge Dita'nın otomatik çeviri sisteminin modüler mimarisini açıklar, kullanılabilirliği, test edilebilirliği ve dayanıklılığı geliştirmek için tanıtıldı.

## Tasarım hedefleri

Yeniden düzenleme orijinal monolithic tasarım ile birkaç endişeye yol açtı:

- ** Endişelerin Ayrılması**: Her çeviri domain (bölgeler, JSON sözlükleri, Markdown) izole edilmiştir.
- **Incremental Continuence**: Dosyalar çeviriden hemen sonra sürekli olarak kaydedilir, hafıza kullanımını azaltır ve daha önceki sonuçları sağlar.
- **Resilience **: Birden çok yeniden deneme seviyesi tüm boru hattını engellemeden geçici başarısızlıkları ele alır.
- **Observability**: Gerçek zamanlı izleme için SignalR aracılığıyla her önemli işlem rapor edilir.
- **Extenability**: Yeni çeviri hedefleri tek bir arayüz uygulamakla eklenebilir.

## servis decomposition

### BackendTranslationService (veyachestrator)

**Responsibiliteler**:
- Boru yaşam döngüsü yönetimi (başlangıç, tamamlanma, hata işleme)
- Semaphore-based concurrency control (prevents resetping run)
- Server doğrulama (değerlendirme, dil kullanılabilirliği, yapılandırma)
- Alt servislere delegasyon

**Does NOT contain**:
- Çeviri mantığı
- Dosya I/O belirli formatlar için
- yeniden deneme mantığı

### ÜlkelerTranslationServiceService

**Responsibiliteler**:
- Read from directory
- Ülke isimleri varsayılan yerel sözlük sözlüğüne
- Çeviri, hedef dilde eksik ülke isimleri
- Çeviriden hemen sonra her hedef kelimeyi kurtarın

**Key behavior**:
- Varsayılan dil İngilizce ise: ülke isimleri as-is
- Varsayılan dil başkaysa: İngilizce isimleri ilk önce varsayılan dile tercüme edilir
- Her dil bağımsız olarak kendi retry döngüsü ile işlenir

### yerelleştirmetranslationservice

**Responsibiliteler**:
- Önceki anlık snapshot ile mevcut varsayılan sözlüğü karşılaştırarak ek / devre dışı anahtarlar
- Translate her hedef dilde anahtarları ekledi
- Her hedef dilden silinen anahtarları kaldırın
- Bir sonraki karşılaştırma için anlık görüntüler

**Key behavior**:
- Manual çeviriler her zaman öncelik alır (never over written)
- Eklenen anahtarlar tercüme edilir ve hemen her dilde kurtarılır
- Kaldırılmış anahtarlar hemen dil için silinir
- Snapshot sadece tüm dillerin başarıyla tamamlandıktan sonra kurtarıldı

### BelgeleriTranslationServiceServiceService

**Responsibiliteler**:
- Walk yapılandırılmış Markdown kökleri recursally
- SHA-256 kullanarak kaynak dosyaları değişti
- Per-block çeviri statüsüne giriş
- blok-by-block per-block retry ile tercüme
- Çeviri sonrası Markdown yapısı
- Her hedef dili dosyasını bağımsız olarak kurtarın

**Key behavior**:
- Block- level granularity: başlıklar, paragraflar, liste öğeleri ayrı ayrı ayrı tercüme edilir
- Metadata hangi bloklar başarılı / dilsiz
- Başarısız bloklar, başarılı bloklar yeniden geçiş olmadan bir sonraki çalıştırılır
- Yapı doğrulama, başlık saymalarını sağlar, listeler, kod blokları vs. maç kaynağı

## yeniden deneme stratejisi

Sistem üç seviyede retries uygular:

### Seviye 1 - HTTP (LibreTranslateService)

- Üst üste 5 girişime kadar (1s, 2s, 3s, 4s, 5s)
- Ağ zamanları, 5xx hataları ve geçici başarısızlıklar
- HTTP istemci konfigürasyonuna inşa edilmiş

### 2. Seviye - Aşama (TranslationRetry)

- 30 saniyelik gecikmelerle 3 girişime kadar
- HTTP seviyesi retries sonrası tüm çeviri isteklerini yeniden yükleyin
- Placeholder maskeleme ve restorasyon bu seviyede uygulanır

### Seviye 3 - Block (DocumentsTranslationService)

- Başarısız olan bireysel Markdown blokları metadata
- Bir sonraki boru hattında otomatik olarak geri alındı
- Başarılı bloklar asla yeniden-translated

## veri akışı

### JSON Sözlük çeviri çevirisi

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

### Markdown çeviri çevirisi

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

### Ülke Adı Çeviri

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

## Devlet devam ediyor

### anlık görüntüler

- **JSON **: Varsayılan sözlüğün yanında bir dosyada depo sağlayıcı tarafından değişir)
- **Purpose**: Önceki vadede neler olduğunu takip ederek enables arter senkronizasyonu

### Hash dosyaları

- **Markdown**: Kaynak dosyasına bir sonraki
- **Fallback**: Eğer birincil konum okunursa
- **Purpose**: gereksiz geri dönüşümden kaçınmak için kaynak değişiklikleri

### Çeviri metadata

- **Markdown**:
- **Contents**:
  - Source content hashh
- Per-dil blok durumu (abooleans)
- Son güncelleme süreleri
- **Purpose**: Sadece başarısız blokların kısmi yeniden-translasyonu

### Placeholder depolama

- **File**:
- **Contents**: Anahtarların Kelime değerli çiftleri yerine getirmesi
- **Purpose**: Uygulamadaki yer sahipleri için varsayılan değerleri sağlayın

## Signal Signal R raporlama R raporlama

### Yayımcı Özet

signalR özellerinden çeviri hizmetleri:

```csharp
public interface ISignalRPublisher
{
    Task PublishStageAsync<T>(Guid runId, ProcessStage stage, T data, ...);
    Task PublishMessageAsync(Guid runId, LocalizationMessageType type, ProcessStage stage, ...);
}
```

### Eşitlik garantileri

- Tek bir koşu içindeki mesajlar monoton bir şekilde sıralanır
- Eşitlik numaraları, per-run üzerinden benzersizdir
- Müşteriler boşlukları tespit edebilir veya yeniden sipariş edebilir

### Hub Mapping

```csharp
app.MapHub<LocalizationHub>("/hubs/localization");
```

## uzatma noktaları

### Yeni bir çeviri hedefi eklemek

1. Yeni bir arayüz oluşturun
2. Domaine özgü mantıkla arayüzü uygulayın
3. DI konteynerinde kayıt
4. İndüktöre
5. Mevcut aşamalardan sonra çağrı

### Özel Yeniden deneme politikası politikası

Override yapılarıor parametreleri:

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

### Özel yer sahipleri

Oturma alanı sözel veya depolamayı değiştirmek için uygulama:

```csharp
public class CustomPlaceholderService : IPlaceholderService
{
    // Use {{name}} syntax instead of {name}
    // Store in database instead of JSON file
    // Encrypt sensitive placeholder values
}
```

## Yapılandırma

### örnekler.json

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

### Runtime ayar

Ayar ayarı
|---------|---------|--------|
80
10
3
30

## Test stratejisi

### Birim testleri

Her alt hizmet bağımsız olarak test edilebilir:

- Başarı/failure'i simüle etmek için
- Mock raporlama raporlamayı doğrulamak için
- Dosya için geçici direktörler I/O
- Per-dil tasarrufu davranışını onaylayın

### Bütünleme testleri

- Full pipeline gerçek (yerel) LibreTranslate örneği ile çalışır
- İşareti Ver R mesajları bağlantılı müşterilere teslim edilir
- Test concurrent run prevent (semaphore)
- Çeviri sonrası Markdown yapısı

### End-to-end testleri

- API veya programcı aracılığıyla çeviri
- Tüm hedef dil dosyalarını onaylayın /
- Check metadata dosyaları doğru blok statüsü içeriyor
- Onay yer sahipleri çevirilerle korunmuştur

## Performans değerlendirmeleri

- **Memory **: Per-dil tasarrufu hafızadaki tüm sözlükleri hafızada tutmayı engelliyor
- **Disk I/O**: Metadata dosyaları küçük bir üst ekler, ancak artımlı çalışma sağlar
- **Network**: Manrottling ile eşit işlem ezici LibreTranslate
- **CPU**: SHA-256 hashing ve regex geçerliliği geçncy çeviri ile hızlı bir şekilde bağlantılıdır
- **SignalR**: Hafif mesajlar, tipik raporlar için gerekli olan ücret sıkıştırması gerekmez

## Monolithic design

Orijinal tek bir sınıfta tüm mantığı içeriyordu. Göç yolu:

1. Ülke mantığı →
2. JSON mantığı →
3. Markdown mantığı →
4. Signal R yayın →
5. Retry mantığı →
6. Orkestracıyı sadece delegasyona basitleştiriyor

Mevcut tüm arayüzler () değişmeden kalır. Boru hattının tüketicileri kırılma değişiklikleri görmüyor.
