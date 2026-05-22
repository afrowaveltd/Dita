# Përkthime në kohë reale

Ky dokument ekziston si një provë e drejtpërdrejtë për tubacionin automatik të përkthimit. Çdo ndryshim në këtë file shkakton rikthimin e të gjithë skedarëve të gjuhës së synuar në drejtimin e caktuar në vazhdim.

## Pasqyrë arkitekture

Tubacioni i përkthimit është ristrukturuar në një arkitekturë moderne me katër nën-shërbime të specializuara të koordinuara nga një orkestrues i lehtë:

- **BackendTranslationService** — Orchestrates the entire pipeline, handles server validation, and delegates work to sub-services.
- **CountriesTranslationService** — Synchronizes country names from `countries.json` into per-language dictionaries.
- **LocalizationTranslationService** — Detects added/removed keys in the default JSON dictionary and translates them into target languages.
- **DocumentsTranslationService** — Translates Markdown documentation files with per-block tracking and metadata.

Çdo nën-shërbim funksionon në mënyrë të pavarur dhe raporton përparim nëpërmjet sinjalizimit në kohë reale.

## Çfarë bën shërbimi

Shërbimi funksionon në një program dhe ekzekuton një tubacion pesë-faqesh: përfundimin e server-it, sinkronizimin e vendeve, sinkronizimin e fjalorëve JSON, përkthimin e skedarëve Markundown, dhe këmbënguljen e rezultateve. Çdo fazë lëshon ngjarje të strukturuara të përparimit në kohë reale mbi SinjalR në mënyrë që klientët e lidhur të mund të ndjekin së bashku ndërsa të ardhurat e punës.

## Fazat e tubacionit

### Faza e 1 - të

Para se të fillojë çdo vepër përkthimi, shërbimi vërteton se të gjitha parakushtet janë të kënaqshme:

- Seksioni i konfigurimit duhet të jetë i pranishëm dhe i vlefshëm.
- Serveri Libre Translate duhet të përgjigjet brenda një mungese të pranueshme.
- Lista e gjuhëve në dispozicion tek serveri i përkthimit është marrë.
- Gjuha e paracaktuar duhet të jetë e pranishme në këtë listë.
- Mungojnë skedarët lokalë JSON për çdo gjuhë të suportuar janë krijuar automatikisht.

Nëse ndonjë kontroll dështon, tubacioni ndalon menjëherë dhe lëshon një mesazh.

### Faza e 2 - të Përktheni llogaritë

Emrat e vendeve mbahen në sinkronizim nga një katalog në vetëm lexim () në fjalorët JSON.

- Nëse gjuha e prezgjedhur e programit është anglisht, çdo emër i vendit ruhet si pa përkthim.
- Nëse gjuha e paracaktuar është ndonjë gjuhë tjetër, emri i vendit anglez përkthehet së pari në atë gjuhë dhe rezultati bëhet hyrja në fjalorin e paracaktuar.
- After the default dictionary is updated, each missing country entry in every target language dictionary is translated and saved **immediately per language**.
- Zë ra tashmë të transformuar janë ruajtur pa modifikim.
- Nëse një përkthim dështon, shërbimi kthehet deri në 3 herë me vonesa 30 sekonda para se të transferohet në gjuhën tjetër.

### Faza 3 Përkthe skedarët Jason

Shërbimi krahason fjalorin e paracaktuar të lokalizimit me një skanim të ruajtur nga funksioni i mëparshëm:

- **Added keys** — entries present in the current default but absent from the snapshot — are translated into every target language that does not already have a manual entry for that key.
- **Removed keys** — entries present in the snapshot but absent from the current default — are deleted from every target language dictionary.
- Përkthimet manuale gjithmonë kanë përparësi. Nëse një fjalor i synuar tashmë përmban një vlerë për një kyç, kjo hyrje mbetet e pandryshuar pavarësisht nga ajo që thotë burimi.
- **Each target language dictionary is saved immediately after its translations complete**, rather than waiting for all languages to finish.
- Nëse një përkthim dështon në një gjuhë specifike, shërbimi kthehet automatikisht. Vetëm gabime të vazhdueshme (p.sh., gjuhë e pasuportuar) shkaktojnë që kjo gjuhë të anashkalohet.
- Pas ekzekutimit, fjalori aktual i prezgjedhur ruhet si fotografia e re për krahasimin në vazhdim.

Të gjithë fjalorët ruhen gjithmonë me çelësa të organizuar alfabetik dhe me JSON të identifikuar për të qenë i lexueshëm nga njeriu.

### Faza 4 Përkthe file

Shërbimi ecën sipas rrënjëve të konfiguruara të dokumentimit (e paracaktuar:) dhe proceson çdo burim:

1. Përmbajtja e file-it burim është lexuar dhe është llogaritur një hashh SHA-256.
2. A `.translation-meta.json` file next to the source tracks per-language, per-block translation status, enabling **incremental re-translation** of only failed blocks.
3. Hash i regjistruar nga funksioni i mëparshëm (i mbajtur në një file në vazhdim me file burues, ose në një pozicion të përkohshëm prapavijë) është krahasuar me hash-in aktual.
4. Për çdo gjuhë, file korrespondues kontrollohet gjithashtu për integritet strukturor.
5. Çdo file që mungon, ka një hash të vjetëruar, dështon në verifikimin e strukturës, ose përmban blloqe të papërkthyera është renditur në rradhë për ripërkthim.
6. **Each target language is translated and saved independently** — if Czech succeeds but French fails, the Czech file is still written to disk.
7. File të përkthyer me sukses janë të vlefshëm për paritet strukturor me burimin (numërimet e barabarta në krye, elementët e listës, blloqet e kodit, blloqet, lidhjet, shënuesit e guximshëm/italik, dhe etiketat HTML) para se të shkruhen në disk.
8. Nëse të gjithë objektivët për një burim të suksesshëm, hash i ri ruhet pranë burimit. Nëse shkrimi pranë burimit dështon (për shembull në vendosjet vetëm në lexim), hash bie përsëri në directory e përkohshme.
9. Nëse ndonjë përkthim objektiv dështon në vlefshmëri, metadata i shënon këto blloqe si të papërkthyera në mënyrë që të ripërsëriten në drejtimin tjetër.

### Faza e 5 - të, duke magazinuar prova

Një konsolidim është mbledhur dhe botuar. Përfshin:

- UTC fillon dhe përfundon oraret.
- Numërimet e skedarëve të ruajtur vendas JSON, shpëtuan skedarët Markdown, shpëtuan skedarët hashh dhe shkruan hashh.
- Çdo gabim i magazinuar gjatë nisjes.
- Statistikat e përkthimit në gjuhën për-gjuhër (numërimi i zgjeruar, numërimi i anashkaluar, numërimi i gabimit).

## Mesazhi

Çdo ngjarje progresi jepet si një me fushat në vijim:

Fusha
|-------|------|-------------|
Bashkëngjituesi i lidhjes për operacionin aktual të tubacionit
Zbarkimi monotonik brenda një vrapimi, duke filluar nga 1
Lloji nga mesazh
Pika e tubacionit i përket mesazhit
Ora UTC kur mesazhi u lëshua
Tregon nëse mesazhi paraqet një kusht gabimi
Përmbledhja e lexueshme nga njeriu
Gjëndja

### Mesazhi:

Vlera
|-------|------|---------|
0
1
2
3
4
5
6

### Fazat e tubacionit

Vlera
|-------|------|-------------|
0
1
2
3
4
5

### Tipike mesazh

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

Nëse ndonjë fazë dështon, fazat e mbetura anashkalohen, një mesazh lëshohet dhe përfundimisht një mesazh mbyllet.

## Përkthimi

Tubacioni zbaton dy nivele elasticiteti:

### Faza

- Në qoftë se një kërkesë përkthimi dështon pas retiteve të brendshme të Libre Translate, ajo kryen deri në 3 rite të tjera me vonesa 30 sekondashe.
- Mashtruesi i vendeve: Vendshënuesit e emëruar () në tekst zëvendësohen përkohësisht me shenja të sigurta () para përkthimit dhe rivendosjes më pas, duke siguruar gramatikën e saktë në gjuhët e synuara.

### Emri i gjuhës

- Para se të përkthehet në një gjuhë të caktuar, shërbimi vërteton se gjuha është mbështetur nga serveri i përkthimit.
- Gjuhët e pasuportuara anashkalohen me një paralajmërim, duke parandaluar përpjekjet e përsëritura të dështuara.

### Poshtë niveli

- Përkthimet e shënuara janë kryer bllok me blloqe (headings, paragrafët, elementët e listës).
- Në qoftë se një bllok individual dështon në përkthim, ai është shënuar si i papërkthyer në dosjen metadata dhe ripërsëritur në rrjedhën tjetër të tubacionit.
- Gjurmët e shërbimit për-gjuhë, gjendja për-bllok në skedarë pranë çdo file burim Markud.

## Gabim

Janë raportuar gabime duke përdorur një enum të bashkuar të grupuar në intervale:

Interval
|-------|----------|
1000 udhërrëfyes
20002999
30003999
4000499
5000599

Çdo gabim në raport përmban burimin e identifikuar (kodi i gjuhës, shtegu i file ose emri i skenës), kodi i gabimit, dhe një mesazh të lexueshëm nga njeriu.

## përkthim i drejtpërdrejtë

Projekti i serverit përfshin një faqe admin në të cilën lidhet me shpërndarësin e sinjalit dhe shfaq të gjitha ngjarjet e tubacionit në kohë reale.

- Shfaq gjendjen e lidhjes, numërimin e mesazheve dhe një tabelë përditësuese e drejtpërdrejtë e të gjitha ngjarjeve.
- Rreshta me ngjyrë të koduar: blu për nisje stade, e gjelbër për kompletim, e kuqe për gabime.
- Suporton pastrimin e ushqimit dhe eksportimin e të gjithë mesazheve në JSON.
- Auto-rilidhjet me mbrapa eksponenciale nëse lidhja bie.

## Projekti

- **Modularity**: Each translation concern is isolated in its own service for maintainability and testability.
- **Incremental persistence**: Dictionaries and Markdown files are saved per-language immediately after translation, reducing memory pressure and providing earlier feedback.
- **Resilience**: Multiple retry levels (HTTP, stage, block) ensure transient failures do not block the pipeline.
- **State tracking**: Per-file metadata (`.translation-meta.json`) and hash files enable precise incremental work on subsequent runs.
- **Real-time visibility**: Every significant operation is reported via SignalR for monitoring and debugging.
- **Manual translations always have priority over automatic additions.**
