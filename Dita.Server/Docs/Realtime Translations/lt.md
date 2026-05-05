# @ info: tooltip

Dokumentas yra kaip tiesioginė bandymo įvesties automatinio vertimo vamzdyno. Kiekvienas šio failo pakeitimas sukelia visų tikslinių kalbos failų re- vertimas kitą numatytą paleisti.

## Architektūros apžvalga

Transliavimo vamzdynas pertvarkytas į modulinę architektūrą, kurią koordinuoja lengvas orkestras:

- **BackendTranslationService** — Orchestrates the entire pipeline, handles server validation, and delegates work to sub-services.
- **CountriesTranslationService** — Synchronizes country names from `countries.json` into per-language dictionaries.
- **LocalizationTranslationService** — Detects added/removed keys in the default JSON dictionary and translates them into target languages.
- **DocumentsTranslationService** — Translates Markdown documentation files with per-block tracking and metadata.

Kiekviena subpaslauga veikia savarankiškai ir realiu laiku per SionalR praneša apie pažangą.

## Ką paslauga daro

PaslaugA veikia tvarkaraštyje ir vykdo penktojo etapo vamzdyną: serverio patvirtinimas, šalies sinchronizavimas, JSON žodyno sinchronizavimas, Markdown failo vertimas, ir išlaikyti rezultatus. Kiekvienas etapas skleidžia struktūrizuotas realiu laiku pažangos įvykius virš Signal R, kad susiję klientai gali sekti kartu kaip darbo pajamos.

## Vamzdynų pakopos

### 1 etapas - Kontroliniai serveriai

Prieš pradedant vertimus, tarnyba patikrina, ar visos išankstinės sąlygos yra įvykdytos:

- Konfigūracijos sekcija turi būti ir galiojanti.
- Lybreplayer serveris turi atsakyti per priimtiną laiką.
- Kalbų sąrašas prieinamas vertimų serverį.
- Sukonfigūruota numatytoji kalba turi būti tame sąraše.
- Trūksta locale JSON failų bet kuriai palaikomai kalbai sukurti automatiškai.

@ info: tooltip.

### 2 etapas - trečiosios šalys

Šalių pavadinimai yra laikomi sinchronizuoti iš read- tik katalogas () į lokalizacijos JSON žodynai.

- NAME OF TRANSLATORS.
- NAME OF TRANSLATORS.
- After the default dictionary is updated, each missing country entry in every target language dictionary is translated and saved **immediately per language**.
- Already-translated įrašai išsaugomi be pakeitimų.
- @ info: whatsthis.

### 3 etapas - TranslateJsonFilms

PaslaugA lygina dabartinį numatytąjį lokalizavimo žodyną su iš ankstesnio kurso saugomu fotografija:

- **Added keys** — entries present in the current default but absent from the snapshot — are translated into every target language that does not already have a manual entry for that key.
- **Removed keys** — entries present in the snapshot but absent from the current default — are deleted from every target language dictionary.
- Rankinis vertimas visada yra prioritetas. NAME OF TRANSLATORS.
- **Each target language dictionary is saved immediately after its translations complete**, rather than waiting for all languages to finish.
- @ info: whatsthis Trūksta tik nuolatinių klaidų (pvz., nepalaikomos kalbos).
- NAME OF TRANSLATORS.

All dictionaries visad yra saugomi su abėcėlės rūšiuojami raktai ir intended JSON už žmogaus skaitomumo.

### 4 etapas - TranslateMarkdownFilds

PaslaugA eina sukonfigūruotas dokumentacijos šaknis (numatytoji:) ir apdoroja kiekvieną šaltinio failą rekursyviai:

1. Šaltinio failo turinys yra perskaitomas ir SHA-256 hash yra apskaičiuojamas.
2. A `.translation-meta.json` file next to the source tracks per-language, per-block translation status, enabling **incremental re-translation** of only failed blocks.
3. Saugomas maišos nuo ankstesnio paleisti (saugomi failo šalia pradinio failo, arba laikinai atsarginė vieta) yra lyginamas su dabartiniu maišos.
4. Trūkstamas kiekvienos tikslinės kalbos struktūrinis vientisumas.
5. NAME OF TRANSLATORS.
6. **Each target language is translated and saved independently** — if Czech succeeds but French fails, the Czech file is still written to disk.
7. Sėkmingai išversti failai yra patvirtinti struktūrinį paritetą su šaltiniu (vienodas antraščių skaičius, sąrašo elementai, kodų blokai, blockcitatos, nuorodos, paryškinti / kursyvu žymekliai, ir HTML žymės), kol jie yra įrašyti į diską.
8. NAME OF TRANSLATORS @ info: whatsthis.
9. NAME OF TRANSLATORS.

### 5 etapas - Storingrezultatai

Surinktas ir paskelbtas konsoliduotas dokumentas. Joms priklauso:

- UTC paleisti pradžios ir užbaigimo laiko žymos.
- NAME OF TRANSLATORS.
- Renkamos visos duomenų saugojimo klaidos.
- Transliavimo per kalbą statistika (išverstas skaičius, praleistas skaičius, klaidų skaičius).

## Signalas R laiško paketas

Kiekvienas pažangos renginys vyksta su šių sričių:

Laukas
|-------|------|-------------|
Turimos vamzdyno veiklos koreliacijos identifikatorius
Monotoninis skaitiklis per paleisti, pradedant nuo 1
Semantinis pranešimo tipas
Vamzdyno etapas pranešimas priklauso
UTC laikas, kai pranešimas buvo išsiųstas
NAME OF TRANSLATORS
Humanitarinė santrauka
Konkretaus etapo naudingoji apkrova (pranešimo objektas arba nulis)

### Laiško tipai

Vertė
|-------|------|---------|
0
1
2
3
4
5
6

### Vamzdynų pakopos

Vertė
|-------|------|-------------|
0
1
2
3
4
5

### Tipinis pranešimų srautas

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

@ info: whatsthis.

## Vertimo pakartojimas logika

Vamzdynu užtikrinamas dviejų lygių atsparumas:

### Ekskursija (TranslationRetryService)

- @ info: whatsthis.
- Kėbulo laikiklis maskavimas: Pavadintas placebas () tekste yra laikinai pakeisti saugiais žetonais () prieš vertimą ir atstatytas po to, užtikrinant teisingą gramatikos tikslinių kalbų.

### Kalbos patvirtinimas

- Prieš verčiant į tikslinę kalbą, tarnyba patikrina kalbą, kurią palaiko vertimo serveris.
- Nekoordinuotos kalbos praleidžiamos įspėjimu, užkertant kelią pakartotiniams nepavykusiems bandymams.

### NAME OF TRANSLATORS

- Žymėjimo vertimai atliekami block- by- block (antraštės, dalys, sąrašas elementai).
- @ info: whatsthis.
- Tarnybos takeliai per- language, per- block būsena failuose šalia kiekvieno šaltinio Markdown failo.

## Klaidų kodai

Klaidos pateikiamos naudojant vieningą enumą, sugrupuotą į intervalus:

Intervalas
|-------|----------|
1999-1000
2999
390- 3999
4000- 4999
5000- 5999

Kiekviena pranešimo klaida turi šaltinio identifikatorių (kalbos kodą, failo kelią arba scenos pavadinimą), klaidos kodą ir žmogaus nuskaitomą pranešimą.

## Name

Serverio projektas apima admin puslapį, kuris jungia su SionalR mazgas ir rodo visus vamzdynų įvykius realiu laiku.

- Rodo ryšio būseną, žinučių skaičių ir visų įvykių gyvybės atnaujinimo lentelę.
- Spalvotos eilutės: mėlynos scenos pradžios, žalios - pabaigos, raudonos - klaidų.
- NAME OF TRANSLATORS.
- Automat- reconnections su eksponentine atsitraukimo, jei ryšys nukrenta.

## Konstrukcijos principai

- **Modularity**: Each translation concern is isolated in its own service for maintainability and testability.
- **Incremental persistence**: Dictionaries and Markdown files are saved per-language immediately after translation, reducing memory pressure and providing earlier feedback.
- **Resilience**: Multiple retry levels (HTTP, stage, block) ensure transient failures do not block the pipeline.
- **State tracking**: Per-file metadata (`.translation-meta.json`) and hash files enable precise incremental work on subsequent runs.
- **Real-time visibility**: Every significant operation is reported via SignalR for monitoring and debugging.
- **Manual translations always have priority over automatic additions.**
