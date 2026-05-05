# Traduzioni in tempo reale

Questo documento esiste come input live test per la pipeline di traduzione automatica. Qualsiasi cambiamento a questo file innesca la ritraslazione di tutti i file di lingua di destinazione sul prossimo run programmato.

## Panoramica dell'architettura

La pipeline di traduzione è stata ristrutturata in un'architettura modulare con quattro sottoservizi specializzati coordinati da un orchestratore leggero:

- **BackendTranslationService** — Orchestra l'intera pipeline, gestisce la validazione del server e i delegati lavorano a sotto-servizi.
- **CountriesTranslationService** — Sincronizza i nomi dei paesi da in dizionari per lingua.
- **LocalizationTranslationService** — Rileva i tasti aggiunti / rimossi nel dizionario JSON predefinito e li traduce in lingue di destinazione.
- **DocumentsTranslationService** — Traduci i file di documentazione Markdown con monitoraggio per blocco e metadati.

Ogni sotto-servizio opera in modo indipendente e riporta i progressi attraverso SignalR in tempo reale.

## Cosa fa il servizio

Il servizio viene eseguito su un programma ed esegue una pipeline di cinque stadi: convalida del server, sincronizzazione del paese, sincronizzazione del dizionario JSON, traduzione del file Markdown e persistenza dei risultati. Ogni fase emette eventi di progresso in tempo reale strutturati su Signal R in modo che i clienti collegati possano seguire come procedere di lavoro.

## Stadi di tubazione

### Fase 1 — CheckServers

Prima di iniziare qualsiasi lavoro di traduzione, il servizio verifica che tutte le condizioni prestabilite sono soddisfatte:

- La sezione di configurazione deve essere presente e valida.
- Il server LibreTranslate deve rispondere entro una latenza accettabile.
- L'elenco delle lingue disponibili sul server di traduzione è recuperato.
- La lingua predefinita configurata deve essere presente in quella lista.
- I file locali mancanti JSON per qualsiasi lingua supportata vengono creati automaticamente.

Se un controllo fallisce, la pipeline si ferma immediatamente e viene emesso un messaggio.

### Fase 2 — TraduzioniPaesi

I nomi dei paesi sono mantenuti in sincronia da un catalogo di sola lettura () nella localizzazione dei dizionari JSON.

- Se la lingua di default dell'applicazione è inglese, ogni nome del paese viene memorizzato come senza traduzione.
- Se la lingua predefinita è qualsiasi altra lingua, il nome del paese inglese viene tradotto per la prima volta in quella lingua, e il risultato diventa l'ingresso nel dizionario predefinito.
- Dopo l'aggiornamento del dizionario predefinito, ogni voce di paese mancante in ogni dizionario di lingua di destinazione viene tradotto e salvato ** immediatamente per lingua**.
- Le voci già traslate sono conservate senza modifiche.
- Se una traduzione fallisce, il servizio si riferisce fino a 3 volte con 30 secondi di ritardo prima di passare alla lingua successiva.

### Fase 3 — TraduttoreJsonFiles

Il servizio confronta il dizionario di localizzazione predefinito corrente con una snapshot memorizzata dall'esecuzione precedente:

- **I tasti aggiunti** — le voci presenti nel default corrente ma assenti dall'istantanea — sono tradotte in ogni lingua di destinazione che non ha già una voce manuale per quella chiave.
- **Le chiavi rimosse** — le voci presenti nell'istantanea ma assenti dal default corrente — vengono eliminate da ogni dizionario di lingua di destinazione.
- Le traduzioni manuali prendono sempre la priorità. Se un dizionario di destinazione contiene già un valore per una chiave, tale voce viene lasciata invariata indipendentemente da quello che dice la fonte.
- **Ogni dizionario di lingua di destinazione viene salvato immediatamente dopo la sua traduzione completa**, piuttosto che aspettare che tutte le lingue finiscano.
- Se una traduzione fallisce per una lingua specifica, il servizio si ritira automaticamente. Solo errori persistenti (ad esempio, lingua non supportata) causano che la lingua venga ignorata.
- Dopo l'esecuzione, il dizionario predefinito corrente viene salvato come nuova snapshot per il confronto successivo.

Tutti i dizionari sono sempre memorizzati con chiavi in ordine alfabetico e JSON indentato per la leggibilità umana.

### Fase 4 — TraduciMarkdownFiles

Il servizio segue le radici della documentazione configurate (default: ) e tratta ogni file sorgente in modo ricorsivo:

1. Il contenuto del file sorgente è letto e un hash SHA-256 è calcolato.
2. Un file accanto alle tracce di origine per lingua, per blocco di stato di traduzione, consentendo ** ritraslazione accidentale** di solo blocchi falliti.
3. L'hash memorizzato dall'esecuzione precedente (kept in un file accanto al file sorgente, o in una posizione di fallback temporanea) è confrontato con l'hash corrente.
4. Per ogni lingua di destinazione, il file corrispondente viene anche controllato per l'integrità strutturale.
5. Qualsiasi file di destinazione mancante, ha un hash obsoleto, non riesce la validazione della struttura, o contiene blocchi non tradotti è in coda per la ritraslazione.
6. **Ogni lingua di destinazione è tradotta e salvata in modo indipendente** — se ceco riesce ma francese non riesce, il file ceco è ancora scritto su disco.
7. I file tradotti con successo sono convalidati per la parità strutturale con la fonte (equale conteggio delle voci, elementi dell'elenco, blocchi di codice, blockquotes, link, marcatori in grassetto/italico e tag HTML) prima che siano scritti su disco.
8. Se tutti i file di destinazione per una fonte riescono, il nuovo hash viene memorizzato accanto alla fonte. Se la scrittura accanto alla fonte fallisce (ad esempio nelle distribuzioni di sola lettura), l'hash rientra nella directory temporanea.
9. Se una traduzione di destinazione non riesce a convalidare, i metadati contrassegnano quei blocchi come non traslati in modo che vengano riattivati sul prossimo run.

### Fase 5 — StoringResults

Un consolidato viene assemblato e pubblicato. Esso comprende:

- UTC run start e timestamp di completamento.
- Contatori di file locali salvati JSON, salvato i file Markdown, file hash salvati, e errori di errore scrive.
- Eventuali errori di archiviazione raccolti durante l'esecuzione.
- Statistiche di traduzione per lingua (conto tradotto, conteggio saltato, conteggio errori).

## Segnale Busta di messaggio R

Ogni evento di progresso viene consegnato come un con i seguenti campi:

Campo
|-------|------|-------------|
Identificatore di correlazione per l'esecuzione corrente della pipeline
Contatore monotonico in esecuzione, a partire da 1
Tipo semantico del messaggio
Pipeline stadio il messaggio appartiene a
Ora UTC quando il messaggio è stato emesso
Se il messaggio rappresenta una condizione di errore
Riepilogo leggibile dall'uomo
Pagamenti specifici per la fase (oggetto di rapporto o null)

### Tipo di messaggio

Valore
|-------|------|---------|
0
1
2
3
4
5
6

### Stadi di tubazione

Valore
|-------|------|-------------|
0
1
2
3
4
5

### Flusso di messaggi tipici

```text
StageStarted  / CheckServers
Progress / CheckServers — Server latency: 42ms
StageCompleted / CheckServers
StageStarted  / TranslateCountries
Progress / TranslateCountries — Found 195 country names
Progress / TranslateCountries — Starting translations for 'cs'...
Progress / TranslateCountries — Saved dictionary for 'cs' (198 entries)
StageCompleted / TranslateCountries
StageStarted  / TranslateJsonFiles
Progress / TranslateJsonFiles — Detected 3 added and 0 removed keys
Progress / TranslateJsonFiles — Starting JSON translations for 'cs'...
Progress / TranslateJsonFiles — Saved dictionary for 'cs' (201 entries)
StageCompleted / TranslateJsonFiles
StageStarted  / TranslateMarkdownFiles
Progress / TranslateMarkdownFiles — Scanning 2 source files in '/Docs'
Progress / TranslateMarkdownFiles — File 'en.md' has 12 translatable blocks
Progress / TranslateMarkdownFiles — Translating 'en.md' to 'cs'...
Progress / TranslateMarkdownFiles — Saved 'cs' translation for 'en.md' (12/12 blocks)
StageCompleted / TranslateMarkdownFiles
StageCompleted / StoringResults
PipelineCompleted / StoringResults
```

Se una fase fallisce, le fasi rimanenti sono saltate, viene emesso un messaggio, e infine un messaggio chiude la corsa.

## Traduzione logica

La pipeline implementa due livelli di resilienza:

### Rettifica a livello di stadio (TranslationRetryService)

- Se una richiesta di traduzione fallisce dopo i retries interni di LibreTranslate, l'esecuzione di fino a 3 ulteriori retries stage-level con ritardi di 30 secondi.
- Mascheramento segnaposto: I segnaposto nominati () nel testo vengono temporaneamente sostituiti con token sicuri () prima della traduzione e poi ripristinati, garantendo una corretta grammatica nelle lingue di destinazione.

### Validazione della lingua

- Prima di tradurre in una lingua di destinazione, il servizio verifica la lingua supportata dal server di traduzione.
- Le lingue non supportate vengono ignorate con un avviso, impedendo ripetuti tentativi falliti.

### Rettifica a livello di blocco Markdown

- Le traduzioni di Markdown vengono eseguite blocco per blocco (intestazioni, paragrafi, elementi di elenco).
- Se un singolo blocco non riesce a tradurre, è contrassegnato come non tradotto nel file dei metadati e ricucito sul prossimo processo pipeline.
- Il servizio traccia per lingua, lo stato per blocco nei file accanto a ogni file di origine Markdown.

## Codici di errore

Gli errori vengono segnalati utilizzando un enum unificato raggruppato in intervalli:

Ampiezza
|-------|----------|
1000–1999
2000-2999
3000–3999
4000–4999
5000–5999

Ogni errore in un rapporto porta l'identificatore sorgente (codice della lingua, percorso del file o nome della fase), il codice di errore e un messaggio leggibile dall'uomo.

## Traduzione dal vivo Dashboard

Il progetto Server include una pagina di amministrazione che si collega al hub SignalR e visualizza tutti gli eventi pipeline in tempo reale.

- Visualizza lo stato di connessione, il numero di messaggi e un tavolo live-updating di tutti gli eventi.
- Righe codificate a colori: blu per inizio fase, verde per il completamento, rosso per gli errori.
- Supporta la cancellazione del feed e l'esportazione di tutti i messaggi a JSON.
- Ricollegamento automatico con backoff esponenziale se la connessione scende.

## Principi di progettazione

- **Modularità**: Ogni preoccupazione di traduzione è isolata nel proprio servizio per manutenbilità e testabilità.
- ** Persistenza fondamentale ** I file Dictionaries e Markdown vengono salvati immediatamente dopo la traduzione, riducendo la pressione della memoria e fornendo un feedback precedente.
- **Resilience**: I livelli di retry multipli (HTTP, stadio, blocco) assicurano che i guasti transitori non blocchino la pipeline.
- **State tracking**: Per-file metadata (`.translation-meta.json`) and hash files enable precise incremental work on subsequent runs.
- **Real-time visibility**: Every significant operation is reported via SignalR for monitoring and debugging.
- **Manual translations always have priority over automatic additions.**
