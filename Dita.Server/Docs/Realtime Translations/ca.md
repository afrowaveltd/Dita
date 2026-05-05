# Traduccions en temps real

Aquest document existeix com a entrada de prova en directe per a la canonada de traducció automàtica. Qualsevol canvi a aquest fitxer fa que es torni a reproduir tots els fitxers de l' idioma objectiu en la propera execució planificada.

## Resum de l' arquitectura

La canonada de traducció s'ha reestructurat en una arquitectura modular amb quatre subserves especialitzats coordinats per un lleuger orquestrator:

- **BackendTranslationService** — Orchestrates the entire pipeline, handles server validation, and delegates work to sub-services.
- **CountriesTranslationService** — Synchronizes country names from `countries.json` into per-language dictionaries.
- **LocalizationTranslationService** — Detects added/removed keys in the default JSON dictionary and translates them into target languages.
- **DocumentsTranslationService** — Translates Markdown documentation files with per-block tracking and metadata.

Cada subservevidor funciona de forma independent i mostra un progrés mitjançant senyalR en temps real.

## El que fa el servei

El servei s' executa en una planificació i executa una canonada de cinc en curs: validació del servidor, la sincronització del país, la sincronització del diccionari JSON, la sincronització del fitxer Markdown, i persisteix els resultats. Cada etapa emet esdeveniments de progrés en temps real estructurat sobre senyalName R per tal que els clients connectats puguin seguir el procés de treball.

## Traces de conducte

### Escena 1 Server Comprovacions

Abans de que comenci qualsevol treball de traducció, el servei indica que totes les condició prèvia estan satisfetes:

- La secció de configuració ha de ser present i vàlida.
- El servidor Libretrage ha de respondre amb una ència acceptable.
- S' ha recuperat la llista d' idiomes disponibles al servidor de traducció.
- El llenguatge configurat per omissió ha d' estar present en aquesta llista.
- No s' han trobat fitxers locale JSON per a qualsevol llengua implementada es creen automàticament.

Si falla qualsevol comprovació, la canonada s' atura immediatament i s' emet un missatge.

### Escena 2  TranslateCountries

Els noms dels països es mantenen sincronitzats per un catàleg de només lectura () al diccionari localització JSON.

- Si l' idioma per omissió de l' aplicació és anglès, cada nom del país està desat com a sense traduir.
- Si l' idioma per omissió és qualsevol altra llengua, el nom del país anglès es tradueix en aquest idioma, i el resultat esdevé l' entrada en el diccionari per omissió.
- Després d' actualitzar el diccionari per omissió, cada entrada del país falta en cada diccionari de l' idioma objectiu està traduïda i desat **immediatment per l' idioma **immediat.
- Les entrades ja traduïdes són preservades sense modificació.
- Si falla una traducció, el servei reintenta 3 cops amb 30 segons retards abans de moure's a la següent llengua.

### Escena 3  TranslateJsonFiles

El servei compara el diccionari per omissió actual amb una instantània emmagatzemada des de l' execució anterior:

- **Added keys** — entries present in the current default but absent from the snapshot — are translated into every target language that does not already have a manual entry for that key.
- **Removed keys** — entries present in the snapshot but absent from the current default — are deleted from every target language dictionary.
- Les traduccions manuals sempre tenen prioritat. Si un diccionari objectiu ja conté un valor per a una clau, aquesta entrada queda sense importar el que digui l' origen.
- **Each target language dictionary is saved immediately after its translations complete**, rather than waiting for all languages to finish.
- Si una traducció falla en un idioma específic, el servei reintenta automàticament. Només errors persistents (p. ex., llenguatge no implementat) perquè s' ometi el llenguatge.
- Després de l' execució, el diccionari per omissió actual es desa com una nova instantània per a la comparació següent.

Tots els diccionaris sempre es desen amb tecles alfabèticament ordenades i sagnats JSON per a la llegibilitat humana.

### Escena 4  translated fitxersMarkdown

El servei camina les arrels de documentació configurades (per omissió: ) i processa cada fitxer font recursivament:

1. El contingut del fitxer font és llegit i s' ha calculat un resum SHA- 56.
2. A `.translation-meta.json` file next to the source tracks per-language, per-block translation status, enabling **incremental re-translation** of only failed blocks.
3. L' entrada desada des de l' execució anterior (kept en un fitxer al costat del fitxer origen, o en una localització de reserva temporal) és comparada amb l' entrada actual.
4. Per a cada llengua de destí, el fitxer corresponent també està marcat per a la integritat estructurada.
5. Qualsevol fitxer objectiu que falta té un resum desactualitzat, falla la validació de l' estructura, o conté blocs sense traduir es troba a la cua per tornar a traduir.
6. **Each target language is translated and saved independently** — if Czech succeeds but French fails, the Czech file is still written to disk.
7. Els fitxers traduïts s' validen correctament per a la paritat estructurada amb el codi font (igualament comptador de capçaleres, elements de llista, blocs de codi, blocs de cometes, enllaços, marques en negreta i etiquetes HTML) abans d' escriure al disc.
8. Si tots els fitxers objectiu per a una font tenen èxit, el nou resum serà desat al costat de la font. Si l' escriptura al costat de la font falla (per exemple, en el desplegament de només lectura), l' haixix torna al directori temporal.
9. Si qualsevol traducció a l'objectiu falla en la validació, les metadades marquen aquests blocs sense traduir per tal que es retribuin a la següent execució.

### Etapa 5 Manveen StoringResult

S'ha reunit i publicat. Això inclou:

- UTC executant- se i finalitzar marques de temps.
- Nombres de fitxers locals desats, desats Markdown, fitxers de resum desats, i les dades de resum.
- Qualsevol error d' emmagatzematge recollit durant l' execució.
- Estadístiques de traducció a l' idioma (compte sense traduir, compte d' error, compte d' error).

## Senyal Sobre del missatge R

Cada esdeveniment de progrés és lliurat com a un dels següents camps:

Camp
|-------|------|-------------|
Identificador de correlació per a l' execució actual de canonada
Comptador monotònic dins d'una execució, començant a 1
Tipus semàntic del missatge
La fase de línia de conducte pertany el missatge
Temps UTC en què es va emesa el missatge
Si el missatge representa una condició d' error
Resum llegible
Càrrega específica de l' audició (objecte de port o nul)

### Tipus de missatge

Valor
|-------|------|---------|
0
1
2
3
4
5
6

### Traces de conducte

Valor
|-------|------|-------------|
0
1
2
3
4
5

### Flux típic del missatge

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

Si falla qualsevol etapa, les fases restants s' ometen, s' emet un missatge i finalment un missatge tanca l' execució.

## La traducció torna a intentar la lògica

La canonada implementa dos nivells de resistència:

### Torna a intentar un nivell d' amplada (traducció del Servei)

- Si una petició de traducció falla després de les reintents internes de Libretravee, l'obra es retrà fins a 3 nivells addicionals d'escenari amb 30 segons retards.
- Màscara de substitució: Les variables de substitució amb nom () del text es reemplaçaran temporalment amb fitxes de seguretat () abans de la traducció i restaurada després, assegurant la gramàtica correcta en idiomes objectiu.

### Validació d' idioma

- Abans de traduir a una llengua de destí, l' idioma verifica l' idioma del servidor de traducció.
- Les llengües no acceptades s' ometen amb un avís, que impedeixen que els intents fallin.

### Torna a provar el nivell de bloqueig Markdown

- En marcar les traduccions enrere s' han de realitzar bloc- a- bloc (encapçament, paràgrafs, elements de llista).
- Si un bloc individual falla la traducció, es marca com a sense traduir en el fitxer de metadades i es retribueix en la següent execució de canonada.
- Les peces de servei per idioma, per blocar l' estat en fitxers al costat de cada fitxer font Markdown.

## Codis d' error

S' han informat d' errors usant un conjunt unificat de intervals:

Interval
|-------|----------|
100019991
2000999299
3000 2001- 1999
40099499
50005.000599

Cada error en un informe conté l' identificador de codi font (codi d' idioma, ruta de fitxer o nom d' escenari), el codi d' error i un missatge llegible.

## Tauler de traducció en directe

El projecte Servidor inclou una pàgina d' administrador en la que es connecta amb el senyal a la vegada i mostra tots els esdeveniments de canonada.

- Mostra l' estat de la connexió, comptador de missatges, i una taula de navegació en directe de tots els esdeveniments.
- Files codificades de color: blau per al començament de l' escenari, verd per a la compleció, vermell pels errors.
- Suporta netejar la font i exportar tots els missatges a JSON.
- Autoconnexió amb l' intercanvi exponencial si la connexió cau.

## Dissenya els principis

- **Modularity**: Each translation concern is isolated in its own service for maintainability and testability.
- **Incremental persistence**: Dictionaries and Markdown files are saved per-language immediately after translation, reducing memory pressure and providing earlier feedback.
- **Resilitat **: Múltiples nivells reintentar- ho (HTTP, etapa, bloc) assegura que els errors transitoris no bloquegen la canonada.
- **State tracking**: Per-file metadata (`.translation-meta.json`) and hash files enable precise incremental work on subsequent runs.
- **Real-time visibility**: Every significant operation is reported via SignalR for monitoring and debugging.
- **Manual translations always have priority over automatic additions.**
