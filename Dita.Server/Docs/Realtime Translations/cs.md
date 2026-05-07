# Překlady v reálném čase

Tento dokument existuje jako živý zkušební vstup pro automatický překlad potrubí. Jakákoli změna tohoto souboru spustí re- překlad všech souborů cílového jazyka v příštím plánovaném běhu.

## Přehled architektury

Překladatelské potrubí bylo restrukturalizováno do modulární architektury se čtyřmi specializovanými subslužbami koordinovanými lehkým orchestrátorem:

- **BackendTranslationService** — Orchestrates the entire pipeline, handles server validation, and delegates work to sub-services.
- **CountriesTranslationService** — Synchronizes country names from `countries.json` into per-language dictionaries.
- **LocalizationTranslationService** — Detects added/removed keys in the default JSON dictionary and translates them into target languages.
- **DocumentsTranslationService** — Translates Markdown documentation files with per-block tracking and metadata.

Každá subslužba funguje nezávisle a v reálném čase hlásí pokrok prostřednictvím SignalR.

## Co služba dělá

Služba běží podle plánu a provede pětistupňový ropovod: validace serveru, synchronizace země, synchronizace slovníku JSON, překlad Markdownových souborů a pokračování výsledků. Každá etapa vysílá strukturované události v reálném čase nad signálem R tak, aby klienti, kteří jsou připojeni, mohli pokračovat v práci.

## Fáze potrubí

### Fáze 1 - Kontrolní servery

Před zahájením překladu, služba ověřuje, že všechny předpoklady jsou splněny:

- Konfigurační část musí být přítomna a platná.
- Server LibreTranslate musí reagovat v přijatelné latenci.
- Seznam jazyků dostupných na překladatelském serveru je načten.
- Nakonfigurovaný výchozí jazyk musí být v tomto seznamu.
- Chybějící locale JSON soubory pro jakýkoli podporovaný jazyk jsou vytvořeny automaticky.

Pokud kontrola selže, potrubí se okamžitě zastaví a vyšle zprávu.

### Fáze 2 - Překladové země

Názvy zemí jsou drženy v synchronizaci z katalogu () pouze pro čtení do slovníků lokalizace JSON.

- Pokud je výchozí jazyk aplikace anglický, každé jméno země je uloženo jako bez překladu.
- Je-li výchozí jazyk je jiný jazyk, anglický název země je nejprve přeložen do tohoto jazyka, a výsledek se stává záznam ve výchozím slovníku.
- After the default dictionary is updated, each missing country entry in every target language dictionary is translated and saved **immediately per language**.
- Přeložené položky jsou zachovány bez úprav.
- Pokud překlad selže, služba se retestuje až třikrát s 30-sekundovým zpožděním před přechodem do dalšího jazyka.

### Fáze 3 - TranslateJsonFiles

Služba porovnává současný výchozí lokalizační slovník se snímkem uloženým z předchozího běhu:

- **Added keys** — entries present in the current default but absent from the snapshot — are translated into every target language that does not already have a manual entry for that key.
- **Removed keys** — entries present in the snapshot but absent from the current default — are deleted from every target language dictionary.
- Ruční překlady mají vždy přednost. Pokud cílový slovník již obsahuje hodnotu klíče, zůstává tento záznam beze změny bez ohledu na to, co říká zdroj.
- **Each target language dictionary is saved immediately after its translations complete**, rather than waiting for all languages to finish.
- Pokud překlad pro určitý jazyk selže, služba se automaticky opakuje. Pouze přetrvávající chyby (např. nepodporovaný jazyk) způsobují přeskočení tohoto jazyka.
- Po spuštění, aktuální výchozí slovník je uložen jako nový snímek pro další srovnání.

Všechny slovníky jsou vždy uloženy s abecedně tříděnými klávesami a indented JSON pro lidskou čitelnost.

### Fáze 4 - TranslateMarkdownFiles

Služba prochází nakonfigurovanými kořeny dokumentace (výchozí:) a zpracovává každý zdrojový soubor rekurzivně:

1. Obsah zdrojového souboru se přečte a vypočítá se SHA-256 hash.
2. A `.translation-meta.json` file next to the source tracks per-language, per-block translation status, enabling **incremental re-translation** of only failed blocks.
3. Uložený haš z předchozího běhu (uložený v souboru vedle zdrojového souboru, nebo v dočasném záložním místě) se porovnává s aktuálním hašišem.
4. Pro každý cílový jazyk je příslušný soubor také kontrolován pro strukturální integritu.
5. Jakýkoli cílový soubor, který chybí, má zastaralý hash, selže ověření struktury, nebo obsahuje nepřeložené bloky je fronta pro retranslation.
6. **Each target language is translated and saved independently** — if Czech succeeds but French fails, the Czech file is still written to disk.
7. Úspěšně přeložené soubory jsou validovány pro strukturální parity se zdrojem (stejný počet položek, seznam položek, kódové bloky, blockcutes, odkazy, tučné / italické markery, a HTML tagy) před tím, než jsou zapsány na disk.
8. Pokud všechny cílové soubory pro zdroj uspějí, nový hash je uložen vedle zdroje. Pokud psaní vedle zdroje selže (např. v read- only nasazení), hash se vrátí do dočasného adresáře.
9. Pokud některý cílový překlad selže při validaci, metadata označují tyto bloky jako nepřeložené, takže jsou znovu vyzkoušeny v příštím kole.

### Fáze 5 - StoringResults

Konsolidovaná je sestavena a zveřejněna. Zahrnuje:

- Startovní a dokončovací hodiny UTC.
- Počítá se se uloženými soubory locale JSON, uloženými soubory Markdown, uloženými soubory hash a zálohovými soubory hash.
- Chyby v úložišti zjištěné během jízdy.
- Per- jazyk překlad statistiky (přeložen počet, přeskočil počet, počet chyb).

## Signál R obálka zprávy

Každý pokrok akce je dodán jako s následujícími poli:

Pole
|-------|------|-------------|
Identifikátor korelace pro stávající provoz potrubí
Monotónní počítadlo v rámci běhu, začíná na 1
Semantický typ zprávy
Pipeline fáze zpráva patří do
Čas UTC, kdy byla zpráva vydána
Zda zpráva představuje chybový stav
Human- čitelný souhrn
Stage- specifické užitečné zatížení (report objekt nebo null)

### Typy zpráv

Hodnota
|-------|------|---------|
0
1
2
3
4
5
6

### Fáze potrubí

Hodnota
|-------|------|-------------|
0
1
2
3
4
5

### Typický tok zpráv

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

Pokud nějaká fáze selže, zbývající fáze jsou přeskočeny, zpráva je vypuštěna a nakonec zpráva ukončí běh.

## Logika přepracování překladu

Plynovod zajišťuje dvě úrovně odolnosti:

### stage- level retry (translationretryservice)

- Pokud žádost o překlad selže po interních repokusech LibreTranslate, provede až 3 další stage-level retests s 30-sekundovým zpožděním.
- Placeholder maskuje: Pojmenovaná místa () v textu jsou dočasně nahrazena bezpečnými tokeny () před překladem a poté obnovena, čímž se zajistí správná gramatika v cílových jazycích.

### Potvrzení jazyka

- Před překladem do cílového jazyka, služba ověřuje jazyk je podporován překlad serveru.
- Nepodporované jazyky jsou přeskočeny s varováním, aby se zabránilo opakované neúspěšné pokusy.

### Markdown block- level retry

- Překlady Markdownu se provádějí block- by- block (položky, odstavce, položky seznamu).
- Pokud jednotlivý blok selže při překladu, je označen jako nepřeložený v souboru metadat a znovu vyzkoušen při dalším plynovodu.
- Služba sleduje per- jazyk, per- block stav v souborech vedle každého zdroje Markdown souboru.

## Kódy chyb

Chyby se vykazují za použití jednotného čísla seskupeného do rozpětí:

Rozsah
|-------|----------|
1000- 1999
2000- 2999
Ostatní
4000- 4999
550- 5999

Každá chyba ve zprávě obsahuje identifikátor zdroje (jazykový kód, cesta k souboru nebo název fáze), chybový kód a lidsky čitelnou zprávu.

## Živý překlad Přístrojová deska

Projekt Server obsahuje admin stránku, která se připojuje k náboji SignalR a zobrazuje všechny události potrubí v reálném čase.

- Zobrazuje stav připojení, počet zpráv a tabulku životních aktualizací všech událostí.
- Barevně kódované řádky: modrá pro start jeviště, zelená pro dokončení, červená pro chyby.
- Podporuje čištění krmiva a vývoz všech zpráv do JSON.
- Automatické spojení s exponenciální zálohou, pokud spojení klesne.

## Zásady návrhu

- **Modularity**: Each translation concern is isolated in its own service for maintainability and testability.
- **Incremental persistence**: Dictionaries and Markdown files are saved per-language immediately after translation, reducing memory pressure and providing earlier feedback.
- **Resilience**: Multiple retry levels (HTTP, stage, block) ensure transient failures do not block the pipeline.
- **State tracking**: Per-file metadata (`.translation-meta.json`) and hash files enable precise incremental work on subsequent runs.
- **Real-time visibility**: Every significant operation is reported via SignalR for monitoring and debugging.
- **Manual translations always have priority over automatic additions.**
