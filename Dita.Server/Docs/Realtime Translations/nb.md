# Real-time oversettelser

Dette dokumentet eksisterer som en direkte testinngang for den automatiske oversettelsesrørledningen. Enhver endring i denne filen utløser re-translation av alle målspråkfiler på neste planlagte kjøre.

## Oversikt over arkitektur

Oversettelsesrørledningen har blitt omstrukturert til en modulær arkitektur med fire spesialiserte undertjenester koordinert av en lett orkesterfører:

- **BackendTranslationService** — Orkesterer hele rørledningen, håndterer servervalidering og delegater jobber til undertjenester.
- **CountrysTranslationService** — Synkroniserer landnavn fra i per-språklige ordbøker.
- **LocalizationTranslationService** — Oppdager lagt til / fjernet nøklar i standard JSON-ordbok og oversetter dem til målspråk.
- **DokumentsTranslationService** — Oversetter Markdown-dokumentasjonsfiler med sporing og metadata per blokk.

Hver undertjeneste opererer uavhengig og rapporterer fremgang via SignalR i sanntid.

## Hva tjenesten gjør

Tjenesten kjører på en tidsplan og utfører en fem-trinns rørledning: server validering, land synkronisering, JSON ordbok synkronisering, Markdown fil oversettelse og vedvarer resultatene. Hvert trinn avgir strukturerte real-time fremgangsarrangementer over SignalR slik at tilkoblede klienter kan følge med etter hvert som arbeidet fortsetter.

## Pipeline-faser

### Trinn 1 — SjekkServer

Før oversettelsesarbeidet begynner, bekrefter tjenesten at alle forutsetninger er oppfylt:

- Konfigurasjonsdelen må være til stede og gyldig.
- LibreTranslate-serveren må svare innenfor en akseptabel latens.
- Listen over tilgjengelige språk på oversettelsesserveren hentes.
- Det konfigurerte standardspråket må være til stede i den listen.
- Manglende lokale JSON-filer for hvilket som helst språk som støttes, opprettes automatisk.

Hvis sjekken mislykkes, stopper rørledningen umiddelbart og en melding sendes ut.

### Fase 2 — Oversett countries

Landnavnene holdes synkronisert fra en skrivebeskyttet katalog () i localization JSON ordbøker.

- Hvis standardspråket på programmet er engelsk, lagres hvert landnavn som uten oversettelse.
- Hvis standardspråket er noe annet språk, blir det engelske landnavnet først oversatt til det språket, og resultatet blir oppføringen i standardordboka.
- Etter at standardordboka er oppdatert, blir hver manglende landoppføring i hver målspråksordbok oversatt og lagret **immediately per språk**.
- Allerede omsatte oppføringer er bevart uten modifikasjon.
- Hvis en oversettelse mislykkes, går tjenesten opp til 3 ganger med 30 sekunders forsinkelser før du flytter til neste språk.

### Stage 3 — OversetterJsonFiles

Tjenesten sammenligner gjeldende standardordbok for lokalisering med et øyeblikksbilde lagret fra forrige løp:

- ** Leggte nøkler** — oppføringer som er tilstede i gjeldende standard men fraværende fra øyeblikksbildet — er oversatt til alle målspråk som ikke allerede har en manuell oppføring for den nøkkelen.
- **Removed keys** — oppføringer som er tilstede i øyeblikksbilde, men fraværende fra gjeldende standard — slettes fra hver målspråkbok.
- Manuelle oversettelser tar alltid prioritet. Hvis en målordbok allerede inneholder en verdi for en nøkkel, er den oppføringen igjen uendret uansett hva kilden sier.
- **Hver målspråkleksikon lagres umiddelbart etter at oversettelsene er fullførte**, i stedet for å vente på at alle språk skal fullføres.
- Hvis en oversettelse mislykkes for et bestemt språk, returnerer tjenesten automatisk. Bare vedvarende feil (f.eks. språk som ikke støttes) fører til at språket hoppes over.
- Etter kjøringen lagres gjeldende standardordbok som det nye øyeblikksbildet for neste sammenligning.

Alle ordbøker er alltid lagret med alfabetiske sorterte nøkler og innrykket JSON for menneskelig leselighet.

### Trinn 4 — OversettMarkdownFiles

Tjenesten går de konfigurerte dokumentasjonsrøtene (standard: ) og behandler hver kildefil rekursivt:

1. Kildefilinnholdet leses og en SHA-256 hash beregnes.
2. En fil ved siden av kildesporene per språk, per blokk oversettelsesstatus, som muliggjør ** inkrementell re-translasjon** av bare mislykkede blokker.
3. Den lagrede hash fra forrige kjøring (kept i en fil ved siden av kildefilen, eller i en midlertidig reserveplassering) sammenlignes med gjeldende hash.
4. For hvert målspråk kontrolleres den tilsvarende filen også for strukturell integritet.
5. Enhver målfil som mangler, har en utdatert hash, mislykkes strukturvalidering, eller inneholder ikke-oversatte blokker er i kø for omtranslasjon.
6. **Hvert målspråk blir oversatt og lagret uavhengig** - hvis tsjekkisk lykkes men fransk mislykkes, er den tsjekkiske filen fortsatt skrevet til disk.
7. Vellykket oversatte filer er validert for strukturell paritet med kilden (lik overskrift teller, liste elementer, kodeblokker, blockquotes, koblinger, dristige/titalske markører og HTML tags) før de er skrevet til disk.
8. Hvis alle målfiler for en kilde lykkes, lagres den nye hash ved siden av kilden. Hvis skriving ved siden av kilden mislykkes (for eksempel i skrivebeskyttede utdelinger), faller hash tilbake til den midlertidige katalogen.
9. Hvis noen måloversettelse mislykkes validering, markerer metadata disse blokkene som uomsettes slik at de blir retridert på neste løp.

### Trinn 5 — Storingsresultater

En konsolidert samles og publiseres. Det inkluderer:

- UTC starter og fullfører tidsstempler.
- Teller av lagrede lokale JSON-filer, lagret Markdown-filer, lagret hash-filer, og hash i reserve.
- Eventuelle lagringsfeil samlet under kjøringen.
- Per-språklig oversettelsesstatistikk (oversatt antall, hoppet over antall, feiltelling).

## SignalR melding konvolutt

Hver fremgangsbegivenhet leveres som en med følgende felt:

Felt
|-------|------|-------------|
Korrelasjonsidentifikator for gjeldende rørledningskjøring
Monoton teller i en løp, starter ved 1
Semantisk type melding
Pipeline fase meldingen tilhører
UTC-tid når meldingen ble sendt ut
Om meldingen representerer en feiltilstand
Menneskeleselig sammendrag
Stagespesifikk nyttelast (rapportere objekt eller null)

### Meldingstyper

Verdi
|-------|------|---------|
0
1
2
3
4
5
6

### Pipeline-faser

Verdi
|-------|------|-------------|
0
1
2
3
4
5

### Typisk meldingsstrøm

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

Hvis et trinn mislykkes, blir de gjenværende trinnene hoppet over, en melding sendes ut, og til slutt stenger en melding.

## Oversettelse forsøk logikk

Rørledningen implementerer to nivåer av motstand:

### Stagenivå reprøv (translationRetryService)

- Hvis en oversettelsesforespørsel mislykkes etter LibreTranslates interne retries, utføres opptil 3 ytterligere trinnnivårettinger med 30 sekunders forsinkelser.
- Stedholder maskering: Navngitt plassholdere () i teksten erstattes midlertidig med trygge polletter () før oversettelse og restaurert etterpå, noe som sikrer korrekt grammatikk på målspråk.

### Språkvalidering

- Før oversettelse til et målspråk, verifiserer tjenesten språket støttes av oversettelsesserveren.
- Ustøtte språk hoppes over med en advarsel, og hindrer gjentatte mislykkede forsøk.

### Merk ned blokknivå reprøv

- Merkeoversettelser utføres blokk-for-blokk (punkter, avsnitt, listeelementer).
- Hvis en enkelt blokk mislykkes oversettelse, er den merket som uoversatt i metadatafilen og retridert på neste rørledningskjøring.
- Tjenestesporene per språk, per-blokk-status i filer ved siden av hver kilde Markdown-fil.

## Feilkoder

Feil er rapportert ved hjelp av en samlet enhet gruppert i områder:

Område
|-------|----------|
1000-1999
2000–2999
3000–3999
4000-4999
5000–5999

Hver feil i en rapport bærer kildeidentifikatoren (språklig kode, filsti eller fasenavn), feilkoden og en menneskelesbar melding.

## Live Oversettelse Dashboard

Serverprosjektet inneholder en admin-side som kobler til SignalR-hubben på og viser alle rørledningshendelser i sanntid.

- Viser tilkoblingsstatus, meldingstelling og en levende tabell over alle hendelser.
- Fargekodede rader: blå for fasestart, grønn for ferdigstillelse, rød for feil.
- Støtter å rydde fôret og eksportere alle meldinger til JSON.
- Auto-tilkobling med eksponentiell backoff hvis forbindelsen faller.

## Designprinsippene

- **Modualitet**: Hver oversettelsesbekymring er isolert i sin egen tjeneste for vedlikehold og testbarhet.
- **Inkrementell utholdenhet**: Dictionarys og Markdown-filer lagres per språk umiddelbart etter oversettelse, reduserer minnetrykket og gir tidligere tilbakemeldinger.
- **Resiliens**: Flere reprøvenivåer (HTTP, trinn, blokk) sikrer forbigående svikt blokkerer ikke rørledningen.
- **State tracking**: Per-fil metadata () og hash-filer muliggjør nøyaktig trinnt arbeid ved påfølgende løp.
- **Real-tid synlighet**: Hver betydelig operasjon er rapportert via SignalR for overvåking og feilsøking.
- ** Manual oversettelser har alltid prioritet over automatiske tillegg.**
