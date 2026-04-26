# Översättningar i realtid

Detta dokument finns som en levande testingång för den automatiska översättningsledningen.

## Vad tjänsten gör

Tjänsten körs på ett schema och validerar översättningsservern, konfiguration och tillgängliga språk innan något översättningsarbete startar.

Efter valideringssteget synkroniserar det landsnamn från de lättlästa ländernas katalog till standardlokaliseringen JSON-ordböckerna. Om applikationsstandardspråket är engelska lagras landets inträde som nyckelvärden. Om standardspråket är annorlunda översätts det engelska landsnamnet först till standardspråket, och lagras först som nyckelvärden i standardordboken.

Därefter jämför tjänsten den nuvarande standardlokaliseringsordboken med den lagrade ögonblicksbilden från föregående körning. Nyligen översatta poster översätts till målspråk endast när nyckeln inte redan finns, så manuella översättningar håller prioritet. Borttagna poster raderas från alla målordböcker för att hålla hela uppsättningen konsekvent.

Slutligen skannar tjänsten konfigurerade dokumentationsrötter för Markdown träd. Varje ämne mapp förväntas innehålla en källfil namngiven efter standardspråket, såsom en.md. Tjänsten hash som källfil, upptäcker ändringar, översätter saknade eller föråldrade mål Markdown filer, och lagrar den aktuella hash bredvid källfilen. Om du skriver hash bredvid källfilen är det inte möjligt, faller den tillbaka till tillfällig lagring.

## Hur tjänsten rapporterar framsteg

Backend avger allmänna SignalR-meddelanden genom lokaliseringsnavet med hjälp av ett meddelandekuvert. Varje meddelande bär en meddelandetyp, det aktuella processstadiet, en UTC-tidsstämpel, en textsammanfattning och valfri scenspecifik nyttolast.

De nuvarande stadierna är:

- CheckServers
- Översättning av länder
- ÖversättningJsonFiles
- TranslateMarkdownFiles
- StoringResults

Typiskt meddelandeflöde startas, steg färdigt och pipeline slutförs. Om ett steg misslyckas markeras meddelandet som ett fel och innehåller strukturerad felinformation med enhetliga felkoder.

## Designprinciper

Översättningar behandlas sekventiellt för att undvika överbelastning av LibreTranslate-servern.

Lokalisering JSON ordböcker lagras alltid med alfabetiskt sorterade nycklar och formaterade JSON för enklare underhåll.

Den tidigare standard ordbok ögonblicksbild lagras ihållande så en omstart av programmet inte förlorar förändring spårning.

**Manliga översättningar har alltid prioritet framför automatiska tillägg.**
