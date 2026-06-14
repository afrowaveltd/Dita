# Cambios no servizo de tradución automática

## Visión

Este documento resume todas as modificacións realizadas no servizo de tradución automática Dita, incluíndo a refactorización de arquitectura, novas características, melloras de observatorio e melloras de localización.

## Cambios de arquitectura

### BackendTranslationService Refactored

O monolítico descomponse en catro servizos especializados coordinados por un orquestrador lixeiro

- **BackendTranslationService** — Pipeline orchestrator (server validation, stage delegation, error handling)
- **CountriesTranslationService** — Sincronización de nomes de país (inglés → lingua de destino)
- **LocalizationTranslationService** — JSON dictionary synchronization (added/removed keys)
- **DocumentsTranslationService** – Tradución de documentación de Markdown con seguimento a nivel de bloqueo
- **SignalRPublisher** Información de progreso en tempo real vía SignalR
- **TranslationRetryService** – Recuperación a nivel de etapa coa preservación dos propietarios de lugares

### Beneficios

- **Separation of concerns**: Each service handles a single translation domain
- **Maintainability**: Smaller classes are easier to understand and test
- **Extensibility**: New translation targets can be added via interface implementation
- **Reliability**: Independent services provide better fault isolation

## Novas características

### Live Translation Monitor

**Location**: `/Admin/LiveTranslation`

Unha nova páxina de administración que proporciona visibilidade en tempo real ao proceso de tradución

- Mostra todos os eventos sinal como ocorren
- Tipo de mensaxe codificado en cor (azul=estrelado, verde=completo, vermello=error)
- Marca de estado de conexión con auto-reconexión
- Exportar e exportar a JSON

### Nomeados propietarios

O sistema de localización agora soporta os localizadores nomeados para mellorar a gramaticalidade en diferentes idiomas:

```csharp
// Usage in code
var message = Localizer["WelcomeMessage", new Dictionary<string, string>
{
    ["userName"] = "John",
    ["count"] = "5"
}];
// Result: "Hello John, you have 5 new messages"
```

Características:
- Valores proporcionados en tempo de execución ou almacenados en
- Máscara/restoración automática para evitar a corrupción
- Compatibilidade coas posicións existentes

### Tradución incremental

Os ficheiros de marcado son traducidos de forma incremental:

- **Per-language saving**: Each target language is saved immediately after translation, reducing memory pressure
- **Block-level tracking**: `.translation-meta.json` tracks translation status per block
- **Selective retry**: Only failed blocks are re-translated on the next run
- **Peristencia**: Estado de tradución para reiniciar a aplicación

### Retry Logic

Tres niveis de resistencia:

1. **HTTP retry** (LibreTranslateService): 5 attempts with exponential backoff (1s–5s)
2. **Stage retry** (TranslationRetryService): 3 additional attempts with 30s delays
3. **Block retry** (DocumentsTranslationService): Failed Markdown blocks retried on next run

### información de sinal

Información en tempo real para todas as operacións de gasoduto:

- Cada etapa publica eventos
- O progreso da lingua por curso publicado como eventos
- Os eventos de erro inclúen o contexto detallado (fonte, código de erro, mensaxe)
- Os números de secuencia garanten a orde dentro de cada execución

## Cambios de configuración

### appettings.json

Sen cambios de ruptura. A configuración actual segue funcionando:

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

### Novos servizos

Rexistrado en:

- /
- `TranslationRetryService`
- /
- /
- /
- /

O hub SignalR está mapeado para conexións de clientes.

## Probas

### Estado de proba

- **243/244 tests passing** (1 skipped due to concurrent file access in test environment)
- Nova cobertura de proba engadida para:
  - Funcionalidade de PlaceholderService
  - BackendTranslationService Orquestración
  - JsonStringLocalizer Localholder indexers

### Limitacións coñecidas

- a proba salta ao executarse en paralelo porque varias instancias de proba comparten o mesmo ficheiro. Pasa cando corre en soidade.

## Nova estrutura de ficheiros

### Servizos en

- Orquestra de Pipeline
- Nome do país tradución
- JSON Dictionary Sincronization
- Tradución Markdown
- Mensaxe de SignalR publicado
- Retry logic with placeholder masking
- Editor interface
- Interface de servizo de país
- Interface de servizo de localización
- Interface de servizo de documentos
- - Interface de orquestra (actualizada)
- Metadatos de tradución por ficheiro

### Servizos actualizados en

- -Adición do nome do titular
- Actualizado para novos parámetros
- Nomeado Gestión de Titulares
- Interface de titular

### Páxina de administración en

- Páxina de monitoraxe en tempo real
- Modelo de páxina

### Nova documentación en

- Actualización de documentación sobre pipeline
- Guía do sistema de propietarios
- Guía de uso de Dashboard
- Arquitectura Técnica Visión

## Compatibilidade retro

Todos os cambios son aditivos:

- O código de localización actual () funciona sen cambios
- O formato posicional () non varía
- O formato do dicionario JSON non se modifica
- A estrutura de marcas non cambia
- As mensaxes de texto usan o mesmo formato

## Camiño de migración

Non é necesaria a migración. A recuperación é interna:

1. O antigo foi conservado como referencia e despois substituído
2. Actualizouse o rexistro para utilizar novas interfaces
3. Os consumidores actuais non experimentan cambios

## Melloras de rendemento

- ** Uso de memoria**: Ficheiros gardados por idioma inmediatamente en vez de manter todo na memoria
- **Faster incremental runs**: Only changed/failed Markdown blocks are re-translated
- ** Máis información**: O progreso en tempo real axuda a diagnosticar etapas lentas

## Melloras futuras

Melloras previstas:

1. **AI fine-tuning** - Revisión de tradución post-máquina para frases > 5 palabras
2. ** Autenticación da administración** Restrinxir as páxinas de administración a usuarios autorizados
3. ** Editor Dicionario** – Web UI para xestionar as claves de localización
4. **Translation statistics** — Charts showing translation counts and error rates over time
5. ** sintaxe do titular de sitio actual** - Soporte para formatos de marcador alternativo

## Contacto

Para preguntas ou problemas co servizo de tradución, consulte a documentación detallada no directorio de cada módulo ou póñase en contacto co equipo de desenvolvemento.
