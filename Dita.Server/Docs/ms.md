# Ringkasan Perubahan pada Dinas Terjemahan Otomatis

## Selayang Pandang

Dokumen ini meringkaskan semua perubahan yang dibuat pada layanan penerjemahan otomatis Dita, termasuk pemfaktoran ulang arsitektur, fitur baru, perbaikan observabilitas, dan peningkatan lokalisasi.

## Perubahan Arsitektur Seni Rupa

### Penerjemahan Ujung Belakang yang Refabel

Monolitik telah terurai menjadi empat layanan khusus yang dikoordinasikan oleh orkestrator ringan:

- **BackendTranslationService** — Pipeline orchestrator (server validation, stage delegation, error handling)
- **CountriesTranslationService** — Country name synchronization (English → target language)
- **LocalizationTranslationService** — JSON dictionary synchronization (added/removed keys)
- **DocumentsTranslationService** — Markdown documentation translation with block-level tracking
- **SignalRPublisher** — Real-time progress reporting via SignalR
- **TranslationRetryService** — Tahap-tahap coba lagi dengan pengawetan placeholder

### Manfaatnya

- **Separation of concerns**: Each service handles a single translation domain
- **Maintainability**: Smaller classes are easier to understand and test
- **Extensibility**: New translation targets can be added via interface implementation
- **Reliability**: Independent services provide better fault isolation

## Fitur - Fitur Baru Keupayaan Baru

### Pemantau Terjemahan Live

**Location**: `/Admin/LiveTranslation`

Halaman admin baru yang menyediakan visibilitas real-time ke dalam pipa terjemahan:

- Freivis menampilkan semua peristiwa SignalR seperti yang terjadi
- Tipe pesan berkode warna (biru=dimulai, hijau=dilengkapi, merah=error)
- Spanduk status sambungan-sendiri dengan koneksi-sendiri
- Penghitungan pesan dan ekspor ke JSON

### Pemegang Tempat yang Dinamakan Dinamakan

Sistem lokalisasi yang sekarang mendukung pemegang tempat yang bernama () untuk tatabahasa yang lebih baik dalam berbagai bahasa:

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
- Nilai placeholder disediakan pada waktu jalan atau disimpan pada
- Pemasok/pencadanganan otomatis secara otomatis selama penerjemahan untuk mencegah korupsi
- Kebelakangan yang kompatibel dengan pemegang tempat kedudukan yang sudah ada

### Terjemahan Tambahan

Berkas Markdown skyd diterjemahkan secara tokokan:

- **Per-language saving**: Each target language is saved immediately after translation, reducing memory pressure
- **Block-level tracking**: `.translation-meta.json` tracks translation status per block
- **Selective retry**: Only failed blocks are re-translated on the next run
- **Metadata persistence**: Translation state survives application restarts

### Logika Coba Lagi yang Dipertingkatkan Logik

Tiga tingkat ketahanan:

1. **HTTP retry** (LibreTranslateService): 5 percobaan dengan eksponensial backoff (1s–5s)
2. **Stage retry** (TranslationRetryService): 3 percobaan tambahan dengan 30s delay
3. **Block retry** (DocumentsTranslationService): Failed Markdown blocks retried on next run

### pengirim sinyal melaporkan

Real-time kemajuan pelaporan untuk semua operasi pipa:

- Setiap panggung menerbitkan peristiwa
- Kemajuan per-bahasa yang diterbitkan sebagai peristiwa
- Peristiwa galat termasuk konteks terperinci (sumber, kode kesalahan, pesan)
- Angka sekuens jaminan pemesanan dalam setiap run

## Perubahan Konfigurasi XAV

### appsettings.json

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

### Layanan Baru Kelayan

Terdaftar dalam :

- /
- `TranslationRetryService`
- /
- /
- /
- /

Hub SignalR dipetakan untuk koneksi klien.

## Pengujian

### Status Uji

- **243/244 tests passing** (1 skipped due to concurrent file access in test environment)
- Liputan uji coba baru tambahan untuk:
  - Fungsi placeholderService
  - Orkestrasi endendTranslationService
  - Pengindeks tempat pemegang tempat JsonStringLocalizer

### Batasan Dikenal

- tes anikel dilewati ketika berjalan dalam paralel karena beberapa contoh tes berbagi file yang sama. Ini berlalu ketika berlari dalam isolasi.

## Struktur Berkas Baru

### Layanan Pelayanan di

- Papeline - orkestrator
- Terjemahan Bahasa Indonesia
- Pensegerakan kamus JSON
- Terjemahan terjemahan Markdown
- Penerbitan pesan SignalR
- Logika ulang dengan topeng pemegang tempat
- Internet Penerbit
- UNESCO
- antarmuka layanan lokalisasi
- Dokumen
- Australia
- Data meta terjemahan Per-berkas —

### Layanan Pemutakhiran Barang pada

- Andel — Dukungan pemegang tempat bernama
- Updated for new parameter
- Nama - nama manajemen pemegang tempat
- Antarmuka Pemegang Tempat

### Halaman Admin Baru di

- Situs pemantauan waktu nyata
- Model Halaman

### Dokumentasi Baru Dokumentasi Baru dalam

- Dokumentasi pipa termutakhir
- Other — Panduan sistem pemegang tempat
- Panduan penggunaan Dashboard —
- Wawasan arsitektur teknis

## Keserasian Keliling Mundur

Semua perubahan adalah aditif:

- Kode lokalisasi yang telah ada () tidak berubah
- Pemformatan posisi () berfungsi tidak berubah
- Format kamus JSON yang ada tidak berubah
- Struktur Markdown yang ada tidak berubah
- Pesan-pesan SignalR isyarat Isyarat Isyarat Isyarat je menggunakan format yang sama

## Path Migrasi

Tidak perlu migrasi. Refacturing adalah internal:

1. Lama dipelihara sebagai referensi dan kemudian diganti
2. Pendaftaran DI telah diperbarui untuk menggunakan antarmuka baru
3. Semua konsumen yang ada tidak melihat perubahan

## Peningkatan Kinerja Kinerja

- **Reduced memory usage**: Files saved per-language immediately instead of holding all in memory
- **Faster incremental runs**: Only changed/failed Markdown blocks are re-translated
- **Better visibility**: Real-time progress helps diagnose slow stages

## Peningkatan Masa Depan

Peningkatan direncanakan:

1. **AI fine-tuning** — Post-machine translation review for phrases > 5 words
2. **Admin authentikasi** — Batasi halaman admin ke pengguna yang berwenang
3. **Dictionary editor** — Web UI for managing localization keys
4. **Translation statistics** — Charts showing translation counts and error rates over time
5. **Custom placeholder syntax** — Support for alternate placeholder formats

## Kenalan

Untuk pertanyaan atau masalah dengan layanan penerjemahan, silakan mengacu pada dokumentasi rinci dalam setiap direktori modul atau menghubungi tim pengembangan.
