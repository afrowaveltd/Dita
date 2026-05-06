# Resumo das mudanças no serviço de tradução automática

## Visão geral

Este documento resume todas as alterações feitas no serviço de tradução automática Dita, incluindo refatoração de arquitetura, novas características, melhorias de observação e melhorias de localização.

## Mudanças de Arquitetura

### Serviço de tradução de infra-estrutura refatorado

A monolítica foi decomposta em quatro serviços especializados coordenados por um orquestrador leve:

- **Backend TranslationService** — Orquestrador Pipeline (validação do servidor, delegação de palco, manipulação de erros)
- **PaísesTranslationService** - Sincronização do nome do país (Inglês → língua alvo)
- **LocalizationTranslationService** — JSON dictionary synchronization (added/removed keys)
- **DocumentsTranslationService** — Markdown documentation translation with block-level tracking
- **SignalRPublisher** — Real-time progress reporting via SignalR
- **TranslationRetryService** — Stage-level retry with placeholder preservation

### Benefícios

- **Separation of concerns**: Each service handles a single translation domain
- **Maintainability**: Smaller classes are easier to understand and test
- **Extensibility**: New translation targets can be added via interface implementation
- **Reliability**: Independent services provide better fault isolation

## Novos recursos

### Monitor de tradução ao vivo

**Location**: `/Admin/LiveTranslation`

Uma nova página de administração que fornece visibilidade em tempo real no oleoduto de tradução:

- Exibe todos os sinais R eventos como eles ocorrem
- Tipos de mensagem com código de cor (azul=iniciado, verde=completado, vermelho=error)
- Banner de status de conexão com conexão automática
- Contador de mensagens e exportação para JSON

### Nomeados como placeholders

O sistema de localização agora suporta placeholders nomeados () para melhorar gramaticalidade em diferentes línguas:

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
- Valores fornecidos em tempo de execução ou armazenados em
- Mascaramento/restauração automático durante a tradução para evitar corrupção
- Para trás, compatível com os espaços de posicionamento existentes

### Tradução Incremental

Arquivos de marcação são traduzidos incrementalmente:

- **Per-language saving**: Each target language is saved immediately after translation, reducing memory pressure
- **Block-level tracking**: `.translation-meta.json` tracks translation status per block
- **Selective retry**: Only failed blocks are re-translated on the next run
- **Metadata persistence**: Translation state survives application restarts

### Lógica de repetição aprimorada

Três níveis de resiliência:

1. **HTTP retry** (LibreTranslateService): 5 attempts with exponential backoff (1s–5s)
2. **Stage retry** (TranslationRetryService): 3 additional attempts with 30s delays
3. **Block retry** (DocumentsTranslationService): Failed Markdown blocks retried on next run

### SinalR Reportando

Relatório de progresso em tempo real para todas as operações do gasoduto:

- Cada etapa publica eventos
- Progresso por idioma publicado como eventos
- Os eventos de erro incluem contexto detalhado (fonte, código de erro, mensagem)
- Números de sequência garantem ordem dentro de cada corrida

## Mudanças de configuração

### applications. json

Nenhuma mudança. A configuração existente continua funcionando:

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

### Novos Serviços

Registrado em:

- /
- `TranslationRetryService`
- /
- /
- /
- /

O Sinal R hub está mapeado para conexões com clientes.

## Testando

### Status do teste

- **243/244 testes passando** (1 pulado devido ao acesso de arquivo concorrente em ambiente de teste)
- Nova cobertura de teste adicionada para:
  - Posicionador Funcionalidade de serviço
  - Tradução de Infra- Estrutura Orquestração de serviço
  - JsonStringLocalizer placeholder indexers

### Limitações Conhecidas

- o teste é ignorado ao correr em paralelo porque várias instâncias de teste compartilham o mesmo arquivo. Passa quando corre em isolamento.

## Nova estrutura de arquivos

### Serviços em

- - Orchestrator Pipeline
- - Tradução do nome do país
- - Sincronização do dicionário JSON
- Tradução:
- - Sinal Publicação de mensagens R
- - Tente novamente a lógica com o mascaramento
- - Interface da editora
- - Interface de serviço do país
- - Interface de serviço de localização
- - Interface de serviço de documentos
- - Interface orquestradora (atualizada)
- Tradução por metadados

### Serviços atualizados em

- - Adicionado apoio de placeholder
- - Não. Atualizado para novo parâmetro
- - Nomeada gerência de placeholder
- - Interface de colocação

### Nova página de administração

- - Página de monitoramento em tempo real
- - Modelo de página

### Nova Documentação em

- - Não. Documentação atualizada do gasoduto
- - Guia do sistema
- - Guia de uso do painel
- - Visão geral da arquitetura técnica

## Compatibilidade para trás

Todas as mudanças são aditivas:

- O código de localização existente funciona inalterado
- A formatação posicional funciona sem alterações
- O formato existente do dicionário JSON está inalterado
- A estrutura de Markdown existente está inalterada
- Sinal Mensagens R usam o mesmo formato

## Caminho Migratório

Nenhuma migração necessária. A refatoração é interna:

1. Velho foi preservado como referência e depois substituído
2. Registros de DI foram atualizados para usar novas interfaces
3. Todos os consumidores existentes não veem mudanças

## Melhorias de Performance

- **Uso reduzido de memória**: Arquivos salvos por idioma imediatamente em vez de guardar tudo na memória
- ** Mais rápido incremental Corre **: Apenas blocos Markdown alterados/falhados são retraduzidos
- **Better visibility**: Real-time progress helps diagnose slow stages

## Melhorias futuras

Melhorias planejadas:

1. **AI fine-tuning** — Post-machine translation review for phrases > 5 words
2. **Autenticação do administrador** — Restrinja páginas de administração aos usuários autorizados
3. **Dictionary editor** — Web UI for managing localization keys
4. **Translation statistics** — Charts showing translation counts and error rates over time
5. **Custom placeholder syntax** — Support for alternate placeholder formats

## Contato

Para perguntas ou problemas com o serviço de tradução, por favor consulte a documentação detalhada no diretório de cada módulo ou entre em contato com a equipe de desenvolvimento.
