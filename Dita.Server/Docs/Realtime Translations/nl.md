# Real-time vertalingen

Dit document bestaat als live test input voor de automatische vertaalpijplijn.

## Wat de dienst doet

De dienst draait op een schema en valideert de vertaalserver, configuratie en beschikbare talen voordat een vertaalwerk begint.

Na de validatie stap, synchroniseert het landnamen uit de alleen-lezen landen catalogus in de standaard localisatie JSON woordenboeken. Als de standaardtaal van de toepassing Engels is, wordt de landinvoer opgeslagen als sleutel gelijk aan waarde. Als de standaardtaal anders is, wordt de Engelse landsnaam eerst vertaald in de standaardtaal, en alleen daarna opgeslagen als sleutel is gelijk aan waarde in het standaard woordenboek.

Vervolgens vergelijkt de service het huidige standaard lokalisatie woordenboek met het opgeslagen snapshot uit de vorige run. Nieuw toegevoegde items worden alleen vertaald in doeltalen als de sleutel nog niet bestaat, dus handmatige vertalingen blijven prioriteit. Verwijderde items worden verwijderd uit alle doel woordenboeken om de hele set consistent te houden.

Tot slot scant de dienst geconfigureerde documentatie wortels voor Markdown bomen. Elke map van het onderwerp zal naar verwachting een bronbestand bevatten dat naar de standaardtaal is vernoemd, zoals en.md. De service hashes die bronbestand, detecteert wijzigingen, vertaalt ontbrekende of verouderde doel Markdown-bestanden, en slaat de huidige hash naast het bronbestand. Als het schrijven van de hash naast het bronbestand niet mogelijk is, valt het terug naar tijdelijke opslag.

## Hoe de dienst de voortgang meldt

De backend zendt algemene SignalR berichten via de localisatie hub met behulp van een bericht envelop. Elk bericht bevat een berichttype, de huidige procesfase, een UTC-tijdstempel, een tekstsamenvatting en optionele fasespecifieke lading.

De huidige stadia zijn:

- Controleservers
- VertalenCountries
- Vertalen Nederlands
- Vertalen Nederlands
- OpslaanResultaten

Typische berichtstroom wordt gestart, stadium voltooid, en pijpleiding voltooid. Als een fase mislukt, wordt het bericht gemarkeerd als een fout en bevat gestructureerde foutinformatie met uniforme foutcodes.

## Ontwerpbeginselen

Vertalingen worden achtereenvolgens verwerkt om overbelasting van de LibreTranslate server te voorkomen.

Lokalisatie JSON woordenboeken worden altijd opgeslagen met alfabetisch gesorteerde toetsen en geformatteerde JSON voor gemakkelijker onderhoud.

De vorige standaard woordenboek snapshot wordt permanent opgeslagen, zodat een herstart van de toepassing niet verliest wijziging tracking.

**Handmatige vertalingen hebben altijd voorrang boven automatische toevoegingen.**
