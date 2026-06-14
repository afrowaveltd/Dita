# Achoimre ar Athruithe ar an tSeirbhís Aistriúcháin Uathoibríoch

## Amharc ar gach eolas

Déanann an doiciméad seo achoimre ar na hathruithe go léir a rinneadh ar an tseirbhís aistriúcháin uathoibríoch Dita, lena n-áirítear athfhachtóir ailtireachta, gnéithe nua, feabhsuithe inbhraiteachta, agus feabhsuithe logánaithe.

## Ailtireacht Athruithe

### Seirbhísí Athsholáthraithe

Rinneadh an monailiteach a dhianscaoileadh i gceithre sheirbhís speisialaithe arna gcomhordú ag ceoltóir éadrom:

- **Seirbhís Aistrithe Teorann ** — Ceolfhoireann na Píblíne (bailíochtú seachtrach, toscaireacht stáitse, láimhseáil earráide)
- ** Seirbhís Aistrithe ** - Sioncrónú ainm Tír (Béarla → sprioctheanga)
- ** Seirbhís Aistrithe Íoslaghdaithe ** - Sioncrónú Foclóra JSON (eochairfhocail bhreise / aistrithe)
- **DocumentsTranslationService** — tiontú doiciméadú Markdown le rianú leibhéal bloc
- **SignalRPublisher ** - Tuairisciú chun cinn fíor-ama trí SignalR
- **AistriúchánRetryService** — Atriail ar leibhéal na Céime le caomhnú na sealbhóirí áite

### An bhfuil a fhios agat na buntáistí a bhaineann..

- ** Imní a réiteach**: Láimhseálann gach seirbhís fearann aistriúcháin amháin
- **Maintainability**: Smaller classes are easier to understand and test
- **Extensibility **: Is féidir spriocanna aistriúcháin nua a chur leis trí chur i bhfeidhm comhéadan
- **Dliteanas **: Soláthraíonn seirbhísí neamhspleácha aonrú lochtanna níos fearr

## Gnéithe Nua

### Monatóireacht a dhéanamh ar Aistriúcháin Beo

** áit **:

Leathanach admin nua a sholáthraíonn infheictheacht fíor-ama isteach sa phíblíne aistriúcháin:

- Taispeáin gach imeacht SignalR mar a tharlaíonn siad
- Cineálacha teachtaireachta dath-chódaithe (gorm = tús, glas = iomlán, dearg = error)
- Banner stádas Ceangal le auto-reconnect
- Teachtaireacht gcuntar agus onnmhairiú go dtí JSON

### Gearáin agus Cur i bhFeidhm

Tacaíonn an córas logánaithe anois le sealbhóirí áite ainmnithe () le haghaidh gramadaí feabhsaithe i dteangacha éagsúla:

```csharp
// Usage in code
var message = Localizer["WelcomeMessage", new Dictionary<string, string>
{
    ["userName"] = "John",
    ["count"] = "5"
}];
// Result: "Hello John, you have 5 new messages"
```

Gnéithe:
- Luachanna sealbhóirí áite a chuirtear ar fáil ag runtime nó a stóráiltear i
- Maisiú uathoibríoch / athchóiriú le linn an aistriúcháin chun éilliú a chosc
- Ar ais ag luí leis na sealbhóirí láithreacha suímh atá ann cheana

### Aistriúcháin Incrimintigh

Comhaid Markdown aistrithe incriminteach:

- **Sábháil teanga **: Tá gach sprioc teanga a shábháil díreach tar éis an aistriúcháin, ag laghdú brú cuimhne
- **Block-leibhéal rianú **: rianta stádas aistriúcháin in aghaidh an bhloc
- ** Fiosrúchán roghnach **: Níl ach bloic theip ath-aistrithe ar an chéad reáchtáil eile
- ** Fanacht sonraí **: Atosaigh an t-iarratas a mhaireann stát aistriúcháin

### Amharc ar gach eolas

Trí leibhéal athléimneachta:

1. ** HTTP retry ** (LibreTranslateService): 5 iarrachtaí le backoff exponential (1s–5s)
2. ** Stóráil ** (Seirbhís Aistrithe): 3 iarrachtaí breise le 30 moill
3. **Glasáil ** (Seirbhís Aistrithe Doiciméid): Failed Markdown blocks retried ar an chéad reáchtáil eile

### Tuairisciú SignalR

Tuairisciú dul chun cinn fíor-ama do gach oibríocht píblíne:

- Foilsíonn gach céim imeachtaí
- Dul chun cinn teanga foilsithe mar imeachtaí
- I measc na n-imeachtaí Earráid comhthéacs mionsonraithe (foinse, cód earráide, teachtaireacht)
- Uimhreacha Seicheamh ráthaíocht ordú laistigh de gach reáchtáil

## Athruithe Cumraíochta

### appsettings.json

Uimh athruithe a bhriseadh. Leanann an chumraíocht atá ann cheana ag obair:

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

### Irl - Library Service

Cláraithe i:

- / m
- `TranslationRetryService`
- / m
- / m
- / m
- / m

Tá an mol SignalR mapáilte ag do naisc chliaint.

## Tástáil

### Stádas tástála

- **243/244 tástálacha ag dul ar aghaidh ** (1 scipeáil mar gheall ar rochtain chomhthráthach comhad i dtimpeallacht tástála)
- Chuir New test coverage leis:
  - Feidhmiúlacht PlaceholderService
  - Ceolfhoireann na Seirbhíse Aistrithe
  - JsonStringLocalizer placeholder innéacsanna

### Teorainneacha a Fhiosrú

- tá tástáil skipped nuair a reáchtáil i comhthreomhar mar gheall ar roinnt cásanna tástála il an comhad céanna. Gabhann sé nuair a reáchtáil ina n-aonar.

## Struchtúr an Chomhaid

### Seirbhísí irl

- —Ceolaire píoblíne
- — aistriúchán ainm Tíre
- - Sioncrónú foclóir JSON
- - Aistriúchán Markdown
- - Foilsiú teachtaireacht SignalR
- - loighic Retry le masc sealbhóir áite
- — Comhéadan foilsitheoir
- — Comhéadan seirbhíse Tír
- - Comhéadan seirbhíse áitiúil
- — Comhéadan seirbhíse doiciméad
- - Comhéadan orchestrator (suas)
- — meiteashonraí aistriúcháin Per-file

### Seirbhísí Nuashonraithe i

- — Tacaíocht do shealbhóirí áite ainmnithe Added
- - Nuashonraithe le haghaidh paraiméadar nua
- — Bainistíocht sealbhóirí áite ainmnithe
- — Comhéadan páirtithe leasmhara

### Nua Riarachán Leathanach i

- - Leathanach monatóireachta fíor-ama
- - Múnla leathanach

### Doiciméadú nua i

- - Doiciméadú píblíne nuashonraithe
- — Treoir maidir le córas páirtithe leasmhara
- – Treoir úsáide Painéal na nIonstraimí
- — Forbhreathnú ar ailtireacht theicniúil

## Comhoiriúnacht ar ais

Tá gach athrú breiseán:

- Cód logánaithe atá ann cheana () oibreacha gan athrú
- Formáidiú Postal () oibreacha gan athrú
- Níl aon athrú ar fhormáid fhoclóra JSON atá ann cheana
- Níl aon athrú ar struchtúr reatha Markdown
- Úsáideann teachtaireachtaí SignalR an fhormáid chéanna

## Imirce Conair

Níl aon imirce ag teastáil. Tá an t-athfhachtóir inmheánach:

1. Caomhnaíodh Sean mar thagairt agus ansin in ionad
2. Rinneadh clárú DI a nuashonrú chun comhéadan nua a úsáid
3. Gach tomhaltóirí atá ann cheana a fheiceáil aon athruithe

## Amharc ar gach eolas

- ** Úsáid chuimhne laghdaithe **: Comhaid shábháil in aghaidh an-teanga láithreach in ionad a shealbhú go léir i gcuimhne
- ** Ritheann incriminteach tubaiste **: Níl ach bloic athraithe / tolgtha Markdown ath-aistrithe
- ** infheictheacht níos fearr **: Cuidíonn dul chun cinn fíor-ama céimeanna mall a dhiagnose

## Feabhsúcháin sa Todhchaí

Feabhsuithe pleanáilte:

1. **AI fine-tuning** — Post-machine translation review for phrases > 5 words
2. **Admin fíordheimhnithe ** - Leathanaigh admin srianta d'úsáideoirí údaraithe
3. ** Eagarthóir grafach ** - Chomhéadain Gréasáin chun eochracha logánaithe a bhainistiú
4. ** Staidreamh aistriúcháin ** — Cairteanna a léiríonn comhaireamh aistriúcháin agus rátaí earráide le himeacht ama
5. **Fiontar na sealbhóirí áite Saincheaptha ** - Tacaíocht le haghaidh formáidí malartacha do shealbhóirí áite

## Déan teagmháil linn

Le haghaidh ceisteanna nó saincheisteanna leis an tseirbhís aistriúcháin, féach ar na doiciméid mhionsonraithe i ngach modúl eolaire nó déan teagmháil leis an bhfoireann forbartha.
