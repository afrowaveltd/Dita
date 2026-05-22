# Samenvatting van wijzigingen aan de automatische vertaaldienst

## Overzicht

Dit document geeft een samenvatting van alle wijzigingen die zijn aangebracht aan de automatische vertaaldienst Dita, waaronder architectuurrefactoring, nieuwe functies, opmerkzaamheidsverbeteringen en lokalisatieverbeteringen.

## Architectuurwijzigingen

### Refactored BackendTranslationService

De monolithische is samengesteld uit vier gespecialiseerde diensten, gecoördineerd door een lichtgewicht orkestrator:

- **BackendTranslationService**
- **CountriesTranslationService**
- **LocalisatieVertalingService**
- **DocumentsTranslationService**
- **SignalRpublisher**
- **VertalingRetryService**

### Voordelen

- **Verdeling van de bezorgdheid**: Elke dienst behandelt één vertaaldomein
- **Behoud**: Kleinere klassen zijn gemakkelijker te begrijpen en te testen
- **Uithoudingsvermogen**: Nieuwe vertaaldoelen kunnen worden toegevoegd via interface implementatie
- **Betrouwbaarheid**: Onafhankelijke diensten bieden betere breukisolatie

## Nieuwe functies

### Live-vertaalmonitor

**Locatie**:

Een nieuwe admin pagina die real-time zichtbaarheid biedt in de vertaalpijplijn:

- Toont alle SignalR-gebeurtenissen als ze optreden
- Kleurgecodeerde berichtentypen (blauw=gestart, groen=voltooid, rood=fout)
- Verbindingsstatusbanner met automatisch opnieuw verbinden
- Berichten teller en exporteren naar JSON

### Genoemde Plaatshouders

Het lokalisatiesysteem ondersteunt nu benoemde plaatshouders () voor verbeterde grammaticaliteit in verschillende talen:

```csharp
// Usage in code
var message = Localizer["WelcomeMessage", new Dictionary<string, string>
{
    ["userName"] = "John",
    ["count"] = "5"
}];
// Result: "Hello John, you have 5 new messages"
```

Kenmerken:
- Plaatshouderwaarden opgegeven op runtime of opgeslagen in
- Automatische masking / restauratie tijdens de vertaling om corruptie te voorkomen
- Achterwaarts compatibel met bestaande positiehouders

### Incrementele vertaling

Markdown-bestanden worden stapsgewijs vertaald:

- **Pertaal opslaan**: Elke doeltaal wordt onmiddellijk na vertaling opgeslagen, waardoor de geheugendruk wordt verminderd
- **Block-level tracking**: tracks vertaalstatus per blok
- **Selectieve herhaling**: Alleen mislukte blokken worden opnieuw vertaald op de volgende run
- **Data persistentie**: Translation state overleeft toepassing herstart

### Verbeterde herhalingslogica

Drie niveaus van veerkracht:

1. **HTTP retry** (LibreTranslateService): 5 pogingen met exponentiële backoff (1s
2. **Stage retry** (TranslationRetryService): 3 extra pogingen met 30s vertraging
3. **Block retry** (DocumentsTranslationService): mislukt Markdown blokken opnieuw opgehaald op volgende run

### SignalR-rapportage

Voortgangsverslagen in realtime voor alle pijpleidingactiviteiten:

- Elke fase publiceert evenementen
- Vooruitgang per taal gepubliceerd als evenementen
- Foutmeldingen omvatten gedetailleerde context (bron, foutcode, bericht)
- Sequentienummers garanderen bestellen binnen elke run

## Configuratiewijzigingen

### apps.json

Geen veranderingen. Bestaande configuratie blijft werken:

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

### Nieuwe diensten

Geregistreerd in:

- '
- `TranslationRetryService`
- '
- '
- '
- '

De SignalR-hub is in kaart gebracht voor clientverbindingen.

## Testen

### Teststatus

- **243/244 tests slagen** (1 overgeslagen vanwege gelijktijdige bestandstoegang in testomgeving)
- Nieuwe testdekking toegevoegd voor:
  - PlaatshouderDienstfunctionaliteit
  - BackendTranslationService orkestation
  - JsonStringLocalizer plaatshouder indexers

### Bekende beperkingen

- test wordt overgeslagen bij parallel draaien omdat meerdere test instanties hetzelfde bestand delen. Het gaat voorbij als het in isolatie loopt.

## Nieuwe bestandsstructuur

### Diensten in

- Orkestmeester
- Landnaam vertaling
- JSON woordenboeksynchronisatie
- Vertaling markeren
- Bericht publiceren
- Retry logica met plaatshouder maskering
- Uitgever
- Country service interface
- Lokalisatie service interface
- Documentdienst-interface
- Orkestinterface (bijgewerkt)
- Metadata per bestand vertalen

### Bijgewerkte diensten in

- Toegevoegd naam plaatshouder ondersteuning
- Bijgewerkt voor nieuwe parameter
- Benoemde plaatshouder management
- Plaatshouder interface

### Nieuwe beheerderspagina in

- Real-time monitoring pagina
- Paginamodel

### Nieuwe documentatie in

- Bijgewerkte documentatie over pijpleidingen
- Plaatshouder systeem gids
- Dashboard gebruikshandleiding
- Technische architectuur overzicht

## Compatibiliteit achteraf

Alle wijzigingen zijn additief:

- Bestaande lokalisatiecode () werkt ongewijzigd
- Positional formatting () werkt ongewijzigd
- Bestaande JSON woordenboekformaat is ongewijzigd
- Bestaande Markdown structuur is ongewijzigd
- SignalR-berichten gebruiken hetzelfde formaat

## Migratiepad

Geen migratie vereist. De refactoring is intern:

1. Oud werd bewaard als referentie en vervangen
2. DI registraties werden bijgewerkt om nieuwe interfaces te gebruiken
3. Alle bestaande consumenten zien geen veranderingen

## Prestatieverbeteringen

- **Verminderd geheugengebruik**: Bestanden opgeslagen per-taal onmiddellijk in plaats van het houden van alle in het geheugen
- **Faster incremental runs**: Alleen gewijzigde/gefaalde Markdown blokken worden opnieuw vertaald
- ** Beter zicht**: Real-time vooruitgang helpt diagnose langzame stadia

## Toekomstige verbeteringen

Geplande verbeteringen:

1. **AI fine-tuning** — Post-machine translation review for phrases > 5 words
2. **Admin-authenticatie**
3. **Dictionary editor**
4. **Vertaalstatistieken**
5. **Aangepaste placeholder syntax** Ondersteuning voor alternatieve plaatshouderformaten

## Contactpersoon

Raadpleeg voor vragen of problemen met de vertaaldienst de gedetailleerde documentatie in de directory van elke module of neem contact op met het ontwikkelingsteam.
