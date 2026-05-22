# Traduzione dal vivo Dashboard

The Live Translation Dashboard è una pagina di amministrazione che fornisce visibilità in tempo reale nel canale di traduzione automatico. Si collega al hub SignalR e visualizza tutti gli eventi pipeline come si verificano.

## URL PAGINA

`/Admin/LiveTranslation`

> Note: Authentication and authorization are not yet implemented. Future versions will restrict this page to admin users only.

## Caratteristiche

### Stream eventi in tempo reale

Tutti gli eventi SignalR della pipeline di traduzione vengono visualizzati in una tabella live-updating:

- **Numero di sequenza** — Contatore monotonico in ogni processo di tubazione
- **Timestamp** — Ora locale quando l'evento è stato ricevuto
- **Run ID** — GUID abbreviato per correlazione
- **Stage** — Tassino di stadio Pipeline (CheckServers, TranslateCountries, ecc.)
- **Tipo** — Tasso di tipo messaggio (StageStarted, Progress, StageCompleted, ecc.)
- **Messaggio** — Descrizione leggibile dall'uomo
- **Dettagli** — Pieno carico JSON dei dati dell'evento

### Codifica colore

Colore
|-------|---------|
Blu ()
Verde ()
Rosso ()
Bianco (default)

### Stato di connessione

Uno stato banner in alto mostra:
- **Connecting** — Creazione di connessione SignalR
- **Connected** — Ricezione di eventi normalmente
- **Reconnecting** — Collegamento perso, tentativo di riconnettersi
- **Disconnected** — Collegamento chiuso

La connessione utilizza la riconnessione automatica con backoff esponenziale: 0s, 2s, 5s, 10s, 30s.

### Controlli

- **Clear Feed** — Rimuove tutti i messaggi visualizzati e ripristina il contatore
- **Esporta JSON** — Scarica tutti i messaggi ricevuti come file JSON per l'analisi
- **Message counter** — Mostra il numero totale di eventi ricevuti in questa sessione

## Mozzo Signal

Il cruscotto si collega a:

```javascript
const connection = new signalr.HubConnectionBuilder()
    .withUrl("/hubs/localization")
    .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
    .build();
```

### Contratto di messaggio

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

### Tipo di evento

Il cruscotto gestisce tutti i valori:

Tipo
|------|---------|
Badge blu
Tasso verde
Badge rosso
Tasso verde
Badge rosso
Info badge
Tasso di avvertimento

## Attuazione tecnica

### Indietro

- **LocalizationHub** () — Mozzo SignalR che trasmette messaggi a tutti i client connessi
- **ISignalRPublisher** — Astratto sul mozzo da utilizzare nei servizi di traduzione
- **SignalRPublisher** — implementazione predefinita che incrementa una sequenza monotonica e trasmette

### Fronte

- Pure HTML/JS con Bootstrap 5 styling
- Utilizza la libreria client JavaScript Microsoft SignalR (caricata da CDN)
- Nessun rendering lato server richiesto per il feed dell'evento

### Struttura della pagina

```
Dita.Server/Pages/Admin/
├── LiveTranslation.cshtml      — Razor page markup + JavaScript
└── LiveTranslation.cshtml.cs     — Page model (empty, data comes via SignalR)
```

## Utilizzo durante lo sviluppo

1. Inizia la Dita. Applicazione server
2. Navigare per
3. Prova un'esecuzione di traduzione (sia aspettare il programmatore o chiamare l'API)
4. Guarda gli eventi appaiono in tempo reale
5. Utilizzare il pulsante Esporta per catturare una traccia completa per il debug

## Miglioramenti futuri

Miglioramenti pianificati per il cruscotto:

- **Authentication** — Limitare l'accesso agli utenti con il ruolo
- **Filtering** — Filtra eventi per fase, tipo, o eseguire ID
- **Esecuzioni storiche** — Visualizza le operazioni completate da un database o file di registro
- **Statistics** — Grafico che mostra i conti di traduzione, i tassi di errore e la latenza nel tempo
- **Manual trigger** — Pulsanti per avviare manualmente specifiche fasi di pipeline
- **Configurazione** — Modifica direttamente dal cruscotto
- **Language management** — Visualizza e modifica le lingue supportate
- **Anteprima differenziata** — Sfoglia e ricerca dizionari di localizzazione

## Risoluzione dei problemi

### Dashboard mostra "Failed to connect"

1. Verificare che il server sia in esecuzione e accessibile
2. Controllare la console del browser per CORS o errori di rete
3. Conferma è presente in
4. Assicurarsi che nessun firewall blocca le connessioni WebSocket

### Eventi non sono in mostra

1. Controllare che l'URL del hub di SignalR corrisponda tra server () e client ()
2. Verificare che il programmatore sia abilitato
3. Guarda i log dei server per gli errori di pipeline di traduzione
4. Controlla la scheda di rete del browser per i messaggi WebSocket

### I messaggi sono fuori ordine

Il campo garantisce l'ordine entro una singola corsa. Se i messaggi appaiono fuori ordine, può indicare:
- Più condotte funziona sovrapposte (non deve accadere a causa di serratura semafora)
- Problemi di rendering del browser (prova di aggiornare la pagina)
