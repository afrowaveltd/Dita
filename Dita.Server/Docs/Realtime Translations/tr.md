# Gerçek zamanlı çeviriler

Bu belge otomatik çeviri hattı için canlı bir test girişi olarak mevcuttur. Bu dosyaya herhangi bir değişiklik, bir sonraki planlanan çalıştırdaki tüm hedef dil dosyalarının yeniden tanımlanmasını tetikliyor.

## Mimari genel bakış

Çeviri hattı hafif bir orkestra tarafından koordine edilen dört özel alt hizmetle modüler bir mimariye yeniden yapılandırılmıştır:

- **BackendTranslationService** - Tüm boru hattını orkestralar, sunucuyu geçerli kılar ve delegeler alt hizmetlere çalışır.
- **ÜlkelerTranslationService** - Ülke isimlerinin per-dil sözlüklerine uydurulmuş.
- **LocalizationTranslationService** - varsayılan JSON sözlüğünde eklenen anahtarları ve bunları hedef dillere çevirmektedir.
- **DocumentsTranslationService** - Çeviriler Markdown doküman dosyaları per-block takip ve metadata ile.

Her alt hizmet, gerçek zamanlı olarak SignalR aracılığıyla bağımsız olarak çalışır ve rapor eder.

## Hizmet ne yapar

Servis bir program üzerinde çalışır ve beş aşamalı bir boru hattı uygular: sunucu doğrulama, ülke senkronizasyonu, JSON söz senkronizasyonu, Markdown dosya çevirisi ve sonuçları devam eder. Her aşama, Signal üzerinde yapısal gerçek zamanlı ilerleme olayları yayıyor R böylece bağlantılı müşteriler iş ilerledikçe takip edebilir.

## Boru aşamaları

### Aşama 1 - CheckServers

Herhangi bir çeviri çalışması başlamadan önce, hizmet tüm ön koşulların memnun olduğunu belirtir:

- Yapılandırma bölümü mevcut ve geçerli olmalıdır.
- LibreTranslate sunucusu kabul edilebilir bir gecikme içinde cevap vermelidir.
- Çeviri sunucusunda mevcut dillerin listesi getirildi.
- Yapılandırılmış varsayılan dil bu listede mevcut olmalıdır.
- Herhangi bir desteklenen dil için yerel JSON dosyaları otomatik olarak oluşturulur.

Herhangi bir kontrol başarısız olursa, boru hattı hemen durur ve bir mesaj yayılıyor.

### 2. Aşama - Translateries

Ülke isimleri, sadece bir katalogdan () yerelleşme JSON sözlüklerine senkronize edilir.

- Uygulama varsayılan dili İngilizce ise, her ülke adı çeviri olmadan depolanır.
- Varsayılan dil başka bir dilse, İngilizce adı bu dilde ilk tercüme edilir ve sonuç varsayılan sözlüğe giriş haline gelir.
- Varsayılan sözlük güncellendikten sonra, her hedef dilde eksik olan ülke girişi tercüme edilir ve kurtarılır ** Dilim için geçerlidir**.
- Zaten tamamlanmış girişler değişiklik olmadan korunmuştur.
- Bir çeviri başarısız olursa, hizmet bir sonraki dile gitmeden önce 30 saniyelik gecikmelerle 3 kata kadar geri döner.

### Aşama 3 - TranslateJsonFiles

Servis, önceki işten saklanan bir snapshot ile mevcut varsayılan yerelleştirme sözlüklerini karşılaştırır:

- **Eklenen anahtarlar** - mevcut varsayılan girişlerde mevcut olan girişler - zaten bu anahtar için bir el girişi olmayan her hedef diline tercüme edilir.
- **Removed keys ** - anlık kayıtlarda mevcut olan girişler ancak mevcut varsayılandan yoksundur - her hedef dil sözlüğünden silinir.
- Kılavuz çevirileri her zaman öncelik alır. Bir hedef sözlüğü zaten bir anahtar için bir değer içeriyorsa, bu giriş kaynağın ne söylediğine bakılmaksızın değişmemiştir.
- **Each target language dictionary is saved immediately after its translations complete**, rather than waiting for all languages to finish.
- Bir çeviri belirli bir dil için başarısız olursa, hizmet otomatik olarak yeniden yapılır. Sadece kalıcı hatalar (örneğin, desteklenmeyen dil) bu dilin atılmasına neden olur.
- Rundan sonra, mevcut varsayılan sözlük bir sonraki karşılaştırma için yeni bir anlık olarak kaydedilir.

Tüm sözlükler her zaman alfabetik olarak sıralanmış anahtarlarla depolanır ve insan okunabilirliği için JSON'u terk eder.

### Aşama 4 - TranslateMarkdownFiles

Servis, yapılandırılmış belge köklerine (default: ) ve her kaynak dosyası yeniden kullanılabilir:

1. Kaynak dosyası içeriği okunur ve bir SHA-256 hash hesaplanır.
2. Kaynak parçalarına bir sonraki bir dosya, per-block çeviri durumu, **incremental re-translation ** sadece başarısız bloklar.
3. Daha önceki rundan gelen depo ( kaynak dosyasına bir dosyada veya geçici bir geri çekilme yerinde) mevcut hash ile karşılaştırılır.
4. Her hedef dili için, ilgili dosya da yapısal bütünlüğü kontrol edilir.
5. Eksik olan herhangi bir hedef dosyası, eski bir hash, yapı geçerliliği yok, ya da devre dışı bloklar yeniden geçiş için sıralanır.
6. **Each target language is translated and saved independently** — if Czech succeeds but French fails, the Czech file is still written to disk.
7. Başarılı bir şekilde tercüme edilen dosyalar kaynakla yapısal parite için geçerlidir (önemli başlıklar, liste öğeleri, kod blokları, alıntılar, bağlantılar, cesur/italic belirteçler ve HTML etiketleri) diske yazılmalıdır.
8. Bir kaynak için tüm hedef dosyaları başarılı olursa, yeni hash kaynağın yanında depolanır. Eğer kaynağa bir sonraki yazı başarısız olursa (örneğin okuma-sadece dağıtımlarda), hash geçici diziye geri döner.
9. Herhangi bir hedef çevirisi geçerli değilse, metadata, bu blokları bir sonraki çalıştırda yeniden besleniyorlar.

### Aşama 5 – StoringResults

Bir konsolidasyon oluşturuldu ve yayınlandı. içerir:

- UTC başlar ve zamanları tamamlar.
- Yerel JSON dosyalarını kurtarın, Markdown dosyaları kurtarın, kaydedilen hash dosyaları ve fallback yazıyor.
- Operasyon sırasında toplanan herhangi bir depolama hatası.
- Dil çevirisi istatistikleri (translated count, atladı sayı, hata say).

## Signal Signal R message zarf

Her ilerleme olayı aşağıdaki alanlardan biri olarak teslim edilir:

alan alanı
|-------|------|-------------|
Mevcut boru hattı için korelasyon tanımlayıcısı
Monotonic sayacı bir run içinde, 1'de başlıyor
Semantic mesajının türü
Boru aşaması mesajı, mesajın ait olduğu
UTC time when the message was spread
Mesaj bir hata koşulu temsil ederse
i̇nsan hazırlanabilir özet
Aşamaya özel ödeme yükü (report object or null)

### Mesaj türleri

Değer değeri
|-------|------|---------|
0
1
2
3
4
5
6

### Boru aşamaları

Değer değeri
|-------|------|-------------|
0
1
2
3
4
5

### Tipik mesaj akışı akışı

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

Herhangi bir aşama başarısız olursa, kalan aşamalar atılır, bir mesaj yayılıyor ve sonunda bir mesaj koşuyor.

## Tercüme mantığı

Boru hattı iki dayanıklılık seviyesini uygular:

### Aşama seviyesi retry (TranslationRetry)

- Bir çeviri isteği LibreTranslate'nin iç retries'ten sonra başarısız olursa, 30 saniyelik gecikmelerle 3 ek aşama seviyesindeki retries'e kadar performanslar.
- Placeholder maskeleme: İsimli yer sahipleri () metinde geçici olarak daha sonra çeviri ve restore etmeden önce güvenli jetonlarla değiştirilir ve hedef dilde doğru gramer sağlar.

### Dil doğrulama

- Hedef bir dili tercüme etmeden önce, servis dili çeviri sunucusu tarafından destekleniyor.
- Desteklenen diller bir uyarı ile atılır, tekrarlanan başarısız girişimleri önlemek.

### Markdown blok seviyesi retry

- Markdown çevirileri blok-by-block (headings, paragraflar, liste öğeleri).
- Bireysel bir blok çevirisi başarısız olursa, metadata dosyasında yayınlanmamış ve bir sonraki boru hattında tekrar tekrarlanan olarak işaretlenir.
- Servis, her kaynak Markdown dosyasına bir sonraki dosyalarda per-block statüsü izler.

## Hata kodları

Hatalar, birleştirilmiş enum grubu kullanarak aralıklara bildirilir:

Range
|-------|----------|
1000–1999
2000-2999
3000-3999
4000-4999
5000-5999

Bir rapordaki her hata kaynağı tanımlayıcısı (dil kodu, dosya yolu veya sahne adı), hata kodu ve insan hazır bir mesaj taşır.

## Canlı Çeviri Dashboard

Server projesi, SignalR merkezine bağlanır ve gerçek zamanlı tüm boru hatları olayları gösterir.

- Bağlantı durumu, mesaj sayımı ve tüm olayların canlı sıralama masası.
- Renk kodlanmış satırlar: sahne için mavi, tamamlanmak için yeşil, hatalar için kırmızı.
- Tüm mesajları JSON'a dağıtmayı ve ihraç etmeyi destekler.
- Auto-re bağlantı azalırsa üstel arka ile bağlantı kurar.

## Tasarım ilkeleri

- **Modularity**: Her çeviri endişesi, kullanılabilirlik ve test edilebilirlik için kendi hizmetinde izole edilmiştir.
- **Incremental Continuence**: Dictionaries ve Markdown dosyaları hemen çeviriden sonra, hafıza baskısını azaltır ve daha önce geri bildirim sağlar.
- **Resilience **: Çoklu yeniden deneme seviyeleri (HTTP, sahne, blok) geçici başarısızlıkların boru hattını engellememesini sağlar.
- **State follow**: Per-file metadata () ve hash dosyaları, daha sonraki işlemlerde kesin bir artış çalışması sağlar.
- ** Gerçek zamanlı görünürlük**: İzleme ve debugging için SignalR aracılığıyla her önemli operasyon rapor edilir.
- **Manual translations always have priority over automatic additions.**
