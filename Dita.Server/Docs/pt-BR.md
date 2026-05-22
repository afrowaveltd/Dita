# Resumo das Alterações ao Serviço de Tradução Automática

## Visão geral

Este documento resume todas as alterações feitas no serviço de tradução automática Dita, incluindo refatoração de arquitetura, novas características, melhorias de observação e melhorias de localização.

## Alterações de Arquitetura

### Serviço de tradução de infra- estrutura refatorado

A monolítica foi decomposta em quatro serviços especializados coordenados por um orquestrador leve:

- **BackendTranslationService** — Pipeline orquestrator (validação do servidor, delegação de palco, manipulação de erros)
- **PaísesTraduçãoService** — Sincronização do nome do país (Inglês → idioma alvo)
- **LocalizaçãoTraduçãoService** — Sincronização de dicionários JSON (chaves adicionadas/removidas)
- **DocumentosTranslationService** — Tradução de documentação Markdown com rastreamento de nível de bloco
- **SignalRPublisher** — Relatório de progresso em tempo real via SignalR
- **TranslationRetryService** — Retentagem de nível de estágio com preservação de espaços

### Benefícios

- ** Separação de preocupações**: Cada serviço lida com um único domínio de tradução
- ** Manutenção**: Aulas menores são mais fáceis de entender e testar
- **Extensibilidade**: Novos alvos de tradução podem ser adicionados através da implementação de interface
- ** Confiabilidade**: Serviços independentes oferecem melhor isolamento de falhas

## Novas funcionalidades

### Monitor de tradução ao vivo

** Localização**:

Uma nova página de administração que fornece visibilidade em tempo real no pipeline de tradução:

- Mostra todos os eventos SignalR à medida que ocorrem
- Tipos de mensagem codificados por cores (azul=iniciado, verde=completado, vermelho=erro)
- Banner de status de conexão com a conexão automática
- Contador de mensagens e exportação para JSON

### Posições nomeadas

O sistema de localização agora suporta placeholders nomeados () para melhorar gramaticalidade em diferentes idiomas:

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
- Valores de posição fornecidos em tempo de execução ou armazenados em
- Mascaramento automático / restauração durante a tradução para evitar a corrupção
- Para trás compatível com os espaços posicionais existentes

### Tradução Incremental

Os arquivos de marcação são traduzidos incrementalmente:

- ** Poupança por idioma**: Cada idioma alvo é salvo imediatamente após a tradução, reduzindo a pressão da memória
- **Tracking-level**: tracks translation status per block
- ** Tentativa seletiva**: Apenas os blocos falhados são retraduzidos na próxima execução
- ** Persistência de metadados**: O estado de tradução sobrevive ao reinício da aplicação

### Lógica de repetição melhorada

Três níveis de resiliência:

1. **HTTP retry** (LibreTranslateService): 5 tentativas com retrocesso exponencial (1s-5s)
2. ** Stage retry** (TranslationRetryService): 3 tentativas adicionais com 30s atrasos
3. **Block retry** (DocumentsTranslationService): Blocos de Markdown falharam novamente na próxima execução

### Relatório do SignalR

Relatório de progresso em tempo real para todas as operações de gasodutos:

- Cada etapa publica eventos
- Progressos por idioma publicados como eventos
- Os eventos de erro incluem contexto detalhado (fonte, código de erro, mensagem)
- Números de sequência garantem a ordenação dentro de cada execução

## Alterações de Configuração

### appsets.json

Não há mudanças. A configuração existente continua a funcionar:

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

Registado em :

- /
- `TranslationRetryService`
- /
- /
- /
- /

O hub SignalR é mapeado para conexões de clientes.

## Teste

### Estado do Teste

- **243/244 testes passando** (1 ignorado devido ao acesso de arquivo concorrente no ambiente de teste)
- Nova cobertura de teste adicionada para:
  - Funcionalidade de serviço do placeholder
  - Infra- EstruturaTranslationService orchestration
  - indexadores de placeholder jsonstringlocalizer

### Limitações Conhecidas

- o teste é ignorado ao ser executado em paralelo porque várias instâncias de teste compartilham o mesmo arquivo. Passa quando corre em isolamento.

## Nova Estrutura de Ficheiros

### Serviços em

- — orquestrador de tubos
- — Tradução do nome do país
- — Sincronização do dicionário JSON
- — Tradução de marcação
- — Publicação de mensagens SignalR
- — Repetir a lógica com mascaramento
- — Interface do editor
- — Interface de serviços por país
- — Interface de serviço de localização
- — Interface de serviço documental
- — Interface orquestradora (actualizada)
- — metadados de tradução por ficheiro

### Serviços actualizados em

- — Adicionado apoio de substituição
- — Actualizado para um novo parâmetro
- — Nomeação de gestora de lugares
- — Interface de substituição

### Nova Página de Administração

- — Página de acompanhamento em tempo real
- — Modelo de página

### Nova Documentação em

- — Documentação actualizada do gasoduto
- — Guia do sistema de substituição
- — Guia de utilização do painel
- — Visão geral da arquitectura técnica

## Compatibilidade para trás

Todas as alterações são aditivas:

- O código de localização existente () funciona inalterado
- A formatação posicional () funciona inalterada
- O formato existente do dicionário JSON está inalterado
- A estrutura de marcação existente está inalterada
- Mensagens SignalR usam o mesmo formato

## Caminho da Migração

Nenhuma migração necessária. A refração é interna:

1. Antigo foi preservado como referência e depois substituído
2. Registros DI foram atualizados para usar novas interfaces
3. Todos os consumidores existentes não vêem alterações

## Melhorias de desempenho

- **Uso reduzido da memória**: Arquivos salvos por idioma imediatamente em vez de manter tudo na memória
- ** Correções incrementais mais rápidas**: Apenas os blocos Markdown alterados/falhados são retraduzidos
- ** Melhor visibilidade**: O progresso em tempo real ajuda a diagnosticar estágios lentos

## Melhorias futuras

Melhorias previstas:

1. **AI fine-tuning** — Revisão pós-tradução automática de frases > 5 palavras
2. **Autenticação do administrador** — Restrinja páginas de administração aos usuários autorizados
3. ** Editor dicionário** — UI Web para gerenciar chaves de localização
4. ** Estatísticas de tradução** — Gráficos mostrando contagens de tradução e taxas de erro ao longo do tempo
5. ** sintaxe de placeholder personalizada** — Suporte para formatos de placeholder alternativos

## Contacto

Para perguntas ou problemas com o serviço de tradução, consulte a documentação detalhada no diretório de cada módulo ou entre em contato com a equipe de desenvolvimento.
