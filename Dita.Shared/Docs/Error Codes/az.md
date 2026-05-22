# Yadda saxla

Dita bir **range-partitioned, birləşdirilmiş məlumat kodu mimarisini istifadə edir** ki, öz domen xüsusi enumlar və bir tutma-all növü verir. Sistemdə hər hansı bir məlumat - konfiqurasiyadan ibarətdən disk I/O-ya şəbəkələrdən - bu qiymətçinin üzvüdür.

## Architecture

### Axtarış

Axtarış
|-------|----------|----------|
1000-1999
2000–2999
3000–3999
AZ1000
5000–5999
6000-6999
AZ1000
AZ1000
AZ1000

### Dual-enum model

Each error domain is represented by **both** a focused sub-enum (e.g. `NetworkError`) and entries in the unified `ErrorCode` enum. The sub-enums use bare names; the unified enum prefixes names with the category:

```csharp
// Sub-enum — domain-specific context
NetworkError.ConnectionRefused    // 1003

// Unified enum — catch-all reference
ErrorCode.NetworkConnectionRefused  // 1003 — same numeric value
```

Bu, səhifənin tanınır olduğu domen xüsusi növləri ilə işləyə imkan verir, həmçinin bütün kompüterlərində işləyən ümumi məsuliyyət dəstəkləyir.

### sent

Hər bir sub-enum onun sıra baz dəyəri kimi (e.g. ). Metod bunu tanıyır və geri verir.

## Yadda saxla

Enum bütün sub-enum qiymətlərini **non-overlapping** tam sayı ilə bir növ dəstəkləyir. yoldaş statik sinif insanlaşdırma təklif edir:

```csharp
// Get human-readable text for any error code
string text = ErrorCodeText.ErrorText(ErrorCode.NetworkConnectionRefused);
// → "Network connection refused"

string text2 = ErrorCodeText.ErrorText(5005);
// → "Localization invalid translation format"
```

### İnsanlaşdırma məhsulları

konfrans-over-configuration tətbiq:

1. PascalCase adları regex ilə sözlərdə bölünür
2. Domen adı qeydiyyatdan keçirt »
3. Bütün kapsullar (e.g.) qeyd olunur
4. Valyuta son

## Domen adı qeydiyyatdan keçir

### AğError (1000-1999)

DNS, SSL/TLS, prinsler, ağacaqlar, HTTP protokol səhvələri, bağlantı və həyahət problemləri istəyir.

Seçki
|---|---|
Bakı
1001
1002
1003
1004
1005
1006
1007
1008
1009
1010
1019
1020
1021

### Yadda saxla

Cover s s . . . . . . . . . . . . . . . . . . . . . . . . . .

Seçki
|---|---|
2000
Bakı
2004
2007
2010
2012
2013
2018
Bakı
2029

### proqram (3000–3999)

Aşağı səviyyəli fiziki disk və sürücü hataları: pis sektorlar, SMART başarısızlıkları, RAID deyilməsi, bölmə masaları, hardware uğursuzluqlar, avadanlıqlar, avadanlıqlar, format və eject əməliyyatları.

Seçki
|---|---|
Bakı
3001
3010
3012
3021
3027
3032

### Domen adı qeydiyyatdan keçirt

Cover , , . : : : : : : : : : : per per per per per per per per per per per per per per per per.

Seçki
|---|---|
Bakı
4001
4013
4011
4023
4024
4028

### YerlileştirmeError (5000–5999)

Yerlileştirme borusunun xüsusi faylları: dictionaries, encoding, yerli validation, ümumi formaları, xarici çeviri APIs (auth, mövcudluğu, sıra, vaxt), və string formatting.

Seçki
|---|---|
Bakı
5001
5007
5014
5015
5016
50

### SeçkiError (6000–6999)

Əməliyyat və idarəetmə: təhlükəsizlik, hesablar (refresh/access), seanslar, MFA/2FA, biometrik, sertifikatlar, OAuth, SSO, və hesab dövlətlər (vəvvəlli, qarşılı, qarşılanmış).

Seçki
|---|---|
6000
600
204
6015
6024
6026

### QeydiyyatError (7000–7999)

Girişin təhlükəsizliyi: format checks (email, telefon, URL, JSON, XML, tarixi, sıra / uzunluqlar, dönüşüm uğursuzluqları, tələb olunan sahələr, model/regex və şəkil kompleksi.

Seçki
|---|---|
Bakı
Qeydiyyat
70
70

### KonfransError (8000–8999)

Çap konfiqurasiyası və parametrləri: fayl giriş, parsing, validation, sırlar/key torpaq, DI, xüsusiyyət rəsmi, DI, xüsusiyyət bayrağı, mühit dəyişiklikləri, və yaxşılıqları.

Seçki
|---|---|
Bakı
Bakı
8016
80

### general (9000–9999)

Proqramlar üçün Catch-all: yaxşılıq, koncurrency, lisenziya, dərman, səhifə, səhifə idarəetmə, xüsusi dəstək və qeyd istisnalar.

Seçki
|---|---|
Axtarış
9004
9007
9015
9014

## Borular

### Proqramlar

Avtomatik çeviri borusunun sequential məhsulları:

Qeydiyyat
|-------|------|-------------|
Qeydiyyat
1
2
3
4
5

### Avtomatik

Boru boru tərəfindən yayılan real-time mesaj açıq:

Qeydiyyat
|-------|------|---------|
Qeydiyyat
1
2
3
4
5
6

### Translation Qeydiyyat

Əməliyyat nömrəsini çevirmək üçün məlumatlaşdırır:

Qeydiyyat
|-------|------|---------------|
Qeydiyyat
1
2

### phrase

Yerlileştirme sözləri girişlər üçün CRUD-like dəyişiklik dövrü:

Qeydiyyat
|-------|------|
Qeydiyyat
1
2
3

### Comparison

Dövlət xidməti üçün istifadə olunan müəyyən operatorlar:

Qeydiyyat
|-------|------|----------|
Qeydiyyat
1
2
3
4
5
6

### Qadın

Yerlileştirme üçün Grammatik / Sosial cinsi:

Qeydiyyat
|-------|------|
Qeydiyyat
1
2
3

## Hata kodları istifadə edin

### Boru maşınları

Əməliyyat faylları rekordlarda keçirilir:

```csharp
public class TranslationError
{
    public string Source { get; set; }        // Language code, file path, or stage name
    public ErrorCode Code { get; set; }       // Unified error code
    public string ErrorMessage { get; set; }  // Human-readable text
}
```

### API cavabları

```csharp
var result = Response<TranslateResult>.Fail(
    ErrorCodeText.ErrorText(ErrorCode.NetworkConnectionRefused),
    ErrorCode.NetworkConnectionRefused);
```

### Heç bir kod

```csharp
// From enum value
string text = ErrorCodeText.ErrorText(ErrorCode.StorageDeadlockDetected);
// → "Storage deadlock detected"

// From raw integer (validates against defined values)
string text2 = ErrorCodeText.ErrorText(2010);
// → "Storage deadlock detected"

// Undefined code
string text3 = ErrorCodeText.ErrorText(99999);
// → "Unknown error (99999)"
```
