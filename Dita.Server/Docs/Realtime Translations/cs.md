# Překlady v reálném čase

Tento dokument existuje jako živý zkušební vstup pro automatický překlad potrubí.

## Co služba dělá

Služba běží podle plánu a validuje překlad serveru, konfigurace, a dostupné jazyky před zahájením jakékoli práce překladu.

Po validačním kroku synchronizuje názvy zemí z katalogu pouze read- only zemí do standardních slovníků lokalizace JSON. Pokud je výchozí jazyk aplikace anglický, položka země je uložena jako klíč rovná se hodnota. Pokud je výchozí jazyk odlišný, název anglické země je nejprve přeložen do výchozího jazyka, a teprve pak uložen jako klíč rovná se hodnota ve výchozím slovníku.

Dále, služba porovnává aktuální výchozí lokalizační slovník s uloženým snímek z předchozího běhu. Nově přidané položky jsou přeloženy do cílových jazyků pouze tehdy, pokud klíč již neexistuje, takže ruční překlady mají přednost. Odstraněné položky se vymažou ze všech cílových slovníků, aby byla celá sada konzistentní.

Konečně, služba skenuje nakonfigurované kořeny dokumentace pro stromy Markdown. Každé téma složky se očekává, že obsahuje zdrojový soubor pojmenovaný po výchozím jazyce, jako je cs.md. Služba hashes, že zdrojový soubor, detekuje změny, překládá chybějící nebo zastaralé cílové Markdown soubory, a ukládá aktuální hash vedle zdrojového souboru. Není-li možné zapsat hašiš vedle zdrojového souboru, vrátí se zpět do dočasného úložiště.

## Jak služba podává zprávy o pokroku

Backend vysílá obecné zprávy SignalR přes lokalizační uzel pomocí jedné zprávy obálky. Každá zpráva obsahuje typ zprávy, aktuální fázi procesu, časové razítko UTC, souhrn textu a volitelné stage- specifické užitečné zatížení.

Aktuální fáze jsou:

- Kontrolní servery
- Překladové země
- Přeložit soubory JsonName
- Přeložit MarkdownFiles
- úspěchy

Typický tok zpráv je fáze zahájena, fáze dokončena, a potrubí dokončeno. Pokud fáze selže, zpráva je označena jako chyba a obsahuje strukturované informace o chybách s jednotnými chybovými kódy.

## Zásady návrhu

Překlady jsou zpracovávány postupně, aby se zabránilo přetížení serveru LibreTranslate.

Lokalizace JSON slovníky jsou vždy uloženy s abecedně seřazené klíče a formátované JSON pro snadnější údržbu.

Předchozí výchozí slovníkový snímek je trvale uložen, takže restart aplikace neztratí sledování změn.

*** Ruční překlady mají vždy přednost před automatickými doplňky. ***
