# Otsetõlke juhtpaneel

Live Translation Dashboard on administraatori leht, mis pakub reaalajas nähtavust automaatsesse tõlketorustikku. See ühendub SignalR-i jaoturiga ja kuvab kõik torujuhtme sündmused nende toimumise ajal.

## URL

`/Admin/LiveTranslation`

> Note: Authentication and authorization are not yet implemented. Future versions will restrict this page to admin users only.

## Omadused

### Reaalaja sündmuste voog

Kõik SignalR sündmused tõlketorustikust kuvatakse reaalajas uuendavas tabelis:

- **Järjenumber** – monotoonne loendur igas torustikus
- **Ajatempel** – sündmuse saabumise kohalik aeg
- **Run ID** – Lühendatud GUID korrelatsiooni jaoks
- ** Lava** – torujuhtme lavamärk (CheckServers, TranslateCountries jne)
- **Type** – sõnumitüübi märk (StageStarted, Progress, StageCompleted jne)
- ** Sõnum** – inimloetav kirjeldus
- **Üksikasjad ** – sündmuse andmete täielik JSON-i kandevõime

### Värvikoodid

Värv
|-------|---------|
Sinine ()
Roheline ()
Punane ()
Valge (vaikimisi)

### Ühenduse olek

Staatuse bänner ülaosas näitab:
- **Ühendamine** – SignalR-ühenduse loomine
- **Ühendatud ** – sündmuste vastuvõtmine tavapäraselt
- ** Taasühendamine ** - ühendus on kadunud, proovitakse uuesti ühendada
- ** Disconnected** – ühendus suletud

Ühendus kasutab automaatset taasühendamist eksponentsiaalse varundamisega: 0s, 2s, 5s, 10s, 30s.

### Kontrollid

- ** Puhastusvoog ** - eemaldab kõik kuvatud sõnumid ja lähtestab loenduri
- **Eksport JSON ** - Laadib kõik vastuvõetud sõnumid analüüsimiseks alla JSON-failina
- ** Sõnumite loendur ** – näitab antud seansil saadud sündmuste koguarvu

## SignalR-i keskus

Armatuurlaud on ühendatud:

```javascript
const connection = new signalr.HubConnectionBuilder()
    .withUrl("/hubs/localization")
    .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
    .build();
```

### Sõnumileping

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

### Sündmuse tüübid

Armatuurlaud käsitleb kõiki väärtusi:

Tüüp
|------|---------|
Sinine märk
Roheline märk
Punane märk
Roheline märk
Punane märk
Infomärk
Hoiatusmärk

## Tehniline rakendamine

### Taustaprogramm

- **LocalizationHub** () – SignalR-jaotur, mis edastab sõnumeid kõigile ühendatud klientidele
- **ISignalRPublisher** – tõlketeenuste keskuses kasutatav abstraktsioon
- **SignalRPublisher** – vaikerakendus, mis suurendab monotoonset jada ja saateid

### Frontend

- Puhas HTML/JS koos Bootstrap 5 stiiliga
- Kasutab Microsoft SignalR JavaScripti klienditeeki (laaditud CDN-ist)
- Sündmusvoo jaoks ei ole vaja serveripoolset renderdamist

### Lehe struktuur

```
Dita.Server/Pages/Admin/
├── LiveTranslation.cshtml      — Razor page markup + JavaScript
└── LiveTranslation.cshtml.cs     — Page model (empty, data comes via SignalR)
```

## Kasutamine arendamise ajal

1. Pane Dita käima. Serverirakendus
2. Liikuge
3. Tõlkejooksu käivitamine (kas oodata planeerijat või helistada API-le)
4. Vaatamissündmused ilmuvad reaalajas
5. Nuppu Ekspordi saab kasutada silumise täieliku jälje jäädvustamiseks

## Edasised parandused

Armatuurlauale kavandatud parandused:

- **Autentimine ** – Juurdepääsu piiramine kasutajatega
- **Filtering** — Filter events by stage, type, or run ID
- ** Ajaloolised jookseb ** - Vaade lõpetatud jookseb andmebaasist või logifailist
- **Statistika ** - diagrammid, mis näitavad tõlkeloendust, veamäärasid ja latentsust aja jooksul
- **Käsitsivõtmed** – nupud konkreetsete torujuhtmeetappide käsitsi käivitamiseks
- ** Konfiguratsioon** – redigeerimine otse armatuurlaualt
- ** Keelehaldus** – toetatud keelte vaatamine ja redigeerimine
- ** Sõnastiku eelvaade** – sirvi ja otsi lokaliseerimissõnastikke

## Tõrkeotsing

### Armatuurlaud näitab "Ebaõnnestunud ühendus"

1. Kontrolli, kas server töötab ja on kättesaadav
2. Kontrollige brauserikonsooli CORS- i või võrguvigade korral
3. Kinnitan
4. Veenduge, et ükski tulemüür ei blokeeri WebSocketi ühendusi

### Sündmused ei ilmu

1. Kontrolli, kas SignalR hub URL sobib serveri () ja kliendi () vahel
2. Kontrolli, kas planeerija on sisse lülitatud
3. Vaata serveri logisid tõlkimise torustiku vigade kohta
4. Kontrollige veebilehitseja võrgukaarti WebSocketi sõnumite jaoks

### Sõnumid on korrast ära

Väli tagab tellimise ühe jooksu jooksul. Kui teated on ebakorrapärased, võib see näidata:
- Mitme torujuhtme läbimine kattub (semafori lukustuse tõttu ei tohiks juhtuda)
- Brauseri renderdamise probleemid (proovige lehte värskendada)
