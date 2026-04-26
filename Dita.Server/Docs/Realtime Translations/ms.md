# Terjemahan waktu-nyata

Dokumen ini ada sebagai input tes langsung untuk pipa penerjemahan otomatis.

## Apa yang pelayanan lakukan

Layanan tersebut berjalan pada jadwal dan memvalidasi server terjemahan, konfigurasi, dan bahasa yang tersedia sebelum pekerjaan penerjemahan dimulai.

Setelah langkah validasi, itu mensinkronkan nama negara dari negara baca-saja katalog ke dalam standar lokalisasi JSON kamus. Jika bahasa baku aplikasi adalah bahasa Inggris, entri negara disimpan sebagai kunci sama dengan nilai. Jika bahasa baku berbeda, nama negara Inggris pertama kali diterjemahkan ke dalam bahasa baku, dan hanya kemudian disimpan sebagai kunci sama dengan nilai dalam kamus baku.

Selanjutnya, layanan membandingkan kamus lokalisasi baku saat ini dengan snapshot tersimpan dari yang sebelumnya dijalankan. Masukan yang baru ditambahkan diterjemahkan ke dalam bahasa target hanya ketika kunci belum ada, sehingga terjemahan manual mengutamakan. Entri dihapus dihapus dihapus dari semua kamus target untuk menjaga seluruh set konsisten.

Akhirnya, layanan scan dikonfigurasi akar dokumentasi untuk pohon Markdown. Setiap folder topik diharapkan berisi berkas sumber yang dinamai menurut bahasa baku, seperti en.md. Layanan Hashes yang sumber file, mendeteksi perubahan, menerjemahkan target yang hilang atau ketinggalan zaman Markdown file, dan menyimpan hash saat ini di samping file sumber. Jika menulis hash di sebelah sumber file tidak mungkin, itu jatuh kembali ke penyimpanan sementara.

## Bagaimana perkembangan laporan layanan

Bagian belakang memancarkan pesan umum SignalR melalui hub lokalisasi menggunakan satu amplop pesan. Setiap pesan yang membawa jenis pesan, tahap proses saat ini, penanda waktu UTC, ringkasan teks, dan muatan spesifik tahap opsional.

Tahap saat ini adalah:

- Pelayan Cek
- Terjemahan Terjemah
- TerjemahkanJsonFiles
- Diterjemahkan oleh MarkdownFiles
- perpustakaan menyimpan

Aliran pesan tipikal adalah tahap dimulai, tahap selesai, dan pipa selesai. Jika sebuah tahap gagal, pesan ditandai sebagai kesalahan dan termasuk informasi kesalahan terstruktur dengan kode kesalahan terpadu.

## Prinsip desain

Terjemahan terjemahan diproses secara berurutan untuk menghindari overloading server LibreTranslate.

Kamus JSON Lokalisasi Kelayakan JSON selalu disimpan dengan kunci diurutkan berdasarkan abjad dan diformat JSON untuk pemeliharaan yang lebih mudah.

Klien kamus baku snapshot sebelumnya disimpan dengan gigih sehingga memulai ulang aplikasi tidak kehilangan pelacakan perubahan.

***Terjemahan visual selalu memiliki prioritas atas penambahan otomatis.***
