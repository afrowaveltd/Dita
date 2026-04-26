# Traducións en tempo real

Este documento existe como unha entrada de proba en directo para a tradución automática.

## O que fai o servizo

O servizo funciona nun horario e valida o servidor de tradución, a configuración e as linguas dispoñibles antes de que se inicie calquera traballo de tradución.

Despois do paso de validación, sincroniza os nomes dos países do catálogo de só lectura nos dicionarios JSON. Se o idioma por defecto da aplicación é o inglés, a entrada do país almacénase como un valor igual. Se a lingua por defecto é diferente, o nome do país é traducido primeiro ao idioma por defecto, e só entón almacenado como un valor igual no dicionario por defecto.

A continuación, o servizo compara o actual dicionario de localización por defecto coa instantánea almacenada da execución anterior. As entradas engadidas recentemente só se traducen en idiomas de destino cando a clave xa non existe, polo que as traducións manuais teñen prioridade. Eliminar entradas son eliminados de todos os dicionarios obxectivo para manter todo o conxunto consistente.

Finalmente, o servizo escanea as raíces de documentación configuradas para as árbores Markdown. Espérase que cada cartafol de temas conteña un ficheiro fonte chamado así polo idioma predeterminado, como en.md. O servizo acelera que o ficheiro fonte, detecta cambios, traduce ficheiros de código de destino que faltan ou obsoletos, e almacena o hash actual xunto ao ficheiro fonte. Se escribir o hash xunto ao ficheiro fonte non é posible, volve ao almacenamento temporal.

## Como progresan os informes de servizos

O backend emite mensaxes de SignalR xeral a través do hub de localización usando unha envoltura de mensaxe. Cada mensaxe leva un tipo de mensaxe, a fase de proceso actual, un timestamp UTC, un resumo de texto e unha carga útil opcional específica do estadio.

As etapas actuais son:

- CheckServers
- traducións
- Tradutores
- Traducir MarkdownFiles
- almacenamento de resultados

O fluxo típico de mensaxes é o estadio comezado, fase rematada e gasoduto rematado. Se unha etapa falla, a mensaxe está marcada como un erro e inclúe información estruturada de erro cun código de erro unificado.

## Principios de deseño

As traducións son procesadas de forma secuencial para evitar sobrecargar o servidor LibreTranslate.

Os dicionarios JSON de localización almacénanse sempre con chaves ordenadas alfabeticamente e formato JSON para un mantemento máis fácil.

A instantánea do dicionario anterior almacénase de forma persistente, polo que unha reiniciación da aplicación non perde o seguimento de cambios.

**As traducións manuais sempre teñen prioridade sobre as adicións automáticas**
