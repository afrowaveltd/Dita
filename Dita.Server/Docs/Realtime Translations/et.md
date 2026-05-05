# Reaalajas tõlked

See dokument on olemas automaatse tõlketorustiku reaalajas testimise sisendina. Selle faili mis tahes muutmine käivitab kõigi sihtkeelefailide uuesti tõlkimise järgmisel planeeritud käivitamisel.

## Arhitektuuri ülevaade

Tõlketorustik on ümber kujundatud modulaarseks arhitektuuriks, millel on neli spetsialiseerunud allteenust, mida koordineerib kergekaaluline orkestraator:

- **BackendTranslationService ** - orkestreerib kogu torujuhet, tegeleb serverite valideerimisega ja delegaadid töötavad alamteenustesse.
- **RiigidTranslationService** — Sünkroonib riikide nimed keelepõhistesse sõnaraamatutesse.
- **LocalizationTranslationService** – tuvastab lisatud/eemaldatud võtmed vaikesõnastikus JSON ja tõlgib need sihtkeeltesse.
- **DocumentsTranslationService** – Tõlgib Markdowni dokumentatsioonifailid plokipõhise jälgimise ja metaandmetega.

Iga alamteenus töötab iseseisvalt ja annab edusammudest teada SignalR-i kaudu reaalajas.

## Mida teenistus teeb

Teenus töötab ajakava järgi ja käivitab viieastmelise torustiku: serveri valideerimine, riigi sünkroniseerimine, JSON sõnastiku sünkroniseerimine, Markdowni failitõlge ja tulemuste püsimine. Iga etapp kiirgab struktureeritud reaalajas edenemissündmusi signaali kaudu R nii, et ühendatud kliendid saaksid töö edenedes kaasa minna.

## Torustiku etapid

### 1. etapp – CheckServers

Enne tõlketöö algust kontrollib teenistus, et kõik eeltingimused on täidetud:

- Konfiguratsiooniosa peab olema olemas ja kehtiv.
- LibreTranslate server peab reageerima vastuvõetava latentsuse piires.
- Tõlkeserveris saadaolevate keelte nimekiri tõmmatakse.
- Selles nimekirjas peab olema seadistatud vaikekeel.
- Iga toetatud keele jaoks puuduvad lokaadi JSON-failid luuakse automaatselt.

Kui kontroll ebaõnnestub, peatub torujuhe kohe ja saadetakse teade.

### 2. etapp – Tõlgitud riigid

Riikide nimesid hoitakse sünkroonis kirjutuskaitstud kataloogist () lokaliseerimissõnastikesse JSON.

- Kui rakenduse vaikimisi keel on inglise keel, salvestatakse iga riigi nimi ilma tõlketa.
- Kui vaikekeel on mõni muu keel, tõlgitakse inglise riiginimi kõigepealt sellesse keelde ja tulemuseks saab vaikesõnastiku kirje.
- Pärast vaikimisi sõnaraamatu uuendamist tõlgitakse ja salvestatakse iga sihtkeele sõnaraamatu puuduv riigikirje ** kohe keele kaupa**.
- Juba tõlgitud kirjed säilitatakse muutmata kujul.
- Kui tõlge ebaõnnestub, proovib teenus kuni 3 korda 30-sekundilise viivitusega enne järgmisesse keelde liikumist.

### 3. etapp – TranslateJsonFiles

Teenus võrdleb aktiivset vaikimisi lokaliseerimissõnaraamatut eelmisest tööst salvestatud hetkepildiga:

- ** Lisatud võtmed ** – kirjed, mis on aktiivses vaikeväärtuses, kuid puuduvad hetkepildist, tõlgitakse igasse sihtkeelde, millel pole veel selle võtme käsitsi kirjet.
- ** Eemaldatavad klahvid ** – hetkepildis esinevad kirjed, mis aga vaikeväärtuses puuduvad, kustutatakse igast sihtkeele sõnaraamatust.
- Manuaalsed tõlked on alati prioriteetsed. Kui sihtsõnastik sisaldab juba võtme väärtust, jäetakse see kirje muutmata olenemata sellest, mida allikas ütleb.
- **Each target language dictionary is saved immediately after its translations complete**, rather than waiting for all languages to finish.
- Kui tõlge ei õnnestu konkreetses keeles, proovib teenus automaatselt uuesti. Ainult püsivad vead (nt toetuseta keel) põhjustavad selle keele vahelejätmise.
- Pärast käivitamist salvestatakse aktiivne vaikesõnastik järgmise võrdluse uue pildina.

Kõik sõnaraamatud on alati salvestatud tähestikuliselt sorteeritud võtmetega ja inimloetavuse jaoks treppitud JSON-iga.

### 4. etapp – TranslateMarkdownFiles

Teenus käitab seadistatud dokumentatsiooni juuri (vaikimisi): ja töötleb iga lähtefaili rekursiivselt:

1. Lähtefaili sisu loetakse ja arvutatakse SHA-256 räsi.
2. A `.translation-meta.json` file next to the source tracks per-language, per-block translation status, enabling **incremental re-translation** of only failed blocks.
3. Eelmisest tööst salvestatud räsi (hoiti lähtefaili kõrval asuvas failis või ajutises varuasukohas) võrreldakse praeguse räsiga.
4. Iga sihtkeele puhul kontrollitakse ka vastava faili struktuurilist terviklikkust.
5. Iga sihtfail, mis puudub, on aegunud räsi, ei suuda struktuuri valideerimist või sisaldab tõlkimata plokke, on järjekorda tõlkimiseks.
6. **Iga sihtkeel tõlgitakse ja salvestatakse iseseisvalt** – kui tšehhi keel õnnestub, kuid prantsuse keel ebaõnnestub, kirjutatakse tšehhi fail ikkagi kettale.
7. Edukalt tõlgitud failid valideeritakse struktuurse pariteedi jaoks allikaga (võrdsed pealkirjad, loendiüksused, koodiplokid, plokid, lingid, rasvased / kursilised markerid ja HTML-sildid) enne nende kirjutamist kettale.
8. Kui kõik allika sihtfailid õnnestuvad, salvestatakse uus räsi allika kõrvale. Kui allika kõrval kirjutamine ebaõnnestub (näiteks kirjutuskaitstud rakendustes), langeb räsi tagasi ajutisesse kataloogi.
9. Kui mõni sihttõlge ebaõnnestub valideerimisel, märgib metaandmed need plokid tõlkimata, nii et neid otsitakse järgmisel käivitamisel uuesti.

### 5. etapp – tulemuste salvestamine

Konsolideeritud dokument koostatakse ja avaldatakse. See hõlmab järgmist:

- UTC käivitamise ja lõpetamise ajatemplid.
- Salvestatud lokaadi JSON-failide loend, salvestatud Markdown-failid, salvestatud räsifailid ja varundatud räsikirjad.
- Töö käigus kogutud salvestusvead.
- Keeltepõhine tõlkestatistika (tõlgitud arv, vahelejäetud arv, vigade arv).

## Signaal R-teate ümbris

Iga edusündmus esitatakse a-na järgmiste väljadega:

väli
|-------|------|-------------|
Torujuhtme jooksva sõidu korrelatsiooniidentifikaator
Monotoonne loendur jooksu sees, alustades punktist 1
Teate semantiline tüüp
Torustiku etapp, kuhu teade kuulub
UTC aeg, mil teade edastati
Kas teade kujutab endast veatingimust
Inimloetav kokkuvõte
Etapipõhine kasulik koormus (aruande objekt või null)

### Kirjatüübid

Väärtus
|-------|------|---------|
0
1
2
3
4
5
6

### Torustiku etapid

Väärtus
|-------|------|-------------|
0
1
2
3
4
5

### Tüüpiline sõnumivoog

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

Kui mõni etapp ebaõnnestub, jäetakse ülejäänud etapid vahele, saadetakse sõnum ja lõpuks suletakse sõnum.

## Tõlkekatse loogika

Torujuhe rakendab kahte vastupidavuse taset:

### Etapitaseme proovimine (TranslationRetryService)

- Kui tõlkepäring pärast LibreTranslate'i sisemisi korduskatseid ebaõnnestub, teeb tõlkepäring kuni 3 täiendavat etapitaseme korduskatset 30-sekundilise viivitusega.
- Kohahoidja mask: Nimega kohahoidjad () asendatakse tekstis ajutiselt turvamärkidega () enne tõlkimist ja taastatakse hiljem, tagades õige grammatika sihtkeeltes.

### Keele valideerimine

- Enne sihtkeelde tõlkimist kontrollib teenus keelt tõlkeserveri poolt.
- Toetamata keeled jäetakse hoiatusega vahele, vältides korduvaid ebaõnnestunud katseid.

### Markdowni plokitaseme proovimine

- Markdown tõlked tehakse ploki kaupa (pealkirjad, lõigud, loendiüksused).
- Kui üksikplokk tõlkimine ebaõnnestub, märgitakse see metaandmete failis tõlkimata ja proovitakse uuesti järgmisel torujuhtmel.
- Teenus jälgib keelepõhist, plokipõhist olekut failides iga lähtekoodi Markdowni faili kõrval.

## Veakoodid

Vead teatatakse ühtse anumi abil, mis on rühmitatud vahemikesse:

ulatus
|-------|----------|
1000–1999
2000–2999
3000–3999
4000–4999
5000–5999

Iga viga aruandes kannab lähtekoodi (keelekood, failitee või etapi nimi), veakoodi ja inimloetavat sõnumit.

## Otsetõlke juhtpaneel

Serveri projekt sisaldab administraatori lehte, mis ühendab SignalR-i jaoturiga ja kuvab kõik torujuhtme sündmused reaalajas.

- Näitab ühenduse olekut, teadete arvu ja kõigi sündmuste reaalajas uuendatavat tabelit.
- Värvikoodiga read: sinine etapi alguseks, roheline lõpetamiseks, punane vigade jaoks.
- Toetab kanali puhastamist ja kõikide sõnumite eksportimist JSON- i.
- Automaatne taasühendamine eksponentsiaalse varundamisega, kui ühendus langeb.

## Projekteerimispõhimõtted

- **Modulaarsus**: iga tõlkeprobleem on hooldatavuse ja testitavuse huvides eraldiseisev.
- ** Järkjärguline püsivus**: Sõnaraamatud ja Markdowni failid salvestatakse keele kaupa kohe pärast tõlkimist, vähendades mälurõhku ja andes varasemat tagasisidet.
- ** Vastupidavus **: mitu kordusproovimise taset (HTTP, etapp, plokk) tagavad, et mööduvad rikked ei blokeeri torustikku.
- ** Riigi jälgimine**: Failipõhised metaandmed () ja räsifailid võimaldavad järgnevatel jooksudel täpset järkjärgulist tööd.
- **Nähtavus reaalajas**: Igast olulisest operatsioonist teatatakse SignalR-i kaudu seireks ja silumiseks.
- **Manual translations always have priority over automatic additions.**
