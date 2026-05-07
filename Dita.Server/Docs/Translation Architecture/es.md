# Traducción Arquitectura

Este documento describe la arquitectura modular del sistema de traducción automática de Dita, introducido para mejorar la capacidad de mantenimiento, testabilidad y resiliencia.

## Objetivos de diseño

el refactoring abordó varias preocupaciones con el diseño monolítico original:

- **Separación de las preocupaciones**: Cada dominio de traducción (cuentas, diccionarios JSON, Markdown) está aislado.
- **La persistencia incremental**: Los archivos se guardan por idioma inmediatamente después de la traducción, reduciendo el uso de la memoria y proporcionando resultados anteriores.
- **Resilience**: Múltiples niveles de retry manejan fallos transitorios sin bloquear todo el oleoducto.
- **Observabilidad**: Cada operación significativa se reporta a través de SignalR para el monitoreo en tiempo real.
- **Extensibilidad**: Se pueden añadir nuevos objetivos de traducción mediante la implementación de una única interfaz.

## Descomposición de servicios

### BackendTranslationService (orchestrator)

** responsabilidades**:
- Manejo de ciclo de vida de tuberías (inicio, finalización, manejo de errores)
- Control de concurrencia basado en la semafora (preventos superpuestos)
- Validación del servidor (latencia, disponibilidad de idiomas, configuración)
- Delegación a los subservicios

** NO contiene**:
- Traducir lógica
- Archivo I/O para formatos específicos
- Retry logic

### países de traducción

** responsabilidades**:
- Lea desde el directorio
- Sincronizar los nombres de los países en el diccionario local predeterminado
- Traducir nombres de países desaparecidos por idioma objetivo
- Guardar cada diccionario de destino inmediatamente después de la traducción

**Comportamientos clave**:
- Si el idioma predeterminado es inglés: nombres de países almacenados como
- Si el idioma predeterminado es otro: Nombres en inglés traducidos al idioma predeterminado primero
- Cada idioma se procesa independientemente con su propio bucle de retry

### LocalizationTranslationService

** responsabilidades**:
- Detectar claves agregadas/removidas comparando el diccionario predeterminado actual con instantáneas anteriores
- Traducir claves agregadas en cada idioma objetivo
- Eliminar las teclas eliminadas de cada idioma de destino
- Guardar instantáneas para la próxima comparación

**Comportamientos clave**:
- Las traducciones manuales siempre toman prioridad (nunca sobrescrito)
- Las teclas agregadas se traducen y se guardan por idioma inmediatamente
- Las teclas eliminadas se eliminan por idioma inmediatamente
- Snapshot se guarda sólo después de que todos los idiomas completen con éxito

### documentos traducción

** responsabilidades**:
- Caminar las raíces de Markdown configuradas recursivamente
- Detectar archivos fuente cambiados usando hashes SHA-256
- Seguimiento de la traducción por bloque estado en
- Traducir bloque-por-bloqueo con la retry de bloqueo
- Validar estructura de marcado después de la traducción
- Guardar cada archivo de idioma objetivo de forma independiente

**Comportamientos clave**:
- Granularidad de nivel de bloque: partidas, párrafos, listas se traducen por separado
- Temas de metadatos que bloquean el éxito/failado por idioma
- Los bloques fallidos se retratan en la próxima carrera sin volver a traducir bloques exitosos
- La validación de la estructura garantiza los recuentos de encabezado, listas, bloques de código, etc

## Estrategia de reingreso

El sistema implementa retries a tres niveles:

### Nivel 1 — HTTP (LibreTranslateService)

- Hasta 5 intentos con retroceso exponencial (1s, 2s, 3s, 4s, 5s)
- Maneja tiempo de red, errores de 5xx y fallos transitorios
- Construido en la configuración del cliente HTTP

### Nivel 2 - Etapa (TraducciónRetryService)

- Hasta 3 intentos con 30 segundos retrasos
- Recupera toda la solicitud de traducción después de que se agoten los registros de nivel HTTP
- El enmascaramiento y la restauración del titular se aplican a este nivel

### Nivel 3 — Bloque (DocumentosTranslationService)

- Los bloques de marcado individual que fallan están marcados en metadatos
- Retried automaticamente en la próxima operación de tubería
- Los bloques exitosos nunca vuelven a traducirse

## Flujo de datos

### Traducción de JSON

```
[Default Dictionary]
       ↓
[Compare with Snapshot]
       ↓
[Detect Added/Removed Keys]
       ↓
For each target language:
   ├─ Load existing dictionary
   ├─ Apply removals
   ├─ Translate additions (with retry)
   └─ Save dictionary immediately
       ↓
[Save new snapshot]
```

### Traducción de Markdown

```
[Source File]
       ↓
[Compute Hash]
       ↓
[Compare with stored hash]
       ↓
[Load translation metadata]
       ↓
For each target language:
   ├─ Extract translatable blocks
   ├─ For each block:
   │   ├─ Check metadata (already translated?)
   │   ├─ Translate with retry
   │   └─ Mark success/failure in metadata
   ├─ Reconstruct Markdown
   ├─ Validate structure
   ├─ Save target file
   └─ Save metadata
```

### Traducción del nombre del país

```
[countries.json]
       ↓
[Build set of country keys]
       ↓
[Update default dictionary]
       ↓
For each target language:
   ├─ Find missing country entries
   ├─ Translate each missing entry (with retry)
   └─ Save dictionary immediately
```

## Perseverencia del Estado

### Juegos de rol

- **JSON**: Almacenado en un archivo junto al diccionario predeterminado (el nombre varía según el proveedor de almacenamiento)
- **Purpose**: Permite la sincronización incremental siguiendo lo que estaba presente en la carrera anterior

### Archivos de Hash

- **Markdown**: junto al archivo fuente
- **Fallback**: si la ubicación principal es sólo lectura
- **Purpose**: Detecta cambios de fuente para evitar la retraducción innecesaria

### Metadatos de traducción

- **abajo**:
- **Contenidos**:
  - Contenido fuente hash
- Estado de bloques por idioma (arreo de booleanos)
- Última actualización timetamp
- **Purpose**: Permite la retranslación parcial de sólo bloques fallidos

### Almacenamiento de propietarios

- **File**:
- **Contenidos**: Diccionario de claves para pares de valor de nombre de marcadores
- **Purpose**: Provee valores predeterminados para los titulares nombrados a través de la aplicación

## Signal R reporting

### Abstracción del editor

decouples servicios de traducción de SignalR específicos:

```csharp
public interface ISignalRPublisher
{
    Task PublishStageAsync<T>(Guid runId, ProcessStage stage, T data, ...);
    Task PublishMessageAsync(Guid runId, LocalizationMessageType type, ProcessStage stage, ...);
}
```

### Garantías de secuencia

- Los mensajes dentro de una sola carrera son monotonicamente secuenciados
- Los números de secuencia son únicos por funcionamiento
- Los clientes pueden detectar lagunas o reordenar

### Cartografía del Hub

```csharp
app.MapHub<LocalizationHub>("/hubs/localization");
```

## Puntos de extensión

### Agregar un nuevo objetivo de traducción

1. Crear una nueva interfaz con
2. Implementar la interfaz con lógica de dominio específico
3. Registro en contenedor DI
4. Inyecte en constructor
5. Llamada después de las etapas existentes

### Política de reingreso aduanero

Superar los parámetros del constructor:

```csharp
services.AddSingleton<TranslationRetryService>(
    sp => new TranslationRetryService(
        sp.GetRequiredService<ILibreTranslateService>(),
        sp.GetRequiredService<IPlaceholderService>(),
        sp.GetRequiredService<ILogger<TranslationRetryService>>(),
        stageMaxRetries: 5,        // More retries
        stageRetryDelaySeconds: 60  // Longer delays
    ));
```

### Manejo de marcadores de posición personalizados

Implementar para cambiar la sintaxis o almacenamiento de marcadores de posición:

```csharp
public class CustomPlaceholderService : IPlaceholderService
{
    // Use {{name}} syntax instead of {name}
    // Store in database instead of JSON file
    // Encrypt sensitive placeholder values
}
```

## Configuración

### appsettings.json

```json
{
  "AutomaticTranslationSettings": {
    "DefaultLanguage": "en",
    "IgnoredLanguages": ["auto", "detect"],
    "MarkdownRoots": ["/Docs", "/Help"],
    "AutomaticRun": true,
    "CheckingPeriod": 30,
    "WaitingTime": "00:00:00",
    "RequestThrottleMs": 80,
    "RequestTimeoutSeconds": 10
  }
}
```

### Ajuste del tiempo de ejecución

Ajuste
|---------|---------|--------|
80
10
3
30

## Estrategia de ensayo

### Pruebas de unidad

Cada subservicio es prueba independiente:

- Mock to simulate success/failure
- Mock to verify reporting
- Utilice directorios temporales para el archivo I/O
- Verificar comportamiento de ahorro por idioma

### Pruebas de integración

- Gasoducto completo corre con instancia real (local) LibreTranslate
- Verificar la señal R mensajes se envían a clientes conectados
- Prueba de prevención de ejecución simultánea (semaphore)
- Validar estructura de marcado después de la traducción

### Pruebas de extremo a extremo

- Traducción de trucos a través de API o programador
- Verificar todos los archivos de idioma de destino se crean o actualizan
- Controlar los archivos de metadatos contienen el estado de bloque correcto
- Confirma que los titulares de puestos se conservan a través de traducciones

## Consideraciones de la ejecución

- **Memoria**: El ahorro por idioma evita mantener todos los diccionarios en memoria
- **Disk I/O**: Los archivos de metadatos añaden una pequeña sobrecabeza pero permiten un trabajo incremental
- **Network**: El procesamiento secuencial con la lucha evita la abrumadora LibreTranslate
- **CPU**: SHA-256 El hashing y la validación del regex son rápidos relativos a la latencia de la traducción
- **SignalR**: Mensajes ligeros, sin compresión de carga útil necesaria para informes típicos

## Migración del diseño monolítico

El original contenía toda lógica en una clase. La vía migratoria:

1. extracto lógica del país →
2. extracto json lógica →
3. Extractar lógica de marcado →
4. Extract Signal R publishing →
5. extracto retry lógica →
6. Simplifique el orquestador solo para la delegación

Todas las interfaces existentes () permanecen sin cambios. Los consumidores del oleoducto no ven cambios de ruptura.
