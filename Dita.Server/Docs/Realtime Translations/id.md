# Terjemahan Real- waktu

Dokumen ini ada sebagai masukan uji langsung untuk pipa terjemahan otomatis.

## Apa yang layanan lakukan

Layanan berjalan pada jadwal dan memvalidasi server terjemahan, konfigurasi, dan bahasa yang tersedia sebelum pekerjaan terjemahan dimulai.

Setelah langkah validasi, itu mensinkronkan nama-nama negara dari katalog baca-saja negara ke lokalisasi standar JSON diktator. Jika bahasa aplikasi default adalah bahasa Inggris, entri negara disimpan sebagai kunci sama dengan nilai. Jika bahasa baku berbeda, nama negara Inggris pertama diterjemahkan ke dalam bahasa baku, dan hanya kemudian disimpan sebagai kunci sama dengan nilai dalam kamus baku.

Selanjutnya, layanan dibandingkan kamus lokalisasi baku saat ini dengan snapshot yang disimpan dari run sebelumnya. Masukan baru ditambahkan diterjemahkan ke dalam bahasa target hanya ketika kunci tidak sudah ada, jadi terjemahan manual tetap prioritas. Entri yang dibuang dihapus dari semua dialog target untuk menjaga seluruh set konsisten.

Akhirnya, pemindaian layanan menentukan akar dokumentasi untuk pohon Markdown. Setiap folder topik diharapkan berisi berkas sumber bernama setelah bahasa baku, seperti en.m. Layanan hashes berkas sumber, mendeteksi perubahan, menerjemahkan hilang atau target usang Markdown berkas, dan menyimpan hash saat ini di sebelah berkas sumber. Jika menulis hash di sebelah sumber file tidak mungkin, itu jatuh kembali ke penyimpanan sementara.

## Bagaimana laporan layanan berlangsung

Backend memancarkan pesan umum SignalR melalui hub lokalisasi menggunakan satu amplop pesan. Setiap pesan membawa jenis pesan, tahap proses saat ini, penanda waktu UTC, ringkasan teks, dan muatan opsional stage- spesifik.

Tahap saat ini adalah:

- checkserver
- Negara Penerjemah
- Diterjemahkan Oleh:
- Diterjemahkan MarkdownFiles
- Hasil Tabung

Aliran pesan khas dimulai, tahap selesai, dan pipa selesai. Jika sebuah tahap gagal, pesan ditandai sebagai kesalahan dan termasuk informasi kesalahan terstruktur dengan kode kesalahan terpadu.

## Desain prinsip

Terjemahan diproses secara berurutan untuk menghindari overloading server LibreTranslate.

Lokalisasi JSON diksionari selalu disimpan dengan tombol diurutkan secara alfabet dan format JSON untuk perawatan yang lebih mudah.

Snapshot kamus baku sebelumnya disimpan terus-menerus sehingga restart dari aplikasi tidak kehilangan pelacakan perubahan.

*** Terjemahan manual selalu memiliki prioritas atas tambahan otomatis. ***
