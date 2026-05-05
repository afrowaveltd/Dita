# Terjemahan Real- waktu

Dokumen ini ada sebagai masukan uji langsung untuk pipa terjemahan otomatis. Setiap perubahan ke berkas pemicu re- terjemahan dari semua target berkas bahasa pada jadwal berikutnya.

## Tampilan arsitektur

Pipa terjemahan telah direstrukturisasi menjadi sebuah arsitektur modular dengan empat layanan sub- khusus dikoordinasikan oleh sebuah realistrator ringan:

- **BackendTranslationService** — Orchestrates the entire pipeline, handles server validation, and delegates work to sub-services.
- **CountriesTranslationService** — Synchronizes country names from `countries.json` into per-language dictionaries.
- **LocalizationTranslationService** — Detects added/removed keys in the default JSON dictionary and translates them into target languages.
- **DocumentsTranslationService** — Translates Markdown documentation files with per-block tracking and metadata.

Setiap sub- layanan beroperasi secara independen dan laporan kemajuan melalui Sinyal secara real time.

## Apa yang layanan lakukan

Layanan berjalan pada jadwal dan mengeksekusi pipeline tahap lima: validasi server, sinkronisasi negara, sinkronisasi kamus JSON, Markdown berkas terjemahan, dan mempertahankan hasil. Setiap tahap memancarkan struktur real-waktu kemajuan peristiwa atas sinyal R sehingga klien yang terhubung dapat mengikuti bersama sebagai hasil kerja.

## Tahap pipa

### Tahap 1 - CheckServers

Sebelum pekerjaan terjemahan dimulai, layanan memverifikasi bahwa semua kondisi puas:

- Bagian konfigurasi harus ada dan valid.
- Server LibreTranslate harus merespon dalam waktu yang dapat diterima.
- Daftar bahasa yang tersedia pada server terjemahan diambil.
- Bahasa baku yang dikonfigurasi mesti ada dalam daftar itu.
- Berkas lokal JSON yang hilang untuk setiap bahasa yang didukung dibuat secara otomatis.

Jika ada pemeriksaan gagal, pipa berhenti segera dan pesan dipancarkan.

### Tahap 2 - Negara Terjemahan

Nama negara disimpan dalam sinkron dari katalog baca-saja () ke lokalisasi kamus JSON.

- Jika bahasa aplikasi default adalah bahasa Inggris, setiap nama negara disimpan sebagai tanpa terjemahan.
- Jika bahasa baku adalah bahasa lain, nama negara Inggris pertama diterjemahkan ke dalam bahasa itu, dan hasilnya menjadi entri dalam kamus baku.
- After the default dictionary is updated, each missing country entry in every target language dictionary is translated and saved **immediately per language**.
- Entri yang sudah diterjemahkan diawetkan tanpa modifikasi.
- Jika terjemahan gagal, layanan mencoba sampai 3 kali dengan penundaan 30 detik sebelum pindah ke bahasa berikutnya.

### Tahap 3 - Diterjemahkan JsonFiles

Layanan ini membandingkan kamus lokalisasi baku saat ini dengan snapshot yang disimpan dari run sebelumnya:

- **Added keys** — entries present in the current default but absent from the snapshot — are translated into every target language that does not already have a manual entry for that key.
- **Removed keys** — entries present in the snapshot but absent from the current default — are deleted from every target language dictionary.
- Terjemahan manual selalu mengambil prioritas. Jika sebuah kamus target telah berisi sebuah nilai untuk sebuah kunci, entri itu dibiarkan tidak berubah terlepas dari apa yang dikatakan oleh sumber.
- **Each target language dictionary is saved immediately after its translations complete**, rather than waiting for all languages to finish.
- Jika terjemahan gagal untuk bahasa tertentu, layanan akan mencoba kembali secara otomatis. Hanya persisten errors (misalnya, bahasa yang tidak didukung) menyebabkan bahasa yang akan dilewati.
- Setelah dijalankan, kamus bawaan saat ini disimpan sebagai snapshot baru untuk perbandingan berikutnya.

Semua kamus selalu disimpan dengan tombol diurutkan secara alfabet dan JSON yang rusak untuk readabilitas manusia.

### Tahap 4 - Diterjemahkan MarkdownFiles

Layanan berjalan akar dokumentasi terkonfigurasi (baku:) dan proses setiap berkas sumber rekursif:

1. Isi berkas sumber dibaca dan hash SHA-256 dihitung.
2. A `.translation-meta.json` file next to the source tracks per-language, per-block translation status, enabling **incremental re-translation** of only failed blocks.
3. Hash yang disimpan dari run sebelumnya (disimpan dalam berkas di sebelah berkas sumber, atau dalam lokasi fallback sementara) dibandingkan dengan hash saat ini.
4. Untuk setiap bahasa target, berkas yang berhubungan juga diperiksa untuk integritas struktural.
5. Setiap berkas target yang hilang, memiliki hash usang, gagal validasi struktur, atau berisi blok tidak diterjemahkan antrikan untuk terjemahan ulang.
6. **Each target language is translated and saved independently** — if Czech succeeds but French fails, the Czech file is still written to disk.
7. Berkas yang sukses diterjemahkan bervalidasi untuk paritas struktural dengan sumber (jumlah judul yang sama, item daftar, blok kode, kutipan blok, link, tebal / miring, dan tag HTML) sebelum mereka ditulis ke disk.
8. Jika semua berkas target untuk sumber sukses, hash baru disimpan di sebelah sumber. Jika menulis di samping sumber gagal (misalnya dalam pengiriman baca-saja), hash kembali ke direktori sementara.
9. Jika target terjemahan gagal validasi, metadata menandai blok-blok sebagai tidak diterjemahkan sehingga mereka dicoba ulang pada run berikutnya.

### Tahap 5 - Hasil Tabung

Sebuah konsolidasi dirakit dan diterbitkan. Termasuk:

- UTC menjalankan awal dan menyelesaikan penanda waktu.
- Menghitung berkas lokal JSON yang disimpan, menyimpan berkas Markdown, menyimpan berkas hash, dan hash fallback menulis.
- Kesalahan penyimpanan yang dikumpulkan selama menjalankan.
- Statistik terjemahan bahasa per- (jumlah terjemahan, hitungan terlewati, kesalahan dihitung).

## Sinyal Amplop pesan R

Setiap progres event disampaikan sebagai dengan ruas berikut:

Ruas
|-------|------|-------------|
Identifier korelasi untuk menjalankan pipeline kini
monotonic counter dalam menjalankan, dimulai dari 1
Tipe semantik pesan
Panggung baris pipa pesan milik
Waktu UTC ketika pesan dipancarkan
Apakah pesan menunjukkan kondisi galat
Ringkasan yang mudah dibaca
Stage- muatan spesifik (laporkan objek atau null)

### Tipe pesan

Nilai
|-------|------|---------|
0
1
2
3
4
5
6

### Tahap pipa

Nilai
|-------|------|-------------|
0
1
2
3
4
5

### Aliran pesan khas

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

Jika setiap tahap gagal, tahap yang tersisa dilewati, pesan dipancarkan, dan akhirnya pesan menutup jalankan.

## Coba ulang logika terjemahan

Penerapan pipeline dua tingkat ketahanan:

### Stage- level coba ulang (Layanan coba terjemahan)

- Jika permintaan terjemahan gagal setelah pengulangan internal LibreTranslate, performa hingga 3 tingkat tambahan mengulang dengan penundaan 30 detik.
- Placeholder masking: Namanya placeholder () dalam teks sementara diganti dengan token aman () sebelum terjemahan dan dipulihkan sesudahnya, memastikan tata bahasa yang benar dalam bahasa target.

### Validasi bahasa

- Sebelum menerjemahkan ke bahasa target, layanan memverifikasi bahasa didukung oleh server terjemahan.
- Bahasa yang tidak didukung dilewati dengan peringatan, mencegah percobaan gagal berulang.

### Coba lagi tingkat blok Markdown

- Terjemahan Markdown dilakukan blok-by- blok (headdings, paragraf, daftar item).
- Jika sebuah blok individu gagal menerjemahkan, itu ditandai sebagai tidak diterjemahkan dalam berkas metadata dan dicoba ulang pada baris pipa berikutnya.
- Layanan melacak perbahasa, status per- blok dalam berkas di sebelah setiap berkas Markdown sumber.

## Kode galat

Galat dilaporkan menggunakan kesatuan enum dikelompokkan ke dalam jangkauan:

Jangkauan
|-------|----------|
10000-1999
2000- 2999
3000- 3999
4000- 4999
5000- 5999

Setiap kesalahan dalam laporan membawa identifier sumber (kode bahasa, path berkas, atau nama panggung), kode kesalahan, dan pesan yang mudah dibaca manusia.

## Dashboard Terjemahan Langsung

Proyek Server memuat halaman admin yang terhubung ke server SignalR dan menampilkan semua peristiwa pipa secara langsung.

- Tampilkan status koneksi, jumlah pesan, dan sebuah tabel pengubahan kehidupan dari semua kejadian.
- Baris warna - kode: biru untuk awal tahap, hijau untuk pelengkapan, merah untuk kesalahan.
- Mendukung menghapus asupan dan mengekspor semua pesan ke JSON.
- Auto- menghubungkan kembali dengan backoff eksponensial jika koneksi turun.

## Desain prinsip

- **Modularity**: Each translation concern is isolated in its own service for maintainability and testability.
- **Incremental persistence**: Dictionaries and Markdown files are saved per-language immediately after translation, reducing memory pressure and providing earlier feedback.
- **Resilience**: Multiple retry levels (HTTP, stage, block) ensure transient failures do not block the pipeline.
- **State tracking**: Per-file metadata (`.translation-meta.json`) and hash files enable precise incremental work on subsequent runs.
- **Real-time visibility**: Every significant operation is reported via SignalR for monitoring and debugging.
- **Manual translations always have priority over automatic additions.**
