# Real- time fordítások

Ez a dokumentum az automatikus fordítóvezeték élő vizsgálati bemeneteként létezik. A fájl bármilyen módosítása elindítja a célnyelvi fájlok fordítását a következő menetrendszerű futáskor.

## Építészeti áttekintés

A fordítóvezetéket moduláris architektúrává alakították át, négy speciális részszolgáltatással, amelyeket könnyű hangszóró koordinált:

- **BackendTranslationService** — Orchestrates the entire pipeline, handles server validation, and delegates work to sub-services.
- **CountriesTranslationService** — Synchronizes country names from `countries.json` into per-language dictionaries.
- **LocalizationTranslationService** — Detects added/removed keys in the default JSON dictionary and translates them into target languages.
- **DocumentsTranslationService** — Translates Markdown documentation files with per-block tracking and metadata.

Minden egyes alszolgáltatás függetlenül működik, és valós időben jelenti az előrehaladást a SignalR-en keresztül.

## Mit tesz a szolgálat

A szolgáltatás menetrend szerint fut, és egy ötlépcsős csővezetéket hajt végre: szerver validálás, country szinkronizáció, JSON szótár szinkronizálás, Markdown fájlfordítás és az eredmények fenntartása. Minden szakasz strukturált valós idejű előrehaladási eseményeket bocsát ki a Signal felett R annak érdekében, hogy a kapcsolódó ügyfelek is követni a munka folytatása.

## A csővezeték szakaszai

### 1. szakasz - Ellenőrzési szerverek

A fordítás megkezdése előtt a szolgáltatás ellenőrzi, hogy minden előfeltétel teljesül:

- A konfigurációs résznek jelen kell lennie és érvényesnek kell lennie.
- A LibreTranslate szervernek elfogadható késéssel kell reagálnia.
- A fordítási kiszolgálón elérhető nyelvek listája elkészül.
- A beállított alapértelmezett nyelvnek meg kell jelennie a listán.
- Hiányzó locale JSON fájlok bármilyen támogatott nyelv jön létre automatikusan.

Ha az ellenőrzés nem sikerül, a csővezeték azonnal megáll, és üzenetet bocsát ki.

### 2. szakasz - Transzlaterországok

Ország nevek tartják szinkronban egy csak olvasható katalógus () a lokalizáció JSON szótárak.

- Ha az alkalmazás alapértelmezett nyelve angol, minden ország neve fordítás nélkül tárolja.
- Ha az alapértelmezett nyelv bármely más nyelv, az angol ország nevét először lefordítják erre a nyelvre, és az eredmény lesz az alapértelmezett szótár.
- After the default dictionary is updated, each missing country entry in every target language dictionary is translated and saved **immediately per language**.
- A lefordított bejegyzéseket módosítás nélkül megőrzik.
- Ha a fordítás nem sikerül, a szolgáltatás a következő nyelvre való áttérés előtt akár 3-szor is visszaáll 30 másodperces késéssel.

### 3. szakasz - TranslateJsonFiles

A szolgáltatás összehasonlítja az aktuális alapértelmezett lokalizációs szótárt egy pillanatfelvétellel tárolt előző fut:

- **Added keys** — entries present in the current default but absent from the snapshot — are translated into every target language that does not already have a manual entry for that key.
- **Removed keys** — entries present in the snapshot but absent from the current default — are deleted from every target language dictionary.
- A kézi fordítás mindig elsőbbséget élvez. Ha egy célszótár már tartalmaz egy kulcsot, ez a bejegyzés változatlan marad, függetlenül attól, hogy a forrás mit mond.
- **Each target language dictionary is saved immediately after its translations complete**, rather than waiting for all languages to finish.
- Ha egy fordítás egy adott nyelven nem sikerül, a szolgáltatás automatikusan visszatér. Csak a tartós hibák (pl. a nem támogatott nyelv) idézik elő ezt a nyelvet.
- A futást követően az aktuális alapértelmezett szótár lesz az új pillanatfelvétel a következő összehasonlításhoz.

Minden szótárak mindig tárolt ábécésorrendben válogatott billentyűket, és belement JSON az emberi olvashatóság.

### 4. szakasz - TranslateMarkdownFiles

A szolgáltatás sétál a beállított dokumentációs gyökerek (alapértelmezett:) és feldolgozza minden forrás fájl rekurzívan:

1. A forrásfájl tartalma olvasható és egy SHA- 256 hash kerül kiszámításra.
2. A `.translation-meta.json` file next to the source tracks per-language, per-block translation status, enabling **incremental re-translation** of only failed blocks.
3. A tárolt hash az előző fut (egy fájl mellett a forrás fájl, vagy egy ideiglenes tartalék helyen) össze kell hasonlítani a jelenlegi hash.
4. Minden célnyelv esetében a megfelelő fájlt a szerkezeti integritás szempontjából is ellenőrzik.
5. Minden hiányzó célfájl, egy elavult hash, meghibásodik a szerkezet validálása, vagy tartalmaz lefordított blokkok sorban áll a re- fordítás.
6. **Each target language is translated and saved independently** — if Czech succeeds but French fails, the Czech file is still written to disk.
7. Sikeresen lefordított fájlok validálják a strukturális paritás a forrás (egyenlő címek száma, listaelemek, kódblokkok, blokkolások, linkek, merész / dőlt markerek, és HTML címkék), mielőtt írnák a lemezre.
8. Ha egy forrás összes célfájlja sikeres, az új hash-t a forrás mellett tároljuk. Ha az írás a forrás mellett nem sikerül (például csak olvasmányban), a hash visszaesik az ideiglenes könyvtárba.
9. Ha a célfordítás nem felel meg a hitelesítésnek, a metaadatok azokat a blokkokat lefordítatlannak jelölik, így a következő körben újra tesztelik őket.

### 5. szakasz - StoringResults

A konszolidált és közzétett. Ide tartoznak a következők:

- UTC futtatás kezdő- és befejező időbélyegzők.
- Számok mentett locale JSON fájlokat, mentett Markdown fájlokat, mentett hash fájlokat, és a fallback hash írásokat.
- A futtatás során gyűjtött tárolási hibák.
- Per- nyelvi fordítási statisztika (lefordított szám, kihagyott szám, hibaszám).

## Jelzés R üzenetboríték

Minden előrehaladási esemény a következő mezőkkel történik:

Mező
|-------|------|-------------|
Megfelelési azonosító a jelenlegi csővezetéken
Monoton számláló futáskor, kezdve 1
Az üzenet szemantikai típusa
Pipeline szakasz az üzenet tartozik
UTC-idő az üzenet kibocsátásakor
Az üzenet hibakeresést jelent-e
Humán-olvasható összefoglaló
Stage- specifikus hasznos teher (jelenteni objektum vagy null)

### Üzenettípusok

Érték
|-------|------|---------|
0
1
2
3
4
5
6

### A csővezeték szakaszai

Érték
|-------|------|-------------|
0
1
2
3
4
5

### Tipikus üzenetáramlás

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

Ha egy szakasz sem sikerül, a fennmaradó szakasz kimarad, egy üzenet jelenik meg, és végül egy üzenet lezárja a futást.

## Fordítás ismételt logika

A csővezeték az ellenálló képesség két szintjét valósítja meg:

### Stage- level retry (TranslationRetryService)

- Ha a fordítás kérés nem sikerül a Libred.Translate belső retry után, a teljesítés akár 3 további szakasz szintű retry 30 másodperces késéssel.
- Helyettesítő elfedése: A szövegben szereplő "placeholders ()" nevet a fordítás előtt ideiglenesen biztonságos zsetonokkal () helyettesítik, majd azt követően helyreállítják, biztosítva a helyes nyelvtant a célnyelveken.

### Nyelvi érvényesítés

- A célnyelv lefordítása előtt a szolgáltatás ellenőrzi, hogy a fordítószerver támogatja-e a nyelvet.
- A nem támogatott nyelveket figyelmeztetéssel hagyják ki, megelőzve az ismételt sikertelen kísérleteket.

### Jelölési blokkszint-helyreállítás

- Jelölések fordítások végzik block- by-block (címek, bekezdések, listás tételek).
- Ha egy egyes blokk nem tud lefordítani, akkor a metaadat fájlban nincs lefordítva, és a következő csővezetéken újra kell próbálni.
- A service tracks per- language, per- block status in files neach each source Marklown file.

## Hibakód

A hibák bejelentése egységes enum tartományokba csoportosítva történik:

Távolság
|-------|----------|
1000- 1999
2000- 2999
3000- 3999
4000- 4999
5000- 5999

Minden egyes hiba a jelentésben tartalmazza a forrásazonosítót (nyelvkód, fájlútvonal vagy szakasznév), a hibakódot és egy emberi olvasható üzenetet.

## Élő fordítás műszerfal

A Server projekt tartalmaz egy admin oldalt, amely csatlakozik a SignalR csomóponthoz, és megjeleníti az összes csővezeték események valós időben.

- Megjeleníti a kapcsolat állapotát, az üzenetszámot, és egy élő-frissítő táblázatot minden eseményről.
- Színkódos sorok: kék a színpad kezdetéhez, zöld a befejezéshez, piros a hibákhoz.
- Támogatja a takarmány kiürítését és az összes üzenetet a JSON-nak exportálja.
- Auto-kapcsolat exponenciális háttérrel, ha a kapcsolat csökken.

## Tervezési elvek

- **Modularity**: Each translation concern is isolated in its own service for maintainability and testability.
- **Incremental persistence**: Dictionaries and Markdown files are saved per-language immediately after translation, reducing memory pressure and providing earlier feedback.
- **Resilience**: Multiple retry levels (HTTP, stage, block) ensure transient failures do not block the pipeline.
- **State tracking**: Per-file metadata (`.translation-meta.json`) and hash files enable precise incremental work on subsequent runs.
- **Real-time visibility**: Every significant operation is reported via SignalR for monitoring and debugging.
- **Manual translations always have priority over automatic additions.**
