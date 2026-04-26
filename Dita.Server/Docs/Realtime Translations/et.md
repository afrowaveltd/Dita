# Reaalajas tõlked

See dokument on olemas automaatse tõlketorustiku reaalajas testimise sisendina.

## Mida teenistus teeb

Teenus töötab ajakava järgi ja kinnitab tõlkeserveri, konfiguratsiooni ja saadaolevad keeled enne tõlketöö algust.

Pärast valideerimisetappi sünkroonib see riikide nimed kirjutuskaitstud riikide kataloogist standardsetesse lokaliseerimissõnastikesse JSON. Kui rakenduse vaikekeel on inglise keel, salvestatakse riigikirje kui võti võrdub väärtusega. Kui vaikimisi keel on erinev, tõlgitakse inglise riigi nimi kõigepealt vaikekeelde ja alles siis salvestatakse võtmena väärtus vaikesõnastikus.

Seejärel võrdleb teenus aktiivset vaikimisi lokaliseerimissõnaraamatut eelmisest tööst salvestatud hetkepildiga. Äsja lisatud kirjed tõlgitakse sihtkeeltesse ainult siis, kui võtit ei ole veel olemas, seega on eelistatud käsitsi tõlge. Eemaldatud kirjed kustutatakse kõikidest sihtsõnastikest, et kogu hulk oleks järjekindel.

Lõpuks skaneerib teenus Markdowni puude konfigureeritud dokumentatsiooni juured. Iga teemakataloog peaks sisaldama vaikimisi keele järgi nime saanud lähtefaili, näiteks en.md. Teenus räsib lähtefaili, tuvastab muudatused, tõlgib puuduvad või aegunud sihtmärgistusfailid ja salvestab lähtefaili kõrvale praeguse räsi. Kui lähtefaili kõrvale räsi kirjutamine ei ole võimalik, langeb see tagasi ajutisele salvestamisele.

## Kuidas teenuse aruanded edenevad

Taustaprogramm saadab üldisi SignalR-sõnumeid läbi lokaliseerimisjao, kasutades ühte sõnumiümbrikku. Igal sõnumil on sõnumi tüüp, aktiivne protsessi etapp, UTC ajatempel, teksti kokkuvõte ja valikuline etapipõhine kasulik koormus.

Praegused etapid on järgmised:

- kontrollijad
- Tõlgitud riigid
- TõlkiJsonFiles
- TõlkiMarkdowni failid
- Tulemuste salvestamine

Tüüpiline sõnumivoog on etapi alustamine, etapi lõpuleviimine ja torujuhe valmis. Kui etapp ebaõnnestub, märgitakse sõnum veaks ja sisaldab struktureeritud veateavet ühtsete veakoodidega.

## Projekteerimispõhimõtted

Tõlkeid töödeldakse järjestikku, et vältida LibreTranslate serveri ülekoormust.

Lokaliseerimise JSON-sõnastikud on alati salvestatud tähestikuliselt sorteeritud võtmetega ja vormindatud JSON-iga, et neid oleks lihtsam hooldada.

Eelmine vaikimisi sõnaraamat salvestatakse püsivalt, nii et rakenduse taaskäivitamine ei kaota muutuste jälgimist.

**Käsitsitõlked on alati automaatsete täienduste ees prioriteetsed**
