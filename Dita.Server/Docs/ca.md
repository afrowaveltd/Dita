# Resum dels canvis al servei de traducció automàtica

## Resum

Aquest document resumeix tots els canvis fets al servei de traducció automàtica de la Dita, incloent la refactorització de l' arquitectura, noves característiques, millores de l' obsvbilitat, i millores de la localització.

## Canvis d' arquitectura

### Dorsal de redeestructuració delService

El monolitètic s'ha descomposat en quatre serveis especialitzats per un lleuger orquestrador:

- ** Retraducció del KSyrcationService ** Pipolílinetrator (traducció del servidor, delegació de l'escenari, gestió d' errors)
- **CountriesTranslationService** — Country name synchronization (English → target language)
- **LocalizationTranslationService** — JSON dictionary synchronization (added/removed keys)
- ** DocumentstrationService ** Markate Markdown translation amb seguiment de blocs
- **SignalRPublisher** — Real-time progress reporting via SignalR
- **TranslationRetryService** — Stage-level retry with placeholder preservation

### Bene corresponts

- **Separation of concerns**: Each service handles a single translation domain
- **Maintainability**: Smaller classes are easier to understand and test
- **Extenibilitat **: Es poden afegir nous objectius de traducció mitjançant la implementació de la interfície
- **Reliability**: Independent services provide better fault isolation

## Característiques noves

### Monitor de traducció en directe

**Location**: `/Admin/LiveTranslation`

Una nova pàgina d' administrador que proporciona visibilitat en temps real a la canonada de traducció:

- Mostra tots els senyals R esdeveniments mentre ocorren
- Tipus de missatge (blue=engegat, verd=completat, roig=error)
- Cartell de l' estat de connexió amb autoconnexió
- Comptador de missatges i exportació a JSON

### Variables de substitució amb nom

El sistema localització ara suporta variables de substitució de nom () per millorar la gramàtica en diferents idiomes:

```csharp
// Usage in code
var message = Localizer["WelcomeMessage", new Dictionary<string, string>
{
    ["userName"] = "John",
    ["count"] = "5"
}];
// Result: "Hello John, you have 5 new messages"
```

Característiques:
- Valors de substitució proporcionats a l' hora d' execució o desat
- Màscara automàtica/trestoració durant la traducció per evitar la corrupció
- Arrere compatible amb les variables de posició existents

### Traducció incremental

Els fitxers de majúscules es tradueixen incrementalment:

- **Per llengua estalviant ** Cada idioma de destí es desa immediatament després de la traducció, reduint la pressió de la memòria
- **Block-level tracking**: `.translation-meta.json` tracks translation status per block
- **Selective retry**: Only failed blocks are re-translated on the next run
- **Metadata persisteixence **: L' estat de traducció sobreviu l' aplicació reinicia

### Reintenta la lògica millorada

Tres nivells de resistència:

1. **HTTP torna a intentar- ho ** (LibretrateService): 5 intents amb l' ordre exponencial (1sIRC5s)
2. **Stage retry** (TranslationRetryService): 3 additional attempts with 30s delays
3. **Block retry** (DocumentsTranslationService): Failed Markdown blocks retried on next run

### Informe senyalR

Informe de progrés en temps real per a totes les operacions de canonada:

- Cada etapa publica esdeveniments
- En curs en llengua publicat com a esdeveniments
- Els esdeveniments d' error inclouen context detallat ( font, codi d' error, missatge)
- Els números de seqüència garanteix l' ordre dins de cada execució

## Canvis de configuració

### appsettings.json

Sense trencar canvis. La configuració existent continua treballant:

```json
{
  "AutomaticTranslationSettings": {
    "DefaultLanguage": "en",
    "IgnoredLanguages": ["auto", "detect"],
    "MarkdownRoots": ["/Docs"],
    "AutomaticRun": true,
    "CheckingPeriod": 30,
    "WaitingTime": "00:00:00"
  }
}
```

### Serveis nous

Registrat en:

- /
- `TranslationRetryService`
- /
- /
- /
- /

El senyal El centre està mapa en les connexions dels clients.

## Proves

### Estat de la prova

- **243/244 tests passing** (1 skipped due to concurrent file access in test environment)
- Nova cobertura de prova afegida per a:
  - Lloc de reserva Funcionalitat de servei
  - Dorsal de reducció Servei d' orquestració
  - Índexs de posicióLocalitzadors JsonString

### Limitacions conegudes

- la prova s' omet quan s' executa en paral· leles perquè múltiples instàncies de proves comparteixen el mateix fitxer. Passa quan s'aïllament.

## Estructura de fitxer nou

### Serveis en

- orquestra d' orquestra d' orquestra
- Traducció al nom del país de l' Amnistia
- sincronització del diccionari Noruega JSON
- Traducció a la part de traducció de la sortida
- Senyal  sign Publicació de missatges R
- Reintenta la lògica amb màscara de posició
- Interfície de publicació de l'Adjecte
- Interfície de servei de països de l' eka
- Interfície de servei per a la localització de l'AKIM
- Interfície de servei de documents Manveen
- Interfície d' eka Orchestrator (actualitzat)
- metadades de traducció de l' ekaPer fitxer

### Serveis actualitzats a

- Implementació de substitució Successful message after an user action
- 1] S' ha actualitzat pel nou paràmetre
- Gestió de marcadors de posició amb nom XLIFF mark type
- Interfície de reserva de substitució de substitució

### Nova pàgina d' administració en

- Pàgina de monitorització en temps real  1]
- Model de pàgina  1]

### Nova documentació en

- 1] S' ha actualitzat la documentació de canonada
- Guia del sistema de paràmetres de substitució de substitució de substitució
- Guia d' ús del tauler de l' Internet
- Resum d' arquitectura tècnica vertical

## Compatibilitat enrere

Tots els canvis són additius:

- El codi localització existent () no funciona
- El formatat posicional () no canvia
- El format existent del diccionari JSON no ha canviat
- L' estructura de majúscules existent no canvia
- Senyal Els missatges R usen el mateix format

## Camí de migració

No es requereix migració. El refactor és intern:

1. Antic va ser conservat com a referència i després substituït
2. S' han actualitzat els registres de registre per a usar noves interfícies
3. Tots els consumidors existents no veuen canvis

## Millores de rendiment

- **Reduït ús de memòria **: Fitxers desats per llengua immediatament en lloc de mantenir tots els de memòria
- **Faster incremental runs**: Only changed/failed Markdown blocks are re-translated
- ** Millor visibilitat **: El progrés en temps real ajuda a diagnosticar fases lentes

## Millora futures

Millores planificades:

1. **AI fi fi-tating * _BAR_La traducció de la màquina de post-màquina per frases > 5 paraules
2. **Admin authentication** — Restrict admin pages to authorized users
3. **Dictionary editor** — Web UI for managing localization keys
4. **Translation statistics** — Charts showing translation counts and error rates over time
5. **Custom placeholder syntax** — Support for alternate placeholder formats

## Contacte

Per a preguntes o problemes amb el servei de traducció, si us plau, referiu- vos a la documentació detallada en el directori de cada mòdul o contacteu amb l' equip de desenvolupament.
