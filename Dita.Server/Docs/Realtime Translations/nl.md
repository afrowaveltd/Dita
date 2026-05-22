# Real-time vertalingen

Dit document bestaat als live test input voor de automatische vertaalpijplijn. Elke wijziging in dit bestand leidt tot hervertaling van alle doeltaalbestanden op de volgende geplande run.

## Overzicht architectuur

De vertaalpijplijn is geherstructureerd in een modulaire architectuur met vier gespecialiseerde subdiensten gecoördineerd door een lichtgewicht orkestmeester:

- **BackendTranslationService** Orkesteert de gehele pijplijn, behandelt servervalidatie en delegeert werken aan sub-services.
- **CountriesTranslationService** .
- **LocalisatieVertalingService** .
- **DocumentsTranslationService** Vertaalt Documentaire bestanden afdrukken met per-block tracking en metadata.

Elke subdienst werkt onafhankelijk en rapporteert vooruitgang via SignalR in real time.

## Wat de dienst doet

De service draait op een schema en voert een vijftraps pijplijn uit: servervalidatie, country synchronisatie, JSON woordenboeksynchronisatie, Markdown-bestandsvertaling, en het volhouden van de resultaten. Elke fase zendt gestructureerde real-time voortgangsgebeurtenissen uit via SignalR zodat verbonden clients mee kunnen volgen als werkopbrengst.

## Pijpleidingen

### Fase 1

Voordat een vertaalwerk begint, controleert de dienst of aan alle voorwaarden is voldaan:

- De configuratie sectie moet aanwezig en geldig zijn.
- De LibreTranslate server moet reageren binnen een aanvaardbare latency.
- De lijst van talen die beschikbaar zijn op de vertaalserver is opgehaald.
- De geconfigureerde standaardtaal moet in die lijst staan.
- Ontbrekende lokale JSON-bestanden voor elke ondersteunde taal worden automatisch aangemaakt.

Als een controle mislukt, stopt de pijpleiding onmiddellijk en wordt een bericht uitgezonden.

### Stadium 2 VertalenLanden

Landnamen worden gesynchroniseerd vanuit een alleen-lezen catalogus () in de localisatie JSON woordenboeken.

- Als de standaardtaal van de applicatie Engels is, wordt elke landnaam opgeslagen als zonder vertaling.
- Als de standaardtaal een andere taal is, wordt de Engelse landsnaam eerst in die taal vertaald en wordt het resultaat de vermelding in het standaardwoordenboek.
- Nadat het standaardwoordenboek is bijgewerkt, wordt elke ontbrekende landcode in elk woordenboek vertaald en opgeslagen **onmiddellijk per taal**.
- Al vertaalde vermeldingen worden bewaard zonder wijziging.
- Als een vertaling mislukt, de dienst opnieuw tot 3 keer met 30-seconde vertragingen alvorens naar de volgende taal.

### Stadium 3 VertalenJsonFiles

De service vergelijkt het huidige standaard lokalisatie woordenboek met een snapshot die is opgeslagen van de vorige run:

- **Toegevoegde sleutels** .
- **Verwijderde sleutels** Ingangen aanwezig in de snapshot, maar afwezig uit de huidige standaard worden verwijderd uit elk woordenboek.
- Handmatige vertalingen hebben altijd prioriteit. Als een doelwoordenboek al een waarde voor een sleutel bevat, blijft dat item ongewijzigd, ongeacht wat de bron zegt.
- **Elk taalwoordenboek wordt onmiddellijk opgeslagen nadat de vertalingen zijn voltooid**, in plaats van te wachten tot alle talen klaar zijn.
- Als een vertaling mislukt voor een bepaalde taal, de dienst opnieuw automatisch. Alleen hardnekkige fouten (bv. niet-ondersteunde taal) veroorzaken dat de taal wordt overgeslagen.
- Na de run wordt het huidige standaard woordenboek opgeslagen als de nieuwe snapshot voor de volgende vergelijking.

Alle woordenboeken worden altijd opgeslagen met alfabetisch gesorteerde toetsen en ingesprongen JSON voor menselijke leesbaarheid.

### Stadium 4 VertalenMarkdownFiles

De service bewandelt de geconfigureerde documentatie roots (standaard: ) en verwerkt elk bronbestand recursief:

1. De inhoud van het bronbestand wordt gelezen en een SHA-256 hash wordt berekend.
2. Een bestand naast de bron tracks per-taal, per-blok vertaalstatus, waardoor **incrementele re-vertaling** van alleen mislukte blokken.
3. De opgeslagen hash van de vorige run (opgeslagen in een bestand naast het bronbestand, of in een tijdelijke terugvallocatie) wordt vergeleken met de huidige hash.
4. Voor elke doeltaal wordt het bijbehorende bestand ook gecontroleerd op structurele integriteit.
5. Elk doelbestand dat ontbreekt, heeft een verouderde hash, faalt structuurvalidatie, of bevat onvertaalde blokken is in de wachtrij voor hervertaling.
6. **Elke doeltaal wordt zelfstandig vertaald en opgeslagen** Als het Tsjechisch lukt maar het Frans faalt, wordt het Tsjechische bestand nog steeds naar de schijf geschreven.
7. Succesvol vertaalde bestanden worden gevalideerd voor structurele pariteit met de bron (gelijke rubriek telt, lijst items, code blokken, blokquotes, links, vet /italic markers, en HTML tags) voordat ze worden geschreven naar de schijf.
8. Als alle doelbestanden voor een bron succesvol zijn, wordt de nieuwe hash naast de bron opgeslagen. Als het schrijven naast de broncode mislukt (bijvoorbeeld in alleen-lezen implementaties), valt de hash terug naar de tijdelijke directory.
9. Als een doelvertaling mislukt, markeert de metadata die blokken als onvertaald, zodat ze op de volgende run worden opgehaald.

### Fase 5

Een geconsolideerde wordt samengesteld en gepubliceerd. Het omvat:

- UTC start en voltooit tijdstempels.
- Telt van opgeslagen locale JSON-bestanden, opgeslagen Markdown-bestanden, opgeslagen hash-bestanden, en fallback hash schrijft.
- Alle opslagfouten verzameld tijdens de run.
- Per-taal vertaalstatistieken (vertaald aantal, overgeslagen aantal, aantal fouten).

## SignaalR bericht envelop

Elke voortgang wordt geleverd als een met de volgende velden:

Veld
|-------|------|-------------|
Concordantietabel voor de lopende pijpleiding
Monotone teller binnen een run, te beginnen bij 1
Semantisch type bericht
Pijpleidingsfase waartoe het bericht behoort
UTC-tijd waarop het bericht werd uitgezonden
Of het bericht een foutvoorwaarde weergeeft
Menselijk leesbare samenvatting
Fasespecifieke lading (rapporteer object of nul)

### Berichttypes

Waarde
|-------|------|---------|
0
1
2
3
4
5
6

### Pijpleidingen

Waarde
|-------|------|-------------|
0
1
2
3
4
5

### Typische berichtenstroom

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

Als een fase mislukt, worden de resterende stadia overgeslagen, wordt een bericht uitgezonden en tenslotte sluit een bericht de run.

## Translation retry logica

De pijpleiding implementeert twee niveaus van veerkracht:

### tweede fase (vertaaldienst)

- Als een vertaalverzoek mislukt na LibreTranslate's interne retrieves, voert het tot 3 extra fase-niveau retrieves met 30 seconden vertraging.
- Plaatshouder masking: Genoemde plaatshouders () in de tekst worden tijdelijk vervangen door veilige tokens () voor vertaling en daarna hersteld, zodat correcte grammatica in doeltalen.

### Taalvalidatie

- Alvorens te vertalen naar een doeltaal, controleert de dienst de taal wordt ondersteund door de vertaalserver.
- Niet-ondersteunde talen worden overgeslagen met een waarschuwing, waardoor herhaalde mislukte pogingen worden voorkomen.

### Block-level opnieuw proberen markeren

- Opmaakvertalingen worden blok voor blok uitgevoerd (rubrieken, alinea's, lijstitems).
- Als een individuele blok faalt vertaling, wordt het gemarkeerd als niet-vertaald in het metagegevensbestand en opnieuw op de volgende pijplijn uitgevoerd.
- De service volgt per taal, per blok status in bestanden naast elke bron Markdown bestand.

## Foutcodes

Fouten worden gerapporteerd met behulp van een unified enum gegroepeerd in bereiken:

Bereik
|-------|----------|
1000
2000
3000
4000
5000

Elke fout in een rapport draagt de broncode (taalcode, bestandspad of podiumnaam), de foutcode en een menselijk leesbaar bericht.

## Live Vertaling Dashboard

Het Server project bevat een admin pagina op die verbinding maakt met de SignalR hub op en toont alle pijpleiding gebeurtenissen in real time.

- Toont verbindingsstatus, aantal berichten en een live-updating tabel van alle gebeurtenissen.
- Kleur-gecodeerde rijen: blauw voor het begin van de etappe, groen voor voltooiing, rood voor fouten.
- Ondersteunt het wissen van de feed en het exporteren van alle berichten naar JSON.
- Automatisch opnieuw verbinden met exponentiële backoff als de verbinding daalt.

## Ontwerpbeginselen

- **Modulariteit**: Elke vertaling is geïsoleerd in zijn eigen dienst voor onderhoud en testbaarheid.
- **Incrementele persistentie**: Woordenboeken en Markdown-bestanden worden per taal onmiddellijk na vertaling opgeslagen, waardoor de geheugendruk wordt verminderd en eerdere feedback wordt gegeven.
- **Resilience**: Meerdere retry niveaus (HTTP, stadium, blok) zorgen ervoor dat tijdelijke storingen de pijpleiding niet blokkeren.
- **State tracking**: Per-file metadata () en hash bestanden maken nauwkeurige incrementele werkzaamheden mogelijk op de volgende runs.
- **Real-time zichtbaarheid**: Elke belangrijke operatie wordt gemeld via SignalR voor monitoring en debugging.
- **Handmatige vertalingen hebben altijd voorrang boven automatische toevoegingen.**
