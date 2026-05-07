# Arquitectura de tradución

Este documento describe a arquitectura modular do sistema de tradución automática de Dita, introducido para mellorar o mantemento, a probabilidade e a resiliencia.

## Obxectivos de deseño

A refactorización abordou varias preocupacións co deseño monolítico orixinal:

- **Separation of concerns**: Each translation domain (countries, JSON dictionaries, Markdown) is isolated.
- **Incremental persistence**: Files are saved per-language immediately after translation, reducing memory usage and providing earlier results.
- **Resilience**: Multiple retry levels handle transient failures without blocking the entire pipeline.
- **Observability**: Every significant operation is reported via SignalR for real-time monitoring.
- **Extensibility**: New translation targets can be added by implementing a single interface.

## Descomposición do servizo

### servizo de tradución (orchestrator)

**Responsibilities**:
- Xestión do ciclo de vida Pipino (comezar, completar, manipular o erro)
- Control de concorrencia baseada en Semaphore (corres superpostos de preventes)
- Validación do servidor (latencia, dispoñibilidade do idioma, configuración)
- Delegación en subservizos

**Does NOT contain**:
- Tradución lóxica
- Arquivo I/O para formatos específicos
- Retry Logic

### Países de tradución

**Responsibilities**:
- Ler desde o directorio
- Sincronizar os nomes dos países no dicionario de localización por defecto
- Traducir nomes de país perdidos por idioma obxectivo
- Gardar todos os dicionarios inmediatamente despois da tradución

**Key behaviors**:
- Se a lingua por defecto é o inglés: country names stored as-is
- Se o idioma por defecto é outro: os nomes en inglés traducíronse primeiro ao idioma por defecto
- Cada lingua é procesada de forma independente co seu propio bucle de retry

### servizo de tradución de localización

**Responsibilities**:
- Detectar teclas engadidas/removedas comparando o dicionario por defecto actual coa instantánea anterior
- Traducir claves engadidas en cada idioma obxectivo
- Eliminar as claves de cada idioma obxectivo
- Gardar imaxes para a seguinte comparación

**Key behaviors**:
- As traducións manuais sempre teñen prioridade (nunca sobrescrito)
- As claves engadidas son traducidas e gardadas por idioma inmediatamente
- Elimina as chaves por idioma inmediatamente
- Snapshot é gardado só despois de que todas as linguas sexan completadas con éxito

### Documentos de tradución

**Responsibilities**:
- Camiña as raíces marcadas recursivamente
- Detectar arquivos de código fonte con hashes SHA-256
- Track per-block translation status in
- Traducir block-by-block con retry
- Validar a estrutura da marca despois da tradución
- Gardar cada ficheiro de idioma obxectivo de forma independente

**Key behaviors**:
- Granularidade de nivel de bloqueo: cabeceiras, parágrafos, artigos de lista son traducidos por separado
- Trazos de metadatos que bloquean o éxito por idioma
- Bloques fallados son retribuídas na seguinte carreira sen volver traducir bloques exitosos
- A validación de estruturas asegura os recontos de títulos, listas, bloques de código, etc

## Estratexia de recuperación

O sistema usa retries a tres niveis:

### Nivel 1 - HTTP (LibreTranslateService)

- Ata 5 intentos con backup exponencial (1s, 2s, 3s, 4s, 5s)
- Xestionar o tempo de acceso á rede, 5xx erros e fallos transitorios
- Configuración do cliente HTTP

### Nivel 2 - Etapa (TranslationRetryService)

- Ata 3 intentos con 30 segundos de atraso
- Re-drive toda a solicitude de tradución despois de que as versións de nivel HTTP estean esgotadas
- A máscara e a restauración son aplicadas a este nivel

### Nivel 3 - Bloque (DocumentsTranslationService)

- Bloques de marcas individuais que fallan están marcados en metadatos
- Retribuída automaticamente no seguinte curso
- Os bloques nunca foron traducidos

## Fluxo de datos

### JSON Dicionario de tradución

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

### Markdown Tradución

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

### Nome do país tradución

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

## Estado de persistencia

### fotos

- **JSON**: Stored in a file next to the default dictionary (name varies by storage provider)
- **Purpose**: Permite a sincronía incremental rastrexando o que estaba presente na versión anterior

### Arquivos de Hash

- **Markdown**: `{sourceFile}.hash.json` next to the source file
- **Fallback**: `{tempDir}/{sanitizedPath}.hash.json` if primary location is read-only
- **Purpose**: Detectar cambios de orixe para evitar unha reescritura innecesaria

### Tradución metadatos

- **Markdown**: `{sourceFile}.translation-meta.json`
- **Contents**:
  - Contido: hash
- Estado de bloqueo por idioma (array of booleans)
- Última actualización de Timestamp
- **Purpose**: Permite a tradución parcial de só bloques fallidos

### Almacenamento de localizadores

- **File**: `Locales/placeholders.json`
- **Contentes**: Dicionario de claves para os pares de valor de nome do titular
- **Purpose**: proporciona valores por defecto para os localizadores nomeados a través da aplicación

## Sinal R Información

### Editor de abstracción

servizos de tradución de SignalR específicos:

```csharp
public interface ISignalRPublisher
{
    Task PublishStageAsync<T>(Guid runId, ProcessStage stage, T data, ...);
    Task PublishMessageAsync(Guid runId, LocalizationMessageType type, ProcessStage stage, ...);
}
```

### Garantías de secuencia

- As mensaxes nunha soa dirección son monotonicamente secuenciadas
- Os números de secuencia son únicos por curso
- Os clientes poden detectar ocos ou reorganizar

### Hub mapeo

```csharp
app.MapHub<LocalizationHub>("/hubs/localization");
```

## Puntos de extensión

### Engadir un novo obxectivo de tradución

1. Crea unha nova interface
2. Implementar a interface coa lóxica específica de dominio
3. Rexistro en contacontos
4. Inxectado en constructor
5. Datas desde as etapas existentes

### Política de devolución personalizada

Parámetros do construtor:

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

### Manexo de locais personalizados

Implementar para cambiar a sintaxe ou almacenamento do marcador de lugar:

```csharp
public class CustomPlaceholderService : IPlaceholderService
{
    // Use {{name}} syntax instead of {name}
    // Store in database instead of JSON file
    // Encrypt sensitive placeholder values
}
```

## Configuración

### appettings.json

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

### Runtime tuning

Establecer
|---------|---------|--------|
80
10
3
30

## Estratexia de proba

### Unidade de probas

Cada sub-servizo pode ser probado de forma independente:

- Mock para simular éxito / fracaso
- Mock para comprobar a información
- Use directorios temporais para o ficheiro I/O
- Comprobar o comportamento de aforro por idioma

### Probas de integración

- Oleoduto completo con instancia LibreTranslate (local)
- Comprobe o sinal As mensaxes R son enviadas a clientes conectados
- Prevención concorrente (semaphore)
- Validar a estrutura da marca despois da tradución

### Probas finais

- Tradución por API ou scheduler
- Comprobar que todos os ficheiros de idioma de destino son creados / actualizados
- Comprobar os ficheiros de metadatos que conteñen o estado do bloque correcto
- Os localizadores de seguridade son preservados en traducións

## Consideracións de rendemento

- **Memory**: Per-language saving prevents holding all dictionaries in memory
- **Disk I/O**: Metadata files add small overhead but enable incremental work
- **Network**: O procesamento secuencial con throtling evita a abafadora LibreTranslate
- **CPU**: SHA-256 hashing and regex validation are fast relative to translation latency
- **SignalR**: Mensaxes lixeiras, sen compresión de carga necesaria para informes típicos

## Migración do deseño monolítico

O orixinal contiña toda a lóxica dunha clase. O camiño da migración:

1. Extraer lóxica do país
2. Extraer JSON lóxica
3. Extraer lóxica Markdown
4. Extraer sinal R publicación
5. Extraer lóxica de retry →
6. Simplificación de orquestras para delegacións

Todas as interfaces existentes permanecen inalteradas. Os consumidores do gasoduto non ven cambios.
