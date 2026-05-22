# Tulkošanas dashboard Live

Live Translation Dashboard ir admin lapa, kas nodrošina reālā laika redzamību uz automātisko tulkošanas cauruļvada. Tas savienojas ar SignalR mezglu un parāda visus cauruļvadu notikumus, kad tie notiek.

## URL

`/Admin/LiveTranslation`

> Note: Authentication and authorization are not yet implemented. Future versions will restrict this page to admin users only.

## Īpašības

### Reālā laika notikumu plūsma

Visi SignalR notikumi no tulkošanas cauruļvada tiek parādīti tiešraides tabulā:

- ** kārtas numurs** – Monotoniskais skaitītājs katrā cauruļvada posmā
- **Laikmets** – Vietējais laiks, kad pasākums tika saņemts
- **Palaists ID** – saīsināta saskarne korelācijai
- **Stage** – Cauruļvadu žetons (CheckServers, TranslateCountries, u.c.)
- **Type** – Ziņojuma tipa žetons (StageStarted, Progress, StageCompleted, utt.)
- **Ziņojums** – Cilvēkiem salasāms apraksts
- **Detalizētie** – Notikuma datu pilna JSON derīgā krava

### Krāsu kods

Krāsa
|-------|---------|
Zils ()
Zaļš ()
Sarkans ()
Balts (noklusētais)

### Savienojuma statuss

Stāvokļa baneris augšā rāda:
- **Savienošana** – SignalR savienojuma izveide
- **Saistīti** – parasti saņemami notikumi
- **Atvienošana** — zaudēts savienojums, mēģinot atjaunot savienojumu
- **Atvienots** – Savienojums slēgts

Savienojumā tiek izmantota automātiskā savienošana ar eksponenciālo aizmuguri: 0s, 2s, 5s, 10s, 30s.

### Kontrole

- ** Attīrīt barotni** – Noņem visus attēlotos ziņojumus un atiestata skaitītāju
- ** Eksports JSON** – Lejupielādē visus saņemtos ziņojumus kā JSON failu analīzei
- **Ziņu skaitītājs** – Parāda šajā sesijā saņemto notikumu kopējo skaitu

## SignālaR rumba

Panelis savienojas ar:

```javascript
const connection = new signalr.HubConnectionBuilder()
    .withUrl("/hubs/localization")
    .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
    .build();
```

### Ziņojuma līgums

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

### Notikumu veidi

Panelis apstrādā visas vērtības:

Veids
|------|---------|
Zilā žetons
Zaļā žetons
Sarkanā žetons
Zaļā žetons
Sarkanā žetons
Informācijas žetons
Brīdinājuma žetons

## Tehniskā īstenošana

### Aizmugure

- **LocalizationHub** () – SignalR centrmezgls, kas pārraida ziņojumus visiem pieslēgtajiem klientiem
- **IsignalRPublizer** – Abstrakcija pa centru izmantošanai tulkošanas pakalpojumos
- **SignalRPubliseer** – Noklusētā implementācija, kas palielina monotonu secību un pārraides

### Priekšpuse

- Pure HTML/ JS ar Bootstrap 5 stilu
- Izmanto Microsoft SignalR JavaScript klienta bibliotēku (ielādēta no CDN)
- Notikumu barotnei nav nepieciešama servera puses renderēšana

### Lapas struktūra

```
Dita.Server/Pages/Admin/
├── LiveTranslation.cshtml      — Razor page markup + JavaScript
└── LiveTranslation.cshtml.cs     — Page model (empty, data comes via SignalR)
```

## Lietošana attīstības laikā

1. Sāk Dita. Servera programma
2. Pārvietoties uz
3. Trigger tulkojumu palaist (vai nu gaidīt plānotāja vai zvanīt API)
4. Skatīt notikumus parādās reālajā laikā
5. Izmantojiet Eksportēšanas pogu, lai notvertu pilnu izsekot atkļūdošanai

## Turpmākie uzlabojumi

Plānotie paneļa uzlabojumi:

- **Autentifikācija** – Ierobežot piekļuvi lietotājiem ar lomu
- **Filtering** – Filtrēt notikumus pēc posma, tipa, vai palaist ID
- ** Historikas trases** — Skats pabeigts no datubāzes vai žurnāla faila
- **Statistika** – Diagrammas, kurās redzams tulkojumu skaits, kļūdu īpatsvars un latentums laika gaitā
- **Manuālie trigeri** – Pogas konkrētu cauruļvada posmu palaišanai ar roku
- ** Konfigurācija** – Rediģēt tieši no paneļa
- **Valodu pārvaldība** — Skatīt un rediģēt atbalstītās valodas
- **Dictionary preview** — Pārlūkot un meklēt lokalizācijas vārdnīcas

## Problēmu novēršana

### Dashboard rāda "Neticami pieslēgties"

1. Pārbaudīt serveri darbojas un pieejams
2. Pārbaudīt pārlūka konsole priekš CORS vai tīkla kļūdām
3. Apstiprināt
4. Pārliecinieties, ka ugunsmūris bloķē WebSocket savienojumus

### Notikumi neparādās

1. Pārbaudiet, vai SignalR centrmezgla URL atbilst starp serveri () un klientu ()
2. Pārbaudīt ieslēgto plānotāju
3. Aplūkot servera žurnālus tulkošanas cauruļvadu kļūdas
4. Pārlūkprogrammas tīkla cilne WebSocket ziņojumiem

### Vēstules nav kārtībā

Lauks garantē pasūtīšanu vienā braucienā. Ja ziņojumi parādās ārpus kārtības, tie var norādīt:
- Vairāki cauruļvadi pārklājas (nevajadzētu notikt semafora bloķēšanas dēļ)
- Pārlūka renderēšanas jautājumi (mēģini atsvaidzināt lapu)
