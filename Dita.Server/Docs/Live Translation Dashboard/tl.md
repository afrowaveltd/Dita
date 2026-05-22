# " Live Translation Dashboard "

Ang Live Translation Dashboard ay isang admin page na nagbibigay ng real-time na imahe sa awtomatikong translation pipeline. Nakakabit ito sa sentro ng SignalR at nagtatanghal ng lahat ng mga pangyayari sa tubo habang nagaganap ang mga ito.

## URL

`/Admin/LiveTranslation`

> Note: Authentication and authorization are not yet implemented. Future versions will restrict this page to admin users only.

## Mga Katangian

### Tuloy ang real-time na kaganapan

Ang lahat ng mga pangyayari ng SignalR mula sa translation pipeline ay itinatanghal sa isang live-upding table:

- **Sequence number** — Monotonic counter within each pipeline run
- **Timstamp** — Lokal na panahon nang tanggapin ang pangyayari
- **Run ID** — Maikling GUD para sa correlation
- **Stage** — Pipeline stage badge (Check Profiders, Translate Communities, atbp.)
- **Type** — Message type badge (StargageStarted, Progress, StageComposted, atbp.)
- **Message** — Human-readable na paglalarawan
- ** Details** — Buong sahod ng JSON na nakabase sa data ng pangyayari

### " Coldering " ng kulay

Kulay
|-------|---------|
asul ()
berde ()
Pula ()
Puti (default)

### Kalagayan ng koneksyon

Isang mataas na baner sa itaas na mga palabas:
- **Connecting** — Pagtatatag ng SignalR koneksyon
- **Konnected** — pagtanggap ng mga pangyayaring normal
- ** Pagkonekta** — Nawala ang koneksyon, sinisikap na muling magkonekta
- ** Hindi nakonekta** — Nagsara ang koneksyon

Ang koneksyon ay gumagamit ng awtomatikong muling pag-uugnay sa exponential backoff: 0s, 2s, 5s, 10s, 30s.

### Mga Pagkontrol

- **Clear Breed** — Inalis ang lahat ng nakatanghal na mensahe at binabago ang counter
- **Export JSON** — Lahat ay tumatanggap ng mga mensahe bilang isang file ng JSON para sa pagsusuri
- **Message counter** — Shows total number of events received in this session

## Ang sentro ng SignalR

Ang dashboard ay nag-uugnay sa:

```javascript
const connection = new signalr.HubConnectionBuilder()
    .withUrl("/hubs/localization")
    .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
    .build();
```

### Kasunduan sa Mensahe

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

### Mga uri ng Pangyayari

Ang dashboard ang humahawak sa lahat ng pamantayan:

Uri
|------|---------|
asul na badge
Bersiyong badge
Pulang badge
Bersiyong badge
Pulang badge
Badge ng Info
Babalang badge

## Technical pagpapatupad

### Likod

- **LocalizationHub** () — Sentro ng SignalR na naghahatid ng mga mensahe sa lahat ng magkakaugnay na kliyente
- **ISlignalRPublisher** — Paglihis sa sentro para gamitin sa mga serbisyo sa pagsasalin
- **SignalRPublisher** — Pagpapatupad ng Default na gumagawa ng monotonic na pagkakasunud - sunod at pagsasahimpapawid

### Harap

- Dalisay na HTML/JS na may Bootstrap 5 styling
- Ginagamit ang Microsoft SignalR JavaScript client library (nakakarga mula sa CDN)
- Walang salin na server-side na kinakailangan para sa pagkain sa okasyon

### Pahina sa istraktura

```
Dita.Server/Pages/Admin/
├── LiveTranslation.cshtml      — Razor page markup + JavaScript
└── LiveTranslation.cshtml.cs     — Page model (empty, data comes via SignalR)
```

## Ginagamit sa panahon ng pagsulong

1. Paandarin ang Pista. Kapalit ng server
2. Paglalakbay Patungo sa
3. Trigger ang isang transaksyon run (alinman ay maghintay sa nag-iskedyul o tumawag ng API)
4. Ang mga pangyayari sa pagmamasid ay lumilitaw sa tunay na panahon
5. Gamitin ang Export button upang kumuha ng isang buong bakas para sa pag - aalis ng sandata

## Mga Pagsulong sa Hinaharap

Isinaplanong mga pagsulong para sa dashboard:

- **Authentication** — Restrict access to users with the `Admin` role
- **Filtering** — Mga pangyayari sa Filter sa pamamagitan ng entablado, tipo, o pagtakbo ng ID
- ** Ang historikal na pagtakbo** — Ang nakumpletong larawan ay mula sa isang database o log file
- **Statistics** — Charts na nagpapakita ng halaga ng pagsasalin, maling bilis, at latency sa paglipas ng panahon
- ** Ang Manual ang nag - uudyok** — Mga Buntot upang manu - manong simulan ang espesipikong mga yugto ng tubo
- **Configuration** — Edit `AutomaticTranslationSettings` directly from the dashboard
- **Language management** — Tingnan at isaayos ang suportadong mga wika
- ** Pagsusuri** — Browse at saliksikin ang lokalisasyong mga diksyunaryo

## Pagputok ng Problema

### Ang Dashboard ay nagpapakita ng "Failed to link"

1. Pare - parehong tumatakbo at madaling makuha ang server
2. Check browser console para sa mga COR o mga pagkakamali sa network
3. May katibayan sa
4. Ang Ensurure no firewall ay ang pagharang sa mga koneksiyon ng WebSocket

### Hindi lumilitaw ang mga pangyayari

1. Tingnan na ang SignalR city URL na posporo sa pagitan ng server () at kliyente ()
2. Ang pagbabago sa iskedyul ay nagagawa sa
3. Tingnan ang mga trosong server para sa mga pagkakamali sa tubo
4. Check browser Network task para sa mga WebSocket message

### Hindi tama ang mga mensahe

Ang larangan ay gumagarantiya ng pag - uutos sa loob lamang ng isang pagtakbo. Kung ang mga mensahe ay lumitaw nang hindi sunud - sunod, maaaring ipahiwatig nito:
- Ang maramihang tubo ay nagsasanib (hindi dapat mangyari dahil sa semaphore lock)
- Mga isyu sa pagsasalin ng mga browser (tumatanggi sa nakarerepreskong pahina)
