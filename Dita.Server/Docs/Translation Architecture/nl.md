# Vertalingsarchitectuur

Dit document beschrijft de modulaire architectuur van Dita's automatische vertaalsysteem, geïntroduceerd om de duurzaamheid, testbaarheid en veerkracht te verbeteren.

## Ontwerpdoelstellingen

De refactoring richtte zich op verschillende zorgen over het oorspronkelijke monolithische ontwerp:

- **Verdeling van de bezorgdheid**: Elk vertaaldomein (landen, JSON woordenboeken, Markdown) is geïsoleerd.
- **Incrementele persistentie**: Bestanden worden per taal onmiddellijk na vertaling opgeslagen, waardoor het geheugengebruik wordt verminderd en eerdere resultaten worden verkregen.
- **Resilience**: Multiple retry levels verwerken voorbijgaande storingen zonder de gehele pijpleiding te blokkeren.
- **Observabiliteit**: Elke belangrijke operatie wordt gemeld via SignalR voor real-time monitoring.
- **Uithoudingsvermogen**: Nieuwe vertaaldoelen kunnen worden toegevoegd door de invoering van één interface.

## Ontbinding van de dienst

### BackendTranslationService (orchester)

** Verantwoordelijkheden**:
- Levenscyclusbeheer van pijpleidingen (start, voltooiing, foutafhandeling)
- Semafore-gebaseerde concurrency control (voorkomt overlappende runs)
- Servervalidatie (latency, taal beschikbaarheid, configuratie)
- Delegatie aan subdiensten

** Bevat NIET**:
- Vertaallogica
- Bestand I/O voor specifieke formaten
- Logica opnieuw proberen

### LandenVertalingService

** Verantwoordelijkheden**:
- Van map lezen
- Landnamen synchroniseren in het standaard locale woordenboek
- Vertalen ontbrekende landennamen per doeltaal
- Elk doelwoordenboek onmiddellijk na vertaling opslaan

**Kerngedrag**:
- Als standaardtaal Engels is: landnamen opgeslagen als-is
- Als standaard taal is andere: Engelse namen vertaald naar standaard taal eerst
- Elke taal wordt zelfstandig verwerkt met zijn eigen retry loop

### LokalisatieVertalingService

** Verantwoordelijkheden**:
- Detecteer toegevoegde/verwijderde sleutels door de huidige standaard woordenboek te vergelijken met vorige snapshot
- Vertalen toegevoegde sleutels in elke doeltaal
- Verwijder verwijderde sleutels uit elke doeltaal
- Snapshot opslaan voor volgende vergelijking

**Kerngedrag**:
- Handmatige vertalingen hebben altijd prioriteit (nooit overschreven)
- Toegevoegde sleutels worden vertaald en per taal onmiddellijk opgeslagen
- Verwijderde sleutels worden per taal onmiddellijk verwijderd
- Snapshot wordt alleen opgeslagen nadat alle talen succesvol zijn voltooid

### DocumentenVertalingService

** Verantwoordelijkheden**:
- Loop geconfigureerd markdown roots recursief
- Detecteer gewijzigde bronbestanden met behulp van SHA-256 hashes
- Track per blok vertaalstatus in
- Block-by-block vertalen met per-block retry
- Valideren Markdown structuur na vertaling
- Elk doeltaalbestand onafhankelijk opslaan

**Kerngedrag**:
- Blokniveau granulariteit: rubrieken, alinea's, lijstposten worden afzonderlijk vertaald
- Metadata tracks die blocks succesvol zijn/zijn mislukt per taal
- Foute blokken worden opnieuw opgehaald op de volgende run zonder opnieuw te vertalen succesvolle blokken
- Structuurvalidatie zorgt voor koptellingen, lijsten, codeblokken, enz

## Strategie opnieuw proberen

Het systeem implementeert herhalingen op drie niveaus:

### Niveau 1: HTTP (LibreTranslateService)

- Tot 5 pogingen met exponentiële backoff (1s, 2s, 3s, 4s, 5s)
- Behandelt netwerk timeouts, 5xx fouten, en voorbijgaande storingen
- Ingebouwd in de HTTP-clientconfiguratie

### Niveau 2 Stage (TranslationRetryService)

- Tot 3 pogingen met 30 seconden vertraging
- Re-drives het hele vertaalverzoek na HTTP-niveau retries zijn uitgeput
- Plaatshouder maskering en restauratie wordt toegepast op dit niveau

### Niveau 3 Block (DocumentsTranslationService)

- Individuele Markdown blokken die falen worden gemarkeerd in metagegevens
- Automatisch opgehaald bij de volgende pijpleiding
- Succesvolle blokken worden nooit opnieuw vertaald

## Gegevensstroom

### JSON woordenboek vertaling

```
[Default Dictionary]
       ↓
[Compare with Snapshot]
       ↓
[Detect Added/Removed Keys]
       ↓
For each target language:
   ├─ Load existing dictionary
   ├─ Apply removals
   ├─ Translate additions (with retry)
   └─ Save dictionary immediately
       ↓
[Save new snapshot]
```

### Vertaling markeren

```
[Source File]
       ↓
[Compute Hash]
       ↓
[Compare with stored hash]
       ↓
[Load translation metadata]
       ↓
For each target language:
   ├─ Extract translatable blocks
   ├─ For each block:
   │   ├─ Check metadata (already translated?)
   │   ├─ Translate with retry
   │   └─ Mark success/failure in metadata
   ├─ Reconstruct Markdown
   ├─ Validate structure
   ├─ Save target file
   └─ Save metadata
```

### Landnaam vertaling

```
[countries.json]
       ↓
[Build set of country keys]
       ↓
[Update default dictionary]
       ↓
For each target language:
   ├─ Find missing country entries
   ├─ Translate each missing entry (with retry)
   └─ Save dictionary immediately
```

## Persistentie van de staat

### Snapshots

- **JSON**: opgeslagen in een bestand naast het standaard woordenboek (naam varieert per opslagprovider)
- **Purpose**: Activeert incrementele synchronisatie door te volgen wat aanwezig was in de vorige run

### Hash-bestanden

- **Markdown**: naast het bronbestand
- **Fallback**: als de primaire locatie alleen-lezen is
- **Purpose**: Detecteert bronwijzigingen om onnodige hervertaling te voorkomen

### Vertalingsmetadata

- **Markdown**:
- **Inhoud**:
  - Broninhoud hash
- Status per taalblok (array van booleanen)
- Laatste update tijdstempel
- **Purpose**: Inschakelt gedeeltelijke hervertaling van alleen mislukte blokken

### Plaatshouder

- **Bestand**:
- **Inhoud**: Woordenboek van de sleutels van plaatshouder naam-waarde paren
- **Purpose**: Biedt standaardwaarden voor benoemde plaatshouders in de toepassing

## Signaal R-rapportage

### Uitgever abstractie

loskoppelt vertaaldiensten van SignalR-specifics:

```csharp
public interface ISignalRPublisher
{
    Task PublishStageAsync<T>(Guid runId, ProcessStage stage, T data, ...);
    Task PublishMessageAsync(Guid runId, LocalizationMessageType type, ProcessStage stage, ...);
}
```

### Sequentiegaranties

- Berichten binnen een enkele run zijn monotonisch gerangschikt
- De volgnummers zijn uniek per rij via
- Klanten kunnen gaten detecteren of herordenen

### Hub-kartering

```csharp
app.MapHub<LocalizationHub>("/hubs/localization");
```

## Uitbreidingspunten

### Een nieuw vertaaldoel toevoegen

1. Maak een nieuwe interface aan met
2. Implementeer de interface met domeinspecifieke logica
3. register in de container
4. Inspuiten in constructor
5. Oproep van na bestaande stadia

### Aangepast retry-beleid

Constructorparameters negeren:

```csharp
services.AddSingleton<TranslationRetryService>(
    sp => new TranslationRetryService(
        sp.GetRequiredService<ILibreTranslateService>(),
        sp.GetRequiredService<IPlaceholderService>(),
        sp.GetRequiredService<ILogger<TranslationRetryService>>(),
        stageMaxRetries: 5,        // More retries
        stageRetryDelaySeconds: 60  // Longer delays
    ));
```

### Aangepaste plaatshouder handling

Implementeren om plaatshouder syntax of opslag te wijzigen:

```csharp
public class CustomPlaceholderService : IPlaceholderService
{
    // Use {{name}} syntax instead of {name}
    // Store in database instead of JSON file
    // Encrypt sensitive placeholder values
}
```

## Instellingen

### apps.json

```json
{
  "AutomaticTranslationSettings": {
    "DefaultLanguage": "en",
    "IgnoredLanguages": ["auto", "detect"],
    "MarkdownRoots": ["/Docs", "/Help"],
    "AutomaticRun": true,
    "CheckingPeriod": 30,
    "WaitingTime": "00:00:00",
    "RequestThrottleMs": 80,
    "RequestTimeoutSeconds": 10
  }
}
```

### Runtime tuning

Instellingen
|---------|---------|--------|
80
10
3
30

## Teststrategie

### Eenheidstests

Elke subdienst is onafhankelijk te testen:

- Spot om succes/fout te simuleren
- Spot om rapportage te verifiëren
- Tijdelijke mappen gebruiken voor bestand I/O
- Verifiëren per-taal opslaan gedrag

### Integratietests

- Volledige pijpleiding loopt met echte (lokale) LibreVertaal instantie
- Signaal verifiëren R berichten worden geleverd aan verbonden klanten
- Test gelijktijdige runpreventie (semafore)
- Valideren Markdown structuur na vertaling

### Eind-tot-eindtests

- Trigger vertaling via API of scheduler
- Controleren of alle doeltaalbestanden zijn aangemaakt of bijgewerkt
- Metadatabestanden controleren die de juiste blokstatus bevatten
- Bevestig plaatshouders worden bewaard in vertalingen

## Prestatieoverwegingen

- **Geheugen**: Per-taal opslaan voorkomt het vasthouden van alle woordenboeken in het geheugen
- **Disk I/O**: Metadatabestanden voegen kleine overhead toe maar maken incrementele werkzaamheden mogelijk
- **Network**: Sequentiële verwerking met throttling voorkomt overweldigende LibreVertalen
- **CPU**: SHA-256 hashing en regex validatie zijn snel ten opzichte van vertaling latentie
- **SignalR**: Lichtgewicht berichten, geen lading compressie nodig voor typische rapporten

## Migratie vanuit monolithisch ontwerp

Het origineel bevatte alle logica in één klasse. Het migratiepad:

1. Landlogica uitpakken →
2. Uitpakken JSON logica →
3. Logica uitpakken →
4. Signaal uitpakken R publiceren →
5. Logica opnieuw uitpakken →
6. Vereenvoudig orkestmeester tot delegatie-alleen

Alle bestaande interfaces () blijven ongewijzigd. Consumenten van de pijpleiding zien geen breuken.
