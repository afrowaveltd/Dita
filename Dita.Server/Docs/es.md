# Resumen de los cambios al servicio de traducción automática

## Sinopsis

Este documento resume todos los cambios realizados en el servicio de traducción automática Dita, incluyendo refactorización de arquitectura, nuevas características, mejoras de observabilidad y mejoras de localización.

## Cambios de arquitectura

### Refactored BackendTranslationService

El monolítico se ha descompuesto en cuatro servicios especializados coordinados por un orquestador ligero:

- **BackendTranslationService** — Pipeline orquestator (validación del servidor, delegación de escenarios, manejo de errores)
- **PaísesTraducciónServicio** - Sincronización del nombre del país (inglés → idioma de destino)
- **LocalizationTranslationService** — JSON Dictionary synchronization (added/removed keys)
- **DocumentosTranslationService** — Traducción de la documentación de Markdown con seguimiento de nivel de bloque
- **SignalRPublisher** — Informe de progreso en tiempo real vía SignalR
- **TranslationRetryService** — Retry-level retry with placeholder preservation

### Beneficios

- **Separación de las preocupaciones**: Cada servicio maneja un solo dominio de traducción
- **Mantenibilidad**: Las clases más pequeñas son más fáciles de entender y probar
- **Extensibilidad**: Nuevos objetivos de traducción se pueden añadir a través de la implementación de interfaces
- **Reliability**: Los servicios independientes proporcionan un mejor aislamiento de fallas

## Nuevas características

### monitor de traducción en vivo

**Ubicación**:

Una nueva página de administración que proporciona visibilidad en tiempo real en el oleoducto de traducción:

- Muestra todos los eventos de SignalR cuando ocurren
- Tipos de mensaje codificados por colores (blue=started, green=completed, red=error)
- Bandera de estado de conexión con auto-reconexión
- Contratista de mensajes y exportación a JSON

### Titulares designados

El sistema de localización apoya ahora a los titulares de puestos nombrados () para mejorar la gramática en diferentes idiomas:

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
- Valores proporcionados a tiempo de ejecución o almacenados en
- Enmascaramiento/restoración automático durante la traducción para prevenir la corrupción
- Atraso compatible con los propietarios de puestos existentes

### Traducción adicional

Los archivos Markdown se traducen incrementalmente:

- **Ahorro por idioma**: Cada idioma objetivo se guarda inmediatamente después de la traducción, reduciendo la presión de memoria
- **Block-level tracking**: tracks translation status per block
- **Retromisión selectiva**: Sólo bloques fallidos son retraducidos en la siguiente carrera
- ** Persistir en los metadatos**: Estado de traducción sobrevive a la aplicación

### Logic de reingreso mejorado

Tres niveles de resiliencia:

1. **HTTP retry** (LibreTranslateService): 5 intentos con retroceso exponencial (1s–5s)
2. **Retromisión en estadio** (TranslationRetryService): 3 intentos adicionales con 30 retrasos
3. **Block retry** (DocumentosTranslationService): Failed Markdown blocks retried on next run

### señalización

Presentación de informes sobre la marcha en tiempo real para todas las operaciones de oleoductos:

- Cada etapa publica eventos
- Avances por idioma publicados como eventos
- Los eventos de error incluyen contexto detallado (fuente, código de error, mensaje)
- Los números de secuencia garantizan el orden dentro de cada carrera

## Cambios de configuración

### appsettings.json

Sin cambios. La configuración existente sigue funcionando:

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

### Nuevos servicios

Registrado en:

- /
- `TranslationRetryService`
- /
- /
- /
- /

El centro SignalR está diseñado para las conexiones de clientes.

## Pruebas

### Estado de prueba

- **243/244 pruebas que pasan** (1 saltada debido al acceso simultáneo de archivos en entorno de prueba)
- Nueva cobertura de prueba agregada para:
  - Función de servicio
  - BackendTranslationOrquestación de servicio
  - JsonStringLocalizer marcador de posición indexadores

### Limitaciones conocidas

- test se salta cuando se ejecuta en paralelo porque múltiples instancias de prueba comparten el mismo archivo. Pasa cuando corre en aislamiento.

## Nueva estructura de archivos

### Servicios en

- — orquestador de tuberías
- — Traducción del nombre del país
- Sincronización del diccionario JSON
- — Traducción de Markdown
- — Publicación de mensajes SignalR
- — Lógica de reingreso con enmascaramiento de marcadores
- — Interfaz de editor
- — Interfaz de servicio a los países
- — Interfaz de servicios de localización
- — Interfaz de servicio de documentos
- — Interfaz de orquestador (actualizado)
- — Metadatos de traducción por archivo

### Servicios actualizados

- - Agregado apoyo de titularidad
- — Actualizado para nuevo parámetro
- - Gestión de los titulares de puestos
- — Interfaz de marcado de posición

### nueva página de administración en

- — Página de monitoreo en tiempo real
- — Modelo de página

### Nueva documentación en

- — Documentación actualizada del oleoducto
- — Guía del sistema de propietarios
- — Guía de uso de tableros de instrumentos
- — Resumen de la arquitectura técnica

## Compatibilidad

Todos los cambios son aditivos:

- Código de localización existente () funciona sin cambios
- El formato de posición () funciona sin cambios
- El formato de diccionario JSON existente no cambia
- La estructura de marcado existente no cambia
- Los mensajes SignalR usan el mismo formato

## Sendero de migración

No se requiere migración. El refactoring es interno:

1. El viejo fue preservado como referencia y luego reemplazado
2. Se actualizaron los registros de DI para utilizar nuevas interfaces
3. Todos los consumidores existentes no ven cambios

## Mejoras de la ejecución

- **Uso de memoria reducido**: Archivos guardados por idioma inmediatamente en lugar de guardar todo en memoria
- **Grupos incrementales rápidos**: Sólo los bloques de Markdown cambiados/failados son re-translated
- **Better visibility**: Real-time progress helps diagnose slow stages

## Mejoras futuras

Mejoras previstas:

1. **AI fine-tuning** — Revisión de la traducción posterior a la máquina para frases
2. ** autenticación de minas**: Restringir las páginas de administración a los usuarios autorizados
3. **Diccionario editor** — Web UI para gestionar las claves de localización
4. ** Estadísticas de traducción** — Gráficos que muestran los recuentos de traducción y tasas de error con el tiempo
5. **Sintaxis del marcador de posición** — Soporte para formatos alternativos del marcador de posición

## Contacto

Para preguntas o problemas con el servicio de traducción, consulte la documentación detallada en el directorio de cada módulo o comuníquese con el equipo de desarrollo.
