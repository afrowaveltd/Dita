# živý preklad prístrojová doska

Live Translation Dashboard je admin stránka, ktorá poskytuje v reálnom čase viditeľnosť do automatického prekladu potrubia. Spája sa s uzlom SignalR a zobrazí všetky udalosti súvisiace s potrubím.

## URL

`/Admin/LiveTranslation`

> Note: Authentication and authorization are not yet implemented. Future versions will restrict this page to admin users only.

## Vlastnosti

### Stream udalostí v reálnom čase

Všetky udalosti SignalR z prekladového potrubia sú zobrazené v live-aktualizačnej tabuľke:

- **Číslo sekvencie**
- ** Časová pečiatka**
- **Run ID**
- **Stage**
- **Type**
- **Message**
- **Detaily**

### Farebný kód

Farba
|-------|---------|
Modrá ()
Zelená ()
Červená ()
Biela (predvolená)

### Stav pripojenia

Status banner na vrchu ukazuje:
- **Connecting**
- **Connected**
- ** Opätovné pripojenie**
- **Dis connected**

Pripojenie využíva automatické opätovné pripojenie s exponenciálnym výstupom: 0s, 2s, 5s, 10s, 30s.

### Kontroly

- **Clear Feed**
- **Export JSON**
- **Message counter**

## SignalR centrum

Palubná doska sa pripája k:

```javascript
const connection = new signalr.HubConnectionBuilder()
    .withUrl("/hubs/localization")
    .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
    .build();
```

### Zmluva o správe

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

### Typy udalostí

Palubná doska spracováva všetky hodnoty:

Typ
|------|---------|
Modrý odznak
Zelený odznak
Červený odznak
Zelený odznak
Červený odznak
Info odznak
Výstražný odznak

## Technická implementácia

### Backend

- **LokalizáciaHub** ()
- **ISignalRPublisher**
- **SignalRPublisher**

### Frontend

- Čistý HTML / JS s Bootstrap 5 styling
- Používa knižnicu Microsoft SignalR JavaScript klienta (načítaná z CDN)
- Pre prenos udalostí nie je potrebné žiadne zobrazovanie na strane servera

### Štruktúra stránky

```
Dita.Server/Pages/Admin/
├── LiveTranslation.cshtml      — Razor page markup + JavaScript
└── LiveTranslation.cshtml.cs     — Page model (empty, data comes via SignalR)
```

## Využitie počas vývoja

1. Naštartuj Ditu. Aplikácia servera
2. Navigovať do
3. Spustiť prekladateľský beh (buď čakať na rozpisovač alebo volajte API)
4. Sledovať udalosti sa objavujú v reálnom čase
5. Použite tlačidlo Export na zachytenie celej stopy pre ladenie

## Budúce zlepšenia

Plánované zlepšenia prístrojovej dosky:

- **Autentifikácia**
- **Filterovanie**
- **Historické behy**
- ** Štatistika**
- **Manual triggers** — Buttons to manually start specific pipeline stages
- **Konfigurácia**
- ** Language management**
- **Dictionary preview**

## Riešenie problémov

### Dashboard zobrazuje "Failed to connect"

1. Overiť, či server beží a je prístupný
2. Skontrolujte konzolu prehliadača pre CORS alebo sieťové chyby
3. Potvrdiť prítomnosť
4. Uistite sa, že žiadny firewall blokuje pripojenie WebSocket

### Udalosti sa neobjavujú

1. Skontrolujte, či SignalR URL sa zhoduje medzi serverom () a klientom ()
2. Overte, či je programátor povolený
3. Pozrite sa na serverové protokoly pre chyby prekladu potrubia
4. Kontrola prehliadača Sieťová karta pre správy WebSocket

### Správy sú mimo prevádzky

Pole zaručuje objednanie v rámci jedného kola. Ak sa správy objavia mimo prevádzky, môžu sa v nich uviesť:
- Viacnásobné presahovanie vedenia potrubia (nemala by sa stať kvôli semaphore zámku)
- Prehliadač renderovanie otázky (skúste osviežiť stránku)
