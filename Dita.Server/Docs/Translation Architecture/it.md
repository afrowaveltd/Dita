# Architettura della traduzione

Questo documento descrive l'architettura modulare del sistema di traduzione automatica di Dita, introdotto per migliorare la manutenbilità, la testabilità e la resilienza.

## Obiettivi di progettazione

Il refactoring ha affrontato diverse preoccupazioni con il design monolitico originale:

- **Separazione delle preoccupazioni**: Ogni dominio di traduzione (conti, dizionari JSON, Markdown) è isolato.
- ** Persistenza fondamentale ** I file vengono salvati per lingua immediatamente dopo la traduzione, riducendo l'utilizzo della memoria e fornendo risultati precedenti.
- **Resilience**: I livelli di retry multipli gestiscono fallimenti transitori senza bloccare l'intero gasdotto.
- **Observability**: Every significant operation is reported via SignalR for real-time monitoring.
- **Extensibility**: New translation targets can be added by implementing a single interface.

## Decomposizione del servizio

### BackendTranslationService (orchestratore)

**Responsibilities**:
- Gestione del ciclo di vita pipeline (start, completamento, gestione degli errori)
- Semaphore-based concurrency control (previene sovrapposizioni piste)
- Convalida del server (latenza, disponibilità della lingua, configurazione)
- Delegazione ai sottoservizi

** NON contiene **
- Traduzione logica
- File I/O per formati specifici
- Logica della ricerca

### Servizi di traduzione

**Responsibilities**:
- Leggi dalla directory
- Sincronizzare i nomi dei paesi nel dizionario locale predefinito
- Tradurre nomi di paese mancanti per lingua di destinazione
- Salvare ogni dizionario di destinazione immediatamente dopo la traduzione

**Key behaviors**:
- Se la lingua predefinita è l'inglese: i nomi di paese memorizzati come-is
- Se la lingua predefinita è un'altra: i nomi inglesi tradotti in lingua predefinita
- Ogni lingua viene elaborata in modo indipendente con il proprio loop di riprovazione

### LocalizzazioneTranslationService

**Responsibilities**:
- Rilevare i tasti aggiunti / rimossi confrontando il dizionario predefinito corrente con l'istantanea precedente
- Tradurre i tasti aggiunti in ogni lingua di destinazione
- Rimuovere i tasti cancellati da ogni lingua di destinazione
- Salva snapshot per il prossimo confronto

**Key behaviors**:
- Traduzioni manuali prendono sempre la priorità (mai sovrascritta)
- Le chiavi aggiunte sono tradotte e salvate immediatamente per lingua
- I tasti rimossi vengono cancellati immediatamente per lingua
- L'istanza viene salvata solo dopo che tutte le lingue completano con successo

### DocumentiServizio di traduzione

**Responsibilities**:
- Passeggiata configurato Markdown radici ricorrenti
- Rileva i file di origine modificati utilizzando le hash SHA-256
- Traccia lo stato di traduzione per blocco in
- Traduci blocco per blocco con riprovazione per blocco
- Convalida la struttura Markdown dopo la traduzione
- Salvare ogni file di lingua di destinazione in modo indipendente

**Key behaviors**:
- Granularità a livello di blocco: voci, paragrafi, elementi di elenco sono tradotti separatamente
- Le tracce dei metadati che bloccano successo/fallito per lingua
- I blocchi falliti vengono riattivati sulla prossima corsa senza ritrasformare i blocchi di successo
- La validazione della struttura garantisce conteggi delle voci, liste, blocchi di codice, ecc

## Strategia di recupero

Il sistema implementa i ripetitori a tre livelli:

### Livello 1 — HTTP (LibreTranslateService)

- Fino a 5 tentativi con backoff esponenziale (1s, 2s, 3s, 4s, 5s)
- Maniglie timeout di rete, errori 5xx e guasti transitori
- Costruito nella configurazione client HTTP

### Livello 2 — Fase (TranslationRetryService)

- Fino a 3 tentativi con ritardi di 30 secondi
- Re-drive l'intera richiesta di traduzione dopo che i ripetitori a livello HTTP sono esauriti
- Il segnaposto mascheramento e restauro è applicato a questo livello

### Livello 3 — Blocco (Servizio di traduzione dei documenti)

- Blocchi singoli Markdown che non riescono sono contrassegnati in metadati
- Recuperato automaticamente sul prossimo processo di tubazione
- I blocchi di successo non sono mai ritraslati

## Flusso di dati

### Traduzione del dizionario JSON

```
[Default Dictionary]
       ↓
[Compare with Snapshot]
       ↓
[Detect Added/Removed Keys]
       ↓
For each target language:
   ├─ Load existing dictionary
   ├─ Apply removals
   ├─ Translate additions (with retry)
   └─ Save dictionary immediately
       ↓
[Save new snapshot]
```

### Traduzione di Markdown

```
[Source File]
       ↓
[Compute Hash]
       ↓
[Compare with stored hash]
       ↓
[Load translation metadata]
       ↓
For each target language:
   ├─ Extract translatable blocks
   ├─ For each block:
   │   ├─ Check metadata (already translated?)
   │   ├─ Translate with retry
   │   └─ Mark success/failure in metadata
   ├─ Reconstruct Markdown
   ├─ Validate structure
   ├─ Save target file
   └─ Save metadata
```

### Traduzione di nome di paese

```
[countries.json]
       ↓
[Build set of country keys]
       ↓
[Update default dictionary]
       ↓
For each target language:
   ├─ Find missing country entries
   ├─ Translate each missing entry (with retry)
   └─ Save dictionary immediately
```

## Persistenza dello Stato

### Istantanee

- **JSON**: memorizzato in un file accanto al dizionario predefinito (il nome varia da provider di archiviazione)
- **Purpose**: Consente la sincronizzazione incrementale tracciando ciò che era presente nel run precedente

### File Hash

- **Markdown**: accanto al file sorgente
- **Fallback**: se la posizione principale è di sola lettura
- **Purpose**: Rileva le modifiche della fonte per evitare inutili ritrasmissioni

### Metadati di traduzione

- **Markdown**: `{sourceFile}.translation-meta.json`
- **Contenuti **
  - Contenuto sorgente hash
- Stato del blocco per lingua (array of booleans)
- Ultimo aggiornamento timestamp
- **Purpose**: Consente la ritraslazione parziale di soli blocchi falliti

### Portaoggetti

- **File**: `Locales/placeholders.json`
- **Contents**: Dizionario delle chiavi per le coppie di valore dei nomi dei segnaposto
- **Purpose**: Fornisce valori predefiniti per i segnaposto nominati in tutta l'applicazione

## Segnale R reporting

### Astrazione editoriale

servizi di traduzione decouples da SignalR specifici:

```csharp
public interface ISignalRPublisher
{
    Task PublishStageAsync<T>(Guid runId, ProcessStage stage, T data, ...);
    Task PublishMessageAsync(Guid runId, LocalizationMessageType type, ProcessStage stage, ...);
}
```

### Garanzia di successione

- I messaggi all'interno di un singolo run sono in sequenza monotonica
- I numeri di sequenza sono unici per-run via
- I clienti possono rilevare lacune o riordinare

### Mappatura del mozzo

```csharp
app.MapHub<LocalizationHub>("/hubs/localization");
```

## Punti di estensione

### Aggiungere un nuovo obiettivo di traduzione

1. Creare una nuova interfaccia con
2. Implementare l'interfaccia con logica specifica di dominio
3. Registrati in contenitore DI
4. Iniezione nel costruttore
5. Chiamata dopo le fasi esistenti

### Politica commerciale

Parametri del costruttore di override:

```csharp
services.AddSingleton<TranslationRetryService>(
    sp => new TranslationRetryService(
        sp.GetRequiredService<ILibreTranslateService>(),
        sp.GetRequiredService<IPlaceholderService>(),
        sp.GetRequiredService<ILogger<TranslationRetryService>>(),
        stageMaxRetries: 5,        // More retries
        stageRetryDelaySeconds: 60  // Longer delays
    ));
```

### Gestione dei segnaposto personalizzata

Implementazione per cambiare sintassi o storage dei segnaposto:

```csharp
public class CustomPlaceholderService : IPlaceholderService
{
    // Use {{name}} syntax instead of {name}
    // Store in database instead of JSON file
    // Encrypt sensitive placeholder values
}
```

## Configurazione

### appsettings.json

```json
{
  "AutomaticTranslationSettings": {
    "DefaultLanguage": "en",
    "IgnoredLanguages": ["auto", "detect"],
    "MarkdownRoots": ["/Docs", "/Help"],
    "AutomaticRun": true,
    "CheckingPeriod": 30,
    "WaitingTime": "00:00:00",
    "RequestThrottleMs": 80,
    "RequestTimeoutSeconds": 10
  }
}
```

### Sintonizzazione di runtime

Impostazione
|---------|---------|--------|
80
10
3
30

## Strategia di test

### Test di unità

Ogni sotto-servizio è testabile in modo indipendente:

- Mock per simulare successo/fallimento
- Mock per verificare la segnalazione
- Utilizzare directory temporanee per file I/O
- Verificare il comportamento di salvataggio per lingua

### Test di integrazione

- Funzionamento completo con l'istanza reale (locale) LibreTranslate
- Verificare il segnale I messaggi R vengono consegnati ai client collegati
- Test di prevenzione corrente (semaphore)
- Convalida la struttura Markdown dopo la traduzione

### Prove finali

- Traduzione del trigger tramite API o scheduler
- Verificare tutti i file di lingua di destinazione sono creati/aggiornamento
- Controllare i file metadati contengono stato corretto del blocco
- I segnaposto confermati sono conservati tra le traduzioni

## Considerazioni di performance

- **Memory**: Il salvataggio in lingua impedisce di tenere tutti i dizionari in memoria
- **Disk I/O**: Metadata files add small overhead but enable incremental work
- **Rete**: L'elaborazione sequenziale con il throttling impedisce la schiacciante LibreTranslate
- **CPU**: SHA-256 hashing e la convalida regex sono veloci rispetto alla latenza di traduzione
- **SignalR**: Messaggi leggeri, nessuna compressione di carico utile necessaria per i rapporti tipici

## Migrazione dal design monolitico

L'originale conteneva tutta la logica in una classe. Il percorso di migrazione:

1. Estrarre la logica del paese →
2. Estrarre JSON logica →
3. Estrarre Markdown logica →
4. Segnale di estratto R publishing →
5. Estrarre la logica retry →
6. Semplificare l'orchestratore solo per le delegazioni

Tutte le interfacce esistenti () rimangono invariate. I consumatori della pipeline non vedono cambiamenti di rottura.
