# Traduzioni in tempo reale

Questo documento esiste come input live test per la pipeline di traduzione automatica.

## Cosa fa il servizio

Il servizio viene eseguito in un programma e convalida il server di traduzione, la configurazione e le lingue disponibili prima dell'inizio del lavoro di traduzione.

Dopo la fase di convalida, sincronizza i nomi dei paesi dal catalogo di sola lettura nei dizionari JSON di localizzazione standard. Se la lingua di default dell'applicazione è l'inglese, l'ingresso del paese viene memorizzato come valore chiave uguale. Se la lingua predefinita è diversa, il nome del paese inglese viene tradotto per la prima volta nella lingua predefinita, e solo allora memorizzato come valore di parità chiave nel dizionario predefinito.

Successivamente, il servizio confronta il dizionario di localizzazione predefinito corrente con l'istantanea memorizzata dall'esecuzione precedente. Le voci appena aggiunte sono tradotte in lingue di destinazione solo quando la chiave non esiste già, quindi le traduzioni manuali tengono la priorità. Le voci eliminate vengono eliminate da tutti i dizionari di destinazione per mantenere l'intero set coerente.

Infine, le scansioni di servizio configurate radici di documentazione per alberi Markdown. Ogni cartella di argomento dovrebbe contenere un file di origine chiamato dopo la lingua predefinita, come en.md. Il servizio hashes che il file sorgente, rileva i cambiamenti, traduce i file Markdown mancanti o obsoleti e memorizza l'hash corrente accanto al file sorgente. Se la scrittura dell'hash accanto al file sorgente non è possibile, si rientra nella conservazione temporanea.

## Come il servizio segnala i progressi

Il backend emette messaggi SignalR generali attraverso il hub di localizzazione utilizzando una busta di messaggio. Ogni messaggio porta un tipo di messaggio, la fase di processo corrente, un timestamp UTC, un riassunto del testo e un carico di pagamento specifico per fase opzionale.

Le fasi attuali sono:

- CheckServer
- TraduttorePaesi
- TraduzioneJsonFiles
- TraduzioneMarkdownFiles
- risultato di stoccaggio

Il flusso di messaggi tipici è iniziato fase, fase completato e pipeline completato. Se una fase non riesce, il messaggio viene contrassegnato come un errore e include informazioni di errore strutturate con codici di errore unificati.

## Principi di progettazione

Le traduzioni vengono elaborate in modo sequenziale per evitare il sovraccarico del server LibreTranslate.

I dizionari JSON di localizzazione sono sempre memorizzati con chiavi in ordine alfabetico e JSON formattato per una manutenzione più semplice.

L'istantanea del dizionario predefinito precedente viene memorizzata in modo persistente in modo che un riavvio dell'applicazione non perda il monitoraggio del cambiamento.

**Le traduzioni manuali hanno sempre la priorità rispetto alle aggiunte automatiche.**
