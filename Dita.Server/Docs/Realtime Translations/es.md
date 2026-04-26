# Traducciones en tiempo real

Este documento existe como entrada de prueba en vivo para el oleoducto de traducción automática.

## Qué hace el servicio

El servicio se ejecuta en un horario y valida el servidor de traducción, configuración y idiomas disponibles antes de iniciar cualquier trabajo de traducción.

Después del paso de validación, sincroniza los nombres de los países del catálogo de sólo lectura en los diccionarios de localización estándar JSON. Si el idioma predeterminado de la aplicación es inglés, la entrada del país se almacena como valor clave igual. Si el idioma predeterminado es diferente, el nombre del país inglés se traduce primero en el idioma predeterminado, y sólo entonces se almacena como valor clave igual en el diccionario predeterminado.

A continuación, el servicio compara el diccionario de localización predeterminado actual con la instantánea almacenada de la ejecución anterior. Las entradas añadidas recientemente se traducen a los idiomas destinatarios sólo cuando la clave ya no existe, por lo que las traducciones manuales mantienen prioridad. Las entradas eliminadas de todos los diccionarios objetivo para mantener todo el conjunto consistente.

Finalmente, los escáneres de servicio configuraron raíces de documentación para árboles Markdown. Se espera que cada carpeta de tema contenga un archivo fuente llamado después del idioma predeterminado, como en.md. El servicio tiene ese archivo fuente, detecta cambios, traduce archivos de Markdown de destino perdidos o obsoletos, y almacena el hash actual junto al archivo fuente. Si la escritura del hash junto al archivo fuente no es posible, se vuelve al almacenamiento temporal.

## Cómo informa el servicio progresa

El backend emite mensajes de SignalR generales a través del hub de localización utilizando un sobre de mensaje. Cada mensaje lleva un tipo de mensaje, la etapa actual del proceso, un timetamp UTC, un resumen de texto y una carga útil opcional específica para cada etapa.

Las etapas actuales son:

- CheckServers
- Traducir
- TraducirJsonFiles
- TraducirMarkdownFiles
- Resultados

El flujo de mensajes típicos es la etapa iniciada, la etapa finalizada y el oleoducto completado. Si una etapa falla, el mensaje está marcado como un error e incluye información de error estructurada con códigos de error unificados.

## Principios de diseño

Las traducciones se procesan secuencialmente para evitar sobrecargar el servidor LibreTranslate.

Localización Los diccionarios JSON siempre se almacenan con teclas ordenadas alfabéticamente y JSON formateado para un mantenimiento más fácil.

La instantánea del diccionario predeterminado anterior se almacena persistentemente para que un reinicio de la aplicación no pierda el seguimiento del cambio.

**Las traducciones manuales siempre tienen prioridad sobre las adiciones automáticas.**
