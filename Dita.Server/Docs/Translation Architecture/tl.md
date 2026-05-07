# Arkitektura sa Pagsasalin

Inilalarawan ng dokumentong ito ang arkitekturang modular ng automatikong sistema ng pagsasalin ni Dita, na ipinakilala upang pagbutihin ang pagpapanatili, pagiging maaasahan, at katatagan.

## Magdisenyo ng mga tunguhin

Binanggit ng muling paggawa ang ilang pagkabahala sa orihinal na disenyong monolito:

- ** Paghahati ng mga alalahanin**: Ang bawat lugar ng pagsasalin (mga bansa, mga diksyunaryo ng JSON, Markdown) ay nakabukod.
- **Incremental persistence**: Files are saved per-language immediately after translation, reducing memory usage and providing earlier results.
- ** Resilience**: Ang maraming antas ng retry ay humahawak ng pansamantalang mga kabiguan nang hindi hinahadlangan ang buong tubo.
- **Hindi maaasahan**: Ang bawat mahalagang operasyon ay iniulat sa pamamagitan ng SignalR para sa real-time monitoring.
- **Extensibilidad**: Ang mga bagong puntirya ng pagsasalin ay maaaring idagdag sa pamamagitan ng pagpapatupad ng isang interface.

## Pagkabigo sa Paglilingkod

### BackendTranslationService (orkestrador)

** Pananagutan**:
- Pipeline lifecycle management (bituin, tapusin, maling paghawak)
- Semaphore-based concordence control (mga prevent na nagsasanib na run)
- Sertipikasyon (latensiya, magagamit na wika, pagsasaayos)
- Delegasyon sa mga sub-service

** WalaNG**:
- Ang lohika sa pagsasalin
- Sawi ang pagbasa ng talaksang I/O para sa espesipikong format
- Maiksing lohika

### Mga Bansa

** Pananagutan**:
- Basahin mula sa directory
- Ilagay ang pangalan ng bansa sa diksiyonaryong default lome
- Isinalin ang nawawalang mga pangalan ng bansa sa bawat target na wika
- Iligtas agad ang bawat target na diksyunaryo pagkatapos ng pagsasalin

**Key asal**:
- Kung ang default language ay Ingles: mga pangalan ng bansa na nakaimbak bilang-is
- Kung ang default language ay iba pa: Mga pangalang Ingles na unang isinalin sa default language
- Ang bawat wika ay sinusuri nang hiwalay sa pamamagitan ng sarili nitong retry loop

### pagsalin sa lokalisasyon

** Pananagutan**:
- Idinagdag pa ni Diagnosis/removed keys sa pamamagitan ng paghahambing ng kasalukuyang default dictionary sa nakaraang speciation
- Isinalin ang idinagdag na mga key sa bawat puntiryang wika
- Alisin ang mga key sa bawat wika
- Mag - ipon ng litrato para sa susunod na paghahambing

**Key asal**:
- Ang mga opisyal na salin ay laging inuuna (hindi kailanman labis ang pagkakasulat)
- Ang mga add key ay isinalin at inipreserba agad-agad
- Tinatanggal agad ang mga natanggal na key
- Naliligtas lamang ang Snapshot matapos na matagumpay na makompleto ang lahat ng wika

### Dokumento ng Dokumento

** Pananagutan**:
- Maglakad - lakad na nakaayos na mga ugat ng Markdown
- Di - sinasadyang binago ang mga source file gamit ang SHA-256 hase
- Tingnan ang per-block translation status sa
- Translate block-by-block na may per-block retry
- Paunlarin ang istrakturang Markdown pagkatapos ng pagsasalin
- Iligtas ang bawat target na talaksan ng wika nang hiwalay

**Key asal**:
- Block-level granularity: mga pamagat, parapo, talaan ng mga bagay ay isinasalin nang hiwalay
- Mga riles ng metadata na hinalinhan ng mga bloke/niliko sa bawat wika
- Ang mga bigong block ay muling ibinibigkas sa susunod na pagtakbo nang hindi muling isinasalin ang matagumpay na mga blocks
- Ang istrukturang aksiyunal ay tumitiyak ng mga paulong aspeto, talaan, mga blokeng kodigo, atbp. source

## Mabigat na estratehiya

Ang mga gamit sa sistema ay muling ginagamit sa tatlong antas:

### antas 1 — http (libre translateservice)

- Hanggang 5 pagtatangka sa exponential backoff (1s, 2s, 3s, 4s, 5s)
- Ang mga network timeout, 5xx error, at pansamantalang mga kabiguan
- Itinayo sa HTTP na kaayusan ng kliyente

### Level 2 — Stage (TranslationRertryService)

- Hanggang 3 pagtatangka na may 30-pangalawang pagkaantala
- Muling-trigger ang buong kahilingan sa pagsasalin matapos maubos ang HTTP-level retries
- Ipinapahid sa antas na ito ang paggamit at pagsasauli ng takip ng lugar

### Level 3 — Bloke (DocumentsTranslationService)

- Ang indibiduwal na mga markdown block na nabigo ay minarkahan sa metadata
- Awtomatiko sa susunod na tubo
- Ang matagumpay na mga bloke ay hindi kailanman isinasalin muli

## Dumadaloy ang Data

### Salin sa diksyunaryo ng JSON

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

### Salin sa wikang Markdown

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

### Salin ng pangalan ng bansa

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

## Pagpupumilit ng Estado

### Mga Snapshot

- **JSON**: Nakaimbak sa isang file na katabi ng default dictionary (ang pangalan ay iba - iba sa pamamagitan ng storage provider)
- **Purpose**: Enables incremental sync by tracking what was present in the previous run

### Mga talaksan ng hash

- ** Markdown**: katabi ng source file
- **Fallback**: kung ang pangunahing lokasyon ay basahin-lamang
- **Purpose**: Detects source changes to avoid unnecessary re-translation

### Metadata ng pagsasalin

- **Markdown**: `{sourceFile}.translation-meta.json`
- ** mga konstente**:
  - Nilalaman
- Per-wika block status (array ng mga boolean)
- Huling timestamp ng update
- **Purpose**: Enables partial re-translation of only failed blocks

### Pag - iimbak ng lugar

- **File**:
- **Contents**: Dictionary of keys to place-holder na pangalan-halagang pares
- **Purpose**: Naglalaan ng default na halaga para sa pinangalanang mga may hawak ng lugar sa ibayo ng aplikasyon

## Tanda Pag - uulat ng R

### Maling Akala

mga serbisyo sa pagsasalin mula sa mga detalye ng SignalR:

```csharp
public interface ISignalRPublisher
{
    Task PublishStageAsync<T>(Guid runId, ProcessStage stage, T data, ...);
    Task PublishMessageAsync(Guid runId, LocalizationMessageType type, ProcessStage stage, ...);
}
```

### Mga garantiya ng Pag - aalinlangan

- Ang mga mensahe sa loob ng iisang pagtakbo ay monotonikal na sunud - sunod
- Ang mga numero ng Sequence ay kakaibang per-run sa pamamagitan ng
- Natutukoy ng mga client ang mga puwang o muling pagsasaayos

### Pag - aayos ng Hub

```csharp
app.MapHub<LocalizationHub>("/hubs/localization");
```

## Mga Extension point

### Pagdaragdag ng bagong target sa pagsasalin

1. Gumawa ng bagong interface kasama ng
2. Itakda ang interface ng domain-specific logic
3. Muling Pag - aasawa sa DaI container
4. Ipasok sa gusali
5. Tumawag pagkatapos ng umiiral na mga yugto

### Kaugaliang patakaran

Forverride constructionor parameter:

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

### Kaugaliang pangangasiwa sa mga humahawak ng lugar

Pagsasaayos na palitan ang placeholder joint o imbakan:

```csharp
public class CustomPlaceholderService : IPlaceholderService
{
    // Use {{name}} syntax instead of {name}
    // Store in database instead of JSON file
    // Encrypt sensitive placeholder values
}
```

## Pagsasaayos

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

### Pag - aayos ng Oras ng Run

Pagtatakda
|---------|---------|--------|
80
10
3
30

## Pamamaraan ng pagsubok

### Mga pagsubok sa Unit

Ang bawat sub-service ay independiyenteng masusubok:

- Sinisikap na gayahin ang tagumpay/failure
- Pakikipagsapalaran na tiyakin ang pag - uulat
- Gumamit ng pansamantalang mga direktoryo para sa talaksang I/O
- Pare-verify per-wika na nagliligtas ng pag-uugali

### Mga pagsubok sa pandarayuhan

- Buong tubo na pinangangasiwaan ng tunay (local) na LibreTranslate
- Panibagong Tanda Ang mga mensahe ng R ay inihahatid sa magkakaugnay na mga kliyente
- Subukin kasabay ng pag - iwas (semaphore)
- Paunlarin ang istrakturang Markdown pagkatapos ng pagsasalin

### Wakas-to-end na mga pagsubok

- Trigger na pagsasalin sa pamamagitan ng API o iskedyul
- Salain ang lahat ng target na language files ay nilikha/updated
- Tingnan ang mga talaksang metadata na naglalaman ng tamang kalagayan ng block
- Ang tapat na mga may - ari ng lugar ay iniingatan sa iba't ibang salin

## Mga Pag - iingat

- **Memory**: Per-language saving prevents holding all dictionaries in memory
- **Disk I/O**: Ang mga talaksang metadata ay nagdaragdag ng maliit sa itaas subalit nagpapangyari sa inkremental na gawain
- **Network**: Ang pagpoproseso ng Sequential na may throtling ay humahadlang sa labis - labis na LibreTranslate
- **CPU**: Ang SHA-256 hashing at regex appearance ay mabilis na nauugnay sa pagsasalin ng latency
- **SingnalR**: Mga mensahe ng magaan na timbang, walang sahod na dala - dala ang compression na kailangan para sa karaniwang mga ulat

## Pandarayuhan mula sa disenyo ng monolito

Ang orihinal ay naglalaman ng lahat ng lohika sa isang klase. Ang landas sa pandarayuhan:

1. Ilabas ang lohika ng lalawigan →
2. Pag - unawa sa lohika →
3. Kumuha ng Markdown logic →
4. Pag - aalis ng Tanda R na naglalathala →
5. Kumuha ng recry logic →
6. Simpleng orkestra sa delegasyon-lamang

Ang lahat ng umiiral na mga interface () ay nananatiling hindi nagbabago. Hindi nakikita ng mga mamimili ng tubo ang mga pagbabago.
