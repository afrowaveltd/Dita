# Real-time oversettelser

Dette dokumentet eksisterer som en direkte testinngang for den automatiske oversettelsesrørledningen.

## Hva tjenesten gjør

Tjenesten kjører på en tidsplan og validerer oversettelsesserveren, konfigurasjonen og tilgjengelige språk før oversettelsesarbeidet starter.

Etter valideringstrinnet synkroniserer det landnavn fra den lesebeskyttede land-katalogen til standard lokalisering JSON-ordbøker. Hvis standardspråket i programmet er engelsk, lagres landoppføringen som nøkkel lik verdi. Hvis standardspråket er annerledes, blir det engelske landnavnet først oversatt til standardspråket, og bare lagret som nøkkel lik verdi i standardordboka.

Deretter sammenligner tjenesten gjeldende standard lokaliseringsordbok med det lagrede øyeblikksbilde fra forrige løp. Nytilsatte oppføringer oversettes kun til målspråk når nøkkelen ikke allerede eksisterer, så manuelle oversettelser har prioritet. Fjernede oppføringer slettes fra alle målordbøker for å holde hele settet konsekvent.

Til slutt skanner tjenesten konfigurerte dokumentasjons røtter for Markdown trær. Hver emnemappe forventes å inneholde en kildefil oppkalt etter standardspråket, for eksempel en.md. Tjenesten hasherer at kildefilen, registrerer endringer, oversetter manglende eller utdaterte målrettsfiler, og lagrer gjeldende hash ved siden av kildefilen. Hvis det ikke er mulig å skrive hash ved siden av kildefilen, faller den tilbake til midlertidig lagring.

## Hvordan tjenesten rapporterer fremgang

Motoren sender ut generelle SignalR-meldinger gjennom lokaliseringshubben ved hjelp av én meldings konvolutt. Hver melding har en meldingstype, den aktuelle prosessstadiet, en UTC tidsstempel, en tekstsammendrag og valgfri fasespesifikk nyttelast.

De nåværende trinnene er:

- CheckServers
- Oversett land
- OversetterJsonFiles
- Oversett MarkdownFiles
- lagringsresultater

Typisk meldingsstrøm er fase startet, fase fullført og rørledningen fullført. Hvis et trinn mislykkes, er meldingen merket som en feil og inneholder strukturert feilinformasjon med felles feilkoder.

## Designprinsippene

Oversettelser behandles sekvensielt for å unngå overbelastning av LibreTranslate-serveren.

Lokalisering JSON ordbøker er alltid lagret med alfabetiske sorterte nøkler og formatert JSON for lettere vedlikehold.

Tidligere standardeksemplar på ordbok lagres vedvarende, slik at en omstart av programmet ikke mister endringssporing.

**Manual oversettelser har alltid prioritet over automatiske tillegg.**
