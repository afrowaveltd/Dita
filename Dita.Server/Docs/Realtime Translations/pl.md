# Tłumaczenie w czasie rzeczywistym

Dokument ten istnieje jako wejście testowe na żywo dla automatycznego rurociągu tłumaczeniowego. Każda zmiana w tym pliku powoduje ponowne tłumaczenie wszystkich plików języka docelowego podczas następnego zaplanowanego uruchomienia.

## Przegląd architektury

Gazociąg został przekształcony w architekturę modułową z czterema specjalistycznymi podusługami koordynowanymi przez lekkiego orkiestratora:

- **BackendTranslationService** — Orchestrates the entire pipeline, handles server validation, and delegates work to sub-services.
- **CountriesTranslationService** — Synchronizes country names from `countries.json` into per-language dictionaries.
- **LocalizationTranslationService** — Detects added/removed keys in the default JSON dictionary and translates them into target languages.
- **DocumentsTranslationService** — Translates Markdown documentation files with per-block tracking and metadata.

Każda podusługa działa niezależnie i zgłasza postępy za pośrednictwem SignalR w czasie rzeczywistym.

## Co robi usługa

Usługa działa na harmonogramie i wykonuje pięcioetapowy rurociąg: walidacja serwera, synchronizacja kraju, synchronizacja słownika JSON, tłumaczenie plików Markdown, i utrzymuje wyniki. Każdy etap emituje ustrukturyzowane zdarzenia w czasie rzeczywistym nad SignalR, tak aby podłączeni klienci mogli śledzić wraz z pracą.

## Etapy

### Etap 1 - Serwery kontrolne

Przed rozpoczęciem prac tłumaczeniowych usługa sprawdza, czy wszystkie warunki wstępne są spełnione:

- Sekcja konfiguracji musi być obecna i poprawna.
- Serwer LibreTranslate musi reagować w akceptowalnym terminie.
- Lista języków dostępnych na serwerze tłumaczeń jest pobierana.
- Konfigurowany język domyślny musi być obecny na tej liście.
- Brakujące pliki locale JSON dla dowolnego obsługiwanego języka są tworzone automatycznie.

Jeżeli jakiekolwiek sprawdzenie nie powiodło się, rurociąg zatrzymuje się natychmiast i wiadomość jest emitowana.

### Etap 2 - TranslateCountries

Nazwy krajów są przechowywane w synchronizacji z katalogu tylko read- () do słowników lokalizacji JSON.

- Jeśli domyślny język aplikacji to angielski, każda nazwa kraju jest zapisywana bez tłumaczenia.
- Jeśli domyślny język jest innym językiem, nazwa kraju angielskiego jest najpierw przetłumaczona na ten język, a wynik staje się wpisem w domyślnym słowniku.
- After the default dictionary is updated, each missing country entry in every target language dictionary is translated and saved **immediately per language**.
- Przetłumaczone wpisy są zachowywane bez modyfikacji.
- Jeśli tłumaczenie się nie powiedzie, usługa powtarza się do 3 razy z 30-sekundowymi opóźnieniami przed przejściem do następnego języka.

### Etap 3 - Pliki TranslateJsonFiles

Usługa porównuje bieżący domyślny słownik lokalizacji z migawką zapisaną w poprzednim uruchomieniu:

- **Added keys** — entries present in the current default but absent from the snapshot — are translated into every target language that does not already have a manual entry for that key.
- **Removed keys** — entries present in the snapshot but absent from the current default — are deleted from every target language dictionary.
- Tłumaczenia ręczne zawsze mają pierwszeństwo. Jeśli słownik docelowy zawiera już wartość klucza, ten wpis pozostaje niezmieniony niezależnie od tego, co mówi źródło.
- **Each target language dictionary is saved immediately after its translations complete**, rather than waiting for all languages to finish.
- Jeśli tłumaczenie nie jest możliwe dla określonego języka, usługa ponownie próbuje działać automatycznie. Tylko trwałe błędy (np. nieobsługiwany język) powodują pominięcie tego języka.
- Po uruchomieniu bieżący domyślny słownik jest zapisany jako nowy migawka dla następnego porównania.

Wszystkie słowniki są zawsze przechowywane z alfabetycznie posortowane klucze i wcięty JSON do czytelności człowieka.

### Etap 4 - TranslateMarkdownFiles

Usługa prowadzi skonfigurowane korzenie dokumentacji (domyślnie:) i przetwarza rekursywnie każdy plik źródłowy:

1. Zawartość pliku źródłowego jest odczytywana i oblicza się hash SHA- 256.
2. A `.translation-meta.json` file next to the source tracks per-language, per-block translation status, enabling **incremental re-translation** of only failed blocks.
3. Zapisany hash z poprzedniego uruchomienia (przechowywany w pliku obok pliku źródłowego lub w tymczasowej lokalizacji awaryjnej) jest porównywany z bieżącym haszem.
4. Dla każdego języka docelowego, odpowiedni plik jest również sprawdzany pod względem integralności strukturalnej.
5. Każdy brakujący plik docelowy, ma przestarzały hasz, nie sprawdza struktury lub zawiera nieprzetłumaczone bloki jest w kolejce do ponownego tłumaczenia.
6. **Each target language is translated and saved independently** — if Czech succeeds but French fails, the Czech file is still written to disk.
7. Udane przetłumaczone pliki są walidowane pod kątem strukturalnej parytetu ze źródłem (równe liczby nagłówków, pozycje listy, bloki kodowe, blokady, linki, pogrubione / italic markery i znaczniki HTML) zanim zostaną zapisane na dysku.
8. Jeśli wszystkie pliki docelowe dla źródła odniosą sukces, nowy hash jest przechowywany obok źródła. Jeśli pisanie obok źródła nie powiodło się (na przykład w wersjach tylko read-), hasz wraca do katalogu tymczasowego.
9. Jeśli jakiekolwiek tłumaczenie docelowe nie powiedzie się, metadane oznaczają te bloki jako nieprzetłumaczone, więc są one ponownie testowane w następnym biegu.

### Etap 5 - StoringResults

Skonsolidowane jest i publikowane. Obejmuje ono:

- UTC uruchom znaczniki czasu rozpoczęcia i zakończenia.
- Liczy zapisywane pliki locale JSON, zapisywane pliki Markdown, zapisywane pliki hash i hash fallback pisze.
- Wszelkie błędy w przechowywaniu zebrane podczas biegu.
- Przetłumaczone statystyki tłumaczeniowe (przetłumaczone na liczenie, pominięte liczenie, błąd liczenie).

## Koperta wiadomości sygnalizacyjnej

Każde zdarzenie postępu jest realizowane jako a z następującymi polami:

Pole
|-------|------|-------------|
Identyfikator korelacji dla bieżącego przebiegu rurociągu
Licznik monotoniczny w trakcie biegu, zaczynając od 1
Semantyczny typ wiadomości
Etap pipeline wiadomość należy do
Czas UTC, kiedy wiadomość została wyemitowana
Czy wiadomość reprezentuje stan błędu
Podsumowanie do odczytu ludzkiego
Specyficzny dla stanu ładunek (obiekt zgłoszenia lub null)

### Typy wiadomości

Wartość
|-------|------|---------|
0
1
2
3
4
5
6

### Etapy

Wartość
|-------|------|-------------|
0
1
2
3
4
5

### Typowy przepływ wiadomości

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

Jeśli jakikolwiek etap zawiedzie, pozostałe etapy są pomijane, wiadomość jest emitowana i ostatecznie wiadomość zamyka bieg.

## Tłumaczenie logiki powtórzenia

Gazociąg wdraża dwa poziomy odporności:

### Wznowienie stanu (TranslationRetriService)

- Jeśli prośba o tłumaczenie nie powiodła się po wewnętrznych powtórzeniach LibreTranslate, wykonuje do 3 dodatkowych powtórzeń poziomu sceny z 30-sekundowymi opóźnieniami.
- Maskowanie placeholder: Nazwy placeholders () w tekście są tymczasowo zamieniane na bezpieczne żetony () przed tłumaczeniem i przywrócone później, zapewniając prawidłową gramatykę w językach docelowych.

### Walidacja języka

- Przed tłumaczeniem na język docelowy usługa weryfikuje język obsługiwany przez serwer tłumaczeń.
- Nieobsługiwane języki są pomijane z ostrzeżeniem, zapobiegając powtarzającym się nieudanym próbom.

### Przywrócenie poziomu blokady markdown

- Tłumaczenia markdown są wykonywane block- by- block (nagłówki, akapity, pozycje listy).
- Jeśli pojedynczy blok nie przetłumaczy tłumaczenia, zostanie on oznaczony jako nieprzetłumaczony w pliku metadanych i ponownie wypróbowany podczas następnego przebiegu rurociągu.
- Serwis śledzi per- language, status per- block w plikach obok każdego pliku źródłowego Markdown.

## Kody błędów

Błędy są zgłaszane za pomocą jednolitego enum pogrupowanego w zakresy:

Zakres
|-------|----------|
1000- 1999
2000- 2999
3000- 3999
4000- 4999
5000- 5999

Każdy błąd w raporcie zawiera identyfikator źródłowy (kod językowy, ścieżkę pliku lub nazwę sceny), kod błędu i wiadomość do odczytu przez człowieka.

## dashboard tłumaczenie na żywo

Projekt Server zawiera stronę administracyjną, która łączy się z węzłem SignalR i wyświetla wszystkie zdarzenia związane z rurociągiem w czasie rzeczywistym.

- Wyświetla status połączenia, ilość wiadomości oraz tabelę aktualizacji życia wszystkich zdarzeń.
- Kolorowe wiersze: niebieski na początek etapu, zielony na zakończenie, czerwony na błędy.
- Obsługuje czyszczenie kanału i eksport wszystkich wiadomości do JSON.
- Auto- reconnects with wykładniczy backoff if the connection drops.

## Zasady projektowania

- **Modularity**: Each translation concern is isolated in its own service for maintainability and testability.
- **Incremental persistence**: Dictionaries and Markdown files are saved per-language immediately after translation, reducing memory pressure and providing earlier feedback.
- **Resilience**: Multiple retry levels (HTTP, stage, block) ensure transient failures do not block the pipeline.
- **State tracking**: Per-file metadata (`.translation-meta.json`) and hash files enable precise incremental work on subsequent runs.
- **Real-time visibility**: Every significant operation is reported via SignalR for monitoring and debugging.
- **Manual translations always have priority over automatic additions.**
