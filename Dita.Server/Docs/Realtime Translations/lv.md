# Reālā laika tulkojumi

Šis dokuments eksistē kā testa ievade automātiskajam tulkošanas cauruļvadam. Jebkuras izmaiņas šajā failā izraisa visu mērķa valodu failu tulkošanu nākamajā ieplānotajā izpildījumā.

## Arhitektūras pārskats

Tulkošanas cauruļvads ir pārbūvēts par modulāru arhitektūru ar četriem specializētiem apakšpakalpojumiem, ko koordinē vieglais orķestrs:

- **BackendTranslationService** — orķestrē visu cauruļvadu, apkalpo servera apstiprināšanu un deleģē darbu apakšpakalpojumiem.
- **CountriesTranslationService** — Sinhronizē valstu nosaukumus no vienas valodas vārdnīcas.
- **LocalizationTranslationService** — nosaka pievienotās/izņemtās atslēgas noklusējuma JSON vārdnīcā un tulko tās mērķa valodās.
- **DokumentiTranslationService** — Tulko iezīmē iezīmēšanas dokumentācijas failus ar per-block izsekošanu un metadatiem.

Katrs apakšpakalpojums darbojas neatkarīgi un reāllaikā ziņo par progresu, izmantojot SignalR.

## Ko dara dienests

Pakalpojums darbojas pēc grafika un izpilda piecu pakāpju cauruļvadu: servera apstiprināšanu, valstu sinhronizāciju, JSON vārdnīcas sinhronizāciju, Markdown failu tulkojumu, un saglabājot rezultātus. Katrs posms emitē strukturētus reālā laika progresa notikumus virs SignalR, lai savienotie klienti varētu sekot līdzi, kad darbs turpinās.

## Cauruļvadu posmi

### Pakāpe

Pirms jebkura tulkošanas darba uzsākšanas dienests pārliecinās, ka visi priekšnoteikumi ir izpildīti:

- Konfigurācijas sadaļai jābūt klāt un derīgai.
- LibreTranslate serverim jāatbild pieņemamā latentumā.
- Tulkošanas serverī pieejamo valodu saraksts ir ielādēts.
- Šajā sarakstā jābūt konfigurētajai noklusētajai valodai.
- Trūkst locale JSON faili jebkurai atbalstītajai valodai tiek radīti automātiski.

Ja kāda pārbaude neizdodas, cauruļvads nekavējoties apstājas un ziņojums tiek raidīts.

### 2. posms – tulkotāji

Valstu nosaukumi tiek saglabāti sinhronizēti no lasāma kataloga () lokalizācijas JSON vārdnīcās.

- Ja pieteikuma noklusējuma valoda ir angļu valoda, katrs valsts nosaukums tiek saglabāts kā bez tulkojuma.
- Ja noklusējuma valoda ir kāda cita valoda, angļu valsts nosaukums vispirms tiek tulkots šajā valodā, un rezultāts kļūst par ierakstu noklusējuma vārdnīcā.
- Pēc noklusējuma vārdnīcas atjaunināšanas, katrs trūkstošais valsts ieraksts katrā mērķa valodas vārdnīcā tiek tulkots un saglabāts **tūlīt katrā valodā**.
- Jau tulkotie ieraksti tiek saglabāti bez izmaiņām.
- Ja tulkojums neizdodas, dienests atkārto līdz 3 reizes ar 30 sekunžu kavēšanos pirms pārcelšanās uz nākamo valodu.

### 3. posms – translateJsonFiles

Pakalpojums salīdzina pašreizējo noklusējuma lokalizācijas vārdnīcu ar iepriekšējā izpildījumā glabātu momentuzņēmumu:

- **Pievienotie taustiņi** – pašreizējā noklusējuma ieraksti, kas nav momentuzņēmums, – tiek tulkoti katrā mērķa valodā, kurā vēl nav manuāla ieraksta par šo atslēgu.
- ** Noņemtie taustiņi** – momentuzņēmums, bet nav pašreizējā noklusējuma ieraksti – tiek dzēsti no katras mērķa valodas vārdnīcas.
- Manuālie tulkojumi vienmēr ir prioritāte. Ja mērķa vārdnīca jau satur atslēgas vērtību, šis ieraksts paliek nemainīgs neatkarīgi no avota teiktā.
- **Katra mērķa valodas vārdnīca tiek saglabāta uzreiz pēc tulkojumu pabeigšanas**, nevis gaida, kad visas valodas tiks pabeigtas.
- Ja tulkojums kādā valodā neizdodas, pakalpojums automātiski atkārtojas. Tikai pastāvīgas kļūdas (piem, neatbalstīta valoda) liek šo valodu izlaist.
- Pēc palaišanas pašreizējā noklusētā vārdnīca tiek saglabāta kā jaunais momentuzņēmums nākamajam salīdzinājumam.

Visas vārdnīcas vienmēr tiek glabātas ar alfabētiski sakārtotām atslēgām un ierindota JSON cilvēka lasāmībai.

### Posms – TranslateMarkdownFiles

Pakalpojums iziet konfigurēto dokumentācijas saknes (noklusējums: ) un apstrādā katru avota failu rekursīvi:

1. Avota faila saturu nolasa un aprēķina SHA-256 hash.
2. Fails blakus avota celiņiem par valodu, par bloka tulkošanas statusu, kas ļauj ** Inkrementāla re- tulkošana** tikai neveiksmīgajiem blokiem.
3. Saglabātais hash no iepriekšējās palaišanas (turēts failā blakus avota failam, vai pagaidu atkāpšanās vietā) tiek salīdzināts ar pašreizējo hash.
4. Katrai mērķa valodai pārbauda arī atbilstošo failu strukturālo integritāti.
5. Jebkurš mērķa fails, kas trūkst, ir novecojis hash, neizdodas struktūras validāciju, vai satur netulkots bloki ir rindā atkārtotai tulkošanai.
6. **Katra mērķa valoda tiek tulkota un saglabāta patstāvīgi** — ja čehu valodai izdodas, bet franču valodai neizdodas, čehu fails joprojām tiek rakstīts diskā.
7. Veiksmīgi tulkotie faili tiek validēti strukturālai paritātei ar avotu (vienāds virsraksta skaits, saraksta ieraksti, kodu bloki, blockquotes, saites, treknraksts/itālisks marķieri, un HTML tagi) pirms tie tiek rakstīti diskā.
8. Ja visi mērķa faili avotam izdodas, jaunais hash tiek saglabāts blakus avotam. Ja rakstīšana blakus avotam neizdodas (piemēram, tikai lasāmos izvietojumos), hash atkrīt uz pagaidu direktoriju.
9. Ja kāds mērķa tulkojums neapstiprina, metadati iezīmē šos blokus kā netulkotus, lai tie tiktu retrideti nākamajā braucienā.

### Pakāpe – rezultāti

Konsolidēts tiek samontēts un publicēts. Tie ietver:

- UTC palaišanas un pabeigšanas laika zīmogi.
- Skaita saglabātos locale JSON failus, saglabātos Markdown failus, saglabātos hash failus, un rezerves hash raksta.
- Visas glabāšanas kļūdas, kas savāktas brauciena laikā.
- Tulkojumu statistika par katru valodu (tulkots skaits, izlaists skaits, kļūdu skaits).

## SignalR ziņojuma aploksne

Katrs progresa pasākums tiek īstenots kā ar šādiem laukiem:

Aile
|-------|------|-------------|
Pašreizējā cauruļvada atbilstības identifikators
Monotoniskais skaitītājs darbības laikā, sākot ar 1
Ziņojuma semantiskais tips
Cauruļvada posms, kuram pieder ziņojums
Zvaigznāju vārdi
Vai ziņojums ir kļūdas nosacījums
Cilvēklasāms kopsavilkums
Posmveida īpatnējā lietderīgā slodze (ziņojuma objekts vai nulle)

### Ziņojumu veidi

Vērtība
|-------|------|---------|
0
1
2
3
4
5
6

### Cauruļvadu posmi

Vērtība
|-------|------|-------------|
0
1
2
3
4
5

### Tipiska ziņojumu plūsma

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

Ja kāds posms neizdodas, pārējie posmi tiek izlaisti, ziņojums tiek emitēts, un visbeidzot ziņojums aizver skrējienu.

## Tulkošanas atkārtošanas loģika

Cauruļvads īsteno divus elastīguma līmeņus:

### Skatuves līmeņa atkārtojums (TranslationRestryService)

- Ja tulkošanas pieprasījums neizdodas pēc LibreTranslate iekšējās retries, veic līdz 3 papildu posma līmeņa retries ar 30 sekunžu kavēšanos.
- Vietnieku maskēšana: Nosauktos vietturus () tekstā uz laiku aizstāj ar drošiem žetoniem () pirms tulkojuma un pēc tam atjauno, nodrošinot pareizu gramatiku mērķa valodās.

### Valodu apstiprināšana

- Pirms tulkošanas mērķa valodā, pakalpojums pārbauda valodu atbalsta tulkošanas serveris.
- Neatbalstītās valodas tiek izlaistas ar brīdinājumu, novēršot atkārtotus neveiksmīgus mēģinājumus.

### Atzīmēšanas bloka līmeņa atkārtojums

- Marķējuma tulkojumi tiek veikti block-by-block (pozīcijas, punkti, saraksta preces).
- Ja atsevišķam blokam neizdodas tulkojums, tas metadatu failā tiek atzīmēts kā netulkots un retranslēts nākamajā vada palaišanas reizē.
- Pakalpojums izseko par valodu, par bloka statusu failos blakus katram avota iezīmēšanas failam.

## Kļūdu kodi

Kļūdas tiek ziņotas, izmantojot vienotu enum grupēti diapazonos:

Diapazons
|-------|----------|
1000–1999
2000–2999
3000–3999
4000–4999
5000–599

Katra kļūda ziņojumā satur avota identifikatoru (valodas kodu, faila ceļu vai posma nosaukumu), kļūdas kodu un cilvēklasāmu ziņojumu.

## Tulkošanas dashboard Live

Servera projekts ietver admin lapu, kas savieno ar SignalR centrmezglu un parāda visus cauruļvadu notikumus reālajā laikā.

- Parāda savienojuma statusu, ziņojumu skaitu un visu notikumu atdzīvināšanas tabulu.
- Krāsu kodētas rindas: zils skatuves sākumam, zaļš pabeigšanai, sarkans kļūdām.
- Atbalsta klīringa barību un eksportē visus ziņojumus uz JSON.
- Automātiski savienojas ar eksponenciālu dublējumu, ja nokrīt savienojums.

## Projektēšanas principi

- **Modularitāte**: Katrs tulkošanas jautājums ir izolēts savā dienestā, lai uzturētu un pārbaudītu.
- ** Inkrementālā noturība**: Vārdnīcas un iezīmēšanas faili tiek saglabāti uz vienu valodu uzreiz pēc tulkojuma, samazinot atmiņas spiedienu un nodrošinot agrāku atgriezenisko saiti.
- ** Noturība**: Vairāki atkārtošanas līmeņi (HTTP, stadija, bloks) nodrošina pārejošas kļūmes, nebloķē cauruļvadu.
- **Valsts izsekošana**: Katram failam metadati () un hash faili ļauj precīzi inkrementāli strādāt nākamajos braucienos.
- **Reālā laika redzamība**: Par katru nozīmīgu darbību tiek ziņots, izmantojot SignalR monitoringam un atkļūdošanai.
- **Manuālie tulkojumi vienmēr ir prioritāte pār automātiskajiem papildinājumiem.**
