# Përkthime në kohë reale

Ky dokument ekziston si një provë e drejtpërdrejtë për tubacionin automatik të përkthimit.

## Çfarë bën shërbimi

Shërbimi punon në një program dhe vërteton serverin e përkthimit, konfigurimin dhe gjuhët në dispozicion para se të fillojë çdo përkthim.

Pas hapit të vleftësimit, ai sinkronizon emrat e vendeve nga vendet në vetëm lexim në fjalorët standartë JSON. Nëse gjuha e prezgjedhur e aplikativit është anglisht, hyrja në vend ruhet si vlera e barabartë me atë të vendit. Nëse gjuha e paracaktuar është ndryshe, emri i vendit anglez përkthehet së pari në gjuhën e paracaktuar, dhe vetëm atëherë ruhet si vlera e barabartë kyçe në fjalorin e paracaktuar.

Më pas, shërbimi krahason fjalorin aktual të përcaktimit me skandin e ruajtur nga funksioni i mëparshëm. Në gjuhët e synuara janë përkthyer pjesë të reja vetëm kur kyçi nuk ekziston, prandaj përkthimet manuale mbajnë përparësi. Zërat e hequr janë fshirë nga të gjithë fjalorët objektivë për të mbajtur të gjithë rregullin në vazhdimësi.

Më në fund, skanimi i shërbimit përcakton rrënjë dokumentacioni për pemët e Markut. Çdo kartelë e temave pritet të përmbajë një file burim të caktuar sipas gjuhës së paracaktuar, të tillë si en.md. Shërbimi ngjitet në skedarin burimor, dikton ndryshimet, përkthen skedarët e humbur apo të vjetëruar, Markdown, dhe ruan hashh aktuale pranë file burimor. Nëse nuk është e mundur shkrimi i hashashit pranë file burim, ajo kthehet në depon e përkohshme.

## Si raporton progres shërbimi

Backend lëshon mesazhe të përgjithshme sinjalizues nëpërmjet shpërndarësit të lokalizimit duke përdorur një zarf mesazhi. Çdo mesazh përmban një lloj mesazhi, fazën aktuale të procesit, një UTC timetamp, një përmbledhje teksti, dhe një ngarkesë specifike stade.

Fazat aktuale janë:

- Kontrollo serverët
- Përkthe llogaritjet
- Përkthe skedarët Jason
- Përkthe file
- Duke grumbulluar pasoja

Rrjedhja tipike e mesazhit është fillimi i skenës, faza e përfunduar dhe tubacioni përfundoi. Nëse një fazë dështon, mesazhi është shënuar si një gabim dhe përfshin informacion të strukturuar të gabimit me kode të unifikuara gabimi.

## Projekti

Përkthimet janë përpunuar në mënyrë sekuente për të shmangur mbingarkesën e serverit Libre Translate.

FJALËT e lokalizimit JSON gjithmonë ruhen me çelësa të renditur alfabetik dhe JSON për mirëmbajtje më të lehtë.

Është a nga programi nuk.

**Përkthimet shumëngjyrëshe kanë gjithmonë përparësi mbi shtesat automatike**
