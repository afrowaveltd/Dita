# Real-am aistriúcháin

Tá an doiciméad seo mar ionchur tástála beo don phíblíne aistriúcháin uathoibríoch. Spreagann aon athrú ar an gcomhad seo ath-aistriú de gach comhad teanga sprioc ar an chéad reáchtáil sceidealta eile.

## Forbhreathnú ar ailtireacht

Rinneadh athstruchtúrú ar an bpíblíne aistriúcháin i ailtireacht modúlach le ceithre fho-seirbhísí speisialaithe arna gcomhordú ag ceoltóir lightweight:

- **BackendTranslationService ** - Orchestrates an píblíne ar fad, Láimhseálann bailíochtú freastalaí, agus toscairí obair le fo-seirbhísí.
- **Seirbhís Aistrithe ** - Synchronizes ainmneacha tíre ó isteach i foclóirí.
- ** LocalizationTranslationService** - Ailtirí a leanas / eochracha a bhaint sa réamhshocraithe JSON foclóir agus aistríonn iad i sprioctheangacha.
- **Seirbhís Aistrithe ** - Aistrithe Comhaid doiciméadaithe Markdown le rianú agus meiteashonraí per-block.

Feidhmíonn gach fo-sheirbhís go neamhspleách agus tuairiscíonn dul chun cinn trí SignalR i bhfíor-am.

## Cad a dhéanann an tseirbhís

Ritheann an tseirbhís ar sceideal agus forghníomhaíonn píblíne cúig chéim: bailíochtú freastalaí, sioncrónaithe tír, sioncrónaithe foclóir JSON, aistriúchán comhad Markdown, agus fós na torthaí. Gach céim astaíonn struchtúrtha imeachtaí dul chun cinn fíor-ama thar Signal R ionas gur féidir le cliaint nasctha a leanúint chomh maith le fáltais oibre.

## Céimeanna Pipeline

### Céim 1 - Checkervers

Sula dtosaíonn aon obair aistriúcháin, fíoraíonn an tseirbhís go bhfuil gach réamhchoinníollacha sásta:

- Ní mór don rannóg cumraíochta a bheith i láthair agus bailí.
- Ní mór don fhreastalaí LibreTranslate freagra a thabhairt laistigh de latency inghlactha.
- Tá an liosta de na teangacha ar fáil ar an bhfreastalaí aistriúcháin fetched.
- Ní mór an teanga réamhshocraithe cumraithe a bheith i láthair sa liosta sin.
- Missing comhaid JSON locale le haghaidh aon teanga tacaíocht a cruthaíodh go huathoibríoch.

Má theipeann ar aon seiceáil, stopann an phíblíne láithreach agus scaoileann teachtaireacht.

### Céim 2 – Iasachtaí

Ainmneacha tíre a choimeád i sync ó chatalóg léamh-amháin () isteach sa logánaithe JSON foclóirí.

- Má tá an teanga réamhshocraithe iarratais Béarla, tá gach ainm tír a stóráil mar gan aistriúchán.
- Má tá an teanga réamhshocraithe aon teanga eile, tá ainm na tíre Béarla aistrithe den chéad uair sa teanga sin, agus is é an toradh an iontráil san fhoclóir réamhshocraithe.
- Tar éis an foclóir réamhshocraithe a thabhairt cothrom le dáta, tá gach iontráil tír ar iarraidh i ngach foclóir sprioc aistrithe agus a shábháil ** láithreach in aghaidh na teanga **.
- Tá iontrálacha cheana féin-aistrithe a chaomhnú gan mhodhnú.
- Má theipeann ar aistriúchán, déanann an tseirbhís suas le 3 huaire le moill 30-dara roimh bogadh go dtí an chéad teanga eile.

### Céim 3 - TranslateJsonFiles

Cuireann an tseirbhís an foclóir logánaithe réamhshocraithe reatha i gcomparáid le pictiúr a stóráiltear ón rith roimhe seo:

- ** Eochracha breise ** – iontrálacha atá i láthair sa mhainneachtain reatha ach as láthair ón léargas – a aistriú go dtí gach sprioctheanga nach bhfuil iontráil láimhe cheana féin don eochair.
- **Eochracha aistrithe ** — scriostar iontrálacha atá i láthair sa phictiúr ach as láthair ón mainneachtain reatha - ó gach foclóir sprioctheanga.
- Aistriúcháin Lámhleabhar ghlacadh i gcónaí tosaíocht. Má tá luach ar eochair cheana féin ag an spriocfhoclóir, fágtar an iontráil sin gan athrú beag beann ar an méid a deir an fhoinse.
- ** Tá gach foclóir sprioc teanga a shábháil díreach tar éis a aistriúcháin iomlán **, seachas ag fanacht le gach teanga a chríochnú.
- Má theipeann ar aistriúchán teanga ar leith, déanann an tseirbhís go huathoibríoch. Ach earráidí leanúnacha (m.sh., teanga gan tacaíocht) a chur faoi deara an teanga a bheith skipped.
- Tar éis an reáchtáil, tá an foclóir réamhshocraithe reatha a shábháil mar an léargas nua don chéad chomparáid eile.

Déantar gach foclóir a stóráil i gcónaí le heochracha atá curtha in oiriúint go haibí agus JSON d'inléiteacht an duine.

### Céim 4 - TranslateMarkdownFiles

Siúlóidí an tseirbhís na fréamhacha doiciméadú cumraithe (réamhshocrú:) agus próisis gach comhad foinse athchúrsach:

1. Tá an t-ábhar comhad foinse a léamh agus tá SHA-256 ríomh.
2. Tá comhad in aice leis na rianta foinse in aghaidh na teanga, stádas aistriúcháin per-block, ar chumas ** incriminteach ath-aistriú ** de na bloic theip amháin.
3. An hash stóráilte as an reáchtáil roimhe seo (a choinneáil i gcomhad in aice leis an comhad foinse, nó i suíomh fallback sealadach) i gcomparáid leis an hash reatha.
4. I gcás gach sprioctheanga, déantar an comhad comhfhreagrach a sheiceáil freisin le haghaidh ionracas struchtúrach.
5. Aon sprioc comhad atá ar iarraidh, Tá hash as dáta, theipeann bailíochtú struchtúr, nó tá bloic untranslated scuaine le haghaidh ath-aistriú.
6. ** Tá gach sprioctheanga aistrithe agus a shábháil go neamhspleách ** - má éiríonn na Seice ach teipeann na Fraince, tá an comhad na Seice scríofa go fóill chun diosca.
7. Comhaid aistrithe go rathúil a bhailíochtú le haghaidh parity struchtúrtha leis an bhfoinse ( chomhaireamh ceannteideal comhionann, míreanna liosta, bloic cód, blockquotes, naisc, marcóirí trom / digiteach, agus clibeanna HTML) sula bhfuil siad scríofa ar diosca.
8. Má éiríonn gach comhad sprioc le haghaidh foinse, tá an hash nua a stóráil in aice leis an bhfoinse. Má theipeann ar scríobh in aice leis an bhfoinse (mar shampla in imscarthaí léitheoireachta amháin), tagann an hash ar ais chuig an eolaire sealadach.
9. Má theipeann ar aon aistriúchán sprioc bailíochtú, marcanna na meiteashonraí na bloic mar untranslated mar sin tá siad retried ar an chéad reáchtáil eile.

### Céim 5 - StoringResults

Tá comhdhlúite le chéile agus a fhoilsiú. Cuimsíonn sé:

- UTC reáchtáil tús agus amstamps críochnaithe.
- Líonta de shábháil comhaid JSON locale, shábháil comhaid Markdown, shábháil comhaid hash, agus hash fallback scríobhann.
- Aon earráidí stórála a bailíodh le linn an reáchtáil.
- Staitisticí aistriúcháin in aghaidh na teanga (cruinneamh aistrithe, comhaireamh skipped, comhaireamh earráide).

## Comharthaíocht R clúdach teachtaireacht

Tá gach imeacht dul chun cinn a sheachadadh mar a bhfuil na réimsí seo a leanas:

Réimse beartas Réimse na Réimse
|-------|------|-------------|
Aitheantóir ceartúcháin don phíblíne reatha á reáchtáil
Gcuntar Monotonic laistigh de reáchtáil, ag tosú ag 1
Cineál Semantic an teachtaireacht
Pipeline céim bhaineann an teachtaireacht
Am UTC nuair a astaíodh an teachtaireacht
Cibé an léiríonn an teachtaireacht coinníoll earráide
Achoimre inléite ag an duine
Ráta pá Céim-shonrach (rud a allmhairiú nó neamhní)

### Cineál gas: in airde

Luach
|-------|------|---------|
0
1
2
3
4
5
6)

### Céimeanna Pipeline

Luach
|-------|------|-------------|
0
1
2
3
4
5

### Sreabhadh teachtaireacht tipiciúil

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

Má theipeann ar aon chéim, na céimeanna atá fágtha a skipped, Tá teachtaireacht astaítear, agus ar deireadh dúnann teachtaireacht an rith.

## Aistriúchán loighic retry

Cuireann an phíblíne dhá leibhéal athléimneachta:

### Fiosrúchán ar leibhéal na Céime (Seirbhís Aistrithe)

- Má theipeann ar iarratas aistriúcháin tar éis retries inmheánacha LibreTranslate, déanann an suas le 3 retries leibhéal céim breise le 30-dara moill.
- Maisiú na sealbhóirí poist: Ainmnítear sealbhóirí áite () i dtéacs in ionad go sealadach le comharthaí sábháilte () roimh an aistriúchán agus ar ais ina dhiaidh sin, ag cinntiú gramadaí ceart i sprioctheangacha.

### Bailíochtú teanga

- Sula aistriú chuig sprioctheanga, fíoraíonn an tseirbhís an teanga tacaíocht ag an bhfreastalaí aistriúcháin.
- Teangacha gan tacaíocht a skipped le rabhadh, a chosc iarrachtaí theip arís agus arís eile.

### Fiosrúchán ar leibhéal an bhloc

- Aistriúcháin Markdown a dhéantar bloc-ar-bloc (ceannteidil, míreanna, míreanna liosta).
- Má theipeann ar bloc aonair aistriúcháin, tá sé marcáilte mar untranslated sa chomhad meiteashonraí agus atried ar an rith píblíne seo chugainn.
- Na rianta seirbhíse in aghaidh na teanga, stádas in aghaidh an-bloc i gcomhaid in aice le gach comhad Markdown foinse.

## Cód Earráid

Tuairiscítear Earráidí ag baint úsáide as enum aontaithe grúpáilte i raonta:

Raon feidhme
|-------|----------|
1000-1999
Plean Gníomhaíochta don Oideachas
3000 - 3999
4000 - 4999
Gach náisiúntacht

Déanann gach earráid i dtuarascáil an t-aitheantóir foinse (cód teanga, cosán comhad, nó ainm stáitse), an cód earráide, agus teachtaireacht inléite ag an duine.

## An tSraith Shinsearach

Áirítear ar an tionscadal Freastalaí leathanach admin ag go nascann leis an mol SignalR ag agus taispeántais gach imeachtaí píblíne i bhfíor-am.

- Taispeáin stádas nasc, comhaireamh teachtaireacht, agus tábla beo-suas de gach imeacht.
- Sraitheanna dath-chódaithe: gorm le haghaidh tús stáitse, glas le críochnú, dearg le haghaidh earráidí.
- Tacaíochtaí imréitigh an beatha agus a onnmhairiú gach teachtaireacht chuig JSON.
- Auto-reconnects le backoff exponential má titeann an nasc.

## Prionsabail deartha

- ** Modúlachas **: Tá gach gnólacht aistriúcháin scoite amach ina sheirbhís féin le haghaidh inchothaitheachta agus intleachta.
- ** Leanúnachas incriminteach **: Tá foclóirí agus comhaid Markdown shábháil in aghaidh na teanga díreach tar éis an aistriúcháin, brú cuimhne a laghdú agus aiseolas níos luaithe a sholáthar.
- ** Athléimneacht **: Leibhéil éagsúla retry (HTTP, céim, bloc) a chinntiú nach bhfuil teipeanna transient bloc an píblíne.
- ** rianú Stáit **: Per-file meiteashonraí () agus comhaid hash chumas obair incriminteach beacht ar Ritheann ina dhiaidh sin.
- ** infheictheacht arís agus arís eile**: Tuairiscítear gach oibríocht shuntasach trí SignalR le haghaidh monatóireachta agus dífhabhtaithe.
- ** Tá tús áite i gcónaí ag aistriúcháin láimhe thar breiseanna uathoibríocha. **
