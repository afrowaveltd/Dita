# Traducións en tempo real

Este documento existe como unha entrada de proba en directo para a tradución automática. Calquera cambio neste ficheiro desencadea a re-tradución de todos os ficheiros de idioma de destino na seguinte execución programada.

## Arquitectura Visión

O oleoduto de tradución foi reestruturado nunha arquitectura modular con catro subservizos especializados coordinados por un orquestrador lixeiro

- **BackendTranslationService** — Orchestrates the entire pipeline, handles server validation, and delegates work to sub-services.
- **CountriesTranslationService** - Sincroniza os nomes dos países de cada dicionario.
- **LocalizationTranslationService** — Detects added/removed keys in the default JSON dictionary and translates them into target languages.
- **DocumentsTranslationService** – Traduce ficheiros de documentación de Markdown con seguimento por bloque e metadatos.

Cada sub-servizo opera de forma independente e informa de progreso a través de SignalR en tempo real.

## O que fai o servizo

O servizo funciona nun horario e executa un oleoduto de cinco etapas: validación do servidor, sincronización do país, sincronización do dicionario JSON, tradución de ficheiros Markdown e persistindo os resultados. Cada etapa emite eventos de progreso en tempo real estruturados sobre SignalR para que os clientes conectados poidan seguir a medida que avanza o traballo.

## Fases da Pipelina

### Etapa 1 - CheckServers

Antes de iniciar calquera traballo de tradución, o servizo verifica que todas as condicións previas están satisfeitas

- A sección de configuración debe estar presente e válida.
- O servidor debe responder dentro dunha latencia aceptable.
- A lista de idiomas dispoñibles no servidor de tradución.
- O idioma predeterminado debe estar presente nesta lista.
- Os ficheiros JSON que faltan para calquera idioma compatible créanse automaticamente.

Se falla algunha comprobación, o oleoduto detense inmediatamente e emítese unha mensaxe.

### Etapa 2 - Tradutores

Os nomes dos países mantéñense sincronicamente desde un catálogo de só lectura () ata os dicionarios JSON.

- Se o idioma por defecto da aplicación é o inglés, cada nome do país almacénase sen tradución.
- Se a lingua por defecto é calquera outra lingua, primeiro se traduce o nome do país nesa lingua, e o resultado convértese na entrada do dicionario por defecto.
- After the default dictionary is updated, each missing country entry in every target language dictionary is translated and saved **immediately per language**.
- As entradas xa traducidas son conservadas sen modificacións.
- Se unha tradución falla, o servizo repítese ata tres veces con atrasos de 30 segundos antes de pasar á seguinte lingua.

### Páxina 3 — TranslateJsonFiles

O servizo compara o actual dicionario de localización por defecto cunha instantánea almacenada na execución anterior:

- **Added keys** — entries present in the current default but absent from the snapshot — are translated into every target language that does not already have a manual entry for that key.
- **Removed keys** — entries present in the snapshot but absent from the current default — are deleted from every target language dictionary.
- As traducións sempre teñen prioridade. Se un dicionario de destino xa contén un valor para unha clave, esta entrada non cambia independentemente do que di a fonte.
- **Each target language dictionary is saved immediately after its translations complete**, rather than waiting for all languages to finish.
- Se unha tradución falla nun idioma específico, o servizo repítese automaticamente. Só os erros persistentes (por exemplo, a linguaxe non soportada) fan que esa linguaxe se borre.
- Despois da execución, o dicionario por defecto actual gárdase como a nova instantánea para a seguinte comparación.

Todos os dicionarios almacénanse sempre con chaves ordenadas alfabeticamente e JSON indentado para lexibilidade humana.

### 4a etapa: Tradutores

O servizo percorre as raíces da documentación configurada (default:) e procesa de forma recursiva cada ficheiro fonte:

1. O contido do ficheiro fonte é lido e calcúlase un hash SHA-256.
2. A `.translation-meta.json` file next to the source tracks per-language, per-block translation status, enabling **incremental re-translation** of only failed blocks.
3. O hash almacenado da anterior execución (cept nun ficheiro xunto ao ficheiro fonte, ou nunha localización temporal de retorno) é comparado co hash actual.
4. Para cada lingua de destino, o ficheiro correspondente tamén se comproba para a integridade estrutural.
5. Calquera ficheiro de destino que falta, ten un hash desactualizado, falla a validación da estrutura, ou contén bloques non traducidos é cola para a re-tradución.
6. **Each target language is translated and saved independently** — if Czech succeeds but French fails, the Czech file is still written to disk.
7. Os ficheiros traducidos con éxito son validados para a paridade estrutural coa fonte (contas de cabeceira iguais, artigos de lista, bloques de código, citas de bloqueo, ligazóns, marcadores audaces e etiquetas HTML) antes de que sexan escritos en disco.
8. Se todos os ficheiros de destino para un éxito fonte, o novo hash almacénase xunto á fonte. Se escribir xunto á fonte falla (por exemplo, nas implementacións de só lectura), o hash volve ao directorio temporal.
9. Se calquera tradución de destino falla de validación, os metadatos marcan eses bloques como non traducidos, polo que son recuperados na seguinte carreira.

### Fase 5 - Resultados

Un libro consolidado está editado e publicado. Inclúe:

- UTC hora de inicio e finalización.
- Contadores de arquivos JSON gardados, arquivos Markdown gardados, hash gardados e hash escribir.
- Erros de almacenamento recollidos durante a execución.
- Estatísticas de tradución por lingua (conteo traducido, conta de patróns, conta de erro).

## Mensaxe de SignalR

Cada evento de progreso entrégase como a cos seguintes campos:

Campo
|-------|------|-------------|
Identificador de Correlación para o gasoduto actual
Contador monotónico dentro dunha carreira, comezando en 1
Tipo semántico da mensaxe
Pipeline, a mensaxe é
UTC cando se envía a mensaxe
Se a mensaxe representa unha condición de erro
Resumo lexible por humanos
Carga específica de etapa (obxecto de informe ou nulo)

### Tipos de mensaxes

Valor
|-------|------|---------|
0
1
2
3
4
5
6

### Fases da Pipelina

Valor
|-------|------|-------------|
0
1
2
3
4
5

### Típico fluxo de mensaxe

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

Se falla algunha etapa, as etapas restantes son saltadas, emítese unha mensaxe e, finalmente, unha mensaxe pecha a execución.

## retry logic

O gasoduto desenvolve dous niveis de resistencia:

### Retry Level (Tradución)

- Se unha solicitude de tradución falla despois das repeticións internas de LibreTranslate, o resultado é de ata 3 repeticións de nivel adicional con 30 segundos de atraso.
- Enmascaramento do marcador de posición: Os propietarios de lugares nomeados () no texto son temporalmente substituídos por tokens seguros () antes da tradución e restaurados despois, asegurando unha correcta gramática nas linguas obxectivo.

### Validación lingüística

- Antes de traducir a unha lingua de destino, o servizo verifica o idioma é soportado polo servidor de tradución.
- As linguas non soportadas son saltadas cun aviso, evitando intentos repetidos.

### Retry nivel de bloqueo

- As traducións de marcado realízanse bloque por bloque (cabezas, parágrafos, elementos de lista).
- Se un bloque individual falla a tradución, está marcado como non traducida no ficheiro de metadatos e retribuída no seguinte curso.
- As pistas de servizo por idioma, estado por bloque en ficheiros xunto a cada ficheiro de código fonte.

## Códigos de erro

Os erros infórmase usando un enum unificado agrupado en rangos:

rango
|-------|----------|
1000–1999
2000–2999
3000-3999
4000-4999
5000-5999

Cada erro nun informe leva o identificador de orixe (código de idioma, rota de arquivo ou nome do estado), o código de erro e unha mensaxe lexible por humanos.

## páxina de tradución ao vivo

O proxecto Server inclúe unha páxina de administración na que se conecta ao hub de SignalR e amosa todos os eventos de oleodutos en tempo real.

- Amosa o estado de conexión, o reconto de mensaxes e unha táboa actualizada en directo de todos os eventos.
- Liñas de cores: azul para o inicio do escenario, verde para a conclusión, vermello por erros.
- Soporte para limpar o feed e exportar todas as mensaxes a JSON.
- Auto-reconexións con backup exponencial se a conexión cae.

## Principios de deseño

- **Modularity**: Each translation concern is isolated in its own service for maintainability and testability.
- **Incremental persistence**: Dictionaries and Markdown files are saved per-language immediately after translation, reducing memory pressure and providing earlier feedback.
- **Resilience**: Multiple retry levels (HTTP, stage, block) ensure transient failures do not block the pipeline.
- **State tracking**: Per-file metadata (`.translation-meta.json`) and hash files enable precise incremental work on subsequent runs.
- **Real-time visibility**: Every significant operation is reported via SignalR for monitoring and debugging.
- **Manual translations always have priority over automatic additions.**
