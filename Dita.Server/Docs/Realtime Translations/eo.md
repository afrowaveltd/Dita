# Realtempaj tradukoj

Tiu dokumento ekzistas kiel viva testenigaĵo por la aŭtomata traduko dukto. Ĉiu ŝanĝo al tiu dosiero ekigas re-tradukon de ĉiuj cellingvaj dosieroj sur la venonta planita kuro.

## Arkitekturo superrigardo

La traduko dukto estis restrukturita en modulan arkitekturon kun kvar specialecaj sub-servoj kunordigitaj fare de malpeza orkestrotor:

- **BackendTranslationService** — Orchestrates the entire pipeline, handles server validation, and delegates work to sub-services.
- **CountriesTranslationService** — Synchronizes country names from `countries.json` into per-language dictionaries.
- **LocalizationTranslationService** — Detects added/removed keys in the default JSON dictionary and translates them into target languages.
- **DocumentsTranslationService** — Translates Markdown documentation files with per-block tracking and metadata.

Ĉiu sub-servo funkciigas sendepende kaj raportas progreson per SignalR en reala tempo.

## Kion la servo faras

La servo kuras en horaro kaj efektivigas kvin-fazan dukton: servila validumado, landosinkronigo, JSON-vorta sinkronigado, Markdown dosiertraduko, kaj persistante la rezultojn. Ĉiu stadio elsendas strukturitajn realtempajn progresojn super Signalo R tiel kiu ligis klientojn povas sekvi kiel laborenspezo.

## Duliniaj stadioj

### Situo 1 - CheckServers

Antaŭ ol iu traduko laboro komenciĝas, la servo konfirmas ke ĉiuj antaŭkondiĉoj estas kontentigitaj:

- La konfiguraciosekcio devas ĉeesti kaj valida.
- La LibreTranslate-servilo devas respondi ene de akceptebla latenteco.
- La listo de lingvoj haveblaj sur la tradukservilo estas fetita.
- La formita defaŭlta lingvo devas ĉeesti en tiu listo.
- Mankantaj ejo-dosieroj por iu apogita lingvo estas kreitaj aŭtomate.

Se iu kontrolo malsukcesas, la dukto ĉesas tuj kaj mesaĝo estas elsendita.

### Ŝtupo 2 - Tradukitaj areoj

Landaj nomoj estas konservitaj en sino de leg-restriktita katalogo () en la lokalizo JSON-vortarojn.

- Se la aplika defaŭlta lingvo estas angla, ĉiu landnomo estas stokita kiel sen traduko.
- Se la defaŭlta lingvo estas iu alia lingvo, la angla landnomo unue estas tradukita en tiun lingvon, kaj la rezulto iĝas la eniro en la defaŭlta vortaro.
- After the default dictionary is updated, each missing country entry in every target language dictionary is translated and saved **immediately per language**.
- Jam-tradukitaj kontribuoj estas konservitaj sen modifo.
- Se traduko malsukcesas, la servretries ĝis 3 fojojn kun 30-duaj prokrastoj antaŭ moviĝado al la venonta lingvo.

### 3 - TranslateJsonFiles

La servo komparas la nunan defaŭltan lokalizilvortaron kun momentfoto stokita de la antaŭa kuro:

- **Added keys** — entries present in the current default but absent from the snapshot — are translated into every target language that does not already have a manual entry for that key.
- **Removed keys** — entries present in the snapshot but absent from the current default — are deleted from every target language dictionary.
- La tradukoj ĉiam havas prioritaton. Se celvortaro jam enhavas valoron por ŝlosilo, tiu eniro estas lasita senŝanĝa nekonsiderante kion la fonto diras.
- **Each target language dictionary is saved immediately after its translations complete**, rather than waiting for all languages to finish.
- Se traduko malsukcesas por specifa lingvo, la servretries aŭtomate. Nur persistaj eraroj (ekz., nepruvitaj lingvoj) igas tiun lingvon esti skiitaj.
- Post la kuro, la nuna defaŭlta vortaro estas ŝparita kiel la nova momentfoto por la venonta komparo.

Ĉiuj vortaroj ĉiam estas stokitaj kun alfabe ordigitaj ŝlosiloj kaj kontraditaj JSON por homa legebleco.

### 4 - TranslateMarkdownFiles

La servo piediras la formitan dokumentarradikojn (defaŭlto: ) kaj prilaboras ĉiun fontdosieron rekursive:

1. La fonta dosierenhavo estas legita kaj SHA-256 hah estas komputita.
2. A `.translation-meta.json` file next to the source tracks per-language, per-block translation status, enabling **incremental re-translation** of only failed blocks.
3. La stokita hah de la antaŭa kuro (konservita en dosiero plej proksime al la fontdosiero, aŭ en provizora senrezigna loko) estas komparita kun la nuna hash.
4. Por ĉiu cellingvo, la ekvivalenta dosiero ankaŭ estas kontrolita por struktura integreco.
5. Ĉiu celdosiero kiu maltrafas, havas malmodernan hah, malsukcesas strukturan validadon, aŭ enhavas netradukitajn blokojn estas ripetitaj por re-traduko.
6. **Each target language is translated and saved independently** — if Czech succeeds but French fails, the Czech file is still written to disk.
7. Sukcesaj tradukitaj dosieroj estas konfirmitaj por struktura egaleco kun la fonto (egalaj titoloj, listeroj, kodblokoj, blokocitoj, ligiloj, aŭdacaj/itaj signoj, kaj HTML-etikedoj) antaŭ ol ili estas skribitaj al disko.
8. Se ĉiuj celdosieroj por fonto sukcesas, la nova hah estas stokita plej proksime al la fonto. Se skribo plej proksime al la fonto malsukcesas (ekzemple en leg-restriktitaj deplojoj), la hah falas reen al la provizora adresaro.
9. Se ĉiu celtraduko malsukcesas validumadon, la metadatenoj markas tiujn blokojn kiel netradukitaj tiel ili estas retried sur la venonta kuro.

### Ŝtupo 5 - Storing Results

Firmigita estas kunvenita kaj publikigita. Ĝi inkludas:

- UTC kuras kaj kompletigas tempostampojn.
- Kalkuloj de savitaj lokaj JSON dosieroj, ŝparis Markdown dosierojn, savis hah dosierojn, kaj rezerva hah skribas.
- Ĉiuj stokaderaroj kolektitaj dum la kuro.
- Per-lingva traduko statistiko (tradukita kalkulo, skiita kalkulo, erarkalkulo).

## Signalo Signalo R-mesaĝo

Ĉiu progresokazaĵo estas farita kiel kun la sekvaj kampoj:

Kampo
|-------|------|-------------|
Correlation-identigilo por la nuna dukto kuras
Monotona vendo ene de kuro, komencante ĉe 1
Semantika speco de la mesaĝo
Pipeline enscenigas la mesaĝon apartenas al
UTC-tempo kiam la mesaĝo estis elsendita
Ĉu la mesaĝo reprezentas erarkondiĉon
Homa-legebla resumo
Scene-specifa utila ŝarĝo (raportobjekto aŭ nulo)

### Mesaĝo tipoj

Valora Valoro
|-------|------|---------|
0
1
2
3
4
5
6

### Duliniaj stadioj

Valora Valoro
|-------|------|-------------|
0
1
2
3
4
5

### Tipa mesaĝfluo

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

Se iu stadio malsukcesas, la ceteraj stadioj estas transsalitaj, mesaĝo estas elsendita, kaj finfine mesaĝo fermas la kuron.

## Translation Retry logiko

La dukto efektivigas du nivelojn de rezistemo:

### Scenejo-nivela reiro (Translation RetryService)

- Se traduko peto malsukcesas post la internaj retries de LibreTranslate, la rezultas ĝis 3 kromaj scennivelaj retries kun 30-dua prokrastoj.
- Lokulo maskanta: Nomita lokposedantoj () en teksto provizore estas anstataŭigitaj kun sekuraj ĵetonoj () antaŭ traduko kaj reestigita poste, certigante ĝustan gramatikon en cellingvoj.

### Lingvo validumado

- Antaŭ tradukado al cellingvo, la servo konfirmas la lingvon estas apogita per la traduko servilo.
- Nepruvitaj lingvoj estas skiitaj kun averto, malhelpante ripetajn malsukcesajn provojn.

### Markdown bloko-nivela retry

- Markdown-tradukoj estas prezentitaj blok-post-bloko (kapoj, paragrafoj, listeroj).
- Se individua bloko malsukcesas tradukon, ĝi estas markita kiel netradukita en la metadatenoj-dosiero kaj retried sur la venonta dukto kuras.
- La servo spuras per-lingvon, per-bloka statuso en dosieroj plej proksime al ĉiu fonto Markdown-dosiero.

## Eraro kodoj

Eraroj estas raportitaj uzi unuigitan enum grupigitan en intervalojn:

Montaro
|-------|----------|
1000-99
2000-2999
3000-3999
4000-4999
5000-5999

Ĉiu eraro en raporto portas la fontidentigilon (lingva kodo, dosierpado, aŭ artistan nomon), la erarkodon, kaj hom-legeblan mesaĝon.

## Traduko de Dashboard

La Servilo projekcias inkludas admin paĝon ĉe tio ligas al la SignalR-nabo ĉe kaj elmontras ĉiujn duktokazaĵojn en reala tempo.

- Aparta ligostatuso, mesaĝkalkulo, kaj viv-supreniranta tablo de ĉiuj okazaĵoj.
- Koloro-koditaj vicoj: blua por scenkomenco, verda por kompletigo, ruĝa por eraroj.
- Subtenu la furaĝon kaj eksporti ĉiujn mesaĝojn al JSON.
- Aŭto-religoj kun eksponenta malantaŭaĵo se la ligo falas.

## Dezajnoprincipoj

- **Modularity**: Each translation concern is isolated in its own service for maintainability and testability.
- **Incremental persistence**: Dictionaries and Markdown files are saved per-language immediately after translation, reducing memory pressure and providing earlier feedback.
- **Resilience**: Multiple retry levels (HTTP, stage, block) ensure transient failures do not block the pipeline.
- **State tracking**: Per-file metadata (`.translation-meta.json`) and hash files enable precise incremental work on subsequent runs.
- **Real-time visibility**: Every significant operation is reported via SignalR for monitoring and debugging.
- **Manual translations always have priority over automatic additions.**
