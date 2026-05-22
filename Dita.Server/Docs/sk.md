# Zhrnutie zmien automatickej prekladateľskej služby

## Prehľad

Tento dokument sumarizuje všetky zmeny v automatickej prekladateľskej službe Dita, vrátane refactoringu architektúry, nových funkcií, vylepšení viditeľnosti a vylepšení lokalizácie.

## Zmeny architektúry

### Refactored backendtranslationService

Monolitická bola rozčlenená na štyri špecializované služby koordinované ľahkým orchesterom:

- **BackendTranslationService**
- **CountriesTranslationService**
- **LokalizáciaPrekladSlužba**
- **DocumentsTranslationService**
- **SignalRPublisher**
- **TranslationRetryService**

### Prínosy

- ** Oddelenie obáv**: Každá služba spracováva jednu doménu prekladu
- ** Udržovateľnosť**: Menšie triedy sa ľahšie chápu a testujú
- ** Extenzibilita **: Nové prekladateľské ciele možno pridať prostredníctvom implementácie rozhrania
- **Spoľahlivosť**: Nezávislé služby poskytujú lepšiu izoláciu chýb

## Nové funkcie

### Monitor živého prekladu

** Miesto**:

Nová admin stránka, ktorá poskytuje viditeľnosť v reálnom čase do prekladového potrubia:

- Zobrazí všetky udalosti SignalR, keď sa vyskytnú
- Farebne kódované typy správ (blue=started, green= completed, red=error)
- Pripojenie banner s auto-opätovným pripojením
- Počítadlo správ a export do JSON

### Pomenovaní držitelia miest

Systém lokalizácie teraz podporuje pomenované osoby () pre zlepšenie gramatiky v rôznych jazykoch:

```csharp
// Usage in code
var message = Localizer["WelcomeMessage", new Dictionary<string, string>
{
    ["userName"] = "John",
    ["count"] = "5"
}];
// Result: "Hello John, you have 5 new messages"
```

Vlastnosti:
- Hodnoty umiestnenia poskytnuté v čase zábehu alebo uložené v
- Automatické maskovanie/obnovenie počas prekladu, aby sa zabránilo korupcii
- Spätne kompatibilné s existujúcimi pozičnými stanovišťami

### Prírastkový preklad

Markdown súbory sú preložené postupne:

- ** Per- language sporenie **: Každý cieľový jazyk je uložený okamžite po preklade, zníženie tlaku pamäte
- ** Sledovanie na úrovni bloku**: stav prekladania skladieb na blok
- ** Selektívne opakovanie **: Iba zlyhali bloky sú re-preložené na ďalšie spustenie
- ** Pretrvávanie údajov **: Prekladový stav prežije reštartovanie aplikácie

### Vylepšená logická rézia

Tri úrovne odolnosti:

1. ** HTTP retry** (LibreTranslateService): 5 pokusov s exponenciálnym spätným účinkom (1s
2. **Stage retry** (TranslationRetryService): 3 ďalšie pokusy s 30-ročnými oneskoreniami
3. **Block retry** (DocumentsTranslationService): Failed Markdown locks retried on next run

### Hlásenie signálu

Podávanie správ o pokroku v reálnom čase pre všetky potrubné operácie:

- Každá etapa zverejňuje udalosti
- Per-jazykový pokrok zverejnený ako podujatia
- Chybové udalosti zahŕňajú podrobný kontext (zdroj, chybový kód, správu)
- Poradové čísla zaručujú objednávanie v rámci každého behu

## Zmeny konfigurácie

### appsettings.json

Žiadne zmeny. Existujúca konfigurácia naďalej funguje:

```json
{
  "AutomaticTranslationSettings": {
    "DefaultLanguage": "en",
    "IgnoredLanguages": ["auto", "detect"],
    "MarkdownRoots": ["/Docs"],
    "AutomaticRun": true,
    "CheckingPeriod": 30,
    "WaitingTime": "00:00:00"
  }
}
```

### Nové služby

Zaevidovaná v:

- /
- `TranslationRetryService`
- /
- /
- /
- /

SignalR centrum je mapované pre klientske spojenia.

## Testovanie

### Stav testu

- **243/244 absolvovaných testov** (1 preskočených kvôli súbežnému prístupu k súboru v testovacom prostredí)
- Nové skúšobné pokrytie pridané pre:
  - Funkcia PlacelderService
  - BackendPrekladService orchestration
  - JsonStringLocalizer indexers

### Známe obmedzenia

- test je preskočený pri spustení paralelne, pretože viaceré testovacie inštancie zdieľajú rovnaký súbor. Prechádza, keď beží v izolácii.

## Nová štruktúra súboru

### Služby v

- — Pipeline orchestrator
- Preklad názvu krajiny
- Synchronizácia slovníka JSON
- — Markdown translation
- — SignalR message publishing
- — Retry logic with placeholder masking
- — Publisher interface
- — Country service interface
- — Localization service interface
- — Document service interface
- — Orchestrator interface (updated)
- — Per-file translation metadata

### Aktualizované služby v

- — Added named placeholder support
- aktualizované pre nový parameter
- — Named placeholder management
- — Placeholder interface

### Nová stránka admin v

- — Real-time monitoring page
- Model stránky

### Nová dokumentácia

- — Updated pipeline documentation
- — Placeholder system guide
- — Dashboard usage guide
- prehľad technickej architektúry

## Spätná zlučiteľnosť

Všetky zmeny sú doplnkové:

- Existujúci lokalizačný kód () funguje nezmenený
- Formátovanie polohy () funguje nezmenené
- Existujúci formát slovníka JSON je nezmenený
- Existujúca štruktúra Markdown je nezmenená
- SignalR správy používajú rovnaký formát

## Cesta k migrácii

Nevyžaduje sa migrácia. Refaktorizácia je vnútorná:

1. Starý bol zachovaný ako referencia a potom nahradený
2. Registrácie DI boli aktualizované s cieľom používať nové rozhrania
3. Všetci súčasní spotrebitelia nevidia žiadne zmeny

## Zlepšenia výkonnosti

- ** Znížené používanie pamäte**: Súbory uložené v jednom jazyku okamžite namiesto toho, aby držali všetko v pamäti
- ** Rastúce prírastkové otáčky**: Iba zmenené / neúspešné Markdown bloky sú znovu preložené
- ** Lepšia viditeľnosť**: Pokrok v reálnom čase pomáha diagnostikovať pomalé fázy

## Budúce zlepšenia

Plánované zlepšenia:

1. **AI dolaďovanie**
2. **Admin autentifikácia**
3. **Slovníkový editor**
4. ** Štatistika prekladov**
5. **Custom placenthold syntax**

## Kontakt

Pre otázky alebo problémy s prekladateľskou službou, pozrite si podrobnú dokumentáciu v adresári každého modulu alebo kontaktujte vývojový tím.
