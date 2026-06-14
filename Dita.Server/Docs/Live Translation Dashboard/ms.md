# Terjemahan Live Dashboard

Terjemahan Live Dashboard adalah sebuah halaman admin yang menyediakan visibilitas real-time ke dalam pipa penerjemahan otomatis. Ini terhubung ke hub SignalR dan menampilkan semua peristiwa pipa saat mereka terjadi.

## URL

`/Admin/LiveTranslation`

> Note: Authentication and authorization are not yet implemented. Future versions will restrict this page to admin users only.

## Keupayaan

### Aliran acara real-time

Semua peristiwa SignalR dari jaringan pipa penerjemahan ditampilkan dalam tabel yang sedang berlangsung:

- **Sequence number** — Monotonic counter within each pipeline run
- **Timestamp** — Local time when the event was received
- **Run ID** — Shortened GUID for correlation
- **Stage** — Lencana tahap baris pipa (CheckServers, TranslateCountries, dll.)
- **Type** — Message type badge (StageStarted, Progress, StageCompleted, etc.)
- **Message** — Human-readable description
- **Details** — Full JSON payload of the event data

### Coding warna

Warna
|-------|---------|
biru muson ()
()
Red ()
Putih putih(default)

### Sambungan status

Sebuah spanduk status di atas menunjukkan:
- **Connecting** — Establishing SignalR connection
- **Connected** — Receiving events normally
- **Penghubung** — Koneksi terputus, mencoba menyambung kembali
- **Disconnected** — Connection closed

Sambungan tersebut menggunakan koneksi otomatis dengan backoff eksponen: 0s, 2s, 5s, 10s, 30s.

### Pengendalian

- **Clear Feed** — Removes all displayed messages and resets the counter
- **Export JSON** — Downloads all received messages as a JSON file for analysis
- **Message counter** — Shows total number of events received in this session

## Hub SignalR jelai

Dashboard menghubungkan ke:

```javascript
const connection = new signalr.HubConnectionBuilder()
    .withUrl("/hubs/localization")
    .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
    .build();
```

### Kontrak pesan

```typescript
interface LocalizationHubMessage {
    runId: string;        // Guid
    sequence: number;     // long
    type: LocalizationMessageType;
    stage: ProcessStage;
    timestampUtc: string; // ISO 8601
    isError: boolean;
    message: string;
    data: object | null;
}
```

### Jenis peristiwa

Semua pemegang dashboard:

Jenis
|------|---------|
Lencana Biru
Lencana Hijau
Lencana merah
Lencana Hijau
Lencana merah
Lencana informasi
Lencana Peringatan Umunia

## Implementasi teknis

### Bagian Belakang

- **LocalizationHub** (`/hubs/localization`) — SignalR hub that broadcasts messages to all connected clients
- **ISignalRPublisher** — Abstraction over the hub for use in translation services
- **SignalRPublisher** — Default implementation that increments a monotonic sequence and broadcasts

### Frontend

- HTML/JS murni dengan Bootstrap 5 styling
- Wourdon menggunakan pustaka klien JavaScript Microsoft SignalR (dimuat dari CDN)
- Tidak ada penerapan sisi-server yang diperlukan untuk feed acara

### Struktur halaman

```
Dita.Server/Pages/Admin/
├── LiveTranslation.cshtml      — Razor page markup + JavaScript
└── LiveTranslation.cshtml.cs     — Page model (empty, data comes via SignalR)
```

## Kegunaan selama pengembangan

1. Mulailah Dita. Aplikasi server X
2. Navigasi ke
3. Pemicu sebuah penterjemahan (baik menunggu penjadwal atau memanggil API)
4. Acara Watch Watch muncul dalam waktu nyata
5. Memanfaatkan butang Ekspor untuk menangkap jejak penuh untuk debug

## Peningkatan masa depan

Peningkatan direncanakan untuk dashboard:

- **Autentikasi** — Batasi akses kepada pengguna dengan peran
- **Filtering** — Filter events by stage, type, or run ID
- **Historical runs** — View completed runs from a database or log file
- **Statistik** — Tabel yang menunjukkan jumlah terjemahan, tingkat kesalahan, dan latensi seiring waktu
- **Manual triggers** — Buttons to manually start specific pipeline stages
- **Configuration** — Edit `AutomaticTranslationSettings` directly from the dashboard
- **Language management** — View and edit supported languages
- **Dictionary preview** — Browse and search localization dictionaries

## Penerjemahan Masalah

### Papan dasbor menunjukkan " failed to connect"

1. Mengesahkan server sedang berjalan dan dapat diakses
2. Periksa konsol peramban untuk CORS atau galat jaringan
3. Konfirmasi adalah hadir dalam
4. Pastikan tidak ada firewall yang menghalangi sambungan WebSocket

### Peristiwa - peristiwa tidak muncul

1. Periksa bahwa URL hub SignalR cocok antara server () dan klien ()
2. Simak jadwal diaktifkan dalam
3. Log log server untuk kesalahan pipa terjemahan
4. Periksa tab jaringan peramban bagi pesan WebSocket

### Pesanan di luar perintah

Lapangan jaminan memerintahkan dalam satu putaran. Jika pesan muncul dari urutan, itu mungkin menunjukkan:
- Multiple pipeline doxine berjalan tumpang tindih (tidak boleh terjadi karena kunci semaphore)
- Isu penerapan Browser (coba menyegarkan halaman)
