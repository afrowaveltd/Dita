# Live käännös Dashboard

Live käännös Dashboard on admin sivu, joka tarjoaa reaaliaikaista näkyvyyttä automaattisen käännös putki. Se on yhteydessä SignalR-keskukseen ja näyttää kaikki putkistotapahtumat.

## URL

`/Admin/LiveTranslation`

> Note: Authentication and authorization are not yet implemented. Future versions will restrict this page to admin users only.

## Ominaisuudet

### Reaaliaikainen tapahtumavirta

Kaikki signaalit Käännösputken R-tapahtumat näytetään live-up-taulukkoon:

- **Sekvenssinumero** ..
- ** Aikaleima ** Paikallinen tapahtuma-aika
- ** Suorita ID**
- ** Tila** Putkisto-vaihemerkki (CheckServers, TranslateCountries jne.)
- **Tyyppi** viestityyppimerkki (StageStarted, Progress, StageCompleted, jne.)
- **Tieto ** Ihmisen luettavissa oleva kuvaus
- **Details** — Full JSON payload of the event data

### Värikoodi

Väri
|-------|---------|
Sininen ()
Vihreä ()
punainen ()
Valkoinen (oletusarvo)

### Yhteystila

Tilamainos ylhäällä osoittaa:
- **Connecting** — Establishing SignalR connection
- **Connected** — Receiving events normally
- **Reconnecting** — Connection lost, attempting to reconnect
- **Disconnected** — Connection closed

Yhdistys käyttää automaattista uudelleenliitäntää eksponentiaaliseen backoffiin: 0s, 2s, 5s, 10s, 30s.

### Tarkastukset

- ** Clear Feed**
- **Export JSON**
- ** Viestilaskuri ** Näytä kaikki tässä istunnossa vastaanotetut tapahtumat

## Signaali R-keskiö

Kojelauta liittyy:

```javascript
const connection = new signalr.HubConnectionBuilder()
    .withUrl("/hubs/localization")
    .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
    .build();
```

### Viestisopimus

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

### Tapahtumatyypit

Kojelauta käsittelee kaikki arvot:

Tyyppi
|------|---------|
Sininen virkamerkki
Vihreä merkki
Punainen virkamerkki
Vihreä merkki
Punainen virkamerkki
Infomerkki
Varoitusmerkki

## Tekninen toteutus

### Taustaosa

- ** Sijainti Hub** ()
- ** ISignalRPublisher ** ..
- **SignalRPublisher** ... Oletustoteutus, joka lisää monotoninen sekvenssi ja lähetykset

### Etusivu

- Puhdasta HTML/JS:ää, jossa on Bootstrap 5 -tyyli
- Käyttää Microsoft SignalR JavaScript -ohjelmakirjastoa (ladattu CDN:stä)
- Tapahtumasyötteelle ei tarvita palvelimen puoleista renderöintiä

### Sivun rakenne

```
Dita.Server/Pages/Admin/
├── LiveTranslation.cshtml      — Razor page markup + JavaScript
└── LiveTranslation.cshtml.cs     — Page model (empty, data comes via SignalR)
```

## Käyttö kehityksen aikana

1. Käynnistä Dita. Palvelinsovellus
2. Siirry
3. Käynnistä käännös ajaa (joko odottaa aikataulu tai soita API)
4. Katso tapahtumia reaaliajassa
5. Käytä Vie-painiketta tallentaaksesi täydellisen jäljen vianetsintää varten

## Tulevat parannukset

Kojelautaan suunnitellut parannukset:

- **Valtuutus** ..
- **Filtering** — Filter events by stage, type, or run ID
- ** Historialliset ajot** ... Näytä valmistuneet ajot tietokannasta tai lokitiedostosta
- **Tilastot**
- ** Manuaaliset laukaisimet** ..
- **Configuration** — Edit `AutomaticTranslationSettings` directly from the dashboard
- **Kielenhallinta**
- **Esikatselu ** Selaa ja etsi lokalisointi sanakirjoja

## Vianmääritys

### Dashboard näyttää "Ei voitu yhdistää"

1. Varmista palvelin on käynnissä ja käytettävissä
2. Tarkista CORS-selainkonsoli tai verkkovirheet
3. Vahvista
4. Varmista, ettei mikään palomuuri estä WebSocket-yhteyksiä

### Tapahtumat eivät näy

1. Tarkista, että SignalR-näppäin täsmää palvelimen () ja asiakkaan () välillä
2. Tarkista aikataulu on käytössä
3. Katso palvelimen lokit käännös putkessa virheitä
4. Tarkista selain Verkkovälilehti WebSocket- viesteihin

### Viestit eivät toimi

Kenttä takaa tilauksen yhdellä juoksulla. Jos viestit eivät toimi, se voi merkitä:
- Useita putkisto kulkee päällekkäin (ei pitäisi tapahtua semafore lukko)
- Selaimen renderointi ongelmia (yritä päivittää sivua)
