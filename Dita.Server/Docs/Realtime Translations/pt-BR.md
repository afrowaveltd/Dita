# Traduções em tempo real

Este documento existe como uma entrada de teste ao vivo para o gasoduto de tradução automática. Qualquer alteração neste arquivo desencadeia a re-tradução de todos os arquivos de idioma alvo na próxima execução agendada.

## Visão geral da arquitetura

O gasoduto de tradução foi reestruturado numa arquitectura modular com quatro sub-serviços especializados coordenados por um orquestrador leve:

- **BackendTranslationService** — Orchestra todo o gasoduto, manipula a validação do servidor e os delegados trabalham para sub-serviços.
- **PaísesTraduçãoService** — Sincroniza nomes de países de dicionários por idioma.
- **LocalizationTranslationService** — Detecta chaves adicionadas/removidas no dicionário padrão do JSON e as traduz em idiomas de destino.
- **DocumentsTranslationService** — Traduz arquivos de documentação Markdown com rastreamento por bloco e metadados.

Cada sub-serviço opera de forma independente e informa o progresso via SignalR em tempo real.

## O que o serviço faz

O serviço é executado em um cronograma e executa um pipeline de cinco estágios: validação do servidor, sincronização do país, sincronização do dicionário JSON, tradução do arquivo Markdown e persistência dos resultados. Cada etapa emite eventos estruturados de progresso em tempo real sobre SignalR para que os clientes conectados possam acompanhar enquanto o trabalho prossegue.

## Fases de tubagem

### Fase 1 — Servidores de Controlo

Antes de qualquer trabalho de tradução começar, o serviço verifica se todas as condições prévias estão satisfeitas:

- A seção de configuração deve estar presente e válida.
- O servidor LibreTranslate deve responder dentro de uma latência aceitável.
- A lista de idiomas disponíveis no servidor de tradução é obtida.
- A linguagem padrão configurada deve estar presente nessa lista.
- Os arquivos locais do JSON em falta para qualquer idioma suportado são criados automaticamente.

Se alguma verificação falhar, o gasoduto pára imediatamente e uma mensagem é emitida.

### Etapa 2 — Traduzir Países

Os nomes dos países são mantidos em sincronia de um catálogo somente de leitura () para os dicionários de localização JSON.

- Se o idioma padrão da aplicação for inglês, cada nome de país é armazenado como sem tradução.
- Se o idioma padrão for qualquer outro idioma, o nome do país inglês é traduzido pela primeira vez para esse idioma, e o resultado se torna a entrada no dicionário padrão.
- Depois que o dicionário padrão é atualizado, cada entrada de país em falta em cada dicionário de idioma alvo é traduzida e salva **imediatamente por idioma**.
- Entradas já traduzidas são preservadas sem modificação.
- Se uma tradução falhar, o serviço repete até 3 vezes com atrasos de 30 segundos antes de se mudar para o próximo idioma.

### Etapa 3 — TranslateJsonFiles

O serviço compara o dicionário de localização padrão atual com um instantâneo armazenado na execução anterior:

- ** Teclas adicionadas** — entradas presentes no padrão atual mas ausentes do instantâneo — são traduzidas para cada idioma alvo que ainda não possui uma entrada manual para essa tecla.
- ** Chaves removidas** — entradas presentes no instantâneo mas ausentes do padrão atual — são apagadas de todos os dicionários de idioma alvo.
- As traduções manuais têm sempre prioridade. Se um dicionário alvo já contém um valor para uma chave, essa entrada fica inalterada independentemente do que a fonte diga.
- **Cada dicionário de línguas alvo é salvo imediatamente após suas traduções completas**, em vez de esperar que todos os idiomas terminem.
- Se uma tradução falhar para uma língua específica, o serviço volta automaticamente. Apenas erros persistentes (por exemplo, linguagem não suportada) fazem com que essa linguagem seja ignorada.
- Após a execução, o dicionário padrão atual é salvo como o novo instantâneo para a próxima comparação.

Todos os dicionários são sempre armazenados com chaves alfabeticamente ordenadas e JSON recuou para legibilidade humana.

### Etapa 4 — TranslateMarkdownFiles

O serviço caminha pelas raizes de documentação configuradas (por omissão: ) e processa cada ficheiro de código recursivamente:

1. O conteúdo do arquivo fonte é lido e um hash SHA-256 é calculado.
2. Um arquivo ao lado das faixas de origem por idioma, por bloco de tradução status, permitindo ** re-tradução incremental** de apenas blocos falhou.
3. O hash armazenado da execução anterior (mantido em um arquivo ao lado do arquivo de origem, ou em um local de retorno temporário) é comparado com o hash atual.
4. Para cada idioma alvo, o arquivo correspondente também é verificado quanto à integridade estrutural.
5. Qualquer arquivo de destino que está faltando, tem um hash desatualizado, falha validação de estrutura, ou contém blocos não traduzidos é filado para re-tradução.
6. **Cada língua-alvo é traduzida e salva independentemente** — se o Checo tiver sucesso, mas o Francês falhar, o arquivo Tcheco ainda é escrito no disco.
7. Arquivos traduzidos com sucesso são validados para paridade estrutural com a fonte (contagens iguais de cabeçalhos, itens de lista, blocos de código, blockquotes, links, marcadores negrito/itálicos e tags HTML) antes de serem escritos no disco.
8. Se todos os arquivos de destino para uma fonte tiverem sucesso, o novo hash é armazenado ao lado da fonte. Se a escrita ao lado da fonte falhar (por exemplo, em implementações somente leitura), o hash cai para trás para o diretório temporário.
9. Se qualquer tradução de destino falhar a validação, os metadados marcam esses blocos como não traduzidos para que eles sejam tentados novamente na próxima execução.

### Etapa 5 — Resultados do Armazenamento

Um consolidado é montado e publicado. Inclui:

- Horários de início e conclusão de execução UTC.
- Contagem de arquivos locais salvos JSON, arquivos Markdown salvos, arquivos hash salvos, e hash fallback escreve.
- Qualquer erro de armazenamento coletado durante a execução.
- Estatísticas de tradução por idioma (contagem traduzida, contagem ignorada, contagem de erros).

## Envelope da mensagem SignalR

Cada evento de progresso é entregue como um com os seguintes campos:

Campo
|-------|------|-------------|
Identificador de correlação para a execução actual do gasoduto
Contador monotônico dentro de uma corrida, começando em 1
Tipo semântico da mensagem
Pipeline palco a mensagem pertence a
Hora UTC em que a mensagem foi emitida
Se a mensagem representa uma condição de erro
Resumo legível para o homem
Carga útil específica do estágio (objeto de relatório ou nulo)

### Tipos de mensagens

Valor
|-------|------|---------|
0
1
2
3
4
5
6

### Fases de tubagem

Valor
|-------|------|-------------|
0
1
2
3
4
5

### Fluxo típico de mensagens

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

Se algum estágio falhar, os estágios restantes são ignorados, uma mensagem é emitida, e finalmente uma mensagem fecha a execução.

## Lógica de repetição da tradução

O gasoduto implementa dois níveis de resiliência:

### Repetição do nível de estágio (TranslationRetryService)

- Se uma solicitação de tradução falhar após as tentativas internas da LibreTranslate, a execução de até 3 repetições adicionais de nível de estágio com 30 segundos de atraso.
- Mascaramento do placeholder: Os placeholders nomeados () no texto são temporariamente substituídos por tokens seguros () antes da tradução e restaurados posteriormente, garantindo a gramática correta nas línguas-alvo.

### Validação linguística

- Antes de traduzir para um idioma de destino, o serviço verifica o idioma é suportado pelo servidor de tradução.
- As linguagens não suportadas são ignoradas com um aviso, evitando tentativas repetidas e falhadas.

### Refazer o nível de bloco de marcação

- As traduções de marcação são realizadas bloco a bloco (rubricas, parágrafos, itens de lista).
- Se um bloco individual falhar na tradução, ele é marcado como não traduzido no arquivo de metadados e repetido na próxima execução do pipeline.
- O serviço rastreia por idioma, por bloco status em arquivos ao lado de cada arquivo Markdown fonte.

## Códigos de erro

Os erros são reportados usando um enum unificado agrupado em intervalos:

Intervalo
|-------|----------|
1000–1999
2000–2999
3000–3999
4000–4999
5000–5999

Cada erro em um relatório carrega o identificador de fonte (código de idioma, caminho do arquivo ou nome do palco), o código de erro e uma mensagem legível por humanos.

## Painel de tradução ao vivo

O projeto Server inclui uma página de administração que se conecta ao hub SignalR e exibe todos os eventos de pipeline em tempo real.

- Exibe o status da conexão, a contagem de mensagens e uma tabela ao vivo de todos os eventos.
- Linhas codificadas por cores: azul para início do palco, verde para conclusão, vermelho para erros.
- Suporta limpar o feed e exportar todas as mensagens para o JSON.
- Reconecta-se automaticamente com o backoff exponencial se a conexão cair.

## Princípios de concepção

- **Modularidade**: Cada preocupação de tradução é isolada em seu próprio serviço para manutenção e testabilidade.
- ** Persistência incremental**: Dicionários e arquivos Markdown são salvos por idioma imediatamente após a tradução, reduzindo a pressão da memória e fornecendo feedback mais cedo.
- ** Resiliência**: Vários níveis de repetição (HTTP, estágio, bloco) garantem que as falhas transitórias não bloqueiem o oleoduto.
- ** Rastreamento de estado**: Os metadados por arquivo () e arquivos de hash permitem um trabalho incremental preciso em sequências.
- **Visibilidade em tempo real**: Toda operação significativa é relatada via SignalR para monitoramento e depuração.
- ** As traduções manuais sempre têm prioridade sobre adições automáticas.**
