# Prekladateľská architektúra

Tento dokument opisuje modulárnu architektúru automatického prekladateľského systému Dita, ktorý bol zavedený na zlepšenie udržateľnosti, testability a odolnosti.

## Ciele projektu

Refaktoring riešil niekoľko obáv s pôvodným monolitický dizajn:

- ** Oddelenie obáv**: Každá prekladateľská doména (krajiny, slovníky JSON, Markdown) je izolovaná.
- ** Prírastková perzistencia **: Súbory sú uložené v jednom jazyku ihneď po preklade, zníženie používania pamäte a poskytovanie skorších výsledkov.
- ** Odolnosť**: Viaceré úrovne opakovania zvládajú prechodné poruchy bez blokovania celého potrubia.
- ** Pozorovateľnosť**: Každá významná operácia je hlásená prostredníctvom SignalR pre monitorovanie v reálnom čase.
- ** Extenzibilita **: Nové prekladateľské ciele možno pridať zavedením jedného rozhrania.

## Rozklad služieb

### Comment

** Povinnosti**:
- Riadenie životného cyklu potrubia (začiatok, dokončenie, manipulácia s chybami)
- Kontrola concurrency založená na Semophore (predchádza prekrývajúcim sa behom)
- Validácia servera (latencia, jazyková dostupnosť, konfigurácia)
- Delegovanie na subslužby

**Neobsahuje **:
- Prekladová logika
- Súbor I/O pre špecifické formáty
- Name

### KrajinyPrekladService

** Povinnosti**:
- Čítanie z adresára
- Synchronizovať názvy krajín do predvoleného slovníka locale
- Preložiť chýbajúce názvy krajín podľa cieľového jazyka
- Uložiť každý cieľový slovník ihneď po preklade

**Kľúčové správanie**:
- Ak je predvolený jazyk angličtina: názvy krajín uložené ako-is
- Ak je predvolený jazyk iný: anglické názvy preložené do predvoleného jazyka prvý
- Každý jazyk je spracovaný nezávisle s vlastnou slučkou retry

### Localization TranslationService

** Povinnosti**:
- Detekovať pridané/odstránené kľúče porovnaním aktuálneho predvoleného slovníka s predchádzajúcou snímkou
- Preložiť pridané kľúče do každého cieľového jazyka
- Odstrániť zmazané kľúče z každého cieľového jazyka
- Uložiť snímku pre ďalšie porovnanie

**Kľúčové správanie**:
- Manuálne preklady majú vždy prednosť (nikdy neprepísané)
- Pridané klávesy sú okamžite preložené a uložené v jednom jazyku
- Odstránené klávesy sú okamžite vymazané v jednom jazyku
- Snapshot sa uloží až po úspešnom dokončení všetkých jazykov

### DokumentyPrekladService

** Povinnosti**:
- Name
- Detekovať zmenené zdrojové súbory pomocou SHA-256 hašés
- Stav track per-block prekladu v
- preložiť blok podľa bloku s opakovaným preskúšaním podľa bloku
- Overiť Markdown štruktúru po preklade
- Uložiť každý súbor cieľového jazyka nezávisle

**Kľúčové správanie**:
- Zrnitá veľkosť bloku: nadpisy, odseky, položky zoznamu sa prekladajú samostatne
- Stopy metaúdajov, ktoré zablokovali/zlyhali podľa jazyka
- Zlyhané bloky sú znovu získané na ďalšom spustení bez opätovného prenosu úspešných blokov
- Validácia štruktúry zabezpečuje počet okruhov, zoznamy, bloky kódov atď. zodpovedajúceho zdroja

## Stratégia

Systém zavádza dotrukcie na troch úrovniach:

### Úroveň 1

- Až 5 pokusov s exponenciálnym spätným účinkom (1s, 2s, 3s, 4s, 5s)
- Ovláda timeouty siete, chyby 5xx a prechodné poruchy
- Zabudované do konfigurácie klienta HTTP

### Úroveň 2

- Až 3 pokusy s 30-sekundovým oneskorením
- Re-drives celá žiadosť o preklad po HTTP-level opakovaní sú vyčerpané
- Zakrývanie a obnova miesta sa uplatňuje na tejto úrovni

### Úroveň 3

- Jednotlivé Markdown bloky, ktoré zlyhali sú označené v metadátach
- Automaticky vyskúšaný na ďalšom chode potrubia
- Úspešné bloky nie sú nikdy preložené

## Tok údajov

### Preklad slovníka JSON

```
[Default Dictionary]
       ↓
[Compare with Snapshot]
       ↓
[Detect Added/Removed Keys]
       ↓
For each target language:
   ├─ Load existing dictionary
   ├─ Apply removals
   ├─ Translate additions (with retry)
   └─ Save dictionary immediately
       ↓
[Save new snapshot]
```

### Markdown preklad

```
[Source File]
       ↓
[Compute Hash]
       ↓
[Compare with stored hash]
       ↓
[Load translation metadata]
       ↓
For each target language:
   ├─ Extract translatable blocks
   ├─ For each block:
   │   ├─ Check metadata (already translated?)
   │   ├─ Translate with retry
   │   └─ Mark success/failure in metadata
   ├─ Reconstruct Markdown
   ├─ Validate structure
   ├─ Save target file
   └─ Save metadata
```

### Preklad názvu krajiny

```
[countries.json]
       ↓
[Build set of country keys]
       ↓
[Update default dictionary]
       ↓
For each target language:
   ├─ Find missing country entries
   ├─ Translate each missing entry (with retry)
   └─ Save dictionary immediately
```

## Pretrvávanie stavu

### Snímky

- **JSON**: Stored in a file next to the default dictionary (name varies by storage provider)
- **Purpose**: Umožňuje prírastkovú synchronizáciu sledovaním toho, čo bolo prítomné v predchádzajúcom behe

### Hash súbory

- ** Markdown**: vedľa zdrojového súboru
- **Fallback**: ak je primárne miesto iba na čítanie
- **Purpose**: Odhaľuje zmeny zdrojov, aby sa predišlo zbytočnej retranslácii

### Prekladové metaúdaje

- ** Markdown **:
- ** Obsah **:
  - Zdrojový obsah haš
- Stav bloku na jeden jazyk (pole booleanov)
- Časová pečiatka poslednej aktualizácie
- **Purpose**: Povolí čiastočnú retransláciu iba neúspešných blokov

### Uskladňovanie miesta

- ** File **:
- **Obsah**: Slovník kľúčov pre páry s menovkou
- **Purpose**: Poskytuje predvolené hodnoty pre pomenovaných držiteľov stanov v rámci celej aplikácie

## Hlásenie signáluR

### Vydavateľská abstrakcia

oddeľuje prekladateľské služby od špecifík SignalR:

```csharp
public interface ISignalRPublisher
{
    Task PublishStageAsync<T>(Guid runId, ProcessStage stage, T data, ...);
    Task PublishMessageAsync(Guid runId, LocalizationMessageType type, ProcessStage stage, ...);
}
```

### Poradové záruky

- Správy v rámci jedného behu sú monotónne sekvencované
- Poradové čísla sú jedinečné na-run cez
- Klienti môžu odhaliť medzery alebo zmeny poradia

### Mapovanie náboja

```csharp
app.MapHub<LocalizationHub>("/hubs/localization");
```

## Body predĺženia

### Pridanie nového prekladateľského cieľa

1. Vytvoriť nové rozhranie s
2. Implementovať rozhranie s logikou špecifickej pre doménu
3. Registrácia v kontajneri DI
4. Vstreknite do konštruktéra
5. Výzva po existujúcich fázach

### Vlastná retry politika

Prepísať parametre konštruktéra:

```csharp
services.AddSingleton<TranslationRetryService>(
    sp => new TranslationRetryService(
        sp.GetRequiredService<ILibreTranslateService>(),
        sp.GetRequiredService<IPlaceholderService>(),
        sp.GetRequiredService<ILogger<TranslationRetryService>>(),
        stageMaxRetries: 5,        // More retries
        stageRetryDelaySeconds: 60  // Longer delays
    ));
```

### Custom placenther manipulation

Implementovať s cieľom zmeniť syntax alebo skladovanie držiteľa miesta:

```csharp
public class CustomPlaceholderService : IPlaceholderService
{
    // Use {{name}} syntax instead of {name}
    // Store in database instead of JSON file
    // Encrypt sensitive placeholder values
}
```

## Nastavenie

### appsettings.json

```json
{
  "AutomaticTranslationSettings": {
    "DefaultLanguage": "en",
    "IgnoredLanguages": ["auto", "detect"],
    "MarkdownRoots": ["/Docs", "/Help"],
    "AutomaticRun": true,
    "CheckingPeriod": 30,
    "WaitingTime": "00:00:00",
    "RequestThrottleMs": 80,
    "RequestTimeoutSeconds": 10
  }
}
```

### ladenie času

Nastavenie
|---------|---------|--------|
80
10
3
30

## Stratégia testovania

### Skúšky jednotiek

Každá čiastková služba je nezávisle otestovateľná:

- Mock simulovať úspech/zlyhanie
- Mock na overenie hlásenia
- Použiť dočasné adresáre pre súbor I/ O
- Overiť správanie v jednotlivých jazykoch

### Integračné skúšky

- Full ropovod beh s reálnym (lokálne) LibreTranslate instance
- Overiť SignalR správy sú dodávané pripojeným klientom
- Test súbežne spustiť prevenciu (semafore)
- Overiť Markdown štruktúru po preklade

### Koncové testy

- Spúšťací preklad cez API alebo programovač
- Overiť všetky súbory cieľového jazyka sú vytvorené/aktualizované
- Kontrola súborov metaúdajov obsahuje správny stav bloku
- Potvrdiť, že držitelia sú uchovaní v prekladoch

## Úvahy o výkonnosti

- **Pamätník**: Ukladanie podľa jazyka zabraňuje uchovaniu všetkých slovníkov v pamäti
- **Disk I/O**: Súbory metadát pridávajú malé režijné náklady, ale umožňujú prírastkovú prácu
- **Sieť**: Postupné spracovanie s thrkotanie zabraňuje ohromujúci LibreTranslát
- **CPU**: SHA-256 overenie hashingu a regexu sú rýchle v porovnaní s latenciou prekladu
- **SignalR**: Ľahké správy, žiadna kompresia užitočného zaťaženia potrebná pre typické hlásenia

## Prechod z monolitického dizajnu

Originál obsahoval celú logiku v jednej triede. Cesta prechodu:

1. Vyberte krajinu logiku →
2. Extrahovať JSON logiku →
3. Extrahovať Markdown logiku →
4. Vydávanie extraktu SIGNÁL →
5. Logická extrakcia →
6. Zjednodušiť orchestra len na delegáciu

Všetky existujúce rozhrania () zostávajú nezmenené. Spotrebitelia potrubia nevidia žiadne prelomové zmeny.
