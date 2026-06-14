# Real- time oversættelser

Dette dokument findes som et live testinput for den automatiske oversættelsesledning. Enhver ændring til denne fil udløser re- oversættelse af alle målsprogfiler på næste planlagte køre.

## Arkitektoversigt

Oversættelsen rørledningen er blevet omstruktureret til en modulær arkitektur med fire specialiserede subtjenester koordineret af en letvægts orkester:

- **BackendTranslationService** — Orchestrates the entire pipeline, handles server validation, and delegates work to sub-services.
- **CountriesTranslationService** — Synchronizes country names from `countries.json` into per-language dictionaries.
- **LocalizationTranslationService** — Detects added/removed keys in the default JSON dictionary and translates them into target languages.
- **DocumentsTranslationService** — Translates Markdown documentation files with per-block tracking and metadata.

Hver deltjeneste fungerer uafhængigt og rapporterer fremskridt via SignatalR i realtid.

## Hvad tjenesten gør

Tjenesten kører på en tidsplan og udfører en femtrins rørledning: server validering, land synkronisering, JSON ordbog synkronisering, Markdown fil oversættelse, og fastholde resultaterne. Hvert trin udsender strukturerede real- tid fremskridt begivenheder over SignatalR, så forbundne klienter kan følge med i arbejdet.

## Rørledningstrin

### Trin 1 - CheckServers

Inden oversættelsesarbejdet påbegyndes, kontrollerer tjenesten, at alle forudsætninger er opfyldt:

- Konfigurationsafsnittet skal være til stede og gyldigt.
- LibreTranslat- serveren skal reagere inden for en acceptabel latency.
- Listen over sprog tilgængelige på oversættelsesserveren er hentet.
- Det konfigurerede standardsprog skal være til stede i denne liste.
- Mangler locale JSON filer for alle understøttede sprog oprettes automatisk.

Hvis en kontrol mislykkes, stopper rørledningen straks, og der udsendes en meddelelse.

### Fase 2 - TranslateCountries

Landenavne holdes i synkronisering fra en read- only katalog () ind i lokalisering JSON ordbøger.

- Hvis applikationens standardsprog er engelsk, gemmes hvert lands navn som uden oversættelse.
- Hvis standardsproget er et andet sprog, oversættes det engelske landenavn først til dette sprog, og resultatet bliver indgangen i standardordbogen.
- After the default dictionary is updated, each missing country entry in every target language dictionary is translated and saved **immediately per language**.
- Alleoversatte indgange bevares uden ændringer.
- Hvis en oversættelse mislykkes, tjenesten forsøger op til 3 gange med 30-sekunders forsinkelser, før du flytter til det næste sprog.

### Trin 3 - TranslateJsonFiles

Tjenesten sammenligner den nuværende standard lokalisering ordbog med et øjebliksbillede gemt fra det foregående løb:

- **Added keys** — entries present in the current default but absent from the snapshot — are translated into every target language that does not already have a manual entry for that key.
- **Removed keys** — entries present in the snapshot but absent from the current default — are deleted from every target language dictionary.
- Manuelle oversættelser prioriteres altid. Hvis en målordbog allerede indeholder en værdi for en nøgle, er denne indgang uændret uanset hvad kilden siger.
- **Each target language dictionary is saved immediately after its translations complete**, rather than waiting for all languages to finish.
- Hvis en oversættelse mislykkes for et bestemt sprog, tjenesten forsøger automatisk. Kun vedvarende fejl (f.eks. ikke-understøttet sprog) får dette sprog til at springe over.
- Efter løbet gemmes den aktuelle standardordbog som det nye øjebliksbillede til næste sammenligning.

Alle ordbøger gemmes altid med alfabetisk sorterede nøgler og indrykkede JSON til menneskelig læsbarhed.

### Fase 4 - TranslateMarkdownFiles

Tjenesten går de konfigurerede dokumentationsrødder (standard:) og behandler hver kildefil rekursivt:

1. Kildefilens indhold er læst og en SHA- 256 hash er beregnet.
2. A `.translation-meta.json` file next to the source tracks per-language, per-block translation status, enabling **incremental re-translation** of only failed blocks.
3. Den gemte hash fra forrige løb (opbevares i en fil ved siden af kildefilen, eller i en midlertidig fallback placering) sammenlignes med den aktuelle hash.
4. For hvert målsprog kontrolleres den tilsvarende fil også for strukturel integritet.
5. Enhver målfil, der mangler, har en forældet hash, mislykkes struktur validering, eller indeholder uoversat blokke er i kø for genoversættelse.
6. **Each target language is translated and saved independently** — if Czech succeeds but French fails, the Czech file is still written to disk.
7. Succesfuldt oversat filer er valideret for strukturel paritet med kilden (lige overskrift tæller, liste poster, kode blokke, blokkitater, links, fed / kursiv markører, og HTML tags), før de er skrevet til disk.
8. Hvis alle målfiler for en kilde lykkes, er den nye hash gemt ved siden af kilden. Hvis skrivning ved siden af kilden mislykkes (for eksempel i read- only installationer), hash falder tilbage til den midlertidige mappe.
9. Hvis nogen måloversættelse mislykkes validering, metadata markerer disse blokke som uoversat, så de er igen prøvet på det næste løb.

### Fase 5 - StoringResults

En konsolideret er samlet og offentliggjort. Omfatter:

- UTC start og fuldførelse tidsstempler.
- Tæller gemte lokale JSON filer, gemte Markdown filer, gemte hash filer, og fallback hash skriver.
- Eventuelle lagerfejl indsamlet under kørslen.
- Per- sprog oversættelse statistik (oversat tæller, skippet tæller, fejltælling).

## Konvolut for signalR-meddelelser

Alle fremskridt er leveret som en med følgende områder:

Felt
|-------|------|-------------|
Korrelationsidentifikator for den aktuelle rørledning
Monotonisk tæller i løbet, startende ved 1
Meddelelsens semantiske type
Pipeline stadie meddelelsen tilhører
UTC-tid, hvor meddelelsen blev udsendt
Om brevet repræsenterer en fejltilstand
Humant læseligt resumé
Stage- specifik nyttelast (rapportobjekt eller null)

### Meddelelsestyper

Værdi
|-------|------|---------|
0
1
2
3
4
5
6

### Rørledningstrin

Værdi
|-------|------|-------------|
0
1
2
3
4
5

### Typiske meddelelser

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

Hvis noget stadie mislykkes, er de resterende stadier sprunget over, en meddelelse er udsendt, og endelig en meddelelse lukker løbet.

## Oversættelse retry logik

Rørledningen gennemfører to niveauer af modstandsdygtighed:

### Stage- level returforsøg (TranslationRetryService)

- Hvis en oversættelse anmodning mislykkes efter LibreTranslates interne reles, udfører op til 3 ekstra scene-niveau reles med 30-sekunders forsinkelser.
- Placeholder maskering: navngivet pladsholdere () i tekst er midlertidigt erstattet med sikre tokens () før oversættelse og restaureres bagefter, hvilket sikrer korrekt grammatik på målsprog.

### Sprogvalidering

- Før oversættelse til et målsprog, tjenesten kontrollerer sproget er understøttet af oversættelsesserveren.
- Ikke-understøttede sprog er sprunget med en advarsel, forhindre gentagne mislykkede forsøg.

### Markdown block- level relry

- Markdown oversættelser udføres block-by-block (overskrifter, afsnit, liste poster).
- Hvis en individuel blok mislykkes oversættelse, er det markeret som uoversat i metadatafilen og genprøvet på den næste pipeline køre.
- Tjenesten sporer per- sprog, per- blok status i filer ved siden af hver kilde Markdown-fil.

## Fejlkoder

Fejl rapporteres ved hjælp af et samlet enum grupperet i intervaller:

Område
|-------|----------|
1000- 1999
2000-2999
3000- 3999
4-4999
5000- 5999

Hver fejl i en rapport indeholder kildekoden (sprogkode, filsti eller scenenavn), fejlkoden og en menneskeligt læsbar meddelelse.

## Live oversættelse Dashboard

Server projektet indeholder en admin side på, der forbinder til SignalR hub på og viser alle rørledninger begivenheder i realtid.

- Viser tilslutningsstatus, meddelelsestælling og en live- updating tabel over alle begivenheder.
- Farvekodede rækker: blå for scenestart, grøn for færdiggørelse, rød for fejl.
- Understøtter clearing feed og eksportere alle meddelelser til JSON.
- Auto- genopretter forbindelse med eksponentiel backoff, hvis forbindelsen falder.

## Konstruktionsprincipper

- **Modularity**: Each translation concern is isolated in its own service for maintainability and testability.
- **Incremental persistence**: Dictionaries and Markdown files are saved per-language immediately after translation, reducing memory pressure and providing earlier feedback.
- **Resilience**: Multiple retry levels (HTTP, stage, block) ensure transient failures do not block the pipeline.
- **State tracking**: Per-file metadata (`.translation-meta.json`) and hash files enable precise incremental work on subsequent runs.
- **Real-time visibility**: Every significant operation is reported via SignalR for monitoring and debugging.
- **Manual translations always have priority over automatic additions.**
