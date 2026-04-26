# Denbora errealeko itzulpenak

Dokumentu hau zuzeneko sarrera gisa dago itzulpen automatikoko kanalizaziorako.

## Zerbitzuak egiten duena

Zerbitzua programa batean exekutatzen da eta itzulpen-zerbitzaria, konfigurazioa eta hizkuntza erabilgarriak balioztatzen ditu edozein itzulpen-lan hasi aurretik.

Balioztatze-urratsaren ondoren, herrialdeen katalogoko herrialdeen izenak JSON hiztegi estandarretara sinkronizatzen ditu. Aplikazioaren hizkuntza lehenetsia ingelesa bada, herrialdeko sarrera balio berdin gisa gordeko da. Hizkuntza lehenetsia desberdina bada, ingelesezko izena lehen hizkuntza lehenetsira itzultzen da, eta orduan bakarrik gordetzen da balio bera hiztegi lehenetsian.

Ondoren, zerbitzuak uneko lokalizazio-hiztegia konparatzen du aurreko exekuziotik gordetako argazkiekin. Sarrera berriak xede-hizkuntzara itzultzen dira gakoa existitzen ez denean soilik, beraz, eskuzko itzulpenek lehentasuna dute. Kendutako sarrerak helburuko hiztegi guztietatik ezabatzen dira, multzo osoa koherentziaz mantentzeko.

Azkenik, zerbitzuak Markdown zuhaitzentzako dokumentazio- sustrai konfiguratuak eskaneatzen ditu. Gai-karpeta bakoitzak hizkuntza lehenetsiaren izena duen iturburu-fitxategi bat izatea espero da, adibidez, en.md. Zerbitzuak iturburu-fitxategia du, aldaketak detektatzen ditu, falta diren edo zaharkitutako helburuko Markdown fitxategiak itzultzen ditu eta uneko hash-a gordetzen du iturburu-fitxategiaren ondoan. Iturburu-fitxategiaren ondoko hash-a idaztea ezinezkoa bada, aldi baterako biltegira itzuliko da.

## Zerbitzuak nola egiten duen aurrera

Motorrak seinale-mezu orokorrak igortzen ditu lokalizazio-zentroaren bidez, gutun-azal bat erabiliz. Mezu bakoitzak mezu mota bat du, uneko prozesua, UTC ordu-zigilua, testu-laburpena eta aukerako ordainketa.

Gaur egungo faseak hauek dira:

- kontrol-zerbitzariak
- kontu itzulgarriak
- TranslateJsonFiles
- TranslateMarkdownFiles
- biltegiak

Mezu-fluxu tipikoa hasi eta amaitu da. Eszena batek huts egiten badu, mezua errore gisa markatuko da, eta errore-kode bateratuekin osatutako errore-informazioa dauka.

## Diseinu-printzipioak

Itzulpenak sekuentzialki prozesatzen dira LibreTranslate zerbitzaria ez gainkargatzeko.

Lokalizazioa JSON hiztegiak alfabetikoki ordenatutako gakoekin gordetzen dira beti, eta JSON formateatua mantentze errazagoa lortzeko.

Aurreko hiztegi-kaptura lehenetsia iraunkorki gordetzen da, beraz aplikazioaren berrabiarazteak ez du aldaketa-aztarna galtzen.

**Eskuzko itzulpenek lehentasuna dute beti gehiketa automatikoen aldean.**
