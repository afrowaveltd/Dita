# Traduceri în timp real

Acest document există ca o intrare de testare live pentru conducta de traducere automată. Orice modificare a acestui fișier declanșează re-transformarea tuturor fișierelor lingvistice țintă pe următoarea cursă programată.

## Prezentare generală a arhitecturii

Conducta de traducere a fost restructurata intr-o arhitectura modulara cu patru sub-servicii specializate coordonate de un orchestrator usor:

- **BackendTranslationService** .
- **CountriesTranslationService** .
- **LocalizareTranslationService** .
- **DocumenteTranslationService** .

Fiecare sub-service funcționează independent și raportează progrese prin SignarR în timp real.

## Ce face serviciul

Serviciul rulează pe un program și execută o conductă în cinci etape: validarea serverului, sincronizarea țării, sincronizarea dicționarului JSON, traducerea fișierelor Markdown și persistența rezultatelor. Fiecare etapă emite evenimente structurate de progres în timp real prin SignarR, astfel încât clienții conectați să poată urmări pe măsură ce activitatea se desfășoară.

## Etapele conductei

### Etapa 1

Înainte de a începe orice lucrare de traducere, serviciul verifică dacă toate condiţiile prealabile sunt îndeplinite:

- Secțiunea de configurare trebuie să fie prezentă și validă.
- Serverul LibreTranslate trebuie să răspundă într-o latență acceptabilă.
- Lista de limbi disponibile pe serverul de traducere este preluată.
- Limba implicită configurată trebuie să fie prezentă în lista respectivă.
- Lipsește fișiere JSON locale pentru orice limbă susținută sunt create automat.

Dacă orice verificare eşuează, conducta se opreşte imediat şi se emite un mesaj.

### Etapa 2

Numele de țară sunt păstrate în sincronizare dintr-un catalog numai-citit () în dicționarele JSON de localizare.

- Dacă limba implicită a aplicației este engleză, fiecare nume de țară este stocat ca fără traducere.
- Dacă limba implicită este orice altă limbă, numele țării engleze este tradus pentru prima dată în această limbă, iar rezultatul devine intrarea în dicționarul implicit.
- După actualizarea dicționarului implicit, fiecare intrare lipsă în fiecare dicționar de limbă țintă este tradusă și salvată **imediat per limbă**.
- Intrările deja traduse sunt păstrate fără modificări.
- În cazul în care o traducere nu reușește, serviciul se retește de până la 3 ori cu întârzieri de 30 de secunde înainte de a trece la limba următoare.

### Etapa 3

Serviciul compară dicţionarul curent implicit de localizare cu un instantaneu stocat din rula anterioară:

- **Added keys** .
- **Tastele modificate** .
- Traducerile manuale au întotdeauna prioritate. Dacă un dicționar țintă conține deja o valoare pentru o cheie, acea intrare este lăsată neschimbată indiferent de ceea ce spune sursa.
- **Fiecare dicționar de limbă țintă este salvat imediat după traducerea sa completă**, mai degrabă decât de așteptare pentru toate limbile pentru a termina.
- În cazul în care o traducere nu reușește pentru o anumită limbă, serviciul retries automat. Numai erorile persistente (de exemplu, limba nesusţinută) fac ca această limbă să fie omisă.
- După rulare, dicționarul implicit curent este salvat ca noua imagine pentru următoarea comparație.

Toate dicţionarele sunt mereu stocate cu chei sortate alfabetic şi marcate JSON pentru lizibilitatea umană.

### Etapa 4

Serviciul umblă rădăcinile de documentare configurate (default: ) și procesează fiecare fișier sursă recursiv:

1. Conținutul fișierului sursă este citit și se calculează un hash SHA-256.
2. Un fișier lângă urmele sursă per-limbă, starea de traducere per-bloc, permițând **re-translație incrementală** de doar blocuri eșuate.
3. Hash-ul stocat din rula anterioară (păstrat într-un fișier lângă fișierul sursă, sau într-o locație de rezervă temporară) este comparat cu hash curent.
4. Pentru fiecare limbă țintă, dosarul corespunzător este, de asemenea, verificat pentru integritatea structurală.
5. Orice fișier țintă care lipsește, are un hash învechit, nu validarea structurii, sau conține blocuri netranslate este coadă pentru re-transformare.
6. **Fiecare limbă-ţintă este tradusă şi salvată independent** .
7. Fişierele traduse cu succes sunt validate pentru paritatea structurală cu sursa (count de poziţie egală, elemente de listă, blocuri de cod, blockquotes, link-uri, aldine / tag-uri HTML,) înainte de a fi scrise pe disc.
8. Dacă toate fișierele țintă pentru o sursă reușesc, noul hash este stocat lângă sursă. În cazul în care scrierea lângă sursă nu reușește (de exemplu, în desfășurare numai citire), hash cade înapoi la directorul temporar.
9. În cazul în care orice traducere țintă nu este validată, metadatele marchează acele blocuri ca fiind netranslate astfel încât acestea să fie rejudecate pe următoarea cursă.

### Etapa 5

O consolidare este asamblată și publicată. Acesta include:

- UTC rulează startul și completează marcajele de timp.
- Numărătoare de fișiere JSON locale salvate, salvate fișiere Markdown, salvate fișiere hash, și hash retur scrie.
- Orice erori de stocare colectate în timpul cursei.
- Statistici de traducere per-limbă (contele tradus, numărul omis, numărul de erori).

## Pachet mesaj semnalR

Fiecare eveniment de progres este livrat ca un cu următoarele domenii:

Câmp
|-------|------|-------------|
Identificator de corespondență pentru funcționarea conductei curente
Contor monoton într-o cursă, începând cu 1
Tipul semantic al mesajului
Stadiul conductei de care aparține mesajul
Ora UTC atunci când mesajul a fost emis
Dacă mesajul reprezintă o condiție de eroare
Rezumat care poate fi citit de om
Sarcina utilă specifică etapei (obiect de raport sau nul)

### Tipuri de mesaje

Valoare
|-------|------|---------|
0
1
2
3
4
5
6

### Etapele conductei

Valoare
|-------|------|-------------|
0
1
2
3
4
5

### Fluxul de mesaje tipic

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

Dacă orice etapă eşuează, etapele rămase sunt omise, un mesaj este emis şi în final un mesaj închide cursa.

## Traducere retry logica

Conducta implementează două niveluri de reziliență:

### Recercetare la nivel de etapă (TranslationRetryService)

- În cazul în care o cerere de traducere nu reușește după retries interne LibreTranslate lui, efectuează până la 3 retries suplimentare de nivel de etapă cu 30 de secunde întârziere.
- Placeholder masching: Numiți deținători () în text sunt înlocuiți temporar cu jetoane sigure () înainte de traducere și restaurate după aceea, asigurând gramatica corectă în limbile țintă.

### Validarea limbii

- Înainte de a traduce într-o limbă țintă, serviciul verifică limba este susținută de serverul de traducere.
- Limbile nesusţinute sunt omise cu un avertisment, prevenind încercările repetate eşuate.

### Reîncercarea la nivel de bloc

- Traducerea Markdown se efectuează bloc cu bloc (rubrici, paragrafe, elemente de listă).
- În cazul în care un bloc individual nu reușește traducerea, acesta este marcat ca netradus în fișierul metadatelor și retried pe următoarea conductă run.
- Serviciul piste per-limbă, starea per-bloc în fișiere lângă fiecare fișier Markdown sursă.

## Coduri de eroare

Erori sunt raportate folosind un enum unificat grupate în intervale:

Gamă
|-------|----------|
1000
2999
3000
11004999
5000/5999

Fiecare eroare dintr-un raport poartă identificatorul sursă (cod de limbă, cale de fișier sau nume de scenă), codul de eroare și un mesaj care poate fi citit de om.

## bord de traducere live

Proiectul Server include o pagină de admin la care se conectează la hub-ul SignalR la și afișează toate evenimentele de conducte în timp real.

- Afișează starea conexiunii, numărul mesajelor și un tabel live-update al tuturor evenimentelor.
- Rânduri colorate: albastru pentru pornirea scenei, verde pentru completare, roșu pentru erori.
- Sprijină compensarea hranei pentru animale și exportul tuturor mesajelor către JSON.
- Reconectează automat cu exponențial dacă conexiunea scade.

## Principii de proiectare

- **Modularitate**: Fiecare problemă de traducere este izolată în propriul serviciu de întreținere și testabilitate.
- ** Persistenţă creativă**: Dictionarele și fișierele Markdown sunt salvate în fiecare limbă imediat după traducere, reducând presiunea de memorie și oferind feedback-ul anterior.
- **Resilience**: Multiple niveluri de retry (HTTP, etapa, bloc) asigura eșecuri tranzitorii nu blochează conducta.
- **State tracking**: Per-file metadate () și hash fișiere permit lucrări incrementale precise pe rulaje ulterioare.
- ** Vizibilitatea în timp real**: Fiecare operațiune semnificativă este raportată prin SignarR pentru monitorizare și depanare.
- ** Traducerile manuale au întotdeauna prioritate față de adăugările automate.**
