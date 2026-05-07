# Tulkošanas arhitektūra

Šajā dokumentā aprakstīta Ditas automātiskās tulkošanas sistēmas modulārā arhitektūra, kas ieviesta, lai uzlabotu uzturamību, pārbaudāmību un izturību.

## Dizaina mērķi

Refaktorēšanā tika ņemtas vērā vairākas bažas saistībā ar sākotnējo monolītu konstrukciju:

- ** Bažu nošķiršana**: Katrs tulkošanas domēns (valstis, JSON vārdnīcas, Markdown) ir izolēts.
- ** Inkrementālā noturība**: Faili tiek saglabāti uz vienu valodu uzreiz pēc tulkojuma, samazinot atmiņas izmantošanu un sniedzot agrākus rezultātus.
- ** Noturība**: Vairāki atkārtošanas līmeņi rīkoties pārejošas neveiksmes, nebloķējot visu cauruļvadu.
- **Observability**: Every significant operation is reported via SignalR for real-time monitoring.
- **Terminalitāte**: Jaunus tulkošanas mērķus var pievienot, ieviešot vienu saskarni.

## Dienesta sadalīšanās

### AizmuguresTulkošanas serviss (orchstrator)

**Responsibilities**:
- Cauruļvadu ekspluatācijas cikla pārvaldība (sākums, pabeigšana, kļūdu apstrāde)
- Uz semaforiem balstīta konvalūtas kontrole (novērš pārklāšanos trases)
- Servera apstiprināšana (latence, valodas pieejamība, konfigurācija)
- Deleģēšana uz apakšpakalpojumiem

** Nesatur**:
- Tulkošanas loģika
- Fails I/O konkrētiem formātiem
- Mēģināt vēlreiz

### ValstisTulkošanas dienests

**Responsibilities**:
- Lasīt no mapes
- Sinhronizēt valstu nosaukumus noklusētajā lokalizācijas vārdnīcā
- Tulkot trūkstošos valstu nosaukumus katrā mērķa valodā
- Saglabāt katru mērķa vārdnīcu uzreiz pēc tulkošanas

**Pamata uzvedība**:
- Ja noklusētā valoda ir angļu: valstu nosaukumi tiek glabāti kā ir
- Ja noklusējuma valoda ir cita: Angļu vārdi tulkoti uz noklusējuma valodu vispirms
- Katra valoda tiek apstrādāta neatkarīgi ar savu retritry cilpa

### LokalizācijaTulkošanas serviss

**Responsibilities**:
- Noteikt pievienotās/izņemtās atslēgas, salīdzinot pašreizējo noklusēto vārdnīcu ar iepriekšējo momentuzņēmumu
- Tulkot pievienotās atslēgas katrā mērķa valodā
- Noņemt dzēstās atslēgas no katras mērķa valodas
- Saglabāt momentuzņēmumu nākamajam salīdzinājumam

**Pamata uzvedība**:
- Manuālie tulkojumi vienmēr ir prioritāte (nekad nav pārrakstīti)
- Pievienotās atslēgas tiek tulkoti un saglabāti par valodu nekavējoties
- Izņemtās atslēgas nekavējoties tiek dzēstas no vienas valodas
- Snapshot tiek saglabāts tikai pēc tam, kad visas valodas veiksmīgi pabeigtas

### DokumentiTranslationService

**Responsibilities**:
- Pastaiga konfigurēta Atzīmēšanas saknes rekursīvi
- Noteikt izmainītos pirmkoda failus, izmantojot SHA-256 hashes
- Celiņa uz bloka tulkošanas statuss
- Tulkot block-by-block ar vienu bloku retritry
- Pārbaudīt iezīmēšanas struktūru pēc tulkojuma
- Saglabāt katru mērķa valodas failu neatkarīgi

**Pamata uzvedība**:
- Bloka līmeņa granularitāte: virsrakstus, punktus, saraksta posteņus tulko atsevišķi
- Metadatu celiņi, kas bloķē veiksmīgi/neveiksmīgi katrā valodā
- Neizdevās bloki tiek retridetēti nākamajā reizē bez veiksmīgi pārtulkotiem blokiem
- Struktūras apstiprināšana nodrošina pozīciju skaitu, sarakstus, kodu blokus utt. atbilstību avots

## Mēģināt vēlreiz

Sistēma īsteno atkārtojumus trīs līmeņos:

### Līmenis – HTTP (LibreTranslateService)

- Līdz 5 mēģinājumiem ar eksponenciālu dublēšanos (1s, 2s, 3s, 4s, 5s)
- Veic tīkla noildzes, 5xx kļūdas un pārejošas kļūdas
- Iebūvēts HTTP klienta konfigurācijā

### Līmenis – posms (TranslationRestryService)

- Līdz 3 mēģinājumiem ar 30 sekunžu kavēšanos
- Pārdzen visu tulkošanas pieprasījumu pēc HTTP līmeņa atkārtojumiem ir izsmelti
- Šajā līmenī tiek veikta vietturu maskēšana un restaurācija

### Līmenis – Bloks (DocumentsTranslationService)

- Individuāli iezīmēšanas bloki, kas neizdoties, ir atzīmēti metadatos
- Ielādēts automātiski nākamajā cauruļvada palaišanas reizē
- Veiksmīgie bloki nekad netiek pārtulkoti

## Datu plūsma

### JSON vārdnīcas tulkojums

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

### Atzīmēšana

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

### Valsts nosaukuma tulkojums

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

## Valsts noturība

### Momentuzņēmumu

- **JSON**: Saglabāts failā blakus noklusētajai vārdnīcai (nosaukums atšķiras pēc glabāšanas nodrošinātāja)
- **Purpose**: Ieslēdz inkrementālo sinhronizāciju, sekojot iepriekšējā izpildījumā esošajam

### Hash faili

- **Markdown**: blakus avota failam
- **Fallback**: ja primārā atrašanās vieta ir tikai lasāma
- **Purpose**: konstatē avota izmaiņas, lai izvairītos no nevajadzīgas pārtulkošanas

### Tulkošanas metadati

- ** uzcenojums**:
- **Saturs**:
  - Avota saturs hash
- Bloka statuss katrai valodai (buleānu masīvs)
- Pēdējā atjaunināšanas laika zīmogs
- **Purpose**: Ieslēdz daļēju tulkojumu tikai neveiksmīgiem blokiem

### Vietnieka uzglabāšana

- **Fails**:
- **Saturs**: Vietnes turētāja vārda vērtību pāru atslēgu vārdnīca
- **Purpose**: Nodrošina noklusējuma vērtības nosauktajiem vietturiem visā pieteikumā

## Signāls R ziņojums

### Publicētāja abstrakcija

atsaista tulkošanas pakalpojumus no SignalR īpatnībām:

```csharp
public interface ISignalRPublisher
{
    Task PublishStageAsync<T>(Guid runId, ProcessStage stage, T data, ...);
    Task PublishMessageAsync(Guid runId, LocalizationMessageType type, ProcessStage stage, ...);
}
```

### Secīguma garantijas

- Vēstules vienā kārtā ir monotoni secīgi
- Kārtas numuri ir unikāli, izmantojot
- Klienti var atklāt nepilnības vai pārkārtošanu

### Humbu kartēšana

```csharp
app.MapHub<LocalizationHub>("/hubs/localization");
```

## Pagarinājuma punkti

### Pievieno jaunu tulkošanas mērķi

1. Izveidot jaunu saskarni ar
2. Ieviest saskarni ar domēnu loģiku
3. Reģistrēties DI konteinerā
4. Ievadīt konstruktorā
5. Zvans pēc esošajiem posmiem

### Pielāgota atkārtošanas politika

Aizstāt konstruktora parametrus:

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

### Muitas vietturis apstrāde

Ieviest, lai mainītu viettura sintaksi vai uzglabāšanu:

```csharp
public class CustomPlaceholderService : IPlaceholderService
{
    // Use {{name}} syntax instead of {name}
    // Store in database instead of JSON file
    // Encrypt sensitive placeholder values
}
```

## Konfigurācija

### appsetings.json

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

### Darbības laika regulēšana

Iestatījums
|---------|---------|--------|
80
10
3
30

## Testēšanas stratēģija

### Vienības testi

Katrs apakšpakalpojums ir neatkarīgi pārbaudāms:

- Mock, lai simulēt panākumus / neveiksmīgs
- Pārbaudīt ziņojumu
- Lietot pagaidu mapes failam I/O
- Pārbaudīt katras valodas saglabāšanas uzvedību

### Integrācijas testi

- Pilna cauruļvada izmantošana ar reālu (vietēju) LibreTranslate instance
- Pārbaudīt signālu R ziņojumi tiek piegādāti saistītajiem klientiem
- Testa vienlaicīgas palaišanas novēršana (semafora)
- Pārbaudīt iezīmēšanas struktūru pēc tulkojuma

### Beigu testi

- Ieslēdz tulkojumu caur API vai plānotāju
- Pārbaudīt visus mērķa valodu failus tiek izveidots / atjaunināts
- Pārbaudīt metadatu failus satur pareizu bloka statusu
- Apstiprināt, ka vietturi tiek saglabāti tulkojumos

## Darbības apsvērumi

- **Atmiņa**: Saglabāšana vienā valodā neļauj paturēt atmiņā visas vārdnīcas
- **Disks I/O**: Metadati faili pievienot mazo gaisvadu bet iespējot pakāpenisks darbs
- **Tīkls**: Secīga apstrāde ar drostling novērš pārliecinošu LibreTranslate
- **CPU**: SHA-256 hashing un regex validācija ir ātri attiecībā pret tulkošanas latentumu
- **SignalR**: viegli ziņojumi, nav nepieciešama derīgas kravas saspiešana tipiskiem ziņojumiem

## Migrācija no monolītās konstrukcijas

Oriģināls saturēja visu loģiku vienā klasē. Migrācijas ceļš:

1. Atspiest valsts loģika →
2. Izvilkuma JSON loģika →
3. Ekstrakcijas iezīmēšanas loģika →
4. Atspiest signālu R izdošana →
5. Ekstrakta atkārtošanas loģika →
6. Vienkāršot orķestra tikai delegāciju

Visas esošās saskarnes () netiek mainītas. Cauruļvada patērētāji neredz nekādas izmaiņas.
