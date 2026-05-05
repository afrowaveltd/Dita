# Arsitektur Terjemahan Bahasa

Dokumen historiografi ini menggambarkan arsitektur modular sistem penerjemahan otomatis Dita, diperkenalkan untuk meningkatkan kemampuan mempertahankan, kemampuan uji, dan ketahanan.

## Tujuan desain

Refactoring yang ditujukan beberapa kekhawatiran dengan desain monolitik asli:

- **Separation of concerns**: Each translation domain (countries, JSON dictionaries, Markdown) is isolated.
- **Incremental persistence**: Files are saved per-language immediately after translation, reducing memory usage and providing earlier results.
- **Resilience**: Multiple retry levels handle transient failures without blocking the entire pipeline.
- **Observability**: Every significant operation is reported via SignalR for real-time monitoring.
- **Extensibility**: New translation targets can be added by implementing a single interface.

## Dekomposisi layanan miya

### (orchestrator)

**Penerimaan**:
- Manajemen daur hidup jalur pipa (mulai, penyempurnaan, penanganan kesalahan)
- Pengendali konkurensi berbasis Semafor (prevents pertumpang tindih runs)
- Pemvalidasi Server XAWN (latensi, ketersediaan bahasa, konfigurasi)
- Delegasi untuk sub-layanan

**Does NOT contain**:
- Logika terjemahan Terjemahan Baru
- Berkas I/O untuk format tertentu
- Logika Coba Ulang Logika Logika

### Penerjemahan Negara

**Penerimaan**:
- Baca dari direktori
- Mensinkronisasi nama negara ke dalam kamus lokal baku
- Terjemahkan nama negara yang hilang per bahasa target
- Anda telah menyimpan kamus target secara segera setelah terjemahan

** Perilaku kunci**:
- Jika bahasa baku adalah bahasa Inggris: nama negara disimpan as-is
- Jika bahasa baku adalah bahasa lain: Nama bahasa Inggris diterjemahkan ke bahasa baku terlebih dahulu
- Setiap bahasa diproses secara independen dengan retry loop sendiri

### Panduan Translasi Lokalisasi

**Penerimaan**:
- Terdeteksi kunci yang ditambahkan/dibuang dengan membandingkan kamus default saat ini dengan snapshot sebelumnya
- Terjemahkan kunci yang ditambahkan ke dalam setiap bahasa target
- Offoffoff dari setiap bahasa target
- Simpan snapshot untuk perbandingan berikutnya

** Perilaku kunci**:
- Terjemahan-terjemahan Manual '%s' selalu diutamakan (tidak pernah ditulis-ganti)
- Kunci yang ditambahkan diterjemahkan dan disimpan per-bahasa segera
- Kunci dibuang akan dihapus per-bahasa segera
- Snapshot disimpan hanya setelah semua bahasa selesai dengan sukses

### DokumenTranslasiService

**Penerimaan**:
- Akar Markdown terkonfigur secara rekursif
- Kesankan adanya perubahan berkas sumber menggunakan sHA-256 hashes
- Status terjemahan per-blok trek dalam
- Terjemahkan blok-by-block dengan per-blok coba ulang
- Sahkan struktur Markdown setelah terjemahan
- Simpan setiap berkas bahasa target secara independen

** Perilaku kunci**:
- Tingkat granularitas: heading, paragraf, daftar item diterjemahkan secara terpisah
- Data meta trek yang mana blok berhasil/digagalkan per bahasa
- Blok Gagal dicoba ulang pada run berikutnya tanpa memindahkan kembali blok sukses
- Validasi struktur Ukraina memastikan jumlah heading, daftar, blok kode, dll cocok dengan sumber

## Strategi coba lagi

Sistem ini menerapkan retries pada tiga tingkat:

### level 1 — http (layanan bebas)

- Hingga 5 percobaan dengan eksponensial mundur (1s, 2s, 3s, 4s, 5s)
- Kehabisan jaringan handles, kesalahan 5xx, dan kegagalan sementara
- Dibangun dalam konfigurasi klien HTTP

### Tahap 2 — Tahap (Percobaan Translasi)

- Sampai 3 percobaan dengan penundaan 30 detik
- Memacu-kembali seluruh permintaan terjemahan setelah retries tahap HTTP habis
- Penopeng dan pemugaran placeholder diterapkan pada tingkat ini

### Aras 3 — Blok (DokumenTranslationService)

- Blok Markdown Individual Individual yang gagal ditandai dalam metadata
- Retried automatically on the next pipeline run
- Blok-blok sukses tidak pernah diterjemahkan kembali

## Aliran Data

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

### Terjemahan Federasi

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

### Terjemahan bahasa Jerman

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

## Kegigihan Negara

### Snapshots

- **JSON**: Disimpan dalam berkas di sebelah kamus lalai (nama bervariasi oleh penyedia penyimpanan)
- **Purpose**: Enables incremental sync by tracking what was present in the previous run

### Berkas hash taskin

- **Markdown**: `{sourceFile}.hash.json` next to the source file
- **Fallback**: `{tempDir}/{sanitizedPath}.hash.json` if primary location is read-only
- **Purpose**: Detects source changes to avoid unnecessary re-translation

### Terjemahan data meta

- **Markdown**: `{sourceFile}.translation-meta.json`
- **Contents**:
  - Sumber isi hash
- Status blok per-bahasa (array dari boolean)
- Setem waktu update terakhir untuk update
- **Purpose**: Enables partial re-translation of only failed blocks

### Tempat penyimpanan placeholder

- **File**: `Locales/placeholders.json`
- **Contents**: Dictionary of keys to placeholder name-value pairs
- **Purpose**: Provides default values for named placeholders across the application

## Isyarat Melapor

### Abspirasi Penerbit

layanan terjemahan dari SignalR spesifik:

```csharp
public interface ISignalRPublisher
{
    Task PublishStageAsync<T>(Guid runId, ProcessStage stage, T data, ...);
    Task PublishMessageAsync(Guid runId, LocalizationMessageType type, ProcessStage stage, ...);
}
```

### Jaminan Frekuensi

- Pesan-pesan dalam satu larian secara monoton diurut
- Nomor urutan adalah unik per-jalan melalui
- Klien peladen dapat mendeteksi celah atau pemesanan ulang

### Pemetaan Hab

```csharp
app.MapHub<LocalizationHub>("/hubs/localization");
```

## Poin hasil sambungan

### Menambahkan target terjemahan baru

1. Name
2. Implementasi antarmuka dengan logika spesifik domain
3. Daftar dalam wadah DI
4. Suntikkan ke dalam konstruktor
5. Panggilan dari Medis setelah tahap yang ada

### Kebijakan uji ulangan langganan bagi custom

Parameter konstruktor Timpa:

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

### Penanganan placeholder custom

Implementasi untuk mengubah sintaks atau penyimpanan pemegang tempat:

```csharp
public class CustomPlaceholderService : IPlaceholderService
{
    // Use {{name}} syntax instead of {name}
    // Store in database instead of JSON file
    // Encrypt sensitive placeholder values
}
```

## Konfigurasi

### appsettings.json

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

### Lulusan waktu berjalan

Pemadanan
|---------|---------|--------|
Fiji 80
Fiji 10
Fiji 3
Fiji 30

## Strategi pengujian

### Tes unit

Setiap sub-service secara independen dapat diuji:

- Si Mock untuk mensimulasikan sukses/gagal
- Mereka akan melapor
- Gunakan direktori sementara untuk berkas I/O
- Verifikasi perilaku hemat per-bahasa

### Uji integrasi

- Saluran pipa penuh dengan kejadian nyata (lokal) LibreTranslate
- Signal Verifikasi SMTP Pesan-pesan dari surat-surat R dikirim ke klien yang terhubung
- Uji percobaan concurrent jalankan pencegahan (semafor)
- Sahkan struktur Markdown setelah terjemahan

### Uji akhir ke akhir

- Terjemahan Pemicu Feater melalui API atau penjadwal
- Sahkan semua berkas bahasa target diciptakan/updated
- Periksa berkas data meta data biodata mengandung status blok yang benar
- Pemegang tempat yang dikonfirmasi telah dipertahankan di seluruh terjemahan

## Pertimbangan Kinerja

- **Memory**: Per-language saving prevents holding all dictionaries in memory
- **Disk I/O**: Metadata files add small overhead but enable incremental work
- **Network**: Sequential processing with throttling prevents overwhelming LibreTranslate
- **CPU**: SHA-256 hashing and regex validation are fast relative to translation latency
- **SignalR**: Lightweight messages, no payload compression needed for typical reports

## Migrasi dari desain monolitik

Yang asli berisi semua logika dalam satu kelas. Jalur migrasi:

1. Logika negeri →
2. Kemakmuran logika JSON →
3. Ekstrak Logika Markdown →
4. Isyarat Penerbitan →
5. Ekstrak kembali logika →
6. Sederhanakan orkestrator ke delegasi-saja

Semua antarmuka yang ada () tetap tidak berubah. Konsumsi pipa tidak melihat ada perubahan yang melanggar.
