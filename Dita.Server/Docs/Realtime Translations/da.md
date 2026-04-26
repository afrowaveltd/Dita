# Real- time oversættelser

Dette dokument findes som et live testinput for den automatiske oversættelsesledning.

## Hvad tjenesten gør

Tjenesten kører på en tidsplan og validerer oversættelsesserveren, konfiguration og tilgængelige sprog, før nogen oversættelse arbejde starter.

Efter validering trin, det synkroniserer landenavne fra read- kun lande katalog i standard lokalisering JSON ordbøger. Hvis applikationens standardsprog er engelsk, gemmes landeposten som nøgle lig med værdien. Hvis standardsproget er anderledes, oversættes det engelske landenavn først til standardsproget, og lagres først som nøgle, der svarer til værdien i standardordbogen.

Dernæst sammenligner tjenesten den aktuelle standard lokalisering ordbog med den gemte snapshot fra det foregående løb. Nyligt tilføjede indgange er kun oversat til målsprog, når nøglen ikke allerede findes, så manuelle oversættelser holde prioritet. Fjernede indgange slettes fra alle målordbøger for at holde hele sættet konsekvent.

Endelig scanner tjenesten konfigurerede dokumentationsrødder for Markdown træer. Hvert emne mappe forventes at indeholde en kildefil navngivet efter standardsproget, såsom en.md. Tjenesten hashes, at kildefilen, registrerer ændringer, oversætter manglende eller forældede mål Markdown filer, og gemmer den aktuelle hash ved siden af kildefilen. Hvis det ikke er muligt at skrive hash ved siden af kildefilen, falder det tilbage til midlertidig opbevaring.

## Hvordan tjenesten rapporterer fremskridt

Bagenden udsender generelle signalR-meddelelser gennem lokaliseringshubben ved hjælp af en meddelelseskonvolut. Hver meddelelse indeholder en meddelelsestype, det aktuelle processtadium, et UTC-tidsstempel, et tekstresumé og valgfri trinspecifik nyttelast.

De nuværende faser er:

- checkservere
- Oversøiske lande
- TranslateJsonFiles
- TranslateMarktow- filer
- StoringResultater

Typiske meddelelse flow er fase startet, fase afsluttet, og rørledning afsluttet. Hvis en fase mislykkes, er meddelelsen markeret som en fejl og omfatter struktureret fejlinformation med ensartede fejlkoder.

## Konstruktionsprincipper

Oversættelser behandles sekventielt for at undgå overbelastning af LibreTranslate- serveren.

Lokalisering JSON ordbøger er altid gemt med alfabetisk sorterede nøgler og formateret JSON for lettere vedligeholdelse.

Den tidligere standard ordbog snapshot gemmes vedholdende, så en genstart af programmet ikke mister ændring sporing.

*** Manuelle oversættelser har altid forrang frem for automatiske tilføjelser. ***
