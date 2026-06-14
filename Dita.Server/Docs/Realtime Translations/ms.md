# Terjemahan waktu-nyata

Dokumen ini ada sebagai input tes langsung untuk pipa penerjemahan otomatis. Setiap perubahan ke file ini memicu translasi ulang semua target file bahasa pada jadwal berikutnya berjalan.

## Ringkasan arkeologi Arsitektur

Jalur pipa penerjemahan telah direstrukturisasi menjadi arsitektur modular dengan empat sub-layanan khusus yang dikoordinasikan oleh orkestrator ringan:

- **BackendTranslationService** — Orchestrates the entire pipeline, handles server validation, and delegates work to sub-services.
- **CountriesTranslationService** — Synchronizes country names from `countries.json` into per-language dictionaries.
- **LocalizationTranslationService** — Detects added/removed keys in the default JSON dictionary and translates them into target languages.
- **DocumentsTranslationService** — Translates Markdown documentation files with per-block tracking and metadata.

Setiap sub-service beroperasi secara independen dan melaporkan kemajuan melalui SignalR secara real time.

## Apa yang pelayanan lakukan

Layanan tersebut berjalan pada jadwal dan menjalankan jalur pipa lima tahap: validasi server, sinkronisasi negara, sinkronisasi kamus JSON, terjemahan berkas Markdown, dan melanjutkan hasilnya. Tahap masing-masing memancarkan peristiwa perkembangan real-time yang terstruktur di atas SignalR sehingga klien yang terhubung dapat mengikuti seiring berjalannya waktu kerja.

## Tahap pipa

### Tahap 1 — Pelayan Cek

Sebelum pekerjaan penerjemahan dimulai, pelayanan membuktikan bahwa semua prekondisi sudah puas:

- Seksi konfigurasi harus ada dan berlaku.
- Server LibreTranslate harus merespon dalam latensi yang dapat diterima.
- Daftar bahasa yang tersedia di server terjemahan akan diambil.
- Bahasa baku yang telah dikonfigurasi harus ada dalam daftar tersebut.
- Berkas JSON lokal hilang untuk setiap bahasa yang didukung dibuat secara otomatis.

Jika pemeriksaan gagal, saluran pipa segera berhenti dan pesan dipancarkan.

### Tahap 2 — Terjemahan

Nama-nama Negara older disimpan selaras dari katalog baca-saja () ke dalam kamus JSON lokalisasi.

- Jika bahasa baku aplikasi adalah bahasa Inggris, setiap nama negara disimpan seperti tanpa terjemahan.
- Jika bahasa baku adalah bahasa lain, nama negara Inggris pertama kali diterjemahkan ke dalam bahasa tersebut, dan hasilnya menjadi entri dalam kamus baku.
- After the default dictionary is updated, each missing country entry in every target language dictionary is translated and saved **immediately per language**.
- Entri yang sudah diterjemahkan dilestarikan tanpa modifikasi.
- Jika sebuah terjemahan gagal, layanannya akan berulang hingga 3 kali dengan penundaan 30 detik sebelum pindah ke bahasa berikutnya.

### Tahap 3 — TerjemahanJsonFiles

Layanan gladien membandingkan kamus lokalisasi baku saat ini dengan snapshot yang disimpan dari run sebelumnya:

- **Added keys** — entries present in the current default but absent from the snapshot — are translated into every target language that does not already have a manual entry for that key.
- **Removed keys** — entries present in the snapshot but absent from the current default — are deleted from every target language dictionary.
- Terjemahan-terjemahan Manual yang selalu diutamakan. Jika kamus target sudah berisi nilai untuk kunci, entri tersebut dibiarkan tidak berubah terlepas dari apa yang dikatakan oleh sumber.
- **Each kamus bahasa sasaran disimpan segera setelah terjemahannya selesai**, daripada menunggu semua bahasa selesai.
- Jika terjemahan gagal untuk bahasa tertentu, layanannya akan kembali secara otomatis. Hanya kesalahan yang persisten (misalnya, bahasa yang tidak didukung) yang menyebabkan bahasa tersebut dilewatkan.
- Setelah dijalankan, kamus baku saat ini disimpan sebagai snapshot baru untuk perbandingan berikutnya.

Semua kamus (kamus) selalu disimpan dengan kunci diurutkan berdasarkan abjad dan diindented JSON untuk kemampuan baca manusia.

### Tahap 4 — Terjemahan

Layanan palagon berjalan akar dokumentasi terkonfigur (lalai: ) dan proses setiap berkas sumber secara rekursif:

1. Isi berkas sumber dibaca dan hash SHA-256 dihitung.
2. A `.translation-meta.json` file next to the source tracks per-language, per-block translation status, enabling **incremental re-translation** of only failed blocks.
3. BAHih yang disimpan dari run sebelumnya (kept dalam file di samping file sumber, atau di lokasi fallback sementara) dibandingkan dengan hash saat ini.
4. Untuk setiap bahasa sasaran, berkas yang bersangkutan juga diperiksa untuk integritas struktural.
5. Berkas target apapun yang hilang, memiliki hash yang ketinggalan zaman, validasi struktur yang gagal, atau berisi blok yang tidak diterjemahkan dibaris gilir untuk re-translasi.
6. **Each target language is translated and saved independently** — if Czech succeeds but French fails, the Czech file is still written to disk.
7. Berkas-berkas yang diterjemahkan secara sukses berhasil divalidasi untuk paritas struktural dengan sumber (hitungan heading sama, daftar item, blok kode, blokquotes, link, penanda tebal/italik, dan tag HTML) sebelum ditulis ke disk.
8. Jika semua file target untuk sumber berhasil, hash baru disimpan di sebelah sumber. Jika penulisan di samping sumber gagal (misalnya dalam penyebaran baca-saja), hash jatuh kembali ke direktori sementara.
9. Jika terjemahan target gagal validasi, metadata menandai blok-blok tersebut sebagai tidak diterjemahkan sehingga mereka dicoba ulang pada lari berikutnya.

### Tahap 5 — Mempertahankan Kembali

Sebuah konsolidasi dikumpulkan dan diterbitkan. Termasuk:

- UTC UTC memulai dan menyelesaikan timestamp.
- Kiraan jumlah dari file JSON lokal yang disimpan, file Markdown disimpan, file hash disimpan, dan fallback hash menulis.
- Kesalahan penyimpanan apapun yang dikumpulkan selama pelarian.
- Statistik penerjemahan bahasa-Peran (hitungan diterjemahkan, jumlah dilewatkan, jumlah kesalahan).

## Amplop pesan SignalR Serah Isyarat

Setiap peristiwa kemajuan disampaikan sebagai sebuah dengan bidang berikut:

Sibuk
|-------|------|-------------|
Pengenal korelasi untuk jalur pipa saat ini
Monotonik monotonik counter dalam menjalankan, mulai dari 1
Tipe pesan yang sederhana
Tahap pipa pipa pesan milik
Waktu UTC ketika pesan dipancarkan
Apakah isi pesan mewakili kondisi kesalahan
Ringkasan dapat dibaca-manusia
Muatan spesifik Tahapan (objek pelaporan atau nol)

### Jenis Pesan

Nilai
|-------|------|---------|
WANITA 0
Perancis
2
Fiji 3
4
Fiji 5
Fiji 6

### Tahap pipa

Nilai
|-------|------|-------------|
WANITA 0
Perancis
2
Fiji 3
4
Fiji 5

### Aliran pesan tipikal

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

Jika tahap apapun gagal, tahap yang tersisa dilewati, pesan dipancarkan, dan akhirnya pesan menutup run.

## Logika retry translation translation

Jalur pipa ini menerapkan dua tingkat ketahanan:

### Uji-ulang tahap-tahapan tahap-Fando (TranslationRetryService)

- Jika permintaan terjemahan gagal setelah retries internal LibreTranslate, melakukan hingga 3 retries tahap tambahan dengan penundaan 30 detik.
- Pemasok placeholder: Pemegang tempat bernama () dalam teks diganti sementara dengan token aman () sebelum terjemahan dan dipulihkan sesudahnya, memastikan tata bahasa yang benar dalam bahasa sasaran.

### Pemvalidasi Bahasa Bahasa Bahasa Bahasa Bahasa Bahasa Bahasa Una

- Sebelum menerjemahkan ke bahasa sasaran, layanan memverifikasi bahasa tersebut didukung oleh server terjemahan.
- Bahasa yang tidak didukung dilewati dengan peringatan, mencegah percobaan yang gagal berulang.

### Coba lagi aras-blok markdown

- Terjemahan-terjemahan markdown dilakukan block-by-block (heading, paragraf, daftar item).
- Jika sebuah blok individu gagal terjemahan, itu ditandai sebagai tidak diterjemahkan dalam file metadata dan dicoba pada jalur pipa berikutnya.
- Jejak layanan per-language, status per-blok dalam berkas di sebelah setiap berkas Markdown sumber.

## Kode galat

Galat dilaporkan menggunakan enum terpadu yang dikelompokkan ke dalam jangkauan:

jangkauan uc
|-------|----------|
1000–1999
2000–99
3000–3999
40000–4999
5000–5999

Setiap kesalahan dalam sebuah laporan membawa narasumber (kode bahasa, jalur berkas, atau nama panggung), kode kesalahan, dan pesan yang dapat dibaca manusia.

## Terjemahan Live Dashboard

Proyek dari Server ini termasuk sebuah halaman admin yang terhubung ke hub SignalR di dan menampilkan semua acara pipa secara real time.

- Memaparkan status koneksi, jumlah pesan, dan tabel yang sedang berlangsung.
- Baris berkode warna: biru untuk tahap awal, hijau untuk pelengkapan, merah untuk kesalahan.
- Sodium Mendukung membersihkan pakan dan mengekspor semua pesan ke JSON.
- Auto-rekoneksi dengan pengunduran eksponensial jika sambungan terputus.

## Prinsip desain

- **Modularity**: Each translation concern is isolated in its own service for maintainability and testability.
- **Incremental persistence**: Dictionaries and Markdown files are saved per-language immediately after translation, reducing memory pressure and providing earlier feedback.
- **Resilience**: Multiple retry levels (HTTP, stage, block) ensure transient failures do not block the pipeline.
- **State tracking**: Per-file metadata (`.translation-meta.json`) and hash files enable precise incremental work on subsequent runs.
- **Real-time visibility**: Every significant operation is reported via SignalR for monitoring and debugging.
- **Manual translations always have priority over automatic additions.**
