# Mga salin sa Real-time

Ang dokumentong ito ay umiiral bilang isang buháy na test input para sa awtomatikong transaksyon na tubo. Ang anumang pagbabago sa file na ito ay nag-udyok ng re-transcription ng lahat ng target language files sa susunod na naka-iskedyul na run.

## Ipinaliwanag ang Arkitektura

Ang translation pipeline ay ginawang isang modular na arkitektura na may apat na espesyalisadong sub-services na pinagtutugma ng isang magaang orkestrator:

- **BackendTranslationService** — Ipininta ang buong tubo, hawakan ang sertipikasyon, at ang mga delegado ay nagtatrabaho sa mga sub-service.
- **CountriesTranslationService** — Ang mga pangalan ng bansa ay ginagawang per-wikang diksyunaryo.
- **LocalizationTranslationService** — Idinagdag/removed keys ang default JSON diksiyonaryo at isinalin ang mga ito sa mga puntiryang wika.
- **DocumentsTranslationService** — Translates Markdown documentation files with per-block tracking and metadata.

Ang bawat sub-service ay kumikilos ng independiyente at ang mga ulat ay sumusulong sa pamamagitan ng SignalR sa tunay na panahon.

## Kung ano ang ginagawa ng paglilingkod

Ang serbisyo ay tumatakbo sa isang iskedyul at nagpapatupad ng isang limang-stage pipeline: server facturesation, county synchronisation, JSON dictionary synchronisation, Markdown file translation, at pinapanatili ang mga resulta. Ang bawat yugto ay naglalabas ng organisadong real-time na mga pangyayari sa pag-unlad sa SignalR upang ang mga nag-uugnay na kliyente ay maaaring sumunod habang ang mga administrative sa trabaho.

## Mga yugto ng tubo

### Hagdan 1 — Mga Tagapagsiyasat

Bago magsimula ang anumang gawaing pagsasalin, ang serbisyo ay nagpapatunay na lahat ng mga prekondisyon ay nasisiyahan:

- Ang bahaging pagsasaayos ay dapat na naroroon at mabisa.
- Ang LiberTranslate server ay dapat tumugon sa loob ng isang katanggap-tanggap na latency.
- Ang talaan ng mga wikang makukuha sa translation server ay mahirap makuha.
- Ang nakaayos na default language ay dapat na naroroon sa listahang iyon.
- Missing lome JSON files para sa anumang suportadong wika ay awtomatikong nalilikha.

Kapag hindi gumana ang anumang tseke, humihinto agad ang tubo at naglalabas ng mensahe.

### Stage 2 — Mga Komplikasyon sa Pagsasalin

Ang mga pangalan ng bansa ay pinananatiling sabay-sabay mula sa isang read-lamang katalogo () sa lokalisasyong mga diksiyunaryong JSON.

- Kung ang aplikasyong default language ay Ingles, ang bawat pangalan ng lalawigan ay iniimbak na walang salin.
- Kung ang default language ay anumang ibang wika, ang pangalan ng lalawigan sa Ingles ay unang isinasalin sa wikang iyon, at ang resulta ay ang pagpasok sa distribusyon ng default.
- Pagkatapos ng default dictionary ay inaapruba, ang bawat nawawalang country entry sa bawat puntiryang diksiyonaryo ng wika ay isinasalin at natitipid **immediately per language**.
- Ang mga isinalin na entry ay iniingatan nang walang pagbabago.
- Kapag nabigo ang isang salin, ang serbisyo ay nagreresulta ng hanggang 3 beses na may 30-pangalawang pagkaantala bago lumipat sa susunod na wika.

### stage 3 — pagsasalin ngjsonfile

Inihahambing ng serbisyo ang kasalukuyang default lokalisasyon diksiyonaryo sa isang litrato na nakaimbak mula sa naunang pagtakbo:

- ** Ang idinagdag na mga susi** — na nasa kasalukuyang default ngunit wala sa larawan — ay isinalin sa bawat puntiryang wika na walang manual entry para sa susing iyon.
- ** Tinatanggal ang mga susi** — na nasa larawan subalit wala sa kasalukuyang default — ay inaalis sa bawat tinatarget na diksyunaryo sa wika.
- Ang mga salin ng Bibliya ay laging inuuna. Kung ang isang tinatarget na diksiyunaryo ay mayroon nang halaga para sa isang susi, ang pagpasok na iyon ay hindi nagbabago anuman ang sabihin ng pinagmulan.
- ** Bawat tinatarget na diksyunaryo sa wika ay naililigtas karaka - raka pagkatapos ng mga salin nito na kumpleto**, sa halip na hintaying matapos ang lahat ng wika.
- Kapag nabigo ang isang salin para sa isang espesipikong wika, ang serbisyo ay kusang nangyayari. Tanging ang walang lubay na mga pagkakamali (hal., di - mapigil na wika) ang dahilan kung bakit ang wikang iyan ay hindi ginagamit.
- Pagkatapos ng pagtakbo, ang kasalukuyang default dictionary ay natitipid bilang bagong litrato para sa susunod na paghahambing.

Ang lahat ng diksyunaryo ay laging iniimbak sa pamamagitan ng mga susing may pagkakabukud - bukod ayon sa alpabeto at inilalagay ang JSON para mabasa ng tao.

### Hagdan 4 — Translate MarthdownFiles

Ang serbisyo ay naglalakad sa nakaayos na mga ugat ng dokumentasyon (default: ) at proseso ang bawat source file ay muling lumilitaw:

1. Ang nilalamang source file ay binabasa at ang isang SHA-256 hash ay computed.
2. Isang file sa tabi ng source tracks per-gage, per-block translation status, na nagpapangyari sa **incremental re-salinion** ng lamang nabigo blocks.
3. Ang nakaimbak na hash mula sa naunang run (iningatan sa isang file na katabi ng source file, o sa isang pansamantalang fallback heresiya) ay inihahambing sa kasalukuyang hash.
4. Sa bawat puntiryang wika, ang katumbas na talaksan ay sinusuri rin para sa integridad ng istraktura.
5. Ang anumang talaksang target na nawawala, may laos na hash, bigong istrukturang aktwal, o naglalaman ng hindi isinalin na mga bloke ay queued para sa muling pagsasalin.
6. ** Ang bawat puntiryang wika ay isinalin at iningatan nang hiwalay** — kung magtagumpay ang Czech subalit nabigo ang Pranses, ang Czech file ay isinusulat pa rin sa disk.
7. Ang mga matagumpay na isinalin na files ay sertipikado para sa istrukturang parsiyal na parsiyal na may pinagmulan (katumbas na mga halaga, listahan ng mga bagay, code block block block, blockquotes, links, matapang/italic markers, at HTML tags) bago ito isulat sa disk.
8. Kung lahat ng target files para sa isang source ay magtatagumpay, ang bagong hash ay iniimbak sa tabi ng source. Kung ang pagsusulat sa tabi ng pinagkunan ay nabigo (halimbawa sa mga read-lamang mga standingment), ang hash ay bumabagsak pabalik sa temporary directory.
9. Kung ang anumang puntiryang salin ay hindi nagbigay ng bisa, ang metadata ang nagmamarka sa mga blokeng iyon bilang hindi isinalin upang ang mga ito ay ibalik sa susunod na pagtakbo.

### Hagdan 5 — Nakapangingilabot na mga Pangyayari

Isang pinagsama - samang gusali ang tinitipon at inilalathala. Kasali rito ang:

- Ang UTC run start at finishing timestamps.
- Ang mga konde ng naligtas na mga file ng lokale JSON, ay nakapagligtas ng mga Markdown files, nag-tipid ng mga hash file, at ang fallback hash ay nagsulat.
- Anumang pagkakamali sa pag - iimbak na natitipon sa panahon ng pagtakbo.
- Per- language statistics (isinasalin, hindi nabilang, maling bilang).

## " SignalR message envelope "

Bawat pangyayaring kaunlaran ay ipinababatid bilang ganito:

Larangan
|-------|------|-------------|
Ang correlation identifier para sa kasalukuyang tubo ay tumatakbo
Monotonic counter sa loob ng isang pagtakbo, nagsisimula sa 1
Semantikong uri ng mensahe
Pipeline stage ang mensahe
Oras ng UTC nang ang mensahe ay inilabas
Kung baga ang mensahe ay kumakatawan sa isang maling kalagayan
Human-readable buod
Stage-specific payload (report o null)

### Mga uri ng mensahe

Halaga
|-------|------|---------|
0
1
2
3
4
5
6

### Mga yugto ng tubo

Halaga
|-------|------|-------------|
0
1
2
3
4
5

### Karaniwang daloy ng mensahe

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

Kung sakaling mabigo ang anumang yugto, ang natitirang mga yugto ay nahihinto, isang mensahe ang inilalabas, at sa wakas isang mensahe ang nagsasara.

## Panibagong lohika sa pagsasalin

Ang tubo ay may dalawang antas ng tibay:

### Stage-level retry (TranslationRertryService)

- Kung mabigo ang isang kahilingan sa pagsasalin matapos ang mga panloob na retrie ng LibreTranslate, ang mga paglalapat ay umaabot sa 3 karagdagang stage-level retries na may 30-pangalawang pagkaantala.
- Placeholder maskarang: Pinangalanang placeholders () sa teksto ay pansamantalang pinapalitan ng mga ligtas na token () bago isalin at ibalik pagkatapos, tinitiyak ang tamang balarila sa mga puntiryang wika.

### Makatuwirang Wika

- Bago isalin sa isang puntiryang wika, ang service verifies ay sinusuportahan ng translation server.
- Ang di - suportadong mga wika ay nilalampasan ng babala, hinahadlangan ang paulit - ulit na nabigong mga pagsisikap.

### Markdown block-level retry

- Ang mga salin ng Markdown ay isinasagawang block-by-block (headings, parapo, listahan ng mga bagay).
- Kung hindi isalin ang isang indibiduwal na bloke, ito ay minarkahan bilang hindi isinalin sa talaksang metadata at muling gagamitin sa susunod na tubo.
- Ang service tracks per-gage, per-block status sa mga file sa tabi ng bawat source Markdown file.

## Error sa mga code

Ang mga pagkakamali ay iniulat na ginagamit ang isang nagkakaisang enum na nakapangkat sa mga hanay:

Ang Range
|-------|----------|
1000–1999
2000–299
3000–399
4000–499
5000–599

Ang bawat error sa isang ulat ay nagdadala ng source identifier (wikang kodigo, file path, o pangalan ng entablado), ang error code, at isang human-readable na mensahe.

## " Live Translation Dashboard "

Kabilang sa proyektong Server ang isang admin page na nag-uugnay sa sentrong SignalR sa at nagtatanghal ng lahat ng mga pangyayaring tubo sa tunay na panahon.

- Ipinapakita ang katayuang koneksyon, pagbilang ng mensahe, at isang live-upding na mesa ng lahat ng mga pangyayari.
- Kulay-coded na mga hanay: asul para sa pagsisimula ng entablado, berde para sa pagkumpleto, pula para sa mga pagkakamali.
- Mga suporta na nag - aalis ng pagkain at nagluluwas ng lahat ng mensahe kay JSON.
- Auto-renects sa exponential backoff kung ang koneksyon ay bumaba.

## Magdisenyo ng mga simulain

- **Modularity**: Each translation concern is isolated in its own service for maintainability and testability.
- **Incremental persistence**: Dictionaries and Markdown files are saved per-language immediately after translation, reducing memory pressure and providing earlier feedback.
- **Resilience**: Multiple retry levels (HTTP, stage, block) ensure transient failures do not block the pipeline.
- ** Ang pagsubaybay**: Per-file metadata () at hash files ay nagpapangyari ng eksaktong inkremental na gawain sa mga susunod na run.
- ** aktuwal na Tingnan**: Ang bawat mahalagang operasyon ay iniuulat sa pamamagitan ng SignalR para sa pagsubaybay at pag - aalis ng mga sandata.
- **Manual translations always have priority over automatic additions.**
