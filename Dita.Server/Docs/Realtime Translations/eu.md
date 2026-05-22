# Denbora errealeko itzulpenak

Dokumentu hau zuzeneko sarrera gisa dago itzulpen automatikoko kanalizaziorako. Fitxategi honen edozein aldaketa programatutako hurrengo exekuzioko helburuko hizkuntza-fitxategi guztiak berriro eraldatzea eragiten du.

## Arkitekturaren ikuspegi orokorra

Itzulpen-kanalizazioa arkitektura modularrean berregituratu da, lau azpizerbitzu espezializatuz orkestratzaile arin batek koordinatuta:

- **Atzera-Zerbitzua** - Kanalizazio osoa antolatzen du, zerbitzariaren balidazioa kudeatzen du, eta delegatuek azpizerbitzuetan lan egiten dute.
- **Kontrantsola-Zerbitzua** - Herrialdeen izenak sintetizatzen ditu hizkuntza bakoitzeko hiztegietan.
- **LocalizationTranslationService** - JSON hiztegi lehenetsian gakoak detektatu eta helburuko hizkuntzara itzultzen ditu.
- **DokumentuakTranslationService** - Markdown dokumentazio-fitxategiak itzultzen ditu blokeko jarraipen eta metadatuekin.

Azpizerbitzu bakoitzak modu independentean funtzionatzen du eta SignalR-en bidez denbora errealean egiten dela jakinarazten du.

## Zerbitzuak egiten duena

Zerbitzuak programa bat exekutatzen du eta bost etapako kanalizazioa exekutatzen du: zerbitzariaren balidazioa, herrialdearen sinkronizazioa, JSON hiztegiaren sinkronizazioa, Markdown fitxategi-itzulpena eta emaitzak jarraitzen ditu. Fase bakoitzak denbora errealeko aurrerapen-gertaera egituratuak igortzen ditu SignalRen bidez, bezero konektatuek lan egiten duten bitartean jarrai dezaten.

## Kanalizazio-faseak

### 1. fasea - CheckServers

Edozein itzulpen-lan hasi aurretik, zerbitzuak baldintza guztiak betetzen direla egiaztatzen du:

- Konfigurazio-atalak presentzia eta balioa izan behar du.
- LibreTranslate zerbitzariak erantzun egin behar du latentzi onargarri batean.
- Itzulpen-zerbitzarian erabilgarri dauden hizkuntzen zerrenda eskuratzen da.
- Konfiguratutako hizkuntza lehenetsia zerrenda horretan agertu behar da.
- Onartutako hizkuntza baterako lokaleko JSON fitxategiak automatikoki sortzen dira.

Kontrolak huts egiten badu, kanalizazioa berehala geldituko da eta mezu bat igorriko da.

### 2. etapa - TranslateCountries

Herrialde-izenak sinkronizatuta mantentzen dira irakurtzeko soilik den katalogo batetik () JSON hiztegiak lokalizatzeko.

- Aplikazioaren hizkuntza lehenetsia ingelesa bada, herrialde-izen bakoitza itzulpenik gabe gordeko da.
- Lehenetsitako hizkuntza beste edozein hizkuntza bada, ingeles izena hizkuntza horretara itzultzen da lehenik, eta emaitza hiztegi lehenetsian sartzen da.
- Hiztegi lehenetsia eguneratu ondoren, helburuko hizkuntza-hiztegi guztietan falta den herrialde-sarrera bakoitza itzuli eta gorde egiten da **berehala hizkuntza bakoitzeko**.
- Itzulitako sarrerak aldaketarik gabe mantentzen dira.
- Itzulpenak huts egiten badu, zerbitzuak hiru aldiz egingo du atzera 30 segundoko atzerapenarekin, hurrengo hizkuntzara joan aurretik.

### 3. fasea - Itzuli JsonFiles

Zerbitzuak uneko lokalizazio-hiztegia konparatzen du aurreko exekuziotik gordetako argazki batekin:

- **Gehitutako gakoak** - uneko lehenetsiko sarrerak, baina ez pantailatik- gako horretarako eskulibururik ez duen helburu-hizkuntza guztietara itzultzen dira.
- **Kendutako gakoak** - snapshot-ean dauden sarrerak, baina uneko lehenetsitik ez daudenak, helburuko hizkuntza-hiztegi guztietatik ezabatzen dira.
- Eskuzko itzulpenek lehentasuna dute beti. Helburuko hiztegi batek gako baten balioa badu, sarrera hori aldatu gabe geratuko da, iturburuak dioena kontuan hartu gabe.
- ** Helburuko hizkuntza-hiztegia berehala gordetzen da itzulpenak burutu ondoren**, hizkuntza guztiak amaitu arte itxaron beharrean.
- Itzulpen batek hizkuntza jakin batean huts egiten badu, zerbitzua automatikoki aldatuko da. Soilik errore iraunkorrek (adibidez, onartzen ez den hizkuntza) hizkuntza hori saihesten dute.
- Exekutatu ondoren, uneko hiztegi lehenetsia gordetzen da hurrengo konparaziorako argazki berri gisa.

Hiztegi guztiak alfabetikoki ordenatutako gakoekin gordetzen dira beti, eta JSON koskatuarekin giza irakurgarritasuna lortzeko.

### Stage 4 - TranslateMarkdownFiles

Zerbitzua konfiguratutako dokumentazioaren erroetan ibiltzen da (lehenetsia:) eta iturburu-fitxategi guztiak errekurtsiboki prozesatzen ditu:

1. Iturburuko fitxategien edukia irakurria dago eta SHA-256 hash bat kalkulatzen da.
2. A `.translation-meta.json` file next to the source tracks per-language, per-block translation status, enabling **incremental re-translation** of only failed blocks.
3. Gordetako hash-a aurreko exekuziotik (iturburu-fitxategiaren ondoko fitxategi batean edo aldi baterako atzerapenaren kokaleku batean) uneko hasharekin konparatzen da.
4. Helburuko hizkuntza bakoitzeko, dagokion fitxategia egiturazko osotasuna ere egiaztatzen da.
5. Helburuko edozein fitxategi falta bada, hash zaharkitua du, egituraren balidazioa huts egiten du, edo itzuli gabeko blokeak ditu berriro itzultzeko ilaran.
6. **Each target language is translated and saved independently** — if Czech succeeds but French fails, the Czech file is still written to disk.
7. Ondo itzulitako fitxategiak iturburuarekin parekotasun estrukturalerako balioztatuta daude (berdintze-izenak, zerrenda-elementuak, kode-blokeak, aipuak, estekak, markatzaile lodiak eta HTML etiketak) diskoan idatzi aurretik.
8. Iturburu baten helburu-fitxategi guztiek arrakasta badute, hash berria iturburuaren ondoan gordetzen da. Iturburuaren ondoan idazteak huts egiten badu (adibidez, irakurtzeko soilik diren hedapenetan), hash-a aldi baterako direktoriora itzuliko da.
9. Helburuko itzulpen batek balidazioa huts egiten badu, metadatuek bloke horiek itzuli gabeko gisa markatzen dituzte, hurrengo lasterketetan berriro saiatzeko.

### 5. fasea - StoringResults

Konstelatutako bat bildu eta argitaratu egiten da. Hona hemen:

- UTC exekuzio-hasiera eta osaketa-orduak.
- Gordetako JSON fitxategi lokalen kopurua, Markdown fitxategiak gorde, hash fitxategiak gorde eta hash-ek idazten du.
- Biltegian bildutako edozein errore.
- Hizkuntza bakoitzeko estatistikak (itzulitako kopurua, saltatutako kopurua, errore kopurua).

## SignalR mezuaren gutun-azala

Progresio-gertaera oro honako eremu hauekin banatzen da:

Eremua
|-------|------|-------------|
Uneko kanalizazioaren identifikatzaile korrelatiboa
Monotonic-a, 1ean hasita
Mezuaren semantika mota
Kanalizazio-fasea mezuarena da
UTC ordua mezua igorri zenean
Mezuak errore-egoera adierazten duen ala ez
Giza laburpen irakurgarria
Eszenen araberako karga (erreportatu objektua edo null)

### Mezu motak

Balioa
|-------|------|---------|
0
1
2
3
4
5
6

### Kanalizazio-faseak

Balioa
|-------|------|-------------|
0
1
2
3
4
5

### Mezu arrunten fluxua

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

Eszenaren batek huts egiten badu, geratzen diren faseak saltatu egiten dira, mezu bat igortzen da, eta, azkenik, mezu batek eten egiten du.

## Itzulpenaren logika

Kanalizazioak bi erresilientzia maila ezartzen ditu:

### Goi-mailako saiakera (TranslationRetryService)

- Itzulpen-eskaerak huts egiten badu LibreTranslateren barne-erretratuen ondoren, 30 segundoko atzerapena duten 3 erretret gehiago egingo dira.
- Leku-markaren maskara: testuko leku-marka izendunak () aldi baterako token seguruekin ordezten dira itzulpenaren aurretik, eta ondoren berreskuratu egiten dira, helburuko hizkuntzetan gramatika zuzena bermatuz.

### Hizkuntzaren balioztatzea

- Helburuko hizkuntza batera itzuli aurretik, itzulpen zerbitzariak hizkuntza egiaztatzen du.
- Onartu gabeko hizkuntzak abisu batekin saihesten dira, huts egindako saiakera errepikatuak saihestuz.

### Markdown bloke mailaren saiakera

- Markdown itzulpenak blokez bloke egiten dira (goiburuak, paragrafoak, zerrenda-elementuak).
- Bloke indibidual batek itzulpena huts egiten badu, metadatuen fitxategian itzuli gabe bezala markatuko da eta hurrengo kanalizazioan berriro agertuko da.
- Zerbitzuaren pistak hizkuntza bakoitzeko, blokeko egoera iturburu bakoitzaren ondoko fitxategietan Markdown.

## Errore-kodeak

Erroreen berri ematen da barrutietan banatutako enum bateratua erabiliz:

Barrutia
|-------|----------|
1000-1999
2000-2999
3000-3999
4000-4999
5000-5999

Txosten bateko errore bakoitzak iturburu-identifikatzailea (hizkuntza-kodea, fitxategi-bidea edo izen eszenikoa), errore-kodea eta gizakientzako irakurgarria den mezua ditu.

## zuzeneko itzulpena

Zerbitzariaren proiektuak admin orri bat dauka SignalR gunearekin konektatzen dena eta kanalizazio-gertaera guztiak denbora errealean bistaratzen dituena.

- Konexio-egoera, mezu-kopurua eta gertaera guztien taula eguneratua bistaratzen ditu.
- Kolorez kodetutako errenkadak: urdina agertokiaren hasierarako, berdea osaketarako, gorria erroreetarako.
- Euskarria garbitzen eta mezu guztiak JSONera esportatzen laguntzen du.
- Auto-konektatzen da atzeraldi esponentzialarekin konexioa jaisten bada.

## Diseinu-printzipioak

- **Modularitatea**: itzulpenen kezka bakoitza bere zerbitzura isolaturik dago, mantengarritasuna eta probagarritasuna lortzeko.
- **Erresistentzia handia**: Hiztegiak eta Markdown fitxategiak hizkuntzako gordetzen dira itzulpenaren ondoren, memoriaren presioa murriztuz eta aurreko iritzia emanez.
- **Erresilientzia**: Erresilientzia maila anitzek (HTTP, agertokia, blokea) bermatzen dute zeharkako hutsegiteek ez dutela kanalizazioa blokeatzen.
- **Estatuaren jarraipena**: datu-fitxategien metadatuak () eta hash fitxategiek ondorengo eragiketen gehikuntza-lan zehatza ahalbidetzen dute.
- ** Denbora errealeko ikusgaitasuna**: Eragiketa esanguratsu oro SignalR bidez jakinarazten da monitorizazio eta arazketarako.
- **Eskuzko itzulpenek lehentasuna dute beti gehiketa automatikoen aldean.**
