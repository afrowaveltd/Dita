# zuzeneko itzulpena

Live Translation Dashboard administratzaile-orri bat da, eta denbora errealeko ikusgaitasuna eskaintzen du itzulpen automatikoko kanalizazioan. SignalR gunearekin konektatzen da eta hoditeriako gertaera guztiak bistaratzen ditu gertatzen den heinean.

## URLA

`/Admin/LiveTranslation`

> Note: Authentication and authorization are not yet implemented. Future versions will restrict this page to admin users only.

## Ezaugarriak

### Denbora errealeko gertaeren korrontea

Seinale guztiak Itzulpen-hodiko R gertaerak taula bizigarri batean erakusten dira:

- **Sequence zenbakia** — Monotonic-en kontagailua hodi bakoitzaren barruan
- **Timestamp** - Gertaera jaso zeneko ordu lokala
- **Run ID** - korrelaziorako GUID laburtua
- **Stage** - Pipeline agertokiko plaka (CheckServers, TranslateCountries, etab.)
- **Type** - Mezu motaren plaka (StageStarted, Progress, StageCompleted, etab.)
- **Mezua** - Giza azalpen irakurgarria
- **Xehetasunak** — Gertaeraren datuen JSON ordainketa osoa

### Kolore-kodeketa

Kolorea
|-------|---------|
Urdina ()
Berdea ()
Gorria ()
Zuria (lehenetsia)

### Konexioaren egoera

Goi-ikuskizunetako egoeraren bandera:
- **Konektatu** - Seinale-konexioa ezartzea
- **Konektatu** - Normalean jasotzen diren gertaerak
- **Berriz konektatzen** - Konexioa galdu da, berriro konektatzen saiatzen
- **Deskonektatu** - Konexioa itxita

Konexioak konexio automatikoa erabiltzen du atzeraldi esponentzialarekin: 0s, 2s, 5s, 10s, 30s.

### Kontrolak

- **Garbitu iturria** - Erakutsitako mezu guztiak kentzen ditu eta kontagailua berrezartzen du
- **Export JSON** - Jasotako mezu guztiak JSON fitxategi gisa deskargatzen ditu analisirako
- **Mezu-kontagailua** - Saio honetan jasotako gertaera kopurua erakusten du

## Seinalea R hub

Arbela honela konektatzen da:

```javascript
const connection = new signalr.HubConnectionBuilder()
    .withUrl("/hubs/localization")
    .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
    .build();
```

### Mezu-kontratua

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

### Gertaera motak

Arbelak balio guztiak kudeatzen ditu:

Mota
|------|---------|
Bandera urdina
Bandera berdea
Bandera gorria
Bandera berdea
Bandera gorria
Informazioa
Abisu-txartela

## Ezarpen teknikoak

### Motorra

- ** Lokalizazioa Hub** () - konektatutako bezero guztiei mezuak igortzen dizkien seinale-zentroa
- **ISignalRPublisher** — Laburpena itzulpen-zerbitzuetan erabiltzeko
- **SignalRPublisher** - Sekuentzia monotoniko bat eta emisioak handitzen dituen inplementazio lehenetsia

### Frontend

- HTML/JS hutsa Bootstrap 5 arkatzarekin
- Microsoft SignalR JavaScript bezeroaren liburutegia erabiltzen du (CDNtik kargatua)
- Ez da zerbitzariaren aldeko errendaziorik behar gertaera-iturrirako

### Orrialdearen egitura

```
Dita.Server/Pages/Admin/
├── LiveTranslation.cshtml      — Razor page markup + JavaScript
└── LiveTranslation.cshtml.cs     — Page model (empty, data comes via SignalR)
```

## Erabilera garapenean

1. Hasi Dita. Zerbitzariaren aplikazioa
2. Nabigatu
3. Atzeratu itzulpen bat (edo itxaron antolatzaileari edo deitu APIari)
4. Ikusi gertaerak denbora errealean
5. Erabili 'Esportatu' botoia arazteko aztarna osoa harrapatzeko

## Etorkizuneko hobekuntzak

Hobekuntzak antolatu dira arbelerako:

- **Autentifikazioa** - Rola duten erabiltzaileentzako sarbidea murriztea
- **Iragazketa** - Iragazki-gertaerak agertoki, mota edo exekutatu IDa
- **Erregistro historikoak** - Ikusi datu-baseko edo egunkari-fitxategiko exekuzioak
- **Estatistikak** — Itzulpen-kopuruak, errore-tasak eta latentziak denboran zehar erakusten dituzten diagramak
- **Esku-abiarazleak** - Hodi-fase espezifikoak eskuz abiarazteko botoiak
- **Konfigurazioa** - Editatu zuzenean arbeletik
- **Hizkuntza-kudeaketa** — Ikusi eta editatu onartutako hizkuntzak
- **Iragarpena** - Arakatu eta bilatu hiztegiak

## Arazoak konpontzea

### Arbelak "Konektatzeko prest" erakusten du

1. Egiaztatu zerbitzaria martxan dagoela eta eskuragarri dagoela
2. Egiaztatu arakatzailearen kontsola CORS edo sareko erroreetarako
3. Berretsi hemen dago
4. Ziurtatu suebakiak ez dituela webSocket konexioak blokeatzen

### Gertaerak ez dira agertzen

1. Egiaztatu SignalR-aren URLa zerbitzariaren () eta bezeroaren () artean bat datorrela
2. Egiaztatu antolatzailea gaituta dagoela
3. Begiratu zerbitzarien erregistroak itzulpen-hodien erroreetarako
4. Egiaztatu arakatzailea WebSocket mezuen sareko fitxa

### Mezuak ez daude ordenan

Eremuak agindu bakarra eskatzen du. Mezuak ordenarik gabe agertzen badira, adieraz dezake:
- Kanalizazio anitzak gainezka egiten du (ez litzateke semaforoaren blokeoagatik gertatu behar)
- Arakatzaile errendatze-arazoak (orrialdea freskatzen saiatu)
