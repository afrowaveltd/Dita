# Preklady v reálnom čase

Tento dokument existuje ako živý skúšobný vstup pre automatické prekladové potrubie. Akákoľvek zmena v tomto súbore aktivuje re-transláciu všetkých súborov cieľového jazyka v ďalšom plánovanom spustení.

## Prehľad architektúry

Prekladateľské potrubie bolo reštrukturalizované do modulárnej architektúry so štyrmi špecializovanými podslužbami koordinovanými ľahkým orchesterom:

- **BackendTranslationService** .
- **CountriesTranslationService** .
- **LokalizáciaPrekladSlužba** .
- **DocumentsTranslationService** .

Každá podslužba pracuje nezávisle a podáva správy o pokroku prostredníctvom SIGUNR v reálnom čase.

## Čo robí služba

Služba funguje podľa plánu a vykonáva päťstupňový ropovod: validácia servera, synchronizácia krajín, synchronizácia slovníka JSON, preklad súboru Markdown a pretrvávajúce výsledky. Každá fáza vysiela štruktúrované udalosti pokroku v reálnom čase cez Signal R tak, že prepojené klienti môžu sledovať, ako pokračuje práca.

## Štádiá potrubia

### Etapa 1

Pred začatím prekladateľských prác služba overí, či sú splnené všetky predpoklady:

- Konfiguračný úsek musí byť prítomný a platný.
- LibreTranslate server musí reagovať v rámci prijateľnej latencie.
- Zoznam jazykov dostupných na prekladateľskom serveri je stiahnutý.
- Nakonfigurovaný predvolený jazyk musí byť prítomný v tomto zozname.
- Chýbajúce lokálne súbory JSON pre akýkoľvek podporovaný jazyk sú vytvorené automaticky.

Ak niektorá kontrola zlyhá, potrubie sa okamžite zastaví a vyšle správu.

### Etapa 2

Názvy krajín sú synchronizované z katalógu () určeného na čítanie do slovníkov lokalizácie JSON.

- Ak je predvolený jazyk aplikácie angličtina, názov každej krajiny je uložený ako bez prekladu.
- Ak je predvolený jazyk iným jazykom, anglické meno krajiny je prvýkrát preložené do tohto jazyka, a výsledok sa stáva záznamom v predvolenom slovníku.
- Po aktualizácii predvoleného slovníka je každá chýbajúca položka krajiny v každom slovníku cieľového jazyka preložená a uložená **hneď na jazyk**.
- Už preložené položky sú zachované bez úpravy.
- Ak preklad zlyhá, služba sa s 30-sekundovým oneskorením vráti do ďalšieho jazyka.

### Etapa 3

Služba porovnáva aktuálny predvolený lokalizačný slovník so snímkom uloženým z predchádzajúceho spustenia:

- **Pridané klávesy** .
- **Removed keys** — entries present in the snapshot but absent from the current default — are deleted from every target language dictionary.
- Manuálne preklady majú vždy prednosť. Ak cieľový slovník už obsahuje hodnotu pre kľúč, tento záznam zostane nezmenený bez ohľadu na to, čo zdroj hovorí.
- **Každý slovník cieľového jazyka sa uloží ihneď po dokončení prekladov**, namiesto čakania na dokončenie všetkých jazykov.
- Ak sa preklad nepodarí pre konkrétny jazyk, služba sa automaticky vráti. Iba pretrvávajúce chyby (napr. nepodporovaný jazyk) spôsobujú, že jazyk je preskočený.
- Po spustení je aktuálny predvolený slovník uložený ako nový snímok pre ďalšie porovnanie.

Všetky slovníky sú vždy uložené abecedne zoradené kľúče a členité JSON pre ľudskú čitateľnosť.

### Etapa 4

Služba prechádza nakonfigurovanými dokumentačnými koreňmi (predvolené:) a spracováva každý zdrojový súbor rekurzívne:

1. Obsah zdrojového súboru sa prečíta a vypočíta sa SHA-256 hash.
2. Súbor vedľa zdrojových skladieb na-jazyk, na-blok stavu prekladu, čo umožňuje ** ďalšie re-preklad** iba neúspešných blokov.
3. Uložené hašiš z predchádzajúceho spustenia (zachovaný v súbore vedľa zdrojového súboru, alebo v dočasnom mieste núdzového volania) je porovnaný s aktuálnym hash.
4. Pre každý cieľový jazyk sa príslušný súbor kontroluje aj pre štrukturálnu integritu.
5. Akýkoľvek cieľový súbor, ktorý chýba, má zastaraný haš, zlyhá validácia štruktúry, alebo obsahuje nepreložené bloky je fronted pre re-preklad.
6. **Každý cieľový jazyk je preložený a uložený nezávisle** .
7. Úspešne preložené súbory sú validované pre štrukturálnu paritu so zdrojom (rovnaké položky, zoznam položiek, kódové bloky, blokky, odkazy, tučné / italic značky, a HTML značky) pred tým, než sú napísané na disk.
8. Ak všetky cieľové súbory pre zdroj uspejú, nový haš sa uloží vedľa zdroja. Ak písanie vedľa zdroja zlyhá (napríklad v nasadení iba na čítanie), haš spadne späť do dočasného adresára.
9. Ak akýkoľvek cieľový preklad zlyhá validáciou, metaúdaje označujú tieto bloky ako nepreložené, takže sú znovu vyskúšané na ďalšom spustení.

### Fáza 5

Konsolidácia sa zhromažďuje a uverejňuje. Zahŕňa:

- UTC beží štart a dokončenie časových pečiatok.
- Počet uložených lokálnych súborov JSON, uložené súbory Markdown, uložené hašové súbory a hašišové zápisy.
- Akékoľvek chyby pri skladovaní získané počas behu.
- Prekladové štatistiky za jazyk (preložený počet, preskočený počet, počet chýb).

## Signál R obálka správ

Každá udalosť pokroku je doručená ako s nasledujúcimi poľami:

Pole
|-------|------|-------------|
Korelačný identifikátor pre súčasný chod potrubia
Monotonické počítadlo v rámci behu, počnúc 1
Sémantický typ správy
Potrubná fáza, do ktorej správa patrí
Čas vydania správy UTC
Či správa predstavuje stav chyby
Zhrnutie čitateľné ľuďmi
Fázy špecifické užitočné zaťaženie (oznam objektu alebo null)

### Typy správ

Hodnota
|-------|------|---------|
0
1
2
3
4
5
6

### Štádiá potrubia

Hodnota
|-------|------|-------------|
0
1
2
3
4
5

### Typický tok správ

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

Ak nejaká fáza zlyhá, zostávajúce etapy sú preskočené, správa je emitované, a nakoniec správa ukončí beh.

## Logika prekladu

Plynovod využíva dve úrovne odolnosti:

### Etapa-level retry (PrekladRetryService)

- Ak žiadosť o preklad zlyhá po internom zápise LibreTranslate, vykoná až 3 ďalšie úkony na úrovni stupňa s 30-sekundovým oneskorením.
- Zakrývanie držadla: Pomenované osoby () v texte sú dočasne nahradené bezpečnými žetónmi () pred prekladom a potom obnovené, čím sa zabezpečí správna gramatika v cieľových jazykoch.

### Potvrdenie jazyka

- Pred prekladom do cieľového jazyka služba overí, či je jazyk podporovaný prekladateľským serverom.
- Nepodporované jazyky sú preskočené s varovaním, zabrániť opakované neúspešné pokusy.

### Zaradenie úrovne bloku

- Preklady Markdown sa vykonávajú podľa jednotlivých blokov (položky, odseky, položky zoznamu).
- Ak jednotlivý blok zlyhá preklad, je označený ako nepreložený v súbore metaúdajov a znovu nájdený v ďalšom ropovode.
- Služba sleduje per-language, na-blok stav v súboroch vedľa každého zdroja Markdown súboru.

## Kódy chýb

Chyby sa vykazujú použitím jednotného enumu zoskupeného do rozsahov:

Rozsah
|-------|----------|
1 000
2999
3000
40004999
5000

Každá chyba v správe obsahuje identifikátor zdroja (jazykový kód, cestu súboru alebo názov etapy), chybový kód a správu čitateľnú pre ľudí.

## živý preklad prístrojová doska

Projekt Server obsahuje admin stránku, na ktorej sa pripája do centra SignalR a zobrazuje všetky diaľkové udalosti v reálnom čase.

- Zobrazí stav pripojenia, počet správ a zoznam všetkých udalostí.
- Farebne kódované riadky: modrá pre začiatok etapy, zelená pre dokončenie, červená pre chyby.
- Podporuje čistenie kanálov a export všetkých správ JSON.
- Automaticky sa spojí s exponenciálnym vypnutím, ak spojenie klesne.

## Princípy návrhu

- **Modularita**: Každý problém s prekladom je izolovaný vo svojej vlastnej službe pre udržanie a testabilitu.
- ** Prírastková perzistencia **: Slovníky a Markdown súbory sú uložené v jednom jazyku ihneď po preklade, zníženie tlaku pamäte a poskytovanie skoršej spätnej väzby.
- **Vzdialenosť**: Viacnásobné úrovne opakovania (HTTP, štádium, blok) zabezpečujú, že prechodné poruchy neblokujú potrubie.
- ** Sledovanie štátu**: Metaúdaje za súbor () a hash súbory umožňujú presnú prírastkovú prácu na následných spustení.
- ** Viditeľnosť v reálnom čase**: Každá významná operácia sa oznamuje prostredníctvom SIGUNR na monitorovanie a ladenie.
- **Manual translations always have priority over automatic additions.**
