# Sammendrag av endringer i den automatiske oversettelsestjenesten

## Oversikt

Dette dokumentet oppsummerer alle endringer som gjøres i Dita automatisk oversettelsestjeneste, inkludert arkitektur refaktoring, nye funksjoner, observerbarhetsforbedringer og lokaliseringsforbedringer.

## Arkitekturendringer

### Refactored MotorTranslationService

Den monolitiske har blitt demontert i fire spesialiserte tjenester koordinert av en lett orkesterator:

- **BackendTranslationService** — Pipeline orkestertor (servervalidering, fase delegasjon, feilhåndtering)
- **CountrysTranslationService** — Landsnavnssynkronisering (engelsk → målspråk)
- **LocalizationTranslationService** — JSON ordbok synkronisering (tilsatt/fjernede nøkler)
- **DokumenterTranslationService** — Merkedokumentasjon oversettelse med blokknivå sporing
- **SignalRPublisher** — Rapportering i sanntid via SignalR
- **TranslationRetryService** — Stagenivå retry med plassholderbevaring

### Fordeler

- **Bekymring av bekymringer**: Hver tjeneste håndterer et enkelt oversettelsesdomene
- **Holdbarhet**: Mindre klasser er enklere å forstå og teste
- ** Omfattbarhet**: Nye oversettelsesmål kan legges til via grensesnitt implementering
- ** Pålitelighet**: Uavhengige tjenester gir bedre feilisolasjon

## Nye funksjoner

### Live Oversettelsesskjerm

** Plassering**:

En ny administratorside som gir sanntid synlighet i oversettelsesrørledningen:

- Viser alle SignalR hendelser som de oppstår
- Fargekodede meldingstyper (blå=startet, grønn=fullført, red=error)
- Tilkoblingsstatusbanner med auto-tilkobling
- Meldingsbehandler og eksport til JSON

### Navngitte plassholdere

Lokaliseringssystemet støtter nå navngitte plassholdere () for forbedret grammatikk på ulike språk:

```csharp
// Usage in code
var message = Localizer["WelcomeMessage", new Dictionary<string, string>
{
    ["userName"] = "John",
    ["count"] = "5"
}];
// Result: "Hello John, you have 5 new messages"
```

Funksjoner:
- Stedholderverdier som gis ved kjøring eller lagres i
- Automatisk maskering/restaurering under oversettelse for å hindre korrupsjon
- Tilbakekompatibel med eksisterende posisjonsholdere

### Økende oversettelse

Merke ned filer oversettes gradvis:

- **Per-språklig lagring**: Hvert målspråk lagres umiddelbart etter oversettelse og reduserer minnetrykket
- **Block-level sporing**: spor oversettelsesstatus per blokk
- **Selektiv gjenforsøk**: Bare feilblokker omsettes på nytt i neste løp
- **Metadataholdighet**: Oversettelsestilstand overlever programmet starter på nytt

### Forbedret reprøv Logic

Tre nivåer av motstandsdyktighet:

1. **HTTP reprøv** (LibreTranslateService): 5 forsøk med eksponentiell backoff (1s–5s)
2. **Stage retry** (TranslationRetryService): 3 ytterligere forsøk med 30s forsinkelser
3. **Block retry** (DokumentsTranslationService): Mislykkes Markdown-blokker på nytt ved neste løp

### SignalR rapportering

Rapportering i sanntid for alle rørledningsoperasjoner:

- Hvert trinn publiserer hendelser
- Perspråklige fremskritt som hendelser
- Feilhendelser inkluderer detaljert kontekst (kilde, feilkode, melding)
- Sekvensnummer garanterer bestilling i hvert løp

## Konfigurasjonsendringer

### appsettings.json

Ingen endringer. Eksisterende konfigurasjon fortsetter å fungere:

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

### Nye tjenester

Registrert i:

- /
- `TranslationRetryService`
- /
- /
- /
- /

SignalR hub er kartlagt på klientforbindelser.

## Testing

### Teststatus

- **243/244 tests passing** (1 skipped due to concurrent file access in test environment)
- Ny testdekning lagt til for:
  - StedholderService funksjonalitet
  - MotorTranslationService orkester
  - JsonString Localizer plassholder indeksere

### Kjente begrensninger

- testen hoppes over når du kjører parallelt fordi flere testinstanser deler den samme filen. Den passerer når den kjører i isolasjon.

## Ny filstruktur

### Tjenester i

- — Pipeline orkester
- — Oversettelse av landnavn
- — JSON ordbok synkronisering
- — Merkeoversettelse
- — SignalR-meldingspublikasjon
- — Prøv logikk på nytt med plassholdermaskering
- — Utgivergrensesnitt
- — Landservicegrensesnitt
- — Lokaliseringstjenestegrensesnitt
- — Dokumenttjenestegrensesnitt
- — Orchestrator grensesnitt (oppdatert)
- — Oversettelsesmetadata per fil

### Oppdaterte tjenester i

- — Lagt til navngitt stedholderstøtte
- - Oppdatert for ny parameter
- — Navngitt stedholderadministrasjon
- — Plassholdergrensesnitt

### Ny annonseside i

- — Sanntidsovervåkningsside
- — Sidemodell

### Ny dokumentasjon i

- — Oppdatert rørledningsdokumentasjon
- — Veiledning for stedholdersystem
- — Dashboard bruk guide
- — Oversikt over teknisk arkitektur

## Bakoverkompatibilitet

Alle endringer er tilsetningsstoffer:

- Eksisterende lokaliseringskode () fungerer uendret
- Posisjonsformatering () fungerer uendret
- Eksisterende JSON ordbokformat er uendret
- Eksisterende markeringsstruktur er uendret
- SignalR-meldinger bruker samme format

## Migrasjonssti

Ingen migrasjon kreves. Omsetningen er intern:

1. Gamle ble bevart som referanse og deretter erstattet
2. DI-registreringer ble oppdatert til å bruke nye grensesnitt
3. Alle eksisterende kunder ser ingen endringer

## Effektforbedringer

- **Redusert minnebruk**: Filer lagret per-språk umiddelbart i stedet for å holde alle i minnet
- **Faster inkremental løp**: Bare endret/falske merkeblokker omsettes
- **Better synlighet**: Real-time fremgang hjelper diagnostisere langsomme stadier

## Fremtidige forbedringer

Planlagte forbedringer:

1. **AI finjustering** — Oversettelsesanmeldelse av ettermaskin for fraser > 5 ord
2. **Admin-autentisering** — Begrense administratorsider til autoriserte brukere
3. **Dictionary editor** — Web UI for å administrere lokaliseringsnøkler
4. **Translationsstatistikk** — Kart som viser antall oversettelser og feilpriser over tid
5. **Personlig stedholdersyntaks** — Støtte for alternative plassholderformater

## Kontakt

For spørsmål eller problemer med oversettelsestjenesten, se den detaljerte dokumentasjonen i hver moduls katalog eller kontakt utviklingsteamet.
