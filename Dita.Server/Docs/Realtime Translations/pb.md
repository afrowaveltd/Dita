# Tradução em tempo real

Este documento existe como uma entrada de teste ao vivo para o oleoduto de tradução automática. Qualquer mudança neste arquivo desencadeia a re-tradução de todos os arquivos de idioma alvo na próxima corrida programada.

## Visão geral da arquitetura

O oleoduto de tradução foi reestruturado em uma arquitetura modular com quatro sub-serviços especializados coordenados por um orquestrador leve:

- ** Backend TranslationService** — Orquestra todo o oleoduto, lida com validação de servidor, e delegados trabalham para sub-serviços.
- **CountriesTranslationService** — Synchronizes country names from `countries.json` into per-language dictionaries.
- **LocalizaçãoTraduçãoService** - Detecta chaves adicionadas/removidas no dicionário padrão JSON e as traduz em línguas alvo.
- **DocumentsTranslationService** - Traduz arquivos de documentação Markdown com rastreamento por bloco e metadados.

Cada sub-serviço opera independentemente e relata progresso via SignalR em tempo real.

## O que o serviço faz

O serviço funciona em um cronograma e executa um pipeline de cinco estágios: validação do servidor, sincronização do país, sincronização do dicionário JSON, tradução do arquivo Markdown, e persistir nos resultados. Cada etapa emite eventos estruturados de progresso em tempo real sobre Signal. R para que clientes conectados possam acompanhar o trabalho.

## Estágios de pipeline

### Estágio 1 - CheckServers

Antes de qualquer trabalho de tradução começar, o serviço verifica que todas as condições estão satisfeitas

- A seção de configuração deve estar presente e válida.
- O servidor LibreTranslate deve responder dentro de uma latência aceitável.
- A lista de idiomas disponíveis no servidor de tradução é obtida.
- A linguagem padrão configurada deve estar presente nessa lista.
- Arquivos locais faltando para qualquer idioma suportado são criados automaticamente.

Se algum cheque falhar, o oleoduto para imediatamente e uma mensagem é emitida.

### Fase 2 - Traduzir Países

Nomes de país são mantidos em sincronia de um catálogo somente de leitura () para os dicionários de localização JSON.

- Se o idioma padrão da aplicação é inglês, cada nome de país é armazenado sem tradução.
- Se o idioma padrão é qualquer outro idioma, o nome de país inglês é traduzido pela primeira vez para esse idioma, e o resultado se torna a entrada no dicionário padrão.
- After the default dictionary is updated, each missing country entry in every target language dictionary is translated and saved **immediately per language**.
- Entradas já traduzidas são preservadas sem modificações.
- Se uma tradução falhar, o serviço retorna até 3 vezes com 30 segundos de atraso antes de se mudar para o próximo idioma.

### Fase 3 - Traduzir Arquivo Json

O serviço compara o dicionário de localização padrão atual com um instantâneo armazenado na execução anterior:

- **Added keys** — entries present in the current default but absent from the snapshot — are translated into every target language that does not already have a manual entry for that key.
- ** Chaves removidas** — entradas presentes no instantâneo, mas ausentes do padrão atual — são apagadas de todos os dicionários de línguas alvo.
- Traduções manuais sempre têm prioridade. Se um dicionário já contém um valor para uma chave, essa entrada fica inalterada, independentemente do que a fonte diz.
- ** Cada dicionário de línguas alvo é salvo imediatamente após suas traduções completas**, em vez de esperar que todas as línguas terminem.
- Se uma tradução falhar para uma língua específica, o serviço volta automaticamente. Apenas erros persistentes (por exemplo, linguagem sem suporte) causam que a linguagem seja ignorada.
- Após a execução, o dicionário padrão atual é salvo como o novo instantâneo para a próxima comparação.

Todos os dicionários são sempre armazenados com chaves ordenadas alfabeticamente e o JSON para leitura humana.

### Fase 4 - Traduzir Arquivos de Marcação

O serviço caminha as raízes de documentação configuradas (padrão: ) e processa cada arquivo fonte recursivamente:

1. O conteúdo do arquivo fonte é lido e um hash SHA-256 é calculado.
2. A `.translation-meta.json` file next to the source tracks per-language, per-block translation status, enabling **incremental re-translation** of only failed blocks.
3. O haxixe armazenado da execução anterior (mantido em um arquivo próximo ao arquivo fonte, ou em um local de retorno temporário) é comparado com o haxixe atual.
4. Para cada língua alvo, o arquivo correspondente também é verificado quanto à integridade estrutural.
5. Qualquer arquivo de destino que esteja faltando, tem um haxixe ultrapassado, falha na validação da estrutura, ou contém blocos não traduzidos está na fila para retradução.
6. **Each target language is translated and saved independently** — if Czech succeeds but French fails, the Czech file is still written to disk.
7. Arquivos traduzidos com sucesso são validados para paridade estrutural com a fonte (tabelas iguais, itens de lista, blocos de código, blockquotes, links, marcadores negritos/itálicos e tags HTML) antes de serem escritos em disco.
8. Se todos os arquivos de uma fonte tiverem sucesso, o novo hash será armazenado ao lado da fonte. Se a escrita ao lado da fonte falhar (por exemplo, em implantações somente leitura), o hash volta para o diretório temporário.
9. Se alguma tradução do alvo falhar na validação, os metadados marcam esses blocos como não traduzidos para que eles sejam tentados novamente na próxima corrida.

### Fase 5 - Resultados de Armazenamento

Um consolidado é montado e publicado. Inclui:

- Hora de início e conclusão da UTC.
- Contagens de arquivos locais salvos do JSON, arquivos Markdown, arquivos de hash salvos, e hash reserva escreve.
- Qualquer erro de armazenamento coletado durante a corrida.
- Estatísticas de tradução por idioma (contagem traduzida, contagem ignorada, contagem de erros).

## Sinal Envelope de mensagem R

Cada evento de progresso é entregue como um com os seguintes campos:

Campo
|-------|------|-------------|
Identificador de correlação para o atual oleoduto
Contador monotônico em uma corrida, começando em 1
Tipo semântico da mensagem
A mensagem pertence a
Hora da UTC quando a mensagem foi emitida
Se a mensagem representa uma condição de erro
Resumo legível pelo homem
Carga de estágio específica (objeto de relatório ou nulo)

### Tipos de mensagem

Valor
|-------|------|---------|
0
1
2
3
4
5
6

### Estágios de pipeline

Valor
|-------|------|-------------|
0
1
2
3
4
5

### Típico fluxo de mensagens

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

Se algum estágio falhar, os estágios restantes são ignorados, uma mensagem é emitida, e finalmente uma mensagem fecha a corrida.

## Tradução:

O gasoduto implementa dois níveis de resiliência:

### Tradução:

- Se um pedido de tradução falhar após as tentativas internas de LibreTranslate, o desempenho será de até 3 repetições adicionais de nível de estágio com 30 segundos de atraso.
- Mascaramento: Nomeados placeholders () no texto são temporariamente substituídos por fichas seguras () antes da tradução e restaurados depois, garantindo gramática correta em línguas alvo.

### Validação da linguagem

- Antes de traduzir para uma língua alvo, o serviço verifica que a língua é suportada pelo servidor de tradução.
- Linguagens não suportadas são ignoradas com um aviso, impedindo repetidas tentativas falhadas.

### Markdown bloco-nível de tentativa

- Traduções de marcação são feitas bloco a bloco (rubricas, parágrafos, itens de lista).
- Se um bloco individual falhar na tradução, é marcado como não traduzido no arquivo de metadados e testado na próxima execução do gasoduto.
- O serviço rastreia por idioma, status por bloco em arquivos ao lado de cada arquivo Markdown.

## Códigos de erro

Erros são relatados usando um enum unificado agrupados em intervalos:

Distância
|-------|----------|
1000-1999
2000-2999
3000-3999
4000–4999
5000-5999

Cada erro em um relatório carrega o identificador de fonte (código da língua, caminho do arquivo, ou nome do palco), o código de erro, e uma mensagem humana legível.

## Painel de tradução ao vivo

O projeto do servidor inclui uma página de administração que se conecta ao centro do SignalR e exibe todos os eventos em tempo real.

- Mostra status de conexão, contagem de mensagens, e uma tabela ao vivo de todos os eventos.
- Linhas de cores: azul para começar o palco, verde para completar, vermelho para erros.
- Suporta limpar o feed e exportar todas as mensagens para JSON.
- Reconecta-se automaticamente com retrocesso exponencial se a conexão cair.

## Princípios de design

- **Modularity**: Each translation concern is isolated in its own service for maintainability and testability.
- **Incremental persistence**: Dictionaries and Markdown files are saved per-language immediately after translation, reducing memory pressure and providing earlier feedback.
- **Resilience**: Multiple retry levels (HTTP, stage, block) ensure transient failures do not block the pipeline.
- **State tracking**: Per-file metadata (`.translation-meta.json`) and hash files enable precise incremental work on subsequent runs.
- **Visibilidade em tempo real**: Todas as operações significativas são relatadas via SignalR para monitoramento e depuração.
- **Manual translations always have priority over automatic additions.**
