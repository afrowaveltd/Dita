# Cairtchlár an Aistriúcháin Beo

Is é an Dashboard Aistriúcháin Beo leathanach admin a sholáthraíonn fíor-ama infheictheacht isteach sa phíblíne aistriúcháin uathoibríoch. Ceanglaíonn sé leis an mol SignalR agus taispeánann sé gach imeacht píblíne mar a tharlaíonn siad.

## URL

`/Admin/LiveTranslation`

> Note: Authentication and authorization are not yet implemented. Future versions will restrict this page to admin users only.

## Gnéithe

### Sruth imeacht fíor-ama

Gach imeachtaí SignalR ón bpíblíne aistriúcháin ar taispeáint i tábla beo-suas:

- ** Uimhir chosanta ** — Fritháireamh Monotonic laistigh de gach píblíne a reáchtáil
- ** Amstampas ** — Am áitiúil nuair a fuarthas an ócáid
- **RÁD ID RÉIGIÚN ** — RÁTHAITHE Giorraithe le haghaidh comhghaoil
- **Stage** — suaitheantas stáitse Pipeline (CheckServers, TranslateCountries, etc.)
- ** Tiomáint ** — suaitheantas cineáil Teachtaireachta (Státáilte, Dul Chun Cinn, Céimnithe, etc.)
- **Message** – Cur síos ar an duine inléite
- ** Mionsonraí ** — Íoc iomlán JSON na sonraí ócáide

### Códú dath

Dath
|-------|---------|
Gorm ()
glas ()
Dearg ()
Bán (réamhshocrú)

### Stádas Ceangal

A banner stádas ag na seónna barr:
- **Connecting** – Nasc Comharthaíochta a bhunú
- **Connected** – Imeachtaí a fháil de ghnáth
- ** Nascadh ** – Ceangal a cailleadh, iarracht a athcheangal
- **Disconnected** — Ceangal dúnta

Úsáideann an nasc athcheangal uathoibríoch le backoff exponential: 0s, 2s, 5s, 10s, 30s.

### Rialúcháin

- ** Clé Feed ** - Bain gach teachtaireacht ar taispeáint agus athshocrú an gcuntar
- **Easpórtáil JSON ** — Íoslódálacha gach teachtaireacht a fuarthas mar chomhad JSON le haghaidh anailíse
- **Message counter** — Taispeáin líon iomlán na n-imeachtaí a fhaightear sa seisiún seo

## Mol SignalR

Nascann an Painéal na nIonstraimí le:

```javascript
const connection = new signalr.HubConnectionBuilder()
    .withUrl("/hubs/localization")
    .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
    .build();
```

### Nuacht agus Imeachtaí

```typescript
interface LocalizationHubMessage {
    runId: string;        // Guid
    sequence: number;     // long
    type: LocalizationMessageType;
    stage: ProcessStage;
    timestampUtc: string; // ISO 8601
    isError: boolean;
    message: string;
    data: object | null;
}
```

### Cineálacha imeacht

Láimhseálann an Painéal na nIonstraimí gach luachanna:

Cineál
|------|---------|
Suaitheantas Gorm
Suaitheantas glas
Suaitheantas dearg
Suaitheantas glas
Suaitheantas dearg
Info suaitheantas
Suaitheantas rabhaidh

## Cur chun feidhme teicniúil

### Amharc ar gach eolas

- ** ÍoslaghdúHub** () - Mol SignalR a chraolann teachtaireachtaí chuig gach cliant ceangailte
- **ISignalRPublisher ** - Abstraction thar an mol lena n-úsáid i seirbhísí aistriúcháin
- **SignalRPublisher ** - Réamhshocrú chur i bhfeidhm go incrimintí seicheamh monotonic agus craoltaí

### An tIarthar

- Pure HTML / JS le Buataisítrap 5 styling
- Úsáidí leabharlann cliant Microsoft SignalR JavaScript (luchtaithe ó CDN)
- Níl aon rindreáil freastalaí-taobh ag teastáil le haghaidh beatha imeacht

### Struchtúr an leathanaigh

```
Dita.Server/Pages/Admin/
├── LiveTranslation.cshtml      — Razor page markup + JavaScript
└── LiveTranslation.cshtml.cs     — Page model (empty, data comes via SignalR)
```

## Úsáid le linn forbartha

1. Tosaigh an Dita. Iarratas Freastalaí
2. Navigate chun
3. Trigger reáchtáil aistriúcháin (fanacht ar an sceidealóir nó glaoch ar an API)
4. Féach ar imeachtaí le feiceáil i bhfíor-am
5. Bain úsáid as an gcnaipe Onnmhairithe a ghabháil rian iomlán le haghaidh debugging

## Feabhsuithe sa todhchaí

Feabhsuithe pleanáilte don Painéal na nIonstraimí:

- ** Éileamh ** — Rochtain ar úsáideoirí a bhfuil ról acu
- ** Dúshlán ** — Imeachtaí Scagaire de réir céim, cineál, nó ID reáchtáil
- ** Ritheann hormónach ** — Ritheann a fheiceáil ó bhunachar sonraí nó logchomhad
- **Statistics** — Cairteanna a léiríonn comhaireamh aistriúcháin, rátaí earráide, agus latency le himeacht ama
- ** triggers láimhe ** - Buttons chun tús a chur de láimh céimeanna píblíne ar leith
- ** Cumraíocht ** - Éist go díreach ón Painéal na nIonstraimí
- **Bainistíocht teanga ** — Teangacha a bhfuil tacaíocht á tabhairt dóibh a fheiceáil agus a eagrú
- ** Réamhamharc dictionary ** - Brabhsáil agus foclóirí logála cuardaigh

## Fabhtcheartú

### Taispeánann Dashboard "Failed chun ceangal"

1. Fíoraigh go bhfuil an freastalaí ag rith agus inrochtana
2. Seiceáil consól bhrabhsálaí do CORS nó earráidí líonra
3. Tá an deimhniú i láthair
4. A chinntiú go bhfuil aon balla dóiteáin blocála naisc WebSocket

### Níl na himeachtaí le feiceáil

1. Seiceáil go oireann an URL mol SignalR idir freastalaí () agus cliant ()
2. Fíoraigh go bhfuil an sceidealóir ar chumas i
3. Féach ar logs freastalaí le haghaidh earráidí píblíne aistriúcháin
4. Seiceáil cluaisín líonra bhrabhsálaí le haghaidh teachtaireachtaí WebSocket

### Teachtaireachtaí atá as ord

Ráthaíonn an réimse ordú laistigh de reáchtáil amháin. Má tá teachtaireachtaí le feiceáil as ord, d'fhéadfadh sé a chur in iúl:
- Ritheann píblíne il forluí (Ní fhéadfadh tarlú mar gheall ar glas semaphore)
- Brabhsálaí saincheisteanna a dhéanamh (taighde an leathanach)
