# Itzulpenaren arkitektura

Dokumentu honek Ditaren itzulpen automatikoko arkitektura modularra azaltzen du, mantengarritasuna, probagarritasuna eta erresilientzia hobetzeko sortua.

## Diseinuaren helburuak

Errefaktoreak hainbat kezka azaldu zituen jatorrizko diseinu monolitikoan:

- ** Kezkak bereiztea**: Itzulpen-domeinu bakoitza (kontakizunak, JSON hiztegiak, Markdown) isolaturik dago.
- **Erresistentzia handia**: Fitxategiak hizkuntza bakoitzeko gordetzen dira itzulpenaren ondoren, memoriaren erabilera murriztuz eta aurreko emaitzak emanez.
- **Erresilientzia**: saiakera anitzek hutsegite iragankorrak kudeatzen dituzte hodi osoa blokeatu gabe.
- **Erreserbagarritasuna**: Eragiketa esanguratsu oro denbora errealeko monitorizaziorako seinalearen bidez jakinarazten da.
- ** Hedapena**: Itzulpen-helburu berriak gehi daitezke interfaze bakar bat inplementatuz.

## Zerbitzua deskonposatzea

### BackendTranslationService (orchestrator)

** Erantzukizunak**:
- Pipeline bizitza-zikloaren kudeaketa (hasiera, osaketa, erroreen kudeaketa)
- Semaforoan oinarritutako konkurrentzi-kontrola (eskaneak gainditzen ditu)
- Zerbitzariaren balidazioa (latentasuna, hizkuntzen erabilgarritasuna, konfigurazioa)
- Azpizerbitzuetarako delegazioa

**Ez dauka**:
- Itzulpenaren logika
- I/O fitxategia formatu zehatzetarako
- Saiatu logikarekin

### Herentzia-zerbitzua

** Erantzukizunak**:
- Irakurri direktoriotik
- Sinkronizatu herrialdeen izenak hiztegi lokal lehenetsira
- Itzuli falta diren herrialdeen izenak helburuko hizkuntza bakoitzeko
- Gorde helburuko hiztegi bakoitza berehala itzulpenaren ondoren

**Gako portaerak**:
- Hizkuntza lehenetsia ingelesa bada: herrialde-izenak honela gordetzen dira:
- Hizkuntza lehenetsia beste bat bada: izen ingelesak lehen hizkuntza lehenetsira itzuli dira
- Hizkuntza bakoitza bere kabuz prozesatzen da bere saiakera begiztarekin

### LokalizazioaTranslationService

** Erantzukizunak**:
- Detektatu tekla gehituak edo lekuz aldatuak uneko hiztegi lehenetsia aurreko argazkiekin konparatuz
- Itzuli gako gehituak helburu-hizkuntza bakoitzean
- Kendu ezabatutako gakoak helburuko hizkuntza bakoitzeko
- Gorde argazkiak hurrengo konparaziorako

**Gako portaerak**:
- Eskuzko itzulpenek lehentasuna dute beti (ez da inoiz gainidatzi)
- Gehitutako teklak berehala itzultzen eta gordetzen dira
- Kendutako teklak berehala ezabatzen dira hizkuntzako
- Snapshot hizkuntza guztiak ongi burutu ondoren bakarrik gordetzen da

### DokumentuakZerbitzua

** Erantzukizunak**:
- Ibili Markdownen sustraiak errekurtsiboki konfiguratuak
- Aldatutako iturburu-fitxategiak detektatu SHA-256 hashes erabiliz
- Blokeko itzulpen-egoeraren jarraipena
- Itzuli blokez bloke blokeko saiakerarekin
- Balidatu Markdown egitura itzulpenaren ondoren
- Gorde helburuko hizkuntza-fitxategi bakoitza independenteki

**Gako portaerak**:
- Blokeen mailaren granularitatea: izenburuak, paragrafoak, zerrendako elementuak banan-banan itzultzen dira
- Hizkuntza bakoitzeko blokeek lortu/huts egin duten metadatuen pistak
- Huts egin duten blokeak hurrengo exekuziora itzultzen dira bloke arrakastatsuak itzuli gabe
- Egituraren balidazioak goiburuak, zerrendak, kode-blokeak eta abar bat datozela ziurtatzen du

## Saiatu berriro estrategia

Sistemak hiru mailatako erretenak inplementatzen ditu:

### 1. maila: HTTP (LibreTranslateService)

- Atzerapen esponentziala duten 5 saiakera (1s, 2s, 3s, 4s, 5s)
- Sareko denbora-mugak, 5xxx erroreak eta hutsegite iragankorrak kudeatzen ditu
- HTTP bezeroaren konfigurazioan eraikia

### 2. maila: etapa (TranslationRetryService)

- Hiru saiakera 30 segundoko atzerapenarekin
- HTTP mailako erretretizioen eskaera osoa birarazten du
- Leku-marka eta zaharberritzea aplikatzen da maila honetan

### 3. maila - Blokea (DokumentuakTranslationService)

- Huts egiten duten bloke indibidualak metadatuetan markatzen dira
- Hurrengo kanalizazioan automatikoki erretiratua
- Bloke arrakastatsuak ez dira inoiz itzuli

## Datu-fluxua

### JSON hiztegiaren itzulpena

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

### Markdown itzulpena

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

### Herrialdearen izena

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

## Estatuaren iraunkortasuna

### argazkiak

- **JSON**: Hiztegi lehenetsiaren ondoko fitxategi batean gordeta (izena aldatu egiten da biltegi-hornitzailearen arabera)
- **Purpose**: sinkronizazio inkrementala gaitzen du aurreko exekuzioan zegoenaren jarraipena eginez

### Hash fitxategiak

- **Markdown**: iturburu-fitxategiaren ondoan
- **Fallback**: kokaleku nagusia irakurtzeko soilik bada
- **Purpose**: iturburu-aldaketak detektatzen ditu, beharrezkoak ez diren itzulpenak saihesteko

### Itzulpen metadatuak

- **Markdown**:
- **Edukia**:
  - Iturburuko edukia hash
- Hizkuntza-blokearen egoera ( boolearrak)
- Azken eguneraketa-ordua
- **Purpose**: huts egindako blokeen eraldaketa partziala gaitzen du

### Biltegia

- **Fitxategia**:
- **Edukia**: Leku-markaren balio-bikoteen gakoen hiztegia
- **Purpose**: Aplikazio osoan izendatutako leku-markaren balio lehenetsiak ematen ditu

## Seinale-informazioa

### Argitaratzailearen abstrakzioa

signalR espezifikoen itzulpen zerbitzuak deskodetzen ditu:

```csharp
public interface ISignalRPublisher
{
    Task PublishStageAsync<T>(Guid runId, ProcessStage stage, T data, ...);
    Task PublishMessageAsync(Guid runId, LocalizationMessageType type, ProcessStage stage, ...);
}
```

### Sekuentzia-bermeak

- Exekuzio bakarreko mezuak monotonikoki sekuentziatzen dira
- Sekuentzia-zenbakiak bakarrak dira
- Bezeroek tarteak detekta ditzakete edo berriro antolatu

### Hub mapaketa

```csharp
app.MapHub<LocalizationHub>("/hubs/localization");
```

## Hedapen-puntuak

### Itzulpen-helburu berria gehitzea

1. Sortu interfaze berria honekin
2. Inplementatu interfazea domeinuaren berariazko logikarekin
3. Erregistroa DI edukiontzian
4. Injektatu eraikitzailean
5. Deitu existitzen diren faseen ondoren

### Saiakera-politika pertsonalizatua

Jaramonik ez egin eraikitzailearen parametroei:

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

### Leku-markaren kudeaketa pertsonalizatua

Leku-markaren sintaxia edo biltegiratzea aldatzeko ezarpena:

```csharp
public class CustomPlaceholderService : IPlaceholderService
{
    // Use {{name}} syntax instead of {name}
    // Store in database instead of JSON file
    // Encrypt sensitive placeholder values
}
```

## Konfigurazioa

### appsettings.json

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

### Denbora-tartea

Ezarpena
|---------|---------|--------|
80
10
3
30

## Proba-estrategia

### Unitate probak

Azpizerbitzu bakoitza independenteki egiazta daiteke:

- Arrakasta/failure simulatzea
- Mock txostena egiaztatzeko
- Erabili aldi baterako direktorioak I/ O
- Egiaztatu hizkuntza bakoitzeko portaera

### Integrazio-probak

- Kanalizazio osoa benetako (lokala) LibreTranslate instantziarekin
- Ziurtatu SignalR mezuak konektatutako bezeroei ematen zaizkiela
- Probako exekuzio-aurrebista (semaforoa)
- Balidatu Markdown egitura itzulpenaren ondoren

### Amaierako probak

- Trigger itzulpena API edo antolatzailearen bidez
- Egiaztatu helburuko hizkuntza-fitxategi guztiak sortu edo eguneratu direla
- Egiaztatu metadatuen fitxategiek bloke-egoera zuzena dutela
- Berretsi leku-markak itzulpenen bidez gordetzen direla

## Errendimendu-neurriak

- **Memoria**: Hizkuntza bakoitzeko aurrezkiak memoriako hiztegi guztiak gordetzea eragozten du
- **Disk I/O**: Metadatuen fitxategiek buru txiki bat gehitzen dute, baina lan gehikuntzazkoa gaitzen dute
- **Sarea**: Prozesamendu sekuentziala, trotling-arekin, LibreTranslate izugarria saihesten du
- **CPU**: SHA-256 hashing eta berrgex balidazioa azkarrak dira itzulpenen latentziari dagokionez
- **SignalR**: Mezu arinak, ez da ordain-konpresiorik behar ohiko txostenetarako

## Diseinu monolitikoaren migrazioa

Jatorrizkoak logika osoa zuen klase batean. Migrazioaren bide-izena:

1. Erauzi herrialdearen logika
2. Erauzi JSON logika →
3. Erauzi Markdown logika →
4. Erauzi SignalR argitalpena →
5. Erauzi saiakeraren logika →
6. Orkestra-zuzendaria delegaziora soildu

Existitzen diren interfaze guztiak () ez dira aldatzen. Kanaleko kontsumitzaileek ez dute aldaketarik ikusten.
