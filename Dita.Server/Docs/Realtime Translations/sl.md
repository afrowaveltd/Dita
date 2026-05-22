# Prevodi v realnem času

Ta dokument obstaja kot vhodni preskus v živo za cevovod za avtomatsko prevajanje. Vsaka sprememba te datoteke sproži ponovno prevajanje vseh ciljnih jezikovnih datotek v naslednjem načrtovanem teku.

## Pregled arhitekture

Prevajalski cevovod je bil preoblikovan v modularno arhitekturo s štirimi specializiranimi podstoritvami, ki jih koordinira lahki orkestrator:

- **BackendTranslationService** — Orkesterira celoten cevovod, upravlja potrjevanje strežnikov in delegati delajo na podstoritve.
- **Služba za prevajanje** — Sinhronizira imena držav iz slovarjev v enem jeziku.
- **LocalizationTranslationService** — Detekti, dodani/odpravljeni ključi v privzetem slovarju JSON in jih prevaja v ciljne jezike.
- **DokumentiPrevajanjeService** — Prevaja dokumentarne datoteke Markdown s sledenjem po bloku in metapodatki.

Vsaka podstoritev deluje neodvisno in poroča o napredku prek SignalR v realnem času.

## Kaj stori služba

Storitev poteka po urniku in izvaja petstopenjski cevovod: potrjevanje strežnika, sinhronizacija države, sinhronizacija slovarja JSON, prevod datoteke Markdown in vztraja pri rezultatih. Vsaka stopnja oddaja strukturirane dogodke napredka v realnem času nad SignalR, tako da lahko povezani odjemalci sledijo, ko se delo nadaljuje.

## Faze cevovodov

### Faza 1 – kontrolni strežniki

Pred začetkom prevajanja služba preveri, ali so izpolnjeni vsi predpogoji:

- Oddelek za konfiguracijo mora biti prisoten in veljaven.
- Strežnik LibreTranslate se mora odzvati znotraj sprejemljive latence.
- Seznam jezikov, ki so na voljo na prevajalskem strežniku, je privzet.
- Nastavljen privzeti jezik mora biti na tem seznamu.
- Manjkajoče krajevne datoteke JSON za kateri koli podprt jezik so ustvarjene samodejno.

V primeru neuspešnega preverjanja se cevovod takoj ustavi in pošlje sporočilo.

### 2. stopnja – prevajalske države

Imena držav se hranijo v sinhronizaciji iz kataloga samo za branje () v slovarje JSON lokalizacije.

- Če je privzeti jezik uporabe angleščina, se ime vsake države shrani kot brez prevoda.
- Če je privzeti jezik katerikoli drug jezik, je angleško ime države najprej prevedeno v ta jezik, rezultat pa postane vnos v privzetem slovarju.
- Po posodobitvi privzetega slovarja se vsak manjkajoči vnos države v vsakem slovarju ciljnega jezika prevede in shrani ** takoj na jezik**.
- Že prevedeni vnosi so ohranjeni brez spremembe.
- Če prevod ne uspe, se storitev pred selitvijo v naslednji jezik vrne do trikrat s 30-sekundnimi zamudami.

### Faza 3 – Prevedi dosjeje Json

Storitev primerja trenutni privzeti slovar lokalizacije s sliko, ki je shranjena v prejšnjem zagonu:

- ** Vgrajeni ključi** – vnosi, ki so prisotni v trenutnem privzetem stanju, vendar jih ni na posnetku – so prevedeni v vsak ciljni jezik, ki še nima ročnega vnosa za ta ključ.
- ** Odvzeti ključi** – vnosi, ki so prisotni na posnetku, vendar jih trenutno ni, se izbrišejo iz vsakega slovarja ciljnega jezika.
- Ročni prevodi imajo vedno prednost. Če ciljni slovar že vsebuje vrednost za ključ, ta vnos ostane nespremenjen ne glede na to, kaj pravi vir.
- ** Vsak slovar ciljnega jezika se shrani takoj po končanju prevodov**, namesto da bi čakal, da se končajo vsi jeziki.
- Če prevod za določen jezik ne uspe, se storitev samodejno povrne. Le vztrajne napake (npr. nepodprt jezik) povzročijo, da se ta jezik preskoči.
- Po zagonu se trenutni privzeti slovar shrani kot nov posnetek za naslednjo primerjavo.

Vsi slovarji so vedno shranjeni z abecedno razvrščenimi ključi in vdolbinami JSON za človeško berljivost.

### Faza 4 – datoteke s prevodi

Storitev hodi nastavljeno dokumentacijo korenine (privzeto: ) in obdeluje vsako izvorno datoteko rekurzivno:

1. Vsebina izvorne datoteke se bere in izračuna se hašiš SHA-256.
2. Datoteka ob izvornih skladbah na jezik, status prevajanja na blok, ki omogoča ** inkrementalno prevajanje** le neuspešnih blokov.
3. Shranjen hašiš iz prejšnjega zaganjanja (urejen v datoteko poleg izvorne datoteke, ali na začasni nadomestni lokaciji) se primerja s trenutnim hašišem.
4. Za vsak ciljni jezik se preveri tudi ustrezna datoteka glede strukturne celovitosti.
5. Vsaka manjkajoča ciljna datoteka, ki ima zastarel hašiš, neuspešno potrjevanje strukture ali vsebuje neprevedene bloke, je v vrsti za prevajanje.
6. ** Vsak ciljni jezik je preveden in shranjen neodvisno** – če če češki uspe, francoski pa ne, je češka datoteka še vedno napisana na disk.
7. Uspešno prevedene datoteke so validirane za strukturno pariteto z virom (enako število naslovov, seznam postavk, kode blokov, blokov, povezav, krepko/italnih označevalcev, in HTML oznake), preden so zapisane na disk.
8. Če vse ciljne datoteke za vir uspejo, se novi haši shrani poleg vira. Če pisanje ob viru spodleti (na primer pri razporeditvah samo za branje), hašiš pade nazaj v začasni imenik.
9. Če kateri koli ciljni prevod ne uspe validirati, metapodatki označijo te bloke kot neprevedene, tako da se ponovno uporabijo v naslednjem teku.

### Faza 5 – ShranjevanjeResults

Konsolidirana se sestavi in objavi. Vključuje:

- Časovni žigi začetka in zaključka delovanja UTC.
- Število shranjenih krajevnih datotek JSON, shrani Markdown datoteke, shrani Hash datoteke, in rezervni hash piše.
- Vse napake pri shranjevanju, zbrane med vožnjo.
- Statistika prevajanja v jeziku (prevedeno štetje, preskoči štetje, štetje napak).

## Ovojnica sporočila SignalR

Vsak dogodek napredka je dostavljen z naslednjimi polji:

Polje
|-------|------|-------------|
Korelacijski identifikator za trenutno delovanje plinovoda
Enotonski števec v teku, z začetkom na 1
Semantična vrsta sporočila
Pipeline faza sporočilo pripada
Čas UTC, ko je bilo sporočilo oddano
Ali sporočilo predstavlja stanje napake
Človeško berljiv povzetek
Plačilni tovor, določen za fazo (prijavi predmet ali nič)

### Vrsta sporočila

Vrednost
|-------|------|---------|
0
1
2
3
4
5
6

### Faze cevovodov

Vrednost
|-------|------|-------------|
0
1
2
3
4
5

### Tipični tok sporočila

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

Če katera stopnja ne uspe, se preostale stopnje preskočijo, pošlje se sporočilo in končno sporočilo zapre tek.

## Logika ponovnega preizkusa prevodov

Cevovod izvaja dve stopnji odpornosti:

### Ponovni preizkus stopnje (TranslationRetryService)

- Če zahtevek za prevod ne uspe po notranjih retrijev LibreTranslate, opravi do 3 dodatne stopnje retries z 30-sekundnimi zamudami.
- Placeholder maskiranje: Imenovani kraji () v besedilu se začasno nadomestijo z varnimi žetoni () pred prevajanjem in se nato obnovi, kar zagotavlja pravilno slovnico v ciljnih jezikih.

### Potrditev jezika

- Pred prevajanjem v ciljni jezik storitev preveri jezik, ki ga podpira prevajalski strežnik.
- Nepodprti jeziki se preskočijo z opozorilom, s čimer se preprečijo ponavljajoči se neuspeli poskusi.

### Označevanje ravni bloka

- Označevanje prevodi se izvajajo blok-po-blok (postavke, odstavki, postavke seznama).
- Če posamezni blok ne uspe prevesti, je označen kot nepreveden v metapodatkovni datoteki in ponovno preizkušen na naslednjem cevovodu.
- Servis sledi na jezik, stanje na blok v datotekah poleg vsakega vira Markdown datoteke.

## Kode napak

O napakah se poroča z uporabo enotnega števila, razvrščenega v razpone:

Razpon
|-------|----------|
1000–1999
2000–2999
3000–3999
4000–4999
5000–5999

Vsaka napaka v poročilu nosi izvorno oznako (jezikovno kodo, pot datoteke ali ime faze), kodo napake in sporočilo, ki ga je mogoče brati.

## Prevodna plošča v živo

Projekt Server vključuje admin stran, ki se navezuje na vozlišče SignalR in prikazuje vse plinovodne dogodke v realnem času.

- Prikazuje stanje povezave, število sporočil in razpredelnico vseh dogodkov v živo.
- Barvno kodirane vrstice: modra za začetek odra, zelena za dokončanje, rdeča za napake.
- Podpira čiščenje krme in izvoz vseh sporočil v JSON.
- Samodejno se priklopi z eksponentnim zaostankom, če povezava pade.

## Načela projektiranja

- **Modularnost**: Vsak prevajalski pomislek je izoliran v svoji službi za vzdrževanje in preizkušanje.
- ** Vztrajnost mišic**: Dictionaryji in Markdown datoteke so shranjene na jezik takoj po prevodu, zmanjšanje pritiska pomnilnika in zagotavljanje zgodnejših povratnih informacij.
- **Odpornost**: Večkratni nivoji ponovnih poskusov (HTTP, stopnja, blok) zagotavljajo, da prehodne napake ne blokirajo cevovoda.
- **Sledenje državi**: Per-file metapodatki () in hash datoteke omogočajo natančno postopna dela na kasnejših tekih.
- ** Vidljivost v realnem času**: Vsaka pomembna operacija se poroča preko SignalR za spremljanje in razhroščevanje.
- ** Ročni prevodi imajo vedno prednost pred samodejnimi dodatki.**
