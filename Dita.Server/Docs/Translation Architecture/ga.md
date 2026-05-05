# Aistriúchán Ailtireacht

Déanann an doiciméad seo cur síos ar ailtireacht modúlach chóras aistriúcháin uathoibríoch Dita, a tugadh isteach chun feabhas a chur ar inchothaitheacht, ar intleachtacht, agus ar athléimneacht.

## Spriocanna deartha

Thug an t-athfhachtóir aghaidh ar roinnt imní leis an dearadh monailiteach bunaidh:

- ** Imní a réiteach**: Tá gach réimse aistriúcháin (contrárthaí, foclóirí JSON, Markdown) scoite amach.
- ** Leanúnachas incriminteach **: Comhaid a shábháil in aghaidh an-teanga díreach tar éis an aistriúcháin, úsáid cuimhne a laghdú agus torthaí níos luaithe a sholáthar.
- **Resilience **: Leibhéil retry Il láimhseáil teipeanna transient gan bac ar an píblíne ar fad.
- ** Inbhraiteacht **: Tuairiscítear gach oibríocht shuntasach trí SignalR le haghaidh monatóireachta fíor-ama.
- **Extensibility **: Is féidir spriocanna aistriúcháin nua a chur leis trí chur i bhfeidhm comhéadan amháin.

## Dianscaoileadh seirbhíse

### SeirbhÃ s um Athlaghdú (orchestrator)

**Responsibilities**:
- Bainistiú saolré Pipeline (tús, críochnú, láimhseáil earráide)
- Rialú concurrency Semaphore-bhunaithe (ritheann forluí)
- Bailíochtú Freastalaí (latency, infhaighteacht teanga, cumraíocht)
- Toscaireacht chun fosheirbhísí

**NÁ bhfuil **:
- Aistriúchán
- Comhad I / O le haghaidh formáidí sonracha
- Amharc ar gach eolas

### Irl - Library Service

**Responsibilities**:
- Léigh an eolaire
- Ainmneacha tíre Synchronize isteach san fhoclóir locale réamhshocraithe
- Trasnaigh ainmneacha tír ar iarraidh in aghaidh na teanga sprioc
- Sábháil gach foclóir sprioc díreach tar éis an aistriúcháin

** Iompar Key **:
- Má tá teanga réamhshocraithe Béarla: ainmneacha tíre a stóráil mar-is
- Má tá teanga réamhshocraithe eile: Ainmneacha Béarla aistrithe chuig teanga réamhshocraithe an chéad
- Tá gach teanga a phróiseáil go neamhspleách lena lúb retry féin

### Seirbhísí Aistrithe

**Responsibilities**:
- Detect breise / eochracha a bhaint trí chomparáid a dhéanamh foclóir réamhshocraithe reatha le pictiúr roimhe seo
- Translate breise eochracha isteach i ngach teanga sprioc
- Bain eochracha a scriosadh as gach teanga sprioc
- Sábháil pictiúr le haghaidh comparáid eile

** Iompar Key **:
- Aistriúcháin Lámhleabhar ghlacadh i gcónaí tosaíocht (never overwritten)
- Eochracha breise a aistriú agus a shábháil in aghaidh na teanga láithreach
- Scriostar eochracha in aghaidh na teanga láithreach
- Snapshot shábháil ach amháin tar éis gach teanga i gcrích go rathúil

### Seirbhísí Aistrithe Doiciméid

**Responsibilities**:
- Siúlóid fréamhacha Markdown cumraithe athchúrsach
- A bhrath athrú comhaid foinse ag baint úsáide as SHA-256 hashes
- Rianú stádas aistriúcháin per-block i
- Translate bloc-by-block le retry per-block
- Struchtúr Markdown Bailí tar éis aistriúcháin
- Sábháil gach comhad teanga sprioc go neamhspleách

** Iompar Key **:
- Granularity bloc-leibhéal: ceannteidil, míreanna, míreanna liosta a aistriú ar leithligh
- Rianta meiteashonraí a d'éirigh le bloic/failed in aghaidh na teanga
- Bloic Failed a retried ar an chéad reáchtáil gan ath-aistriú bloic rathúil
- Cinntíonn bailíochtú struchtúr ceannteideal comhaireamh, liostaí, bloic cód, etc mheaitseáil foinse

## Straitéis Fiosrúcháin

Cuireann an córas retries chun feidhme ag trí leibhéal:

### Leibhéal 1 - HTTP (LibreTranslateService)

- Suas go dtí 5 iarrachtaí le backoff exponential (1s, 2s, 3s, 4s, 5s)
- Láimhseálann timeouts líonra, 5xx earráidí, agus teipeanna transient
- Tógtha isteach sa chumraíocht cliant HTTP

### Leibhéal 2 – Céim (Seirbhís Aistrithe)

- Suas go dtí 3 iarrachtaí le 30-dara moill
- Re-tiomáineann an t-iarratas aistriúcháin ar fad tar éis HTTP-leibhéal retries ídithe
- Cuirtear mascáil agus athchóiriú i bhfeidhm ar an leibhéal seo

### Leibhéal 3 - Bloc (Seirbhís Aistrithe Doiciméid)

- Bloic Markdown Aonair a theipeann marcáilte i meiteashonraí
- Retried go huathoibríoch ar an chéad píblíne eile reáchtáil
- Riamh bloic Rathúil ath-aistrithe

## Sreabhadh sonraí

### JSON aistriúchán foclóir

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

### Aistriúchán Markdown

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

### Ainm Tír aistriúchán

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

## Toradh an Stáit

### grianghraif

- **JSON**: Stóráilte i gcomhad in aice leis an bhfoclóir réamhshocraithe (ní athraíonn ainm an tsoláthraí stórála)
- **Purpose **: Cumasaigh sync incriminteach ag rianú cad a bhí i láthair sa reáchtáil roimhe

### Comhaid Hash

- **Markdown**: in aice leis an gcomhad foinse
- ** Aiseolas **: más rud é go bhfuil an suíomh bunscoile ar siúl
- **Purpose**: Ailtirí athruithe foinse a sheachaint ath-aistriú gan ghá

### Meiteashonraí aistriúcháin

- **Márta **:
- **Conarthaí **:
  - Foinse ábhar hash
- Stádas bloc Per-teanga (Roinn na mbothleans)
- Nuashonrú is déanaí
- **Purpose**: Cumasaigh athaistriú páirteach de bhloic theip amháin

### Stóráil sealbhóirí poist

- **File**: `Locales/placeholders.json`
- **Contents**: Foclóir na heochracha do sealbhóirí ainm-luach péirí
- **Purpose**: Soláthraíonn luachanna réamhshocraithe do sealbhóirí áite ainmnithe ar fud an t-iarratas

## Comharthaíocht Tuairisciú RR

### Déan Teagmháil Linn

decouples seirbhísí aistriúcháin ó shonraí SignalR:

```csharp
public interface ISignalRPublisher
{
    Task PublishStageAsync<T>(Guid runId, ProcessStage stage, T data, ...);
    Task PublishMessageAsync(Guid runId, LocalizationMessageType type, ProcessStage stage, ...);
}
```

### Ráthaíochtaí sábháilteachta

- Tá Teachtaireachtaí laistigh de reáchtáil amháin monotonically seicheamh
- Tá uimhreacha Seicheamh uathúil in aghaidh an-rith trí
- Is féidir le cliaint bearnaí nó athordú a bhrath

### Léarscáiliú Hub

```csharp
app.MapHub<LocalizationHub>("/hubs/localization");
```

## Pointí síneadh

### Ag cur sprioc nua aistriúcháin

1. Cruthaigh comhéadan nua le
2. Cur i bhfeidhm an comhéadan le loighic fearainn-sonrach
3. Cláraigh i gcoimeádán DI
4. Instealladh isteach tógálaí
5. Glaoch ó tar éis céimeanna atá ann cheana

### Beartas maidir le hathchúrsáil

Paraiméadair tógálaí override:

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

### Láimhseáil sealbhóirí áite an Chustaim

Cur i bhfeidhm chun sineirgíocht nó stóráil sealbhóirí áite a athrú:

```csharp
public class CustomPlaceholderService : IPlaceholderService
{
    // Use {{name}} syntax instead of {name}
    // Store in database instead of JSON file
    // Encrypt sensitive placeholder values
}
```

## Cumraíocht

### riachtanais uisce: measartha

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

### Téamh Runtime

Amharc ar gach eolas
|---------|---------|--------|
80 bliain
tréimhse saoil: ilbhliantúil
3
30

## Straitéis Tástáil

### Tástálacha aonaid

Tá gach fo-sheirbhís intástáil go neamhspleách:

- Mock chun insamhail rath / fabraic
- Coileach chun tuairisciú a fhíorú
- Bain úsáid as eolairí sealadacha le haghaidh comhad I/O
- Fíoraigh iompar coigilte in aghaidh na teanga

### Tástálacha comhtháite

- Píblíne iomlán reáchtáil le fíor (áitiúil) LibreTranslate shampla
- Fíoraigh Comharthaíocht R teachtaireachtaí a sheachadadh chuig cliaint ceangailte
- Tástáil a chosc reáchtáil comhthráthach (semaphore)
- Struchtúr Markdown Bailí tar éis aistriúcháin

### Tástálacha deireadh le deireadh

- Aistriúchán trí API nó sceidealóir
- Fíoraigh gach comhad teanga sprioc a cruthaíodh / uasghrádaithe
- Seiceáil comhaid meiteashonraí bhfuil stádas bloc ceart
- Daingniú sealbhóirí áit a chaomhnú ar fud aistriúcháin

## Breithnithe feidhmíochta

- ** Cuimhne **: Cuireann an coigilt teanga cosc ar gach foclóir a choinneáil i gcuimhne
- **Disk I/O**: Comhaid meiteashonraí a chur lastuas beag ach ar chumas obair incriminteach
- **Network **: Coscann próiseáil seicheamhach le throttling LibreTranslate mór
- **CPU**: SHA-256 Tá hashing agus regex bailíochtú tapa i gcoibhneas le latency aistriúcháin
- **SignalR**: teachtaireachtaí éadroma, gan aon chomhbhrú pálasta ag teastáil le haghaidh tuarascálacha tipiciúla

## Imirce ó dhearadh monolithic

An bunaidh go léir loighic i rang amháin. An cosán imirce:

1. Sliocht loighic tír →
2. Sliocht loighic JSON →
3. Sliocht Markdown loighic →
4. Sliocht Comharthaí R foilsiú →
5. Sliocht loighic retry →
6. Ceolfhoireann a shimpliú go toscaireacht amháin

Gach comhéadan atá ann cheana () fós gan athrú. Feiceann tomhaltóirí na píblíne aon athruithe a bhriseadh.
