# bord de traducere live

Live Translation Dashboard este o pagină admin care oferă vizibilitate în timp real în conducta de traducere automată. Se conectează la hub-ul SignarR și afișează toate evenimentele de conducte în timp ce acestea apar.

## URL

`/Admin/LiveTranslation`

> Note: Authentication and authorization are not yet implemented. Future versions will restrict this page to admin users only.

## Caracteristici

### Fluxul de evenimente în timp real

Toate evenimentele SignalR de la conducta de traducere sunt afișate într-un tabel live-updated:

- **Secvenţa numărul**
- **Timestamp**
- **Run ID**
- **Stage**
- **Tip**
- **Mesaj**
- **Detalii**

### Codificarea culorilor

Culoare
|-------|---------|
Albastru ()
Verde ()
roșu ()
Alb (default)

### Starea conexiunii

Un banner de stare în top arată:
- ** Conectarea**
- **Conectat**
- ** Reconectarea**
- ** Deconectat**

Conexiunea folosește reconectare automată cu exponențial backoff: 0s, 2s, 5s, 10s, 30s.

### Controale

- **Clear Feed**
- **Export JSON**
- **Mesaj contra**

## Conector de semnal

Tabloul de bord se conectează la:

```javascript
const connection = new signalr.HubConnectionBuilder()
    .withUrl("/hubs/localization")
    .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
    .build();
```

### Contractul de mesaj

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

### Tipuri de evenimente

Tabloul de bord se ocupă de toate valorile:

Tip
|------|---------|
Insignă albastră
Insignă verde
Insignă roșie
Insignă verde
Insignă roșie
Insigna de informații
Insigna de avertizare

## Implementarea tehnică

### Platformă

- **LocalizareHub** ()
- **ISignalRPublisher**
- **SignalRPublisher**

### Frontend

- Pure HTML/JS cu Bootstrap 5 styling
- Utilizeaza biblioteca de clienti Microsoft SignalR JavaScript (incarcata de pe CDN)
- Nu este necesară redarea pe server pentru feed-ul evenimentului

### Structura paginii

```
Dita.Server/Pages/Admin/
├── LiveTranslation.cshtml      — Razor page markup + JavaScript
└── LiveTranslation.cshtml.cs     — Page model (empty, data comes via SignalR)
```

## Utilizarea în timpul dezvoltării

1. Porneşte Dita. Aplicație server
2. Navighează la
3. Declanşează o cursă de traducere (fie să aştepţi programatorul, fie să suni la API)
4. Urmăriți evenimentele care apar în timp real
5. Utilizați butonul Export pentru a captura o urmă completă pentru depanare

## Îmbunătăţiri viitoare

Îmbunătăţiri planificate pentru tabloul de bord:

- **Autentificare**
- **Filtering**
- ** Rulează historic**
- **Statistics** — Charts showing translation counts, error rates, and latency over time
- ** Declanşatoare manuale**
- **Configurare**
- **Managementul limbii**
- **Dictionar de previzualizare**

## Depanare

### Tabloul de bord arată "Nu a reușit să se conecteze"

1. Verificați dacă serverul rulează și este accesibil
2. Verificați consola browser pentru CORS sau erori de rețea
3. Confirmaţi că este prezent în
4. Asigurați-vă că niciun firewall nu blochează conexiunile WebSocket

### Evenimentele nu apar

1. Verificați dacă URL-ul hubului SignalR se potrivește între server () și client ()
2. Verificați programatorul este activat în
3. Uită-te la jurnalele serverului pentru erori de conducte de traducere
4. Verificați fila de rețea a browser-ului pentru mesajele WebSocket

### Mesajele sunt deplasate

Câmpul garantează comandarea într-o singură cursă. În cazul în care mesajele nu sunt în ordine, acestea pot indica:
- Conducte multiple se suprapun (nu ar trebui să se întâmple din cauza blocare semafor)
- Comment
