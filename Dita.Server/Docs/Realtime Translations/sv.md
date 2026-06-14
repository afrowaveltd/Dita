# Översättningar i realtid

Detta dokument finns som en levande testingång för den automatiska översättningsledningen. Alla ändringar i denna fil utlöser återöversättning av alla målspråksfiler på nästa schemalagda körning.

## Arkitekturöversikt

Översättningsledningen har omstrukturerats till en modulär arkitektur med fyra specialiserade undertjänster som samordnas av en lätt orkestrator:

- **BackendTranslationService** – Orchestrerar hela rörledningen, hanterar servervalidering och delegerar arbete till undertjänster.
- **CountriesTranslationService** - Synkroniserar landsnamn från till perspråkiga ordböcker.
- **LocalizationTranslationService** – Detekterar adderade/avlägsna nycklar i standard-JSON-ordboken och översätter dem till målspråk.
- **DocumentsTranslationService** — Translates Markdown documentation files with per-block tracking and metadata.

Varje undertjänst fungerar oberoende och rapporterar framsteg via SignalR i realtid.

## Vad tjänsten gör

Tjänsten körs på ett schema och utför en femstegs pipeline: server validering, landsynkronisering, JSON ordbok synkronisering, Markdown filöversättning och kvarstår resultaten. Varje steg avger strukturerade realtidsframstegshändelser över SignalR så att uppkopplade kunder kan följa med som arbete fortsätter.

## Pipeline stadier

### Steg 1 - CheckServers

Innan något översättningsarbete påbörjas kontrollerar tjänsten att alla förutsättningar är nöjda:

- Konfigurationssektionen måste vara närvarande och giltig.
- LibreTranslate-servern måste svara inom en acceptabel latens.
- Listan över språk som finns på översättningsservern hämtas.
- Det konfigurerade standardspråket måste finnas i den listan.
- Saknar lokala JSON-filer för alla språk som stöds skapas automatiskt.

Om någon check misslyckas, stoppar rörledningen omedelbart och ett meddelande avges.

### Steg 2 – Översättning

Landsnamn hålls i synkronisering från en lättläst katalog () i lokaliseringen JSON ordböcker.

- Om applikationsstandardspråket är engelska lagras varje landsnamn som utan översättning.
- Om standardspråket är något annat språk översätts det engelska landsnamnet först till det språket, och resultatet blir inträdet i standardordboken.
- After the default dictionary is updated, each missing country entry in every target language dictionary is translated and saved **immediately per language**.
- Redan översatta poster bevaras utan modifiering.
- Om en översättning misslyckas går tjänsten upp till 3 gånger med 30 sekunders förseningar innan du flyttar till nästa språk.

### Steg 3 - TranslateJsonFiles

Tjänsten jämför den nuvarande standardlokaliseringsordboken med en ögonblicksbild lagrad från föregående körning:

- **Lägg till nycklar** - poster som finns i den nuvarande standarden men frånvarande från ögonblicksbilden - översätts till varje målspråk som inte redan har en manuell post för den nyckeln.
- ** Ta bort nycklar** – poster som finns i ögonblicksbilden men frånvarande från den nuvarande standarden – raderas från varje målspråksordbok.
- Manuella översättningar prioriterar alltid. Om en målordbok redan innehåller ett värde för en nyckel, lämnas den posten oförändrad oavsett vad källan säger.
- **Each target language dictionary is saved immediately after its translations complete**, rather than waiting for all languages to finish.
- Om en översättning misslyckas för ett visst språk, returnerar tjänsten automatiskt. Endast ihållande fel (t.ex. ostödda språk) gör att språket hoppas över.
- Efter körningen sparas den nuvarande standardordboken som den nya ögonblicksbilden för nästa jämförelse.

Alla ordböcker lagras alltid med alfabetiskt sorterade nycklar och indragna JSON för mänsklig läsbarhet.

### Steg 4 - TranslateMarkdownFiles

Tjänsten går de konfigurerade dokumentationsrötterna (standard:) och behandlar varje källfil återkommande:

1. Källans filinnehåll läses och en SHA-256 hash beräknas.
2. En fil bredvid källspår per språk, per block översättningsstatus, vilket möjliggör ** stegvis återöversättning** av endast misslyckade block.
3. Den lagrade hash från föregående körning (håll i en fil bredvid källfilen, eller i en tillfällig nedgångsplats) jämförs med den aktuella hashen.
4. För varje målspråk kontrolleras motsvarande fil också för strukturell integritet.
5. Alla målfiler som saknas, har en föråldrad hash, misslyckas struktur validering eller innehåller översatta block är köad för återöversättning.
6. **Each target language is translated and saved independently** — if Czech succeeds but French fails, the Czech file is still written to disk.
7. Framgångsrikt översatta filer valideras för strukturell paritet med källan (lika rubrikräkningar, listobjekt, kodblock, blockquotes, länkar, djärva/itala markörer och HTML-taggar) innan de skrivs till disk.
8. Om alla målfiler för en källa lyckas lagras den nya hashen bredvid källan. Om du skriver bredvid källan misslyckas (till exempel i lätta utplaceringar) faller hashen tillbaka till den tillfälliga katalogen.
9. Om någon målöversättning misslyckas med validering markerar metadata dessa block som oöversatta så att de hämtas på nästa körning.

### Steg 5 - StoringResults

En konsoliderad monteras och publiceras. Det inkluderar:

- UTC kör start och slutförande timestamps.
- Räknar av sparade lokal JSON-filer, sparade Markdown-filer, sparade hash-filer och fallback hash skriver.
- Alla lagringsfel som samlats in under loppet.
- Översättningsstatistik per språk (översatt räkning, hoppad räkning, felräkning).

## SignalR meddelandekuvert

Varje utvecklingsevenemang levereras som ett med följande fält:

Fält
|-------|------|-------------|
Korrelationsidentifierare för den aktuella rörledningen
Monotonic räknare inom en körning, börjar vid 1
Semantisk typ av meddelande
Pipeline scenen meddelandet tillhör
UTC-tid när meddelandet släpptes
Om meddelandet representerar ett feltillstånd
Mänsklig läsbar sammanfattning
Stegspecifik nyttolast (rapportera objekt eller null)

### Meddelandetyper

Värde
|-------|------|---------|
0
1
2
3
4.4 4
5.5
6

### Pipeline stadier

Värde
|-------|------|-------------|
0
1
2
3
4.4 4
5.5

### Typiskt meddelandeflöde

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

Om något skede misslyckas, de återstående stadierna hoppas, ett meddelande avges, och slutligen ett meddelande stänger loppet.

## Översättning retry logic

Rörledningen genomför två nivåer av motståndskraft:

### Steg-nivå retry (TranslationRetryService)

- Om en översättningsbegäran misslyckas efter LibreTranslates interna retries, utför upp till 3 ytterligare steg-nivå retries med 30 sekunders förseningar.
- Placeholder masking: Namngivna platshållare () i text ersätts tillfälligt med säkra tokens () före översättning och återställs efteråt, vilket säkerställer korrekt grammatik i målspråk.

### Språkvalidering

- Innan du översätter till ett målspråk verifieras språket av översättningsservern.
- Osupporterade språk hoppas med en varning, vilket förhindrar upprepade misslyckade försök.

### Markdown block-nivå retry

- Markdown översättningar utförs block-by-block (rubriker, punkter, listobjekt).
- Om ett enskilt block misslyckas med översättningen markeras det som översatt i metadatafilen och hämtas på nästa pipelinekörning.
- Tjänsten spårar per språk, per block status i filer bredvid varje källa Markdown fil.

## Felkoder

Fel rapporteras med hjälp av en enhetlig enumgrupp i intervall:

utbud
|-------|----------|
1000-1999
2000–2999
3000–3999
4000-4999
5000–5999

Varje fel i en rapport bär källidentifieraren (språkkod, filväg eller scennamn), felkoden och ett mänskligt läsbart meddelande.

## Live Translation Dashboard

Server-projektet innehåller en administratörssida som ansluter till SignalR-navet och visar alla pipelinehändelser i realtid.

- Visar anslutningsstatus, meddelanderäkning och en live-updating tabell över alla händelser.
- Färgkodade rader: blå för scenstart, grön för slutförande, röd för fel.
- Stöder clearing av fodret och exporterar alla meddelanden till JSON.
- Auto återansluter med exponentiell backoff om anslutningen sjunker.

## Designprinciper

- **Modularity**: Each translation concern is isolated in its own service for maintainability and testability.
- **Inkrementell uthållighet**: Ordböcker och Markdown filer sparas per språk omedelbart efter översättning, minska minnestrycket och ge tidigare återkoppling.
- **Resiliens**: Flera retrynivåer (HTTP, scen, block) säkerställer att övergående misslyckanden inte blockerar rörledningen.
- **State tracking**: Per-file metadata (`.translation-meta.json`) and hash files enable precise incremental work on subsequent runs.
- ** Realtidssynlighet**: Varje betydande operation rapporteras via SignalR för övervakning och felsökning.
- **Manliga översättningar har alltid prioritet framför automatiska tillägg.**
