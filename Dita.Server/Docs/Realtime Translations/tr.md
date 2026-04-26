# Gerçek zamanlı çeviriler

Bu belge otomatik çeviri hattı için canlı bir test girişi olarak mevcuttur.

## Hizmet ne yapar

Servis bir program üzerinde çalışır ve çeviri sunucuyu, yapılandırmayı ve herhangi bir çeviri çalışması başlamadan önce mevcut diller uygular.

Geçerlilik adımından sonra, ülke isimleri, yalnızca standart yerelleştirme JSON sözlüklerine senkronize eder. Uygulama varsayılan dili İngilizce ise, ülke girişi anahtar eşit değer olarak depolanır. Varsayılan dil farklıysa, İngilizce adı ilk olarak varsayılan dilde tercüme edilir ve ancak o zaman varsayılan sözlükte anahtar eşit olarak depolanır.

Sonraki, hizmet, önceki işten saklanan snapshot ile mevcut varsayılan yerelleştirme sözünü karşılaştırır. Yeni olarak ek girişler yalnızca anahtarın zaten var olmadığı zaman hedef dillere çevrilir, bu nedenle manuel çeviriler öncelik tutar. Kaldırılan girişler, tüm seti tutarlı tutmak için tüm hedef sözlüklerden silinir.

Son olarak, hizmet Markdown ağaçları için yapısal belge köklerini tarar. Her konu klasörün varsayılan dilden sonra adı verilen bir kaynak dosyası olması bekleniyor, örneğin en.md. Servis, bu kaynak dosyasına sahiptir, değişiklikleri tespit eder, eksik veya eski hedef Markdown dosyaları çevirir ve mevcut belge dosyasına bir sonraki mağazaları depolar. Kaynak dosyasına bir sonraki yazı yazmak mümkün değilse, geçici depolamaya geri düşer.

## Servis ilerleme raporları nasıl ilerlemektedir

Geri dönüş, bir mesaj zarfı kullanarak yerelleştirme merkezi aracılığıyla genel SignalR mesajlarını yayıyor. Her mesaj bir mesaj türü taşır, mevcut süreç aşaması, bir UTC zamantamp, bir metin özeti ve opsiyonel aşama özel ödeme yükü.

Mevcut aşamalar şunlardır:

- CheckServers
- çeviriler
- çeviri çevirisi
- çeviri çevirisi
- KillResults

Tipik mesaj akışı başladı, sahne tamamlandı ve boru hattı tamamlandı. Bir aşama başarısız olursa, mesaj bir hata olarak işaretlenir ve birleşik hata kodları ile yapısal hata bilgilerini içerir.

## Tasarım ilkeleri

Çeviriler LibreTranslate sunucusunu aşırı yüklemekten kaçınmak için tutarlı bir şekilde işlenir.

Yerelleştirme JSON sözlükleri her zaman alfabetik olarak sıralanmış anahtarlar ve daha kolay bakım için formatlanmış JSON ile depolanır.

Önceki varsayılan sözlük anlık kalıcı olarak depolanır, böylece uygulamanın yeniden başlaması değişim izlemesini kaybetmez.

**Manual çevirileri her zaman otomatik ekler üzerinde önceliklidir.**
