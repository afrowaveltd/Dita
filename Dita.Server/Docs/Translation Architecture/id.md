# Arsitektur Terjemahan

Dokumen ini menggambarkan arsitektur modular sistem terjemahan otomatis Dita, yang diperkenalkan untuk meningkatkan daya tahan, stabilitas, dan ketahanan.

## Mendesain tujuan

Refactoring ditujukan beberapa kekhawatiran dengan desain monolitik asli:

- **Separation of concerns**: Each translation domain (countries, JSON dictionaries, Markdown) is isolated.
- **Incremental persistence**: Files are saved per-language immediately after translation, reducing memory usage and providing earlier results.
- **Resilience**: Multiple retry levels handle transient failures without blocking the entire pipeline.
- **Observability**: Every significant operation is reported via SignalR for real-time monitoring.
- **Extensibility**: New translation targets can be added by implementing a single interface.

## Dekomposisi layanan

### Layanan Pembalikan Terjemahan (penggaris)

**Responsibilities**:
- Manajemen lifecycle pipa (awal, pelengkapan, penanganan kesalahan)
- Semaphore- berbasis kontrol konkusi (mencegah tumpang tindih berjalan)
- Validasi server (latensi, ketersediaan bahasa, konfigurasi)
- Delegasi ke layanan sub-

**Does NOT contain**:
- Logika terjemahan
- Berkas I / O untuk format spesifik
- Coba lagi logika

### Layanan Pembagian

**Responsibilities**:
- Baca dari direktori
- Selaraskan nama negara ke dalam kamus lokal baku
- Terjemahkan nama negara yang hilang per bahasa target
- Simpan setiap kamus target segera setelah terjemahan

**Key behaviors**:
- Jika bahasa baku adalah bahasa Inggris: nama negara disimpan as- adalah
- Jika bahasa baku lain: Nama bahasa Inggris diterjemahkan ke bahasa baku pertama
- Setiap bahasa diproses secara independen dengan loop coba ulang sendiri

### Layanan Translasi Localization

**Responsibilities**:
- Deteksi tombol ditambahkan / dihapus dengan membandingkan kamus baku kini dengan snapshot sebelumnya
- Terjemahkan kunci ke dalam setiap bahasa target
- Hapus kunci yang dihapus dari setiap bahasa target
- Simpan snapshot untuk perbandingan berikutnya

**Key behaviors**:
- Terjemahan manual selalu mengambil prioritas (tidak pernah ditimpa)
- Tombol ditambahkan diterjemahkan dan disimpan per- bahasa segera
- Tombol yang dibuang segera dihapus ke bahasa yang lain
- Snapshot hanya disimpan setelah semua bahasa selesai dengan sukses

### Layanan Terjemahan Dokuments

**Responsibilities**:
- Berjalan akar Markdown yang dikonfigurasi rekursif
- Mendeteksi berkas sumber yang diubah memakai hashes SHA-256
- Lacak status terjemahan perblok dalam
- Terjemahkan block- by- blok dengan retry per- blok
- Validasi struktur Markdown setelah terjemahan
- Simpan setiap berkas target bahasa secara independen

**Key behaviors**:
- Banci tingkat granularitas: heading, paragraf, daftar item diterjemahkan secara terpisah
- Trek metadata yang blok berhasil / gagal per bahasa
- Blocks gagal dicoba ulang pada run berikutnya tanpa menerjemahkan blok sukses
- Validasi struktur memastikan jumlah pos, daftar, blok kode, etc. cocok sumber

## Coba lagi strategi

Sistem menerapkan ulang pada tiga tingkat:

### Tingkat 1 - HTTP (Layanan LibreTranslator)

- Hingga 5 percobaan dengan latar belakang eksponensial (1s, 2s, 3s, 4s, 5)
- Menangani timeout jaringan, 5xx error, dan kegagalan transien
- Dibangun ke konfigurasi klien HTTP

### Level 2 - Stage (TranslationRetryService)

- Sampai 3 kali percobaan dengan penundaan 30 detik
- Didorong ulang seluruh permintaan terjemahan setelah nilai ulang HTTP habis
- Placeholder masking dan restorasi diterapkan pada tingkat ini

### Tingkat 3 - Blok (DokumentsTranslasi Service)

- Masing-masing blok Markdown yang gagal ditandai dalam metadata
- Dicoba secara otomatis pada baris pipa berikutnya
- Blok sukses tidak pernah diterjemahkan kembali

## Aliran data

### Terjemahan kamus JSON

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

### Terjemahan Markdown

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

### Terjemahan nama negara

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

## Kegigihan keadaan

### Snapshots

- **JSON**: Stored in a file next to the default dictionary (name varies by storage provider)
- **Purpose**: Enables incremental sync by tracking what was present in the previous run

### Berkas Hash

- **Markdown**: `{sourceFile}.hash.json` next to the source file
- **Fallback**: `{tempDir}/{sanitizedPath}.hash.json` if primary location is read-only
- **Purpose**: Detects source changes to avoid unnecessary re-translation

### Metadata terjemahan

- **Markdown**: `{sourceFile}.translation-meta.json`
- **Contents**:
  - Hash isi sumber
- Status blok per- bahasa (array dari bool)
- Penanda waktu update terakhir
- **Purpose**: Enables partial re-translation of only failed blocks

### Penyimpanan pemegang posisi

- **File**: `Locales/placeholders.json`
- **Contents**: Dictionary of keys to placeholder name-value pairs
- **Purpose**: Provides default values for named placeholders across the application

## Sinyal pelaporan

### Penerbit abstrak

membatalkan layanan terjemahan dari SigngalR spesifik:

```csharp
public interface ISignalRPublisher
{
    Task PublishStageAsync<T>(Guid runId, ProcessStage stage, T data, ...);
    Task PublishMessageAsync(Guid runId, LocalizationMessageType type, ProcessStage stage, ...);
}
```

### Jaminan urutan

- Pesan dalam satu run diurutkan secara monoton
- Nomor seperi adalah unik per- run melalui
- Klien dapat mendeteksi kesenjangan atau pemesanan ulang

### Pemetaan Hub

```csharp
app.MapHub<LocalizationHub>("/hubs/localization");
```

## Titik ekstensi

### Menambahkan sebuah target terjemahan baru

1. Buat antar muka baru dengan
2. Implikasi antarmuka dengan logika domain- spesifik
3. Register dalam DI kontainer
4. Inject ke konstruktor
5. Panggilan dari setelah tahap yang ada

### Kebijakan coba ulang gubahan

Timpa parameter konstruktor:

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

### Menangani placeholder gubahan

Implikasi untuk mengubah placeholder sintaks atau penyimpanan:

```csharp
public class CustomPlaceholderService : IPlaceholderService
{
    // Use {{name}} syntax instead of {name}
    // Store in database instead of JSON file
    // Encrypt sensitive placeholder values
}
```

## Konfigurasi

### applatings.json

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

### Tuning runtime

Tatanan
|---------|---------|--------|
80
10
3
30

## Strategi pengujian

### Tes unit

Setiap layanan sub- independen diuji:

- Mock untuk mensimulasikan sukses / gagal
- Mock untuk memverifikasi pelaporan
- Gunakan direktori sementara untuk berkas I / O
- Verifikasi perilaku penyimpanan bahasa

### Tes integrasi

- Jalankan pipa penuh dengan instansi LibreTranslate asli (lokal)
- Verifikasi pesan SignalR dikirim ke klien yang terhubung
- Uji pencegahan run concurrent (semaphore)
- Validasi struktur Markdown setelah terjemahan

### End-to-end test

- Translasi pemicu melalui API atau penjadwalan
- Verifikasi seluruh berkas target bahasa dibuat / diperbarui
- Periksa berkas metadata berisi status blok yang benar
- Konfirmasi placeholder diawetkan di seluruh terjemahan

## Performance consiations

- **Memory**: Per-language saving prevents holding all dictionaries in memory
- **Disk I/O**: Metadata files add small overhead but enable incremental work
- **Network**: Sequential processing with throttling prevents overwhelming LibreTranslate
- **CPU**: SHA-256 hashing and regex validation are fast relative to translation latency
- **SignalR**: Lightweight messages, no payload compression needed for typical reports

## Migrasi dari desain monolitik

Asli berisi semua logika dalam satu kelas. Jalur migrasi:

1. Ekstrak logika negara
2. Ekstrak logika JSON 1f
3. Ekstrak logika Markdown Az
4. Ekstrak IgnalR penerbitan
5. Ekstraksi coba ulang logika 1f
6. Sederhanakan antristrator ke delegation-saja

Semua antarmuka yang ada tetap tidak berubah. Konsumen dari baris pipa tidak melihat perubahan yang putus.
