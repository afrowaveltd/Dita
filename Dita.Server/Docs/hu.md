# Az automatikus fordítási szolgáltatás módosításainak összefoglalása

## Összefoglaló

Ez a dokumentum összefoglalja a Dita automatikus fordítási szolgáltatás valamennyi változását, beleértve az architektúrákat, az új funkciókat, a megfigyelhetőségi fejlesztéseket és a lokalizációs fejlesztéseket.

## Építészeti változások

### Átdolgozott Fordítás

A monolitikum négy speciális szolgáltatássá bomlott, melyeket egy könnyűsúlyú hangszer koordinál:

- **BackendTranslationService** — Pipeline orchestrator (server validation, stage delegation, error handling)
- **CountriesTranslationService** — Country name synchronization (English → target language)
- **LocalizationTranslationService** — JSON dictionary synchronization (added/removed keys)
- **DocumentsTranslationService** — Markdown documentation translation with block-level tracking
- **SignalRPublisher** — Real-time progress reporting via SignalR
- **TranslationRetryService** — Stage-level retry with placeholder preservation

### Előnyök

- **Separation of concerns**: Each service handles a single translation domain
- **Maintainability**: Smaller classes are easier to understand and test
- **Extensibility**: New translation targets can be added via interface implementation
- **Reliability**: Independent services provide better fault isolation

## Új jellemzők

### Élő fordító

**Location**: `/Admin/LiveTranslation`

Egy új adminisztrációs oldal, amely valós idejű láthatóságot biztosít a fordítóvezetékben:

- Megjeleníti az összes SignalR eseményt, ahogy azok előfordulnak
- Színezett kódolt üzenettípusok (kék = indítás, zöld = befejezés, piros = hiba)
- Csatlakozási állapot banner auto- reconnect
- Üzenetszámláló és exportálás JSON-ba

### Szelepek elnevezése

A lokalizációs rendszer most támogatja a neves plakettezők () a jobb nyelvtanítás különböző nyelveken:

```csharp
// Usage in code
var message = Localizer["WelcomeMessage", new Dictionary<string, string>
{
    ["userName"] = "John",
    ["count"] = "5"
}];
// Result: "Hello John, you have 5 new messages"
```

Jellemzők:
- A területfoglalási értékek futásidőben vagy
- Automatikus elfedés / helyreállítás fordítás közben a korrupció megelőzése érdekében
- Hátrameneti kompatibilis a meglévő helyzetjelző táblákkal

### kiegészítő fordítás

Jelölési fájlok fordítása fokozatosan:

- **Per-language saving**: Each target language is saved immediately after translation, reducing memory pressure
- **Block-level tracking**: `.translation-meta.json` tracks translation status per block
- **Selective retry**: Only failed blocks are re-translated on the next run
- **Metadata persistence**: Translation state survives application restarts

### Fokozott retry logika

Három szintű ellenálló képesség:

1. **HTTP retry** (LibreTranslateService): 5 attempts with exponential backoff (1s–5s)
2. **Stage retry** (TranslationRetryService): 3 additional attempts with 30s delays
3. **Block retry** (DocumentsTranslationService): Failed Markdown blocks retried on next run

### SignalR Reporting

Az összes csővezeték-üzemeltetésre vonatkozó valós idejű helyzetjelentés:

- Minden szakasz közzéteszi az eseményeket
- Rendezvényként publikált nyelvi haladás
- A hibaesemények közé tartozik a részletes háttér (forrás, hibakód, üzenet)
- A szekvencia számok garantálják a rendelést minden egyes menetben

## Beállítások

### apsettings.json

Nincs törés. A meglévő konfiguráció továbbra is működik:

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

### Új szolgáltatások

Regisztrálva:

- /
- `TranslationRetryService`
- /
- /
- /
- /

A SignalR csomópontot feltérképezték az ügyfélkapcsolatokra.

## Vizsgálat

### Vizsgálati állapot

- **243/244 tests passing** (1 skipped due to concurrent file access in test environment)
- Új vizsgálati lefedettség hozzáadva:
  - PlaceholderService funkcionalitás
  - BackendTranslationService zenekara
  - JsonStringLocalizer plaketter indexek

### Ismert korlátozások

- a vizsgálat kimarad, ha párhuzamosan fut, mert több vizsgálati eset ugyanazt a fájlt használja. Elmúlik, ha egyedül fut.

## Új fájlszerkezet

### Szolgáltatások

- - Pipeline hangszóró
- - Ország név fordítás
- - JSON szótár szinkronizálás
- - Jelölés fordítás
- - SignalR üzenetkiadás
- - A logika visszaállítása a placeholder maszkjával
- - Publisher interface
- - Ország szolgáltatás interfész
- - Lokalizáció szolgáltatás interfész
- - Dokumentumszolgáltatás interfész
- - Regisztrátor interfész (frissítve)
- - Per- file fordítási metaadatok

### Frissített szolgáltatások

- - Hozzáadott név placeholder támogatás
- - Új paraméter frissítése
- - Lakástulajdonos-kezelés
- - Helyettesítő interfész

### Új Admin oldal

- - Real-time monitoring oldal
- - Oldalmodell

### Új dokumentáció

- - Frissített csővezeték dokumentáció
- - Helyettesítő rendszer útmutató
- - Dashboard használati útmutató
- - Műszaki architektúra áttekintés

## Hátrameneti összeegyeztethetőség

Minden módosítás adalékanyag:

- A meglévő lokalizációs kód () változatlan
- A pozicionálási forma () változatlan
- A meglévő JSON szótár formátuma változatlan
- A meglévő jelzésszerkezet változatlan
- A SignalR üzenetek ugyanazt a formátumot használják

## Migrációs útvonal

Nincs szükség migrációra. A kritika belső:

1. A régi maradt, mint egy hivatkozás, majd felváltotta
2. A DI regisztrációkat frissítették az új interfészek használatához
3. Minden meglévő fogyasztó nem lát változást

## Teljesítményjavítások

- **Reduced memory usage**: Files saved per-language immediately instead of holding all in memory
- **Faster incremental runs**: Only changed/failed Markdown blocks are re-translated
- **Better visibility**: Real-time progress helps diagnose slow stages

## Jövőbeli fejlesztések

Tervezett fejlesztések:

1. **AI fine-tuning** — Post-machine translation review for phrases > 5 words
2. **Admin authentication** — Restrict admin pages to authorized users
3. **Dictionary editor** — Web UI for managing localization keys
4. **Translation statistics** — Charts showing translation counts and error rates over time
5. **Custom placeholder syntax** — Support for alternate placeholder formats

## Kapcsolat

A fordítási szolgáltatással kapcsolatos kérdésekért vagy kérdésekért kérjük, olvassa el az egyes modulok könyvtárában található részletes dokumentációt, vagy lépjen kapcsolatba a fejlesztési csoporttal.
