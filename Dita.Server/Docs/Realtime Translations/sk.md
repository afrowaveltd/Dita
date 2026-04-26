# Preklady v reálnom čase

Tento dokument existuje ako živý skúšobný vstup pre automatické prekladové potrubie.

## Čo robí služba

Služba beží na rozvrhu a validuje prekladateľský server, konfiguráciu a dostupné jazyky pred začatím prekladateľskej práce.

Po validačnom kroku synchronizuje názvy krajín z katalógu krajín len na čítanie do štandardných slovníkov JSON. Ak je predvolený jazyk aplikácie angličtina, vstup krajiny je uložený ako kľúč sa rovná hodnote. Ak je predvolený jazyk odlišný, anglický názov krajiny je najprv preložený do predvoleného jazyka a iba potom uložený ako kľúč sa rovná hodnote v predvolenom slovníku.

Služba ďalej porovnáva aktuálny predvolený lokalizačný slovník s uloženým snímkom z predchádzajúceho spustenia. Novo pridané položky sú preložené do cieľových jazykov len vtedy, keď kľúč už neexistuje, takže ručné preklady majú prednosť. Odstránené položky sa odstránia zo všetkých cieľových slovníkov, aby celá sada bola konzistentná.

Napokon, servisné skeny nakonfigurované korene dokumentácie pre stromy Markdown. Očakáva sa, že každá tematická zložka bude obsahovať zdrojový súbor pomenovaný po predvolenom jazyku, ako napríklad end.md. Služba hašuje, že zdrojový súbor, detekuje zmeny, prekladá chýbajúce alebo zastarané cieľové Markdown súbory, a ukladá aktuálny hašiš vedľa zdrojového súboru. Ak písanie hash vedľa zdrojového súboru nie je možné, spadne späť do dočasného úložiska.

## Ako služba hlási pokrok

Backend vysiela všeobecné správy SignalR cez lokalizačné centrum pomocou jednej obálky správ. Každá správa obsahuje typ správy, aktuálnu fázu procesu, časovú pečiatku UTC, textové zhrnutie a voliteľné užitočné zaťaženie špecifické pre fázu.

Aktuálne etapy sú:

- checkervers
- krajiny prekladu
- PreložiťJsonFiles
- Preložiť MarkdownFiles
- Uchovávanie výsledkov

Typický tok správ je fáza spustená, etapa dokončená, a potrubia dokončené. Ak fáza zlyhá, správa je označená ako chyba a obsahuje štruktúrované informácie o chybe s jednotnými chybovými kódmi.

## Princípy návrhu

Preklady sa spracúvajú postupne, aby sa zabránilo preťaženiu servera LibreTranslate.

Lokalizácia slovníkov JSON sú vždy uložené abecedne zoradené kľúče a formátované JSON pre jednoduchšiu údržbu.

Predchádzajúci predvolený slovník snímky je uložený trvalo, takže reštart aplikácie nestratí sledovanie zmien.

** Manuálne preklady majú vždy prednosť pred automatickými doplneniami. **
