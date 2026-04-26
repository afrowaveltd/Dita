# Tłumaczenie w czasie rzeczywistym

Dokument ten istnieje jako wejście testowe na żywo dla automatycznego rurociągu tłumaczeniowego.

## Co robi usługa

Usługa działa zgodnie z harmonogramem i zatwierdza serwer tłumaczeń, konfigurację i dostępne języki przed rozpoczęciem prac tłumaczeniowych.

Po etapie walidacji synchronizuje nazwy krajów z katalogu tylko read- krajów do standardowych słowników lokalizacyjnych JSON. Jeśli domyślny język aplikacji to angielski, wpis kraju jest zapisany jako klucz równa się wartości. Jeśli język domyślny jest inny, nazwa kraju angielskiego jest najpierw tłumaczona na język domyślny, a dopiero potem zapisywana jako klucz równa się wartość w słowniku domyślnym.

Następnie usługa porównuje bieżący słownik lokalizacji z zapisanym migawką z poprzedniego uruchomienia. Nowo dodane wpisy są tłumaczone na języki docelowe tylko wtedy, gdy klucz jeszcze nie istnieje, więc tłumaczenia ręczne zachowują priorytet. Usunięte wpisy są usuwane ze wszystkich słowników docelowych, aby cały zestaw był spójny.

Wreszcie, usługa skanuje skonfigurowane korzenie dokumentacji dla drzew Markdown. Każdy folder tematyczny powinien zawierać plik źródłowy nazwany po domyślnym języku, np. en.md. Usługa hashuje ten plik źródłowy, wykrywa zmiany, tłumaczy brakujące lub przestarzałe pliki docelowe Markdown i przechowuje bieżący hash obok pliku źródłowego. Jeśli napisanie haszu obok pliku źródłowego nie jest możliwe, wraca do tymczasowego przechowywania.

## Jak usługa przedstawia postępy

Backend emituje ogólne komunikaty SignalR poprzez węzeł lokalizacji za pomocą jednej koperty wiadomości. Każda wiadomość zawiera typ wiadomości, obecny etap procesu, znacznik czasu UTC, podsumowanie tekstu i opcjonalne obciążenie dyspozycyjne specyficzne dla etapu.

Obecne etapy to:

- Serwery kontrolne
- TranslateCountries
- TranslateJsonFiles
- TranslateMarkdownfiles
- wyniki

Typowy przepływ wiadomości rozpoczyna się etap, etap zakończony i rurociąg zakończony. Jeśli etap zawiedzie, komunikat jest oznaczony jako błąd i zawiera ustrukturyzowane informacje o błędzie z ujednoliconymi kodami błędów.

## Zasady projektowania

Tłumaczenia są przetwarzane kolejno, aby uniknąć przeciążenia serwera LibreTranslate.

Lokalizacja słowników JSON są zawsze przechowywane z alfabetycznie posortowane klucze i sformatowane JSON dla łatwiejszej konserwacji.

Poprzedni domyślny migawka słownika jest przechowywany w sposób ciągły, więc ponowne uruchomienie aplikacji nie traci śledzenia zmian.

*** Ręczne tłumaczenia zawsze mają pierwszeństwo nad automatycznymi dodatkami. ***
