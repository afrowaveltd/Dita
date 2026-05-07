# Traducciones en tiempo real

Este documento existe como entrada de prueba en vivo para el oleoducto de traducción automática. Cualquier cambio en este archivo activa la re-traducción de todos los archivos de lenguaje de destino en la próxima ejecución programada.

## Resumen de la arquitectura

El gasoducto de traducción ha sido reestructurado en una arquitectura modular con cuatro subservicios especializados coordinados por un orquestador ligero:

- **BackendTranslationService** — Orquesta toda la tubería, maneja la validación del servidor y los delegados trabajan en los subservicios.
- **CountriesTranslationService** — Synchronizes country names from into per-language dictionaries.
- **LocalizationTranslationService** — Detecta claves agregadas/removidas en el diccionario JSON predeterminado y las traduce en lenguajes de destino.
- **DocumentosTranslationService** — Translates Markdown documentation files with per-block tracking and metadata.

Cada subservicio opera independientemente e informa de los avances a través de SignalR en tiempo real.

## Qué hace el servicio

El servicio funciona en un horario y ejecuta un gasoducto de cinco etapas: validación del servidor, sincronización del país, sincronización del diccionario JSON, traducción del archivo Markdown y persistencia de los resultados. Cada etapa emite eventos de progreso estructurados en tiempo real sobre Signal R para que los clientes conectados puedan seguir a medida que avanza el trabajo.

## Etapas de tubería

### Etapa 1 - CheckServers

Antes de comenzar cualquier trabajo de traducción, el servicio verifica que todas las condiciones previas estén satisfechas:

- La sección de configuración debe estar presente y válida.
- El servidor LibreTranslate debe responder dentro de una latencia aceptable.
- Se incluye la lista de idiomas disponibles en el servidor de traducción.
- El idioma predeterminado configurado debe estar presente en esa lista.
- Los archivos JSON perdidos para cualquier idioma compatible se crean automáticamente.

Si falla algún cheque, el oleoducto se detiene inmediatamente y se emite un mensaje.

### Etapa 2 — TraducirLos países

Los nombres de los países se guardan en sincronía de un catálogo de sólo lectura () en los diccionarios JSON localización.

- Si el idioma predeterminado de la aplicación es inglés, cada nombre de país se almacena como sin traducción.
- Si el idioma predeterminado es cualquier otro idioma, el nombre del país inglés se traduce primero en ese idioma, y el resultado se convierte en la entrada en el diccionario predeterminado.
- Después de que se actualice el diccionario predeterminado, cada entrada de país que falta en cada diccionario de idioma objetivo se traduce y se guarda ** inmediatamente por idioma**.
- Las entradas ya traducidas se conservan sin modificaciones.
- Si una traducción falla, el servicio se registra hasta 3 veces con retrasos de 30 segundos antes de pasar al siguiente idioma.

### Etapa 3 — TranslateJsonFiles

El servicio compara el diccionario de localización predeterminado actual con una instantánea almacenada desde la ejecución anterior:

- **Las claves agregadas** — las entradas presentes en el defecto actual pero ausentes de la instantánea— se traducen en cada idioma objetivo que no tiene ya una entrada manual para esa clave.
- **Las claves modificadas** — las entradas presentes en la instantánea pero ausentes del predeterminado actual— se eliminan de cada diccionario de idioma objetivo.
- Las traducciones manuales siempre tienen prioridad. Si un diccionario de destino ya contiene un valor para una clave, esa entrada se deja sin cambios independientemente de lo que diga la fuente.
- **Cada diccionario de idioma objetivo se guarda inmediatamente después de que sus traducciones completen**, en lugar de esperar que todos los idiomas terminen.
- Si una traducción falla para un idioma específico, el servicio se vuelve automáticamente. Sólo errores persistentes (p. ej., lenguaje no soportado) hacen que el lenguaje sea saltado.
- Después de la ejecución, el diccionario predeterminado actual se guarda como la nueva instantánea para la próxima comparación.

Todos los diccionarios siempre se almacenan con teclas ordenadas alfabéticamente y JSON se identifica para la legibilidad humana.

### Etapa 4 — Traducir los mercados

El servicio recorre las raíces de documentación configuradas (predeterminado: ) y procesa cada archivo fuente recursivamente:

1. Se lee el contenido del archivo fuente y se calcula un hash SHA-256.
2. Un archivo junto a las pistas de origen por idioma, estado de traducción por bloque, permitiendo ** re-traducción incremental** de sólo bloques fallidos.
3. El hash almacenado de la ejecución anterior (que se guarda en un archivo al lado del archivo fuente, o en una ubicación temporal de retroceso) se compara con el hash actual.
4. Para cada idioma objetivo, el archivo correspondiente también se verifica para la integridad estructural.
5. Cualquier archivo de destino que falta, tiene un hash anticuado, falla la validación de la estructura, o contiene bloques no traducidos es apagado para la re-traducción.
6. **Cada idioma objetivo es traducido y guardado independientemente** — si checo tiene éxito pero el francés falla, el archivo checo todavía está escrito al disco.
7. Los archivos traducidos exitosamente se validan para la paridad estructural con la fuente (equal heading counts, list items, code blocks, blockquotes, links, bold/italic markers, and HTML tags) antes de que se escriban al disco.
8. Si todos los archivos de destino para una fuente tienen éxito, el nuevo hash se almacena junto a la fuente. Si la escritura junto a la fuente falla (por ejemplo, en los despliegues de sólo lectura), el hash vuelve al directorio temporal.
9. Si alguna traducción de destino falla la validación, los metadatos marcan esos bloques como no traducidos por lo que se retiran en la siguiente carrera.

### Etapa 5 — Resultados de la búsqueda

Se reúne y publica un consolidado. Incluye:

- UTC ejecuta timetamps de inicio y terminación.
- Cuentas de archivos guardados locale JSON, archivos guardados Markdown, archivos de hash guardados, y hash fallback escribe.
- Cualquier error de almacenamiento recogido durante la ejecución.
- Estadísticas de traducción por idioma (conteo traducido, cuenta saltada, cuenta de error).

## Signal Sobre del mensaje

Cada evento de progreso se realiza como a con los siguientes campos:

Campo
|-------|------|-------------|
Identificador de correlación para el funcionamiento de tubería actual
Contador monotónico dentro de una carrera, comenzando a 1
Tipo semántico del mensaje
Etapa de la tubería el mensaje pertenece a
Hora de la UTC cuando el mensaje fue emitido
Si el mensaje representa una condición de error
Resumen legible por el hombre
Carga de pago específica (objeto de presentación o nulo)

### Tipos de mensaje

Valor
|-------|------|---------|
0
1
2
3
4
5
6

### Etapas de tubería

Valor
|-------|------|-------------|
0
1
2
3
4
5

### Flujo de mensaje típico

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

Si alguna etapa falla, las etapas restantes se saltan, se emite un mensaje, y finalmente un mensaje cierra la carrera.

## Traducción lógica retry

El oleoducto implementa dos niveles de resiliencia:

### Retry a nivel de estadio (TranslationRetryService)

- Si una solicitud de traducción falla después de los registros internos de LibreTranslate, los resultados de hasta 3 series adicionales de nivel de etapa con 30 segundos de retrasos.
- Enmascaramiento del marcador: Los titulares de lugar designados () en texto son reemplazados temporalmente por fichas seguras () antes de la traducción y restaurado después, asegurando la gramática correcta en los idiomas de destino.

### Validación del idioma

- Antes de traducir a un idioma objetivo, el servicio verifica el idioma es apoyado por el servidor de traducción.
- Los idiomas no compatibles se saltan con una advertencia, evitando repetidos intentos fallidos.

### Regreso a nivel de bloques

- Las traducciones de marcación se realizan de bloque por bloque (cabezas, párrafos, lista de elementos).
- Si un bloque individual falla la traducción, está marcado como no traducido en el archivo de metadatos y retrigado en el próximo funcionamiento del gasoducto.
- Las pistas de servicio por idioma, estado de bloqueo en los archivos junto a cada archivo Markdown fuente.

## Códigos de error

Los errores se reportan utilizando un enum unificado agrupado en rangos:

Rango
|-------|----------|
1000–1999
2000–2999
3000–3999
4000–4999
5.000 a 5999

Cada error en un informe lleva el identificador fuente (código de idioma, ruta de archivo o nombre de estadio), el código de error, y un mensaje legible por humanos.

## Traducción en vivo

El proyecto Server incluye una página de administración en la que se conecta al centro SignalR y muestra todos los eventos de tubería en tiempo real.

- Muestra el estado de conexión, el recuento de mensajes y una tabla de actualización de todos los eventos.
- Filas codificadas por colores: azul para el inicio del escenario, verde para la terminación, rojo para errores.
- Soporta limpiar el pienso y exportar todos los mensajes a JSON.
- auto-reconexión con retroceso exponencial si la conexión cae.

## Principios de diseño

- **Modularidad**: Cada preocupación por la traducción está aislada en su propio servicio de mantenimiento y testabilidad.
- **La persistencia incremental**: Los diccionarios y archivos Markdown se guardan por idioma inmediatamente después de la traducción, reduciendo la presión de memoria y proporcionando comentarios anteriores.
- **Resilience**: Múltiples niveles de retry (HTTP, estadio, bloque) aseguran que los fallos transitorios no bloquean el oleoducto.
- ** Seguimiento de los Estados**: Los metadatos per-file () y los archivos hash permiten un trabajo incremental preciso en carreras posteriores.
- **La visibilidad del tiempo real**: Cada operación significativa se reporta a través de SignalR para monitoreo y depuración.
- **Manual translations always have priority over automatic additions.**
