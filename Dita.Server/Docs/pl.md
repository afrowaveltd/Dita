# Podsumowanie zmian do usługi automatycznego tłumaczenia

## Przegląd

Niniejszy dokument podsumowuje wszystkie zmiany wprowadzone do usługi tłumaczenia automatycznego Dita, w tym refakturowanie architektury, nowe funkcje, poprawę widoczności i ulepszenie lokalizacji.

## Zmiany architektury

### refakturowane usługi translacyjne

Monolit został podzielony na cztery specjalistyczne usługi koordynowane przez lekki orchestrator:

- **BackendTranslationService** — Pipeline orchestrator (server validation, stage delegation, error handling)
- **CountriesTranslationService** — Country name synchronization (English → target language)
- **LocalizationTranslationService** — JSON dictionary synchronization (added/removed keys)
- **DocumentsTranslationService** — Markdown documentation translation with block-level tracking
- **SignalRPublisher** — Real-time progress reporting via SignalR
- **TranslationRetryService** — Stage-level retry with placeholder preservation

### Korzyści

- **Separation of concerns**: Each service handles a single translation domain
- **Maintainability**: Smaller classes are easier to understand and test
- **Extensibility**: New translation targets can be added via interface implementation
- **Reliability**: Independent services provide better fault isolation

## Nowe funkcje

### Monitor tłumaczeń na żywo

**Location**: `/Admin/LiveTranslation`

Nowa strona admin, która zapewnia real- time widoczność do rurociągu tłumaczenia:

- Wyświetla wszystkie zdarzenia SignalR w miarę ich występowania
- Kolorowe typy wiadomości (niebieski = rozpoczęty, zielony = zakończony, czerwony = błąd)
- Baner stanu połączenia z auto- reconnect
- Licznik wiadomości i eksport do JSON

### Nazwy posiadaczy instalacji

System lokalizacji obsługuje teraz nazwane placeholders () dla poprawy gramatyczności w różnych językach:

```csharp
// Usage in code
var message = Localizer["WelcomeMessage", new Dictionary<string, string>
{
    ["userName"] = "John",
    ["count"] = "5"
}];
// Result: "Hello John, you have 5 new messages"
```

Cechy:
- Wartości uchwytu w czasie pracy lub przechowywane w
- Automatyczne maskowanie / przywracanie podczas tłumaczenia w celu zapobiegania korupcji
- Kompatybilność wsteczna z istniejącymi posiadaczami miejsc pozycyjnych

### Tłumaczenie dodatkowe

Pliki Markdown są tłumaczone stopniowo:

- **Per-language saving**: Each target language is saved immediately after translation, reducing memory pressure
- **Block-level tracking**: `.translation-meta.json` tracks translation status per block
- **Selective retry**: Only failed blocks are re-translated on the next run
- **Metadata persistence**: Translation state survives application restarts

### Zwiększona logika retry

Trzy poziomy odporności:

1. **HTTP retry** (LibreTranslateService): 5 attempts with exponential backoff (1s–5s)
2. **Stage retry** (TranslationRetryService): 3 additional attempts with 30s delays
3. **Block retry** (DocumentsTranslationService): Failed Markdown blocks retried on next run

### Sprawozdawczość w zakresie sygnałów

Real- time progress reporting for all gapes operations:

- Każdy etap publikuje wydarzenia
- Per- postęp językowy opublikowany jako wydarzenia
- Zdarzenia błędów obejmują szczegółowy kontekst (źródło, kod błędu, wiadomość)
- Numery sekwencji gwarantują zamawianie w ramach każdej operacji

## Zmiany konfiguracji

### appsettings.json

Żadnych zmian. Istniejąca konfiguracja nadal działa:

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

### Nowe usługi

Zarejestrowany w:

- /
- `TranslationRetryService`
- /
- /
- /
- /

Głowica SignalR jest przyporządkowana do połączeń z klientami.

## Badanie

### Status badania

- **243/244 tests passing** (1 skipped due to concurrent file access in test environment)
- Dodano nowy zakres badań dla:
  - Funkcje PlaceholderService
  - translationservice orchestration
  - JsonStringLocalizer indexers

### Znane ograniczenia

- test jest pomijany podczas równoległego uruchamiania, ponieważ wiele instancji testowych dzieli ten sam plik. Przechodzi, gdy biegnie w izolacji.

## Nowa struktura pliku

### Usługi

- - Orchestrator rurociągu
- - Tłumaczenie nazwy kraju
- - Synchronizacja słownika JSON
- - Tłumaczenie Markdown
- - Wydawnictwo wiadomości SignalR
- - Retry logika z maskowania uchwytu
- - Interfejs wydawcy
- - Interfejs usług krajowych
- - Interfejs usług lokalizacyjnych
- - Interfejs obsługi dokumentów
- - Interfejs Orchestratora (zaktualizowany)
- - Per- plik metadanych tłumaczenie

### Aktualizacja usług w

- - Dodano nazwę wsparcia opiekuna
- - Aktualizacja nowego parametru
- - Zarządzanie nazwami właścicieli miejsc
- - Interfejs schowka

### Nowa strona administracyjna

- - Strona monitorowania czasu rzeczywistego
- - Model strony

### Nowa dokumentacja

- - Aktualizacja dokumentacji rurociągu
- - Przewodnik po systemie uchwytów
- - Przewodnik użytkowania deski rozdzielczej
- - Przegląd architektury technicznej

## Zgodność wsteczna

Wszystkie zmiany są addytywne:

- Istniejący kod lokalizacji () działa bez zmian
- Formowanie pozycyjne () działa bez zmian
- Istniejący format słownika JSON jest niezmieniony
- Istniejąca struktura markdown pozostaje niezmieniona
- Komunikaty SignalR używają tego samego formatu

## Ścieżka migracyjna

Migracja nie jest wymagana. Refaktoring jest wewnętrzny:

1. Stary został zachowany jako odniesienie, a następnie zastąpiony
2. Rejestracja DI została zaktualizowana w celu wykorzystania nowych interfejsów
3. Wszyscy obecni konsumenci nie widzą zmian

## Poprawa wydajności

- **Reduced memory usage**: Files saved per-language immediately instead of holding all in memory
- **Faster incremental runs**: Only changed/failed Markdown blocks are re-translated
- **Better visibility**: Real-time progress helps diagnose slow stages

## Przyszłe udoskonalenia

Planowane ulepszenia:

1. * * * AI fine- tuning * * * - Przegląd tłumaczenia maszynowego frazy > 5 słów
2. **Admin authentication** — Restrict admin pages to authorized users
3. **Dictionary editor** — Web UI for managing localization keys
4. **Translation statistics** — Charts showing translation counts and error rates over time
5. **Custom placeholder syntax** — Support for alternate placeholder formats

## Kontakt

W przypadku pytań lub problemów związanych z tłumaczeniem prosimy zapoznać się ze szczegółową dokumentacją w katalogu każdego modułu lub skontaktować się z zespołem ds. rozwoju.
