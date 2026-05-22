# Traducción en vivo

El Dashboard de traducción en vivo es una página de administración que proporciona visibilidad en tiempo real en el tubería de traducción automática. Se conecta al centro SignalR y muestra todos los eventos de oleoductos a medida que ocurren.

## URL

`/Admin/LiveTranslation`

> Note: Authentication and authorization are not yet implemented. Future versions will restrict this page to admin users only.

## Características

### Flujo de eventos en tiempo real

Todos los eventos SignalR de la traducción se muestran en una tabla de actualización:

- **Número de secuencia**: contador monotónico dentro de cada tubería
- **Tiempo** - Hora local cuando se recibió el evento
- ** ID de vuelo** - Acortado GUID por correlación
- **Estadio** — Insignia de etapa de tubería (CheckServers, TranslateCountries, etc.)
- **Tipo** — Tipo de mensaje insignia (StageStarted, Progress, StageCompleted, etc.)
- ** Mensaje** - Descripción legible por el hombre
- **detalles** — carga útil json completa de los datos del evento

### Codificación de color

Color
|-------|---------|
Azul ()
Verde ()
Rojo ()
Blanco (por defecto)

### Estado de conexión

Una bandera de estado en la parte superior muestra:
- **Connecting** - Establecer la conexión de SignalR
- **Connected** - Recibir eventos normalmente
- **Reconexión** - Conexión perdida, intentando reconectarse
- ** Desconectado** - Conexión cerrada

La conexión utiliza la reconexión automática con retroceso exponencial: 0s, 2s, 5s, 10s, 30s.

### Controles

- **Clear Feed** — Elimina todos los mensajes mostrados y restaura el contador
- **Export JSON** — Descargas de todos los mensajes recibidos como archivo JSON para el análisis
- **Message counter** — Muestra el número total de eventos recibidos en este período de sesiones

## Central de señalización

El panel se conecta a:

```javascript
const connection = new signalr.HubConnectionBuilder()
    .withUrl("/hubs/localization")
    .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
    .build();
```

### Contrato de mensaje

```typescript
interface LocalizationHubMessage {
    runId: string;        // Guid
    sequence: number;     // long
    type: LocalizationMessageType;
    stage: ProcessStage;
    timestampUtc: string; // ISO 8601
    isError: boolean;
    message: string;
    data: object | null;
}
```

### Tipos de evento

El panel maneja todos los valores:

Tipo
|------|---------|
Insignia azul
Insignia verde
Insignia roja
Insignia verde
Insignia roja
Info badge
Insignia de advertencia

## Aplicación técnica

### Backend

- **LocalizationHub** () — SignalR hub que transmite mensajes a todos los clientes conectados
- **ISignalRPublisher** — Abstract over the hub for use in translation services
- **SignalRPublisher** — Implementación predeterminada que aumenta una secuencia monotónica y transmisiones

### Frontend

- HTML/JS puro con estilo Bootstrap 5
- Utiliza la biblioteca cliente JavaScript de Microsoft SignalR (cargada de CDN)
- No se requiere renderización del lado del servidor para la alimentación del evento

### Estructura de la página

```
Dita.Server/Pages/Admin/
├── LiveTranslation.cshtml      — Razor page markup + JavaScript
└── LiveTranslation.cshtml.cs     — Page model (empty, data comes via SignalR)
```

## Uso durante el desarrollo

1. Comienza la Dita. Aplicación del servidor
2. Navigate a
3. Trigger una carrera de traducción (ya sea esperar al programador o llamar a la API)
4. Los eventos de reloj aparecen en tiempo real
5. Utilice el botón Exportar para capturar un trazo completo para depurar

## Mejoras futuras

Mejoras previstas para el tablero:

- **Authentication** — Restrict access to users with the role
- **Filtering** — Filtrar eventos por escenario, tipo o ID de ejecución
- **Cosas históricas** — Vista completadas corre desde una base de datos o un archivo de registro
- **Estadística** — Gráficos que muestran conteos de traducción, tasas de error y latencia con el tiempo
- **Manual dispara** - Botones para iniciar manualmente etapas específicas de tuberías
- **Configuración** — Editar directamente desde el tablero
- ** Gestión de idiomas** — Ver y editar idiomas compatibles
- **Dictionary preview** — Browse and search localization dictionaries

## Solución de problemas

### Dashboard muestra "Failed to connect"

1. Verifique que el servidor está funcionando y accesible
2. Consola del navegador para errores de CORS o de red
3. La confirmación está presente
4. Asegúrese de que ningún firewall está bloqueando las conexiones WebSocket

### Los acontecimientos no aparecen

1. Comprueba que la URL del hub de SignalR coincide entre el servidor () y el cliente ()
2. Verificar el programador está habilitado en
3. Vea los registros del servidor para errores de traducción
4. Consultar ficha del navegador Red para mensajes WebSocket

### Los mensajes están fuera de orden

El campo garantiza el orden dentro de una sola carrera. Si los mensajes aparecen fuera de orden, puede indicar:
- Múltiples tuberías se superponen (no debería ocurrir debido a la cerradura de semaforo)
- Problemas de renderización del navegador (intentar refrescar la página)
