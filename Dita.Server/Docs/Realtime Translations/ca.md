# Traduccions en temps real

Aquest document existeix com a entrada de prova en directe per a la canonada de traducció automàtica.

## Què fa el servei

El servei s' executa en una planificació i valida el servidor de traducció, configuració i llengües disponibles abans d' iniciar qualsevol treball de traducció.

Després del pas de validació, sincronitza els noms del país del catàleg dels països de només lectura en els diccionaris locals estàndard JSON. Si l' idioma per omissió de l' aplicació és anglès, l' entrada país es desarà com a valor de clau igual a valor. Si l' idioma per omissió és diferent, el nom del país anglès és el primer traduït a l' idioma per omissió, i només després desat com a valor de clau igual al diccionari per omissió.

Després, el servei compara el diccionari de localització per omissió actual amb la instantània desada des de l' execució anterior. Les entrades noves afegides només es tradueixen en idiomes de destí quan la clau no existeix, així que les traduccions manuals tenen prioritat. S' eliminaran les entrades eliminades de tots els diccionaris objectiu per mantenir el conjunt consistent.

Finalment, el servei explora les arrels de documentació configurades per als arbres Markdown. S' espera que cada carpeta de tema contingui un fitxer origen anomenat després de l' idioma per omissió, com en. md. El servei té hes aquest fitxer origen, detecta canvis, tradueix els fitxers de destí perduts o obsolets, i desa l' haixix actual al costat del fitxer origen. Si s' escriu el resum al costat del fitxer origen no és possible, tornarà a l' emmagatzematge temporal.

## Com progressa el progrés dels informes de servei

El dorsal emet missatges de senyal general mitjançant el centre de localització usant un sobre de missatges. Cada missatge conté un tipus de missatge, l' etapa actual del procés, una marca de temps UTC, un resum de text i un carregador opcional de l' escenari específic.

Les fases actuals són:

- Comprova servidors
- Translació
- Tradueix fitxers Json
- Tradueix els fitxersMarkers
- S' estan desant elsResults

L'escenari típic del flux del missatge s'inicia, l'escenari s' ha completat i s' ha completat la canonada. Si falla una etapa, el missatge està marcat com un error i inclou informació estructurada d' errors amb codis d' error unificats.

## Dissenya els principis

Les traduccions es processen seqüencialment per evitar la sobrecàrrega del servidor Libretrate.

La localització dels diccionaris JSON sempre es desen amb tecles alfabèticament ordenades i formatada JSON per a un manteniment més fàcil.

La instantània prèvia del diccionari per omissió es desa persistentment, de manera que un reinici de l' aplicació no perdi el seguiment.

**Manual Les traduccions sempre tenen prioritat sobre les sumes automàtiques.**
