# Otomatik Tercüme Hizmetine Değişiklikler Özeti

## Genel

Bu belge, mimari yeniden faktörleme, yeni özellikler, gözlemlenebilirlik geliştirmeleri ve yerelleşme geliştirmeleri dahil olmak üzere Dita otomatik çeviri servisine yapılan tüm değişiklikleri özetliyor.

## Mimarlık Değişiklikleri

### Emekli Geri Döndürme Hizmeti

Monolithic, hafif bir orkestracı tarafından koordine edilen dört özel hizmete maruz bırakıldı:

- **BackendTranslationService** - Boru orkestrası (server validasyon, aşama delegasyonu, hata işleme)
- **CountriesTranslationService** - Ülke adı senkronizasyon (İngilizce → hedef dili)
- **LocalizationTranslationService** - JSON söz senkronizasyonu (added/removed anahtarları)
- **DocumentsTranslationService** - Markdown doküman çevirisi blok seviyesindeki izleme ile
- **SignalRPublisher** - SignalR ile gerçek zamanlı ilerleme raporlama
- **TranslationRetryService** – Aşama düzeyinde retry with placeholder protection

### Faydaları

- ** Endişelerin Ayrılması**: Her hizmet tek bir çeviri domaini ele alır
- **Maintainability**: Küçük sınıflar anlamak ve test etmek daha kolaydır
- **Extenability**: Yeni çeviri hedefleri arayüz uygulamaları ile eklenebilir
- **Reliability**: Bağımsız hizmetler daha iyi hata izolasyonu sağlar

## Yeni Özellikler

### canlı çeviri monitörü

**Location**:

Çeviri hattına gerçek zamanlı görünürlük sağlayan yeni bir yönetici sayfası:

- Görünüşe göre tüm SignalR olayları gösterir
- Renkli kodlanmış mesaj türleri (mavi = başlangıç, yeşil = tamamlanmamış, kırmızı = terörizm)
- Bağlantı durumu otomatik bağlantı ile
- Mesaj sayacı ve JSON'a ihracat

### Add Placeholders

Yerelleştirme sistemi artık farklı dillerde gelişmiş grammatiksellik için isim sahibileri () destekliyor:

```csharp
// Usage in code
var message = Localizer["WelcomeMessage", new Dictionary<string, string>
{
    ["userName"] = "John",
    ["count"] = "5"
}];
// Result: "Hello John, you have 5 new messages"
```

Özellikler:
- Runtime'da sağlanan yer sahipleri değerleri veya depolandığında depolanır
- Yolsuzluk önlemek için çeviri sırasında otomatik maskeleme/restorasyon
- Mevcut konumsal yer sahipleri ile Backward uyumlu

### Çeviri çevirisi

Markdown dosyaları kademeli olarak tercüme edilir:

- **Per-dil tasarrufu**: Her hedef dili hemen çeviriden sonra kurtarılır, hafıza basıncının azaltılması
- **Block- seviyesi takip**: blok başına çeviri statüsü izler
- **Seçmeli retry**: Sadece başarısız bloklar bir sonraki runda yeniden canlanıyor
- **Metadata kalıcılık**: Çeviri devleti başvuru yeniden başlar

### Geliştirilmiş Retry Logic

Üç dayanıklılık seviyesi:

1. **HTTP retry** (LibreTranslateServ): Üst üste 5 girişim (1s-5s)
2. **Stage retry** (TranslationRetryService): 30 gecikmeli 3 girişim
3. **Block retry** (DocumentsTranslationService): Bir sonraki runda Markdown blokları tekrar tekrarladı

### SignalR raporlama

Tüm boru operasyonları için gerçek zamanlı ilerleme raporlama:

- Her aşama olayları yayınlar
- Per-dil ilerleme olayları olarak yayınlandı
- Hata olayları ayrıntılı bağlam içerir (kaynak, hata kodu, mesaj)
- Eşitlik numaraları her bir run içinde sipariş etmeyi garanti eder

## Yapılandırma Değişiklikleri

### örnekler.json

Hiçbir kırılma değişikliği yok. Mevcut yapılandırma çalışmaya devam ediyor:

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

### Yeni Hizmetler

Kayıt:

- /
- `TranslationRetryService`
- /
- /
- /
- /

SignalR merkezi müşteri bağlantıları için haritalandı.

## Test Testi Test Testi

### Test durumu

- **243/244 testleri geçer** (1 test ortamında mevcut dosya erişimi nedeniyle atılır)
- Yeni test kapsamı eklendi:
  - placeholderservice functionality functionality functionality
  - BackendTranslationService orkestration
  - JsonStringLocalizer yer sahipleri indexers

### Bilinen Sınırlar

- test paralel olarak çalışırken atılır, çünkü birden çok test örneği aynı dosyayı paylaşıyor. izolasyondayken geçer.

## Yeni File Structure

### Hizmetler

- - Boru orkestrası
- Ülke adı çeviri
- – JSON söz senkronizasyonu
- - Markdown çeviri
- - SignalR mesaj yayınlama
- -Yer sahibi maskeleme ile İlişki mantığı
- - Yayıncı arayüzü
- Ülke hizmetleri arayüzü
- - Yerelleştirme hizmeti arayüzü
- - Doküman hizmeti arayüzü
- - Orkestra arayüzü (updated)
- - Per-file çeviri metadata

### Güncelleme Hizmetlerinde

- - adı verilen yer sahibi desteği
- - Yeni parametre için Güncelleme
- - Named placeholder management
- - Placeholder arayüzü

### Yeni Admin Page in In New Admin Page in

- Gerçek zamanlı izleme sayfası
- - Page model

### Yeni Dokümantasyon

- - Güncelleme boru belgeleri
- - Placeholder sistemi rehberi
- – Dashboard kullanımı rehberi
- - Teknik mimarlık genel bakış

## Backward Uyumluluk

Tüm değişiklikler katkıda bulunur:

- Yerelleştirme kodu () değişmeden çalışır
- Pozisyonal formatlama () değişmeden çalışır
- JSON sözlük formatının mevcut olması değişmemiştir
- Markdown yapısı mevcut değildir
- SignalR mesajları aynı formatı kullanır

## Göç Yolu

Hiçbir göç gerekli değildir. Refaksiyon içseldir:

1. Eski bir referans olarak korunmuş ve sonra değiştirildi
2. DI kayıtları yeni arayüzler kullanmak için güncellendi
3. Mevcut tüm tüketiciler hiçbir değişiklik görmüyor

## Performans İyileştirmeleri

- **Redük hafıza kullanımı**: Dosyalar her şeyi hafızada tutmak yerine hemen dil kurtardı
- **Faster artımlı çalışır**: Sadece değişmiş / Markdown blokları yeniden-translated
- **Better görünürlük**: Gerçek zamanlı ilerleme yavaş aşamaları teşhis etmeye yardımcı olur

## Future

Planlanan gelişmeler:

1. **AI fine-tuning** - Post-makin çeviri incelemesi > 5 kelime
2. **Yönetim** - Restrict admin sayfaları yetkili kullanıcılar için
3. **Dictionary editörü** - Web UI yerelleştirme anahtarlarını yönetmek için
4. **Translation istatistiklerini ** - Çeviri sayılarını gösteren grafikler ve zaman içinde hata oranları gösterir
5. **Müşteri sözcülüğü** - Alternatif yer sahipleri formatlarına destek

## İletişim

Çeviri hizmeti ile ilgili sorular veya sorunlar için lütfen her modülün rehberinde ayrıntılı belgelere veya gelişim ekibine ulaşın.
