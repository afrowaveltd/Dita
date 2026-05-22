# Ringkasan Perubahan ke Layanan Terjemahan Otomatis

## Tinjau

Dokumen ini merangkum semua perubahan yang dibuat ke layanan terjemahan otomatis Dita, termasuk penekanan arsitektur, fitur baru, perbaikan pengamatan, dan peningkatan lokalisasi.

## Arsitektur Perubahan

### Refactored BackendTranslation Service

Monolitik telah terurai menjadi empat layanan khusus yang dikoordinasikan oleh seorang penggaris ringan:

- **BackendTranslationService** — Pipeline orchestrator (server validation, stage delegation, error handling)
- **CountriesTranslationService** — Country name synchronization (English → target language)
- **LocalizationTranslationService** — JSON dictionary synchronization (added/removed keys)
- **DocumentsTranslationService** — Markdown documentation translation with block-level tracking
- **SignalRPublisher** — Real-time progress reporting via SignalR
- **TranslationRetryService** — Stage-level retry with placeholder preservation

### Keuntungan

- **Separation of concerns**: Each service handles a single translation domain
- **Maintainability**: Smaller classes are easier to understand and test
- **Extensibility**: New translation targets can be added via interface implementation
- **Reliability**: Independent services provide better fault isolation

## Fitur Baru

### Monitor Terjemahan Langsung

**Location**: `/Admin/LiveTranslation`

Halaman admin baru yang menyediakan visibilitas real-time ke dalam pipeline terjemahan:

- Tampilkan semua kejadian SignalR saat mereka terjadi
- Tipe pesan berkode warna (blue = started, green = completed, red = error)
- Banner status koneksi dengan auto-reconnect
- Penghitung pesan dan ekspor ke JSON

### Nama pemegang Placeholder

Sistem lokalisasi kini mendukung placeholder bernama () untuk meningkatkan tata bahasa dalam berbagai bahasa:

```csharp
// Usage in code
var message = Localizer["WelcomeMessage", new Dictionary<string, string>
{
    ["userName"] = "John",
    ["count"] = "5"
}];
// Result: "Hello John, you have 5 new messages"
```

Fitur:
- Nilai placeholder yang diberikan pada waktu-jalan atau disimpan
- Masking / restorasi otomatis selama terjemahan untuk mencegah korupsi
- Backward kompatibel dengan pemegang placeholder posisi yang ada

### Terjemahan Incremental

Berkas Markdown diterjemahkan secara bertahap:

- **Per-language saving**: Each target language is saved immediately after translation, reducing memory pressure
- **Block-level tracking**: `.translation-meta.json` tracks translation status per block
- **Selective retry**: Only failed blocks are re-translated on the next run
- **Metadata persistence**: Translation state survives application restarts

### Logika Retri Diaktifkan

Tiga tingkat ketahanan:

1. **HTTP retry** (LibreTranslateService): 5 attempts with exponential backoff (1s–5s)
2. **Stage retry** (TranslationRetryService): 3 additional attempts with 30s delays
3. **Block retry** (DocumentsTranslationService): Failed Markdown blocks retried on next run

### SignalR Pelaporan

Real- waktu pelaporan untuk semua operasi pipeline:

- Setiap tahap menerbitkan peristiwa
- Kemajuan bahasa yang diterbitkan sebagai peristiwa
- Kejadian galat termasuk konteks rinci (source, error code, message)
- Nomor urutan jaminan pemesanan dalam setiap run

## Perubahan Konfigurasi

### applatings.json

Tidak ada perubahan. Konfigurasi yang ada terus bekerja:

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

### Layanan Baru

Terdaftar dalam:

- /
- `TranslationRetryService`
- /
- /
- /
- /

Pusat Sinyal dipetakan untuk koneksi klien.

## Pengujian

### Status Uji

- **243/244 tests passing** (1 skipped due to concurrent file access in test environment)
- Cakupan tes baru ditambahkan untuk:
  - Fungsi Layanan PlaceholderService
  - Orkestra Layanan Translasi latar
  - JsonStringLocalizer placeholder indexers

### Batas Dikenal

- tes dilewati ketika berjalan dalam paralel karena beberapa kejadian tes berbagi berkas yang sama. Ini berlalu ketika berjalan dalam isolasi.

## Struktur Berkas Baru

### Layanan di

- - Penata pipa
- - Terjemahan nama negara
- - JSON kamus sinkronisasi
- - Terjemahan Markdown
- - Sinyal pesan penerbitan
- - Coba lagi logika dengan placeholder masking
- - Antar muka penerbit
- - Antar muka layanan negara
- - Antarmuka layanan Lokalisasi
- - Antarmuka layanan dokumen
- - Antarmuka orchestrator (diperbarui)
- - Metadata terjemahan per- file

### Layanan Diperbarui di

- - Ditambah dukungan placeholder bernama
- - Diperbarui untuk parameter baru
- - Namanya manajemen placeholder
- - Antarmuka placeholder

### Halaman Admin Baru di

- - Real- waktu halaman monitor
- - Model halaman

### Dokumentasi Baru di

- - Dokumentasi pipa terupdate
- - Pemandu sistem placeholder
- - Pemandu penggunaan Dashboard
- - Tampilan arsitektur teknis

## Kompatibilitas Mundur

Semua perubahan additif:

- Cara kerja lokalisasi () tidak berubah
- Pemformatan posisi () bekerja tidak berubah
- Format kamus JSON yang ada tidak berubah
- Struktur Markdown yang ada tidak berubah
- Pesan SignalR memakai format yang sama

## Path Migrasi

Tidak diperlukan migrasi. Refaktoring adalah internal:

1. Lama diawetkan sebagai referensi dan kemudian diganti
2. Registrasi DI diperbarui untuk menggunakan antarmuka baru
3. Semua konsumen yang ada melihat tidak ada perubahan

## Peningkatan Penampilan

- **Reduced memory usage**: Files saved per-language immediately instead of holding all in memory
- **Faster incremental runs**: Only changed/failed Markdown blocks are re-translated
- **Better visibility**: Real-time progress helps diagnose slow stages

## Keberhasilan Masa Depan

Perbaikan yang direncanakan:

1. **AI fine-tuning** — Post-machine translation review for phrases > 5 words
2. **Admin authentication** — Restrict admin pages to authorized users
3. **Dictionary editor** — Web UI for managing localization keys
4. **Translation statistics** — Charts showing translation counts and error rates over time
5. **Custom placeholder syntax** — Support for alternate placeholder formats

## Kontak

Untuk pertanyaan atau masalah dengan layanan terjemahan, silakan merujuk ke dokumentasi rinci dalam setiap direktori modul atau kontak tim pengembangan.
