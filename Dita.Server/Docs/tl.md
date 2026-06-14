# Sumaryo ng mga Pagbabago sa Automatikong Paglilingkod sa Pagsasalin

## ipaliwanag

Binubuod ng dokumentong ito ang lahat ng mga pagbabagong ginawa sa Dita automatic translation service, kabilang ang arkitekturang muling paggawa, mga bagong katangian, mga pagpapabuti na observable, at mga pagpapainam ng lokalisasyon.

## Mga Pagbabago sa Arkitektura

### refactored backend transaksyonervice

Ang monolito ay nabulok sa apat na pantanging serbisyo na pinagtutugma ng isang magaan na orkestra:

- **BackendTranslationService** — Pipeline orkestrator (seryeng may bisa, delegasyon sa entablado, pangangasiwa sa pagkakamali)
- ** CountriesTranslationService** — Pangalan ng Bansa na coordination (Ingles → target na wika)
- **LocalizationTranslationService** — Diksyunaryo ng JSON na nagdurugtong (dagdag/removed keys)
- **DocumentsTranslationService** — Markdown dokumentasyon na may block-level tracking
- **SignalRPublisher** — Real-time na pagsulong na nag-uulat sa pamamagitan ng SignalR
- **TranslationRestryService** — Stage-level retry na may placeholder preserve

### Mga Pakinabang

- ** Paghahati ng mga alalahanin**: Bawat serbisyo ay nangangasiwa sa isang lugar na may iisang salin
- ** Nakakapanatili**: Mas madaling unawain at subukin ang mas maliliit na klase
- **Extensibilidad**: Maaaring idagdag ang mga bagong puntirya ng pagsasalin sa pamamagitan ng pagpapatupad sa anyo
- **Reliability**: Independent services provide better fault isolation

## Bagong mga Katangian

### Buháy na Monitor sa Pagsasalin

**Location**: `/Admin/LiveTranslation`

Isang bagong admin page na nagbibigay ng real-time na imahe sa transaksyon:

- Ipinakikita ang lahat ng mga pangyayaring SignalR habang nagaganap ang mga ito
- Kulay-coded na mga uri ng mensahe (blue=stated, green=fulled, red=error)
- Pag-uugnay ng status baner sa auto-renect
- Kontra ng Mensahe at iniluluwas sa JSON

### Ipinangalan na mga May - ari ng Lugar

Ang sistema ng lokalisasyon ay sumusuporta ngayon sa mga pinangalanang placeholder () para sa pinahusay na balarila sa iba't ibang wika:

```csharp
// Usage in code
var message = Localizer["WelcomeMessage", new Dictionary<string, string>
{
    ["userName"] = "John",
    ["count"] = "5"
}];
// Result: "Hello John, you have 5 new messages"
```

Mga Katangian:
- Ilagay ang mga pamantayang ibinibigay sa panahon ng pagtakbo o pag - iimbak
- Awtomatikong maskara/restorasyon sa panahon ng pagsasalin upang maiwasan ang korupsiyon
- Ang pabalik ay kasuwato ng umiiral na mga humahawak ng puwesto

### Impluwensyang Salin

Ang mga Markdown file ay isinalin nang inkremental:

- **Per-wikang nagliligtas**: Ang bawat puntiryang wika ay natitipid karaka - raka pagkatapos ng pagsasalin, binabawasan ang presyon ng memorya
- **Block-level tracking**: `.translation-meta.json` tracks translation status per block
- **Selective retry**: Mga bigong block lamang ang muling isinalin sa susunod na run
- **Metadata persistence**: Translation state survives application restarts

### Nakakulong Lohika

Tatlong antas ng pakikibagay:

1. **HTTP retry** (LibreTranslateService): 5 pagtatangka na may exponential backoff (1s–5s)
2. **Stage retry** (TranslationRertryService): 3 karagdagang pagtatangka na may 30s na pagkaantala
3. **Block retry** (DocumentsTranslationService): Sawi Markdown blocks retrick run

### Pag - uulat ng Tanda

Real-time na pag-uulat para sa lahat ng mga operasyon ng pipeline:

- Ang bawat yugto ay naglalathala ng mga pangyayari
- Per-wikang pagsulong na inilathala bilang mga pangyayari
- Kasama sa mga pagkakamali ang detalyadong konteksto (oras, maling kodigo, mensahe)
- Ang mga numerong panukat ay gumagarantiya ng kaayusan sa bawat pagtakbo

## Mga Pagbabago sa Pag - aayos

### appsettings.json

Walang nasisirang mga pagbabago. Ang umiiral na kaayusan ay patuloy na gumagana:

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

### Bagong mga Serbisyo

Isinunod sa :

- /
- `TranslationRetryService`
- /
- /
- /
- /

Ang sentro ng SignalR ay may mapa para sa mga ugnayan ng kliyente.

## Pagsubok

### Pagsubok sa Kalagayan

- **243/244 na pagsubok na pumasa** (1ffed dahil sa concurrent file access sa test environment)
- Idinagdag pa ang bagong pagsubok:
  - Kakayahang gumawa ng Lugar
  - Ang orkestra ng orkestra na backendTranslation
  - json frylocalizer placeholder indexers

### Alam na mga Kahinaan

- ang pagsubok ay nababawasan kapag tumatakbong magkahanay sapagkat ang maraming halimbawa ng pagsubok ay may iisang salansan. Dumaraan ito kapag tumatakbo nang nakabukod.

## Bagong Eskwela

### Naglilingkod

- — orkestra ng Pipeline
- Bansa na salin ng pangalan
- — Diksiyonaryo ng JSON
- — Saling Markdown
- — Paglalathala ng mensahe ng SignalR
- — Panimulang lohika na may maskarang placeholder
- — pagitan ng mamamahayag
- Ang serbisyo ng bansa
- — Paglilipat ng serbisyo sa lokalisasyon
- — Serbisyo ng dokumento
- — Orkestrator interface (nakaraan)
- — Salin ng Per-file na metadata

### Naunang mga Serbisyo

- — Karagdagang pangalan na placeholder support
- — Isinaayos para sa bagong parameter
- — Ipinangalanang pangangasiwa sa mga humahawak ng lugar
- — Paglalagay ng Lugar sa ibabaw

### Bagong Admin Pahina

- — tunay na-time monitoring page
- — Pahina

### Bagong Dokumentasyon sa

- — Updated pipeline dokument
- — Gabay na gabay sa sistema ng Placeholder
- — Gabay na gabay sa paggamit ng Dashboard
- — Technical architecture

## Pasubaling Bahagi

Lahat ng pagbabago ay may kaugnayan:

- Ang pag - iral ng lokalisasyong code () ay hindi nagbabago
- Ang Positional formatting () ay hindi nagbabago
- Ang umiiral na anyo ng diksyunaryo sa JSON ay hindi nagbabago
- Hindi nagbabago ang istraktura ng Markdown
- Ang mga mensahe ng SignalR ay gumagamit ng iisang format

## Landas ng Pandarayuhan

Hindi na kailangan ang pandarayuhan. Ang muling paggawa ay panloob:

1. Ang luma ay naingatan bilang reperensiya at pagkatapos ay pinapalitan
2. Binago ang mga pagpaparehistro ng DI upang gumamit ng bagong mga interface
3. Lahat ng umiiral na mamimili ay walang nakikitang pagbabago

## Pagsulong sa Pagganap

- **Republikang gamit sa memorya**: Nagtipid ang mga talaksan ng per-wika kaagad sa halip na alalahanin ang lahat
- **Faster incremental runs**: Only changed/failed Markdown blocks are re-translated
- ** Mas Mabuting Tingnan**: Ang real-time na pag-unlad ay tumutulong upang masuri ang mga mabagal na yugto

## Mga Pagbabago sa Hinaharap

Isinaplanong mga pagsulong:

1. **AI pinong-tuning** — Post-machine translation review para sa mga pariralang > 5 salita
2. **Admin realityation** — Magtakda ng mga pahinang admin sa awtorisadong mga gumagamit nito
3. ** Diksiyonaryong editor** — Web UI para sa pangangasiwa ng mga susi sa lokalisasyon
4. **Translation statistic** — Charts na nagpapakita ng halaga ng pagsasalin at maling bilis sa paglipas ng panahon
5. **Custom placeholder Institusyong** — Suporta sa kahaliling mga format ng placeholder

## Makipag - ugnayan

Para sa mga tanong o mga isyu sa pagsasalinwika, pakisuyong tukuyin ang detalyadong dokumentasyon sa bawat directory ng module o makipag-ugnayan sa development team.
