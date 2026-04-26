# Real-time çeviri

Bu səhifə avtomatik çeviri boru üçün canlı test giriş kimi var.

## Xidmət nədir

Xidmət bir proqram çalışır və çeviri server, konfiqurasiya, və hər hansı bir çeviri işi başlamadan əvvvəl mövcud dillər doğrulamak.

Xüsusi inkişafdan sonra, yalnız ölkələrin siyahısı standart yerlileştirme JSON dictionaries daxildir. İnformasiya default dili İngilis deyil, ölkə giriş əsas eşits dəstək kimi saxlanılır. default dil müxtəlif ise, İngilis ölkə adı ilk default dili çevrilənir və yalnız sonra default sözdə əsas eşitsə dəstək kimi saxlanılır.

Next, xidmət əvvəlki run saxlanılmış snapshot ilə cari default lokalizasiya sözlərini müqayisə edir. Yeni əlavə əlavə əlavə dəstək dillərinə çevrildiyi zaman, əsas mövcud deyil, bu klassik çevirilər əvvvəlliyi saxlayır. Bütün tətbiqli sabit saxlamaq üçün bütün hedef dictionaries silinir.

Sonda, xidmət Markdown ağacları üçün konfrans kökləri tarar. Hər hansı bir mövcudluq məlumatı endirim kimi default dilindən sonra adlanan bir məhsul ehtiva edilir. Xidmət mərkəzi məhsul, dəyişikliklərini aşkar, eksik və ya qeydli hedef Markdown faylları çevirmək və məqsədi faktından sonra cari hash tapmaq var. məhsulun yanında hash yazırsa mümkün deyil, mühüm saxlamaq üçün geri düşür.

## Xidmət göstərdiyi tədbirlər necə

Backend bir mesaj qurmaqdan yerlileştirme hub ilə ümumi SignalR mesaj yaymaq. Hər mesaj bir mesaj növü, cari proses mövzu, bir UTC vaxtamp, bir məhsul, və isteğe bağlı məhsul.

Cari mövzular:

- Daxil ol
- Axtarış
- Kateqoriya
- Axtarış
- Daxil ol

Tipik mesaj axtarış məhsul başladı, məhsul tamamlandı, və boru dəyişdirildi. Bir məhsul başarısız olursa, mesaj bir səhv kimi qeyd edilir və birləşdirilmiş səhv kodu ilə struktur məlumat daxildir.

## Dizayn prinsləri

Translations LibreTranslate server yükləndirilməsi üçün sequentially işləyir.

Localization JSON dictionaries hər zaman daha asan təhlükəsizlik üçün klassik sıralanmış anahtarlar və formatlanmış JSON ilə saxlanılır.

Əvvəlki default sözləşdirici snapshot davamlı olaraq davamlı olaraq istifadə edir.

**Manual komponentlər avtomatik əlavələr üzərində əvvvəllik edir.**
