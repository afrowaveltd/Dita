# Sintesi delle modifiche al servizio di traduzione automatica

## Panoramica

Questo documento riassume tutte le modifiche apportate al servizio di traduzione automatica Dita, tra cui rifattori di architettura, nuove funzionalità, miglioramenti dell'osservanza e miglioramenti della localizzazione.

## Cambiamenti di architettura

### rifattore backendtranslationservice

Il monolitico è stato decomposto in quattro servizi specializzati coordinati da un orchestratore leggero:

- **BackendTranslationService** — Orchestratore Pipeline (valida server, delegazione scenica, gestione degli errori)
- **CountriesTranslationService** — Sincronizzazione del nome di paese (lingua di destinazione inglese →)
- **LocalizationTranslationService** — Sincronizzazione del dizionario JSON (tasti aggiunti/rimossi)
- **DocumentsTranslationService** — Traduzione della documentazione di Markdown con monitoraggio a livello di blocco
- **SignalRPublisher** — Report sui progressi in tempo reale tramite SignalR
- **TranslationRetryService** — Repertorio a livello di stadio con conservazione degli placeholder

### Vantaggi

- **Separazione delle preoccupazioni**: Ogni servizio gestisce un dominio di traduzione singolo
- **Maintainability** Le classi più piccole sono più facili da capire e da testare
- **Extensibility**: New translation targets can be added via interface implementation
- ** Affidabilità**: I servizi indipendenti forniscono un migliore isolamento dei guasti

## Nuove funzionalità

### Monitor di traduzione dal vivo

**Posizione **

Una nuova pagina di amministrazione che fornisce visibilità in tempo reale nella pipeline di traduzione:

- Visualizza tutti i segnali Eventi R come si verificano
- Tipo di messaggio codificato a colori (blue=started, green=completed, red=error)
- Banner di stato di connessione con ricollegamento automatico
- Contatore messaggi ed esportazione a JSON

### Destinatari nominati

Il sistema di localizzazione ora supporta segnaposto () per una migliore grammatica in diverse lingue:

```csharp
// Usage in code
var message = Localizer["WelcomeMessage", new Dictionary<string, string>
{
    ["userName"] = "John",
    ["count"] = "5"
}];
// Result: "Hello John, you have 5 new messages"
```

Caratteristiche:
- Valori dei segnaposto forniti a runtime o memorizzati in
- Mascheramento automatico / restauro durante la traduzione per prevenire la corruzione
- Backward compatibile con i segnaposto posizionali esistenti

### Traduzione incredibile

I file Markdown sono tradotti in modo incrementale:

- **Risparmio per lingua ** Ogni lingua di destinazione viene salvata immediatamente dopo la traduzione, riducendo la pressione della memoria
- **Block-level tracking**: traccia lo stato di traduzione per blocco
- **Ricerca selettiva ** Solo i blocchi falliti sono ritraslati sulla prossima corsa
- **Persistenza dei dati**: lo stato di traduzione sopravvive riavviamento delle applicazioni

### Logica di riprovazione avanzata

Tre livelli di resilienza:

1. **HTTP retry** (LibreTranslateService): 5 tentativi con backoff esponenziale (1s–5s)
2. **Stage retry** (TranslationRetryService): 3 tentativi aggiuntivi con ritardi di 30s
3. **Block retry** (DocumentsTranslationService): Blocchi di Markdown non funzionanti

### Reporting SignalR

Rapporti in tempo reale per tutte le operazioni di pipeline:

- Ogni fase pubblica eventi
- Progressi per lingua pubblicati come eventi
- Gli eventi di errore includono contesto dettagliato (fonte, codice di errore, messaggio)
- I numeri di sequenza garantiscono l'ordine entro ogni corsa

## Cambiamenti di configurazione

### appsettings.json

Nessun cambiamento di rottura. La configurazione esistente continua a funzionare:

```json
{
  "AutomaticTranslationSettings": {
    "DefaultLanguage": "en",
    "IgnoredLanguages": ["auto", "detect"],
    "MarkdownRoots": ["/Docs"],
    "AutomaticRun": true,
    "CheckingPeriod": 30,
    "WaitingTime": "00:00:00"
  }
}
```

### Nuovi servizi

Registrato in:

- /
- `TranslationRetryService`
- /
- /
- /
- /

Il segno R hub è mappato per le connessioni client.

## test di prova

### Stato di prova

- **243/244 test di passaggio** (1 saltato a causa dell'accesso al file concomitante in ambiente di prova)
- Nuova copertura di prova aggiunta per:
  - Portaoggetti Funzionalità di servizio
  - IndietroTraduzione Orchestrazione di servizio
  - JsonStringLocalizer segnaposto indici

### Limitazioni conosciute

- test viene saltato quando si esegue in parallelo perché più istanze di test condividono lo stesso file. Passa quando viene eseguito in isolamento.

## Nuova struttura di file

### Servizi in

- — Orchestratore di tubatura
- — Traduzione di un nome di paese
- — Sincronizzazione del dizionario JSON
- — Traduzione Markdown
- — Segnale Pubblicazione di un messaggio
- — Promuovere la logica con la maschera del segnaposto
- — Interfaccia editoriale
- — Interfaccia di servizio di paese
- — Interfaccia di servizio di localizzazione
- — Interfaccia di servizio del documento
- — Interfaccia orchestratrice (aggiornata)
- — metadati di traduzione per file

### Servizi aggiornati

- — Aggiunto il supporto del segnaposto
- — Aggiornato per il nuovo parametro
- — Denominato gestione degli azionisti
- — Interfaccia segnaposto

### Nuova pagina di amministrazione

- — Pagina di monitoraggio in tempo reale
- — Modello di pagina

### Nuova documentazione

- — Documentazione aggiornata delle tubazioni
- — Guida del sistema di segnaposto
- — Guida all'uso di Dashboard
- — Panoramica dell'architettura tecnica

## Compatibilità

Tutti i cambiamenti sono additivi:

- Il codice di localizzazione esistente () funziona invariato
- Formattazione posizionale () funziona invariata
- Il formato del dizionario JSON esistente è invariato
- La struttura esistente di Markdown è invariata
- Segnale I messaggi R utilizzano lo stesso formato

## Percorso di migrazione

Nessuna migrazione richiesta. Il refactoring è interno:

1. Vecchio fu conservato come riferimento e poi sostituito
2. Le registrazioni DI sono state aggiornate per usare nuove interfacce
3. Tutti i consumatori esistenti non vedono modifiche

## Miglioramenti delle prestazioni

- ** Utilizzo della memoria ridotta ** File salvati per lingua immediatamente invece di tenere tutti in memoria
- **Faster incremental runs**: Only changed/failed Markdown blocks are re-translated
- ** Migliore visibilità ** Il progresso in tempo reale aiuta a diagnosticare le fasi lente

## Miglioramenti futuri

Miglioramenti pianificati:

1. **AI fine-tuning** — Revisione della traduzione automatica per le frasi > 5 parole
2. **Aautenticazione di amministratore** — Limitare le pagine di amministrazione agli utenti autorizzati
3. **Dictionary editor** — Web UI per la gestione delle chiavi di localizzazione
4. **Statistiche di traduzione** — Grafico che mostra i conti di traduzione e i tassi di errore nel tempo
5. **Custom placeholder syntax** — Supporto per formati placeholder alternativi

## Contatto

Per domande o problemi con il servizio di traduzione, fare riferimento alla documentazione dettagliata nella directory di ciascun modulo o contattare il team di sviluppo.
