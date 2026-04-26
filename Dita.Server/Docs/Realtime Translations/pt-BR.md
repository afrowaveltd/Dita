# Traduções em tempo real

Este documento existe como uma entrada de teste ao vivo para o gasoduto de tradução automática.

## O que o serviço faz

O serviço é executado em um cronograma e valida o servidor de tradução, configuração e idiomas disponíveis antes de qualquer trabalho de tradução começar.

Após a etapa de validação, ele sincroniza nomes de países do catálogo de países somente leitura para os dicionários padrão de localização JSON. Se o idioma padrão da aplicação for inglês, a entrada do país é armazenada como a chave é igual ao valor. Se o idioma padrão for diferente, o nome do país inglês é traduzido pela primeira vez para o idioma padrão, e somente então armazenado como chave é igual ao valor no dicionário padrão.

Em seguida, o serviço compara o dicionário de localização padrão atual com o instantâneo armazenado da execução anterior. Entradas recentemente adicionadas são traduzidas para idiomas de destino apenas quando a chave já não existe, então traduções manuais mantêm a prioridade. As entradas removidas são excluídas de todos os dicionários de destino para manter o conjunto inteiro consistente.

Finalmente, a verificação de serviços configurou as raízes da documentação para árvores Markdown. Espera-se que cada pasta de tópicos contenha um arquivo fonte com o nome do idioma padrão, como en.md. O serviço hashes que o arquivo de origem, detecta alterações, traduz arquivos Markdown de destino ausentes ou desatualizados, e armazena o hash atual ao lado do arquivo de origem. Se escrever o hash ao lado do arquivo fonte não é possível, ele cai de volta para armazenamento temporário.

## Como o serviço informa o progresso

A infra-estrutura emite mensagens SignalR gerais através do hub de localização usando um envelope de mensagem. Cada mensagem carrega um tipo de mensagem, a fase atual do processo, uma data-limite UTC, um resumo de texto e uma carga útil opcional específica do estágio.

As etapas atuais são:

- Servidores de Verificação
- TraduzirPaíses
- TranslateJsonFiles
- TranslateMarkdownFiles
- ArmazenarResultados

O fluxo típico de mensagens é iniciado, o estágio concluído e o gasoduto concluído. Se um estágio falhar, a mensagem é marcada como um erro e inclui informações de erro estruturadas com códigos de erro unificados.

## Princípios de concepção

As traduções são processadas sequencialmente para evitar sobrecarga do servidor LibreTranslate.

Os dicionários JSON de localização são sempre armazenados com chaves ordenadas alfabeticamente e JSON formatado para facilitar a manutenção.

O snapshot do dicionário padrão anterior é armazenado persistentemente para que um reinício do aplicativo não perca o rastreamento de alterações.

** As traduções manuais sempre têm prioridade sobre as adições automáticas.**
