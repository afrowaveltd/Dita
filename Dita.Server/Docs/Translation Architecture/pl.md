# Architektura tłumaczeń

Niniejszy dokument opisuje modułową architekturę automatycznego systemu tłumaczeniowego Dita, wprowadzoną w celu poprawy zachowania, stabilności i odporności.

## Cele projektu

Refaktoring odniósł się do kilku kwestii w pierwotnej konstrukcji monolitycznej:

- **Separation of concerns**: Each translation domain (countries, JSON dictionaries, Markdown) is isolated.
- **Incremental persistence**: Files are saved per-language immediately after translation, reducing memory usage and providing earlier results.
- **Resilience**: Multiple retry levels handle transient failures without blocking the entire pipeline.
- **Observability**: Every significant operation is reported via SignalR for real-time monitoring.
- **Extensibility**: New translation targets can be added by implementing a single interface.

## Rozkład usług

### BackendTranslationService (orkiestrator)

**Responsibilities**:
- Zarządzanie cyklem życia rurociągów (rozpoczęcie, ukończenie, obsługa błędów)
- Sterowanie kontraktami oparte na semaforach (zapobiega pokrywaniu się połączeń)
- Walidacja serwera (opóźnienie, dostępność języka, konfiguracja)
- Delegacja do podsłużb

**Does NOT contain**:
- Logika tłumaczenia
- Plik I / O dla określonych formatów
- Logika przywracania

### CountriesTranslationService

**Responsibilities**:
- Czytaj z katalogu
- Synchronizuj nazwy krajów do domyślnego słownika locale
- Przetłumacz brakujące nazwy krajów na język docelowy
- Zapisz każdy słownik docelowy natychmiast po tłumaczeniu

**Key behaviors**:
- Jeśli domyślny język jest angielski: nazwy kraju przechowywane jako -is
- Jeśli domyślny język jest inny: angielskie nazwy przetłumaczone na język domyślny
- Każdy język jest przetwarzany niezależnie za pomocą własnej pętli retry

### LokalizacjaTranslationService

**Responsibilities**:
- Wykrywanie dodanych / usuniętych klawiszy przez porównanie bieżącego domyślnego słownika z poprzednim migawką
- Przetłumacz dodane klawisze na każdy język docelowy
- Usuń usunięte klucze z każdego języka docelowego
- Zapisz migawkę do następnego porównania

**Key behaviors**:
- Tłumaczenia ręczne zawsze mają pierwszeństwo (nigdy nie są nadpisane)
- Dodano klawisze są natychmiast przetłumaczone i zapisane perilanguage
- Usunięte klucze są natychmiast usuwane w języku per-
- Snapshot jest zapisywany tylko po pomyślnym zakończeniu wszystkich języków

### Dokumenty TranslationService

**Responsibilities**:
- Walk konfigurowane korzenie markdown rekursywnie
- Wykrywanie zmienionych plików źródłowych przy użyciu sha- 256 hash
- Status tłumaczenia przeblokowego utworu w
- Przetłumacz block- by- block z powtórzeniem bloku per-
- Potwierdź strukturę markdown po przetłumaczeniu
- Zapisz każdy plik języka docelowego niezależnie

**Key behaviors**:
- Graniowość na poziomie blokady: nagłówki, akapity, pozycje listy są tłumaczone oddzielnie
- Ścieżki metadanych, które blokują sukces / awarię na język
- Nieudane bloki są ponownie testowane w następnym uruchomieniu bez ponownego przetłumaczenia udanych bloków
- Walidacja struktury zapewnia ilość nagłówków, list, bloków kodu itp. źródło dopasowania

## Strategia pobierania

System wdraża powtórki na trzech poziomach:

### Poziom 1 - HTTP (LibreTranslateService)

- Do 5 prób z odwróceniem wykładniczym (1 s, 2 s, 3 s, 4 s, 5 s)
- Obsługuje timeout sieciowy, błędy 5xx i usterki przejściowe
- Wbudowany w konfigurację klienta HTTP

### Poziom 2 - etap (TranslationRetriService)

- Do 3 prób z 30-sekundowymi opóźnieniami
- Przekierowuje cały wniosek o tłumaczenie po powtórnych próbach poziomu HTTP są wyczerpane
- Na tym poziomie stosuje się maskowanie i przywracanie uchwytu

### Poziom 3 - Blok (DocumentTranslationService)

- Indywidualne bloki Markdown, które nie są zaznaczone w metadanych
- Retried automatycznie na następnym rurociągu
- Udane bloki nigdy nie są przetłumaczone

## Przepływ danych

### Tłumaczenie słownika JSON

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

### Tłumaczenie markdown

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

### Tłumaczenie nazwy kraju

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

## Wytrwałość państwa

### Szampony

- **JSON**: Stored in a file next to the default dictionary (name varies by storage provider)
- **Purpose**: Enables incremental sync by tracking what was present in the previous run

### Pliki Hash

- **Markdown**: `{sourceFile}.hash.json` next to the source file
- **Fallback**: `{tempDir}/{sanitizedPath}.hash.json` if primary location is read-only
- **Purpose**: Detects source changes to avoid unnecessary re-translation

### Metadane tłumaczeń

- **Markdown**: `{sourceFile}.translation-meta.json`
- **Contents**:
  - Zawartość źródła hash
- Per- language block status (tablica boolends)
- Ostatni znacznik czasu aktualizacji
- **Purpose**: Enables partial re-translation of only failed blocks

### Przechowywanie zasobników

- **File**: `Locales/placeholders.json`
- **Contents**: Dictionary of keys to placeholder name-value pairs
- **Purpose**: Provides default values for named placeholders across the application

## Sygnał R sprawozdawczość

### Abstrakcja wydawcy

oddziela usługi tłumaczeniowe od specyfiki SignalR:

```csharp
public interface ISignalRPublisher
{
    Task PublishStageAsync<T>(Guid runId, ProcessStage stage, T data, ...);
    Task PublishMessageAsync(Guid runId, LocalizationMessageType type, ProcessStage stage, ...);
}
```

### Gwarancje sekwencji

- Wiadomości w ramach pojedynczego uruchomienia są monotonicznie sekwencjonowane
- Numery sekwencji są unikalne per- run poprzez
- Klienci mogą wykryć luki lub przezamawianie

### Mapowanie piasty

```csharp
app.MapHub<LocalizationHub>("/hubs/localization");
```

## Punkty rozszerzenia

### Dodanie nowego celu tłumaczenia

1. Utwórz nowy interfejs z
2. Wdrożenie interfejsu z logiką domain- specific
3. Zarejestruj się w kontenerze DI
4. Wstrzyknąć do konstruktora
5. Zaproszenie po istniejących etapach

### Niestandardowa polityka ponownego testowania

Przekroczyć parametry konstruktora:

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

### Niestandardowe postępowanie z posiadaczem miejsca

Wdrożenie zmiany składni lub składni posiadacza miejsca:

```csharp
public class CustomPlaceholderService : IPlaceholderService
{
    // Use {{name}} syntax instead of {name}
    // Store in database instead of JSON file
    // Encrypt sensitive placeholder values
}
```

## Konfiguracja

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

### Dostrajanie czasu działania

Ustawienie
|---------|---------|--------|
80
10
3
30

## Strategia badań

### Badania jednostkowe

Każda podusługa jest niezależnie testowana:

- Mock do symulacji sukcesu / porażki
- Mock do weryfikacji raportowania
- Użyj tymczasowych katalogów dla pliku I / O
- Zweryfikuj zachowanie oszczędzania przejęzykowego

### Testy integracji

- Pełna gazociąg uruchomić z rzeczywistym (lokalnym) LibreTranslate instancji
- Weryfikacja sygnału Komunikaty R są dostarczane do podłączonych klientów
- Profilaktyka równoczesnego prowadzenia badań (semafora)
- Potwierdź strukturę markdown po przetłumaczeniu

### Badania końcowe

- Tłumaczenie za pomocą API lub terminarza
- Weryfikacja tworzenia / aktualizacji wszystkich plików języka docelowego
- Sprawdź pliki metadanych zawierają poprawny status bloku
- Potwierdzani posiadacze miejsc są zachowane w różnych tłumaczeniach

## Uwagi dotyczące wyników

- **Memory**: Per-language saving prevents holding all dictionaries in memory
- **Disk I/O**: Metadata files add small overhead but enable incremental work
- **Network**: Sequential processing with throttling prevents overwhelming LibreTranslate
- **CPU**: SHA-256 hashing and regex validation are fast relative to translation latency
- **SignalR**: Lightweight messages, no payload compression needed for typical reports

## Migracja z projektu monolitycznego

Oryginał zawierał całą logikę w jednej klasie. Ścieżka migracji:

1. Wyciągnij logikę kraju →
2. Wyciąg logiki JSON →
3. Wyciąg logiki Markdown →
4. Wyciągnij sygnał R publikacji →
5. Wyciąg retry logiki →
6. Uproszczenie organizatora do delegowania - tylko

Wszystkie istniejące interfejsy () pozostają niezmienione. Konsumenci rurociągu nie widzą żadnych przełomowych zmian.
