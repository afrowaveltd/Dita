# Dashboard Terjemahan Langsung

Dashboard Terjemahan Langsung adalah halaman admin yang menyediakan visibilitas real time ke dalam pipa terjemahan otomatis. Ini menghubungkan ke signalR hub dan menampilkan semua peristiwa pipa saat mereka terjadi.

## URL

`/Admin/LiveTranslation`

> Note: Authentication and authorization are not yet implemented. Future versions will restrict this page to admin users only.

## Fitur

### Arus event Real- time

Semua SignalR kejadian dari pipa terjemahan ditampilkan dalam sebuah live- update tabel:

- **Sequence number** — Monotonic counter within each pipeline run
- **Timestamp** — Local time when the event was received
- **Run ID** — Shortened GUID for correlation
- **Stage** — Pipeline stage badge (CheckServers, TranslateCountries, etc.)
- **Type** — Message type badge (StageStarted, Progress, StageCompleted, etc.)
- **Message** — Human-readable description
- **Details** — Full JSON payload of the event data

### Pengkodean warna

Warna
|-------|---------|
Biru ()
Hijau ()
Merah ()
Putih (baku)

### Status koneksi

Sebuah bendera status di acara puncak:
- **Connecting** — Establishing SignalR connection
- **Connected** — Receiving events normally
- **Reconnecting** — Connection lost, attempting to reconnect
- **Disconnected** — Connection closed

Koneksi menggunakan koneksi otomatis reconnect dengan backoff eksponensial: 's, 2s, 5s, 10s, 30s.

### Kontrol

- **Clear Feed** — Removes all displayed messages and resets the counter
- **Export JSON** — Downloads all received messages as a JSON file for analysis
- **Message counter** — Shows total number of events received in this session

## Pusat Sinyal

Dashboard terhubung ke:

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

### Tipe kejadian

Dashboard menangani semua nilai:

Tipe
|------|---------|
Lencana biru
Lencana hijau
Lencana merah
Lencana hijau
Lencana merah
Lencana Info
Lencana peringatan

## Implementasi teknis

### Backend

- **LocalizationHub** (`/hubs/localization`) — SignalR hub that broadcasts messages to all connected clients
- **ISignalRPublisher** — Abstraction over the hub for use in translation services
- **SignalRPublisher** — Default implementation that increments a monotonic sequence and broadcasts

### antarmuka

- Murni HTML / JS dengan Bootstrap 5 styling
- Menggunakan pustaka klien Microsoft SignalR JavaScript (dimuat dari CDN)
- Tidak perlu merender sisi server untuk feed acara

### Struktur halaman

```
Dita.Server/Pages/Admin/
├── LiveTranslation.cshtml      — Razor page markup + JavaScript
└── LiveTranslation.cshtml.cs     — Page model (empty, data comes via SignalR)
```

## Penggunaan selama pengembangan

1. Mulai Dita. Aplikasi server
2. Navigasi ke
3. Trigger a translation run (baik tunggu penjadwalan atau panggil API)
4. Menonton acara muncul secara real time
5. Gunakan tombol Ekspor untuk menangkap jejak lengkap untuk debug

## Tambahan Masa Depan

Perbaikan yang direncanakan untuk dasbor:

- **Authentication** — Restrict access to users with the `Admin` role
- **Filtering** — Filter events by stage, type, or run ID
- **Historical runs** — View completed runs from a database or log file
- **Statistics** — Charts showing translation counts, error rates, and latency over time
- **Manual triggers** — Buttons to manually start specific pipeline stages
- **Configuration** — Edit `AutomaticTranslationSettings` directly from the dashboard
- **Language management** — View and edit supported languages
- **Dictionary preview** — Browse and search localization dictionaries

## Penelusuran masalah

### Dashboard menampilkan "Gagal menyambung"

1. Verifikasi server sedang berjalan dan dapat diakses
2. Periksa konsol peramban untuk CORS atau galat jaringan
3. Konfirmasi hadir di
4. Pastikan tidak ada firewall yang memblokir koneksi WebSocket

### Kejadian tidak muncul

1. Periksa apakah URL basis SignalR cocok antara server () dan klien ()
2. Verifikasi penjadwalan diaktifkan dalam
3. Lihatlah log server untuk kesalahan jalur pipa terjemahan
4. Periksa tab jaringan peramban untuk pesan WebSocket

### Pesan tidak terurut

Bidang menjamin pesanan dalam satu run. Jika pesan muncul diluar urutan, itu mungkin menunjukkan:
- Multiple pipeline runs overlapping (should not happen because to semaphore lock)
- Masalah rendering peramban (coba menyegarkan halaman)
