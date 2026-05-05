# Tradução Arquitetura

Este documento descreve a arquitetura modular do sistema de tradução automática de Dita, introduzido para melhorar a manutenção, testabilidade e resiliência.

## Objetivos de design

A refatoração abordou várias preocupações com o projeto monolítico original:

- **Separation of concerns**: Each translation domain (countries, JSON dictionaries, Markdown) is isolated.
- **Incremental persistence**: Files are saved per-language immediately after translation, reducing memory usage and providing earlier results.
- **Resilience**: Multiple retry levels handle transient failures without blocking the entire pipeline.
- **Observability**: Every significant operation is reported via SignalR for real-time monitoring.
- **Extensibility**: New translation targets can be added by implementing a single interface.

## Serviço de decomposição

### Serviço de tradução de infra-estrutura (orquestrador)

**Responsibilities**:
- Gestão do ciclo de vida da tubulação
- Controle de convergência baseado em Semaphore
- Validação do servidor (latência, disponibilidade de linguagem, configuração)
- Delegação aos sub-serviços

**Does NOT contain**:
- Lógica de tradução
- Arquivo I/O para formatos específicos
- Tente novamente a lógica

### Serviço de Tradução de Países

**Responsibilities**:
- Leia do diretório
- Sincronize nomes de países no dicionário local padrão
- Traduzir nomes de país desaparecidos por idioma alvo
- Salve cada dicionário imediatamente após a tradução

**Key behaviors**:
- Se o idioma padrão é o inglês: nomes de países armazenados como é
- Se o idioma padrão for outro, os nomes em inglês serão traduzidos para o idioma padrão primeiro
- Cada idioma é processado independentemente com seu próprio loop de repetição

### Serviço de Tradução de Localização

**Responsibilities**:
- Detecte chaves adicionadas/removidas comparando o dicionário padrão atual com o instantâneo anterior
- Traduza chaves adicionadas em cada idioma alvo
- Remova as chaves apagadas de cada idioma alvo
- Guarde fotos para próxima comparação

**Key behaviors**:
- Traduções manuais sempre têm prioridade
- Chaves adicionadas são traduzidas e salvas por idioma imediatamente
- Chaves removidas são apagadas por idioma imediatamente
- Snapshot só é salvo depois de todas as línguas completarem com sucesso

### Serviço de Tradução de Documentos

**Responsibilities**:
- Caminhe configurando raízes Markdown recursivamente
- Detecte arquivos de origem alterados usando hashes SHA-256
- Traduzir status por bloco em
- Traduzir bloco a bloco com repetição por bloco
- Validar estrutura Markdown após tradução
- Salve cada arquivo de idioma de destino independentemente

**Key behaviors**:
- Granularidade de nível de bloco: títulos, parágrafos, itens de lista são traduzidos separadamente
- Faixas de metadados que bloqueiam sucesso/falha por idioma
- Blocos fracassados são tentados na próxima corrida sem retraduzir blocos bem sucedidos
- A validação da estrutura garante a contagem de cabeçalhos, listas, blocos de código, etc

## Retentar estratégia

O sistema implementa repetições em três níveis:

### Nível 1 - HTTP (LibreTranslateService)

- Até 5 tentativas com retrocesso exponencial (1s, 2s, 3s, 4s, 5s)
- Lida com tempo limite de rede, erros de 5xx e falhas transitórias
- Construído na configuração do cliente HTTP

### Nível 2 - Etapa (Serviço de Tradução)

- Até 3 tentativas com 30 segundos de atraso
- Re-drives todo o pedido de tradução após HTTP-level retries estão esgotados
- Mascaramento e restauração são aplicados neste nível

### Nível 3 - Bloco (Documentos TranslationService)

- Blocos de marcação individuais que falham são marcados em metadados
- Tentamos automaticamente na próxima corrida
- Blocos bem sucedidos nunca são retraduzidos

## Fluxo de dados

### Tradução do dicionário JSON

```
[Default Dictionary]
       ↓
[Compare with Snapshot]
       ↓
[Detect Added/Removed Keys]
       ↓
For each target language:
   ├─ Load existing dictionary
   ├─ Apply removals
   ├─ Translate additions (with retry)
   └─ Save dictionary immediately
       ↓
[Save new snapshot]
```

### Tradução:

```
[Source File]
       ↓
[Compute Hash]
       ↓
[Compare with stored hash]
       ↓
[Load translation metadata]
       ↓
For each target language:
   ├─ Extract translatable blocks
   ├─ For each block:
   │   ├─ Check metadata (already translated?)
   │   ├─ Translate with retry
   │   └─ Mark success/failure in metadata
   ├─ Reconstruct Markdown
   ├─ Validate structure
   ├─ Save target file
   └─ Save metadata
```

### Tradução do nome do país

```
[countries.json]
       ↓
[Build set of country keys]
       ↓
[Update default dictionary]
       ↓
For each target language:
   ├─ Find missing country entries
   ├─ Translate each missing entry (with retry)
   └─ Save dictionary immediately
```

## Persistência do Estado

### Instantâneos

- **JSON**: Stored in a file next to the default dictionary (name varies by storage provider)
- **Purpose**: Enables incremental sync by tracking what was present in the previous run

### Arquivos de hash

- **Markdown**: ao lado do arquivo fonte
- **Fallback**: `{tempDir}/{sanitizedPath}.hash.json` if primary location is read-only
- **Purpose**: Detects source changes to avoid unnecessary re-translation

### Metadados de tradução

- **Markdown**: `{sourceFile}.translation-meta.json`
- **Contents**:
  - Hash de conteúdo fonte
- Status do bloco por linguagem
- Última atualização da hora
- **Purpose**: Enables partial re-translation of only failed blocks

### Armazém de reposição

- **File**: `Locales/placeholders.json`
- **Contents**: Dictionary of keys to placeholder name-value pairs
- **Purpose**: Provides default values for named placeholders across the application

## Sinal R relatando

### Abstração da editora

dissocia os serviços de tradução dos específicos do SignalR:

```csharp
public interface ISignalRPublisher
{
    Task PublishStageAsync<T>(Guid runId, ProcessStage stage, T data, ...);
    Task PublishMessageAsync(Guid runId, LocalizationMessageType type, ProcessStage stage, ...);
}
```

### Garantias de sequência

- Mensagens dentro de uma única corrida são sequenciadas monotonicamente
- Números de sequência são únicos por corrida via
- Clientes podem detectar lacunas ou reordenar

### Mapeamento Hub

```csharp
app.MapHub<LocalizationHub>("/hubs/localization");
```

## Pontos de extensão

### Adicionando um novo alvo de tradução

1. Criar uma nova interface com
2. Implemente a interface com a lógica específica do domínio
3. Registre-se no contêiner DI
4. Injetar no construtor
5. Chamada após estágios existentes

### Política de repetição personalizada

Sobrescrever parâmetros do construtor:

```csharp
services.AddSingleton<TranslationRetryService>(
    sp => new TranslationRetryService(
        sp.GetRequiredService<ILibreTranslateService>(),
        sp.GetRequiredService<IPlaceholderService>(),
        sp.GetRequiredService<ILogger<TranslationRetryService>>(),
        stageMaxRetries: 5,        // More retries
        stageRetryDelaySeconds: 60  // Longer delays
    ));
```

### Manuseio personalizado de placeholder

Implementar para mudar a sintaxe ou armazenamento de espaços:

```csharp
public class CustomPlaceholderService : IPlaceholderService
{
    // Use {{name}} syntax instead of {name}
    // Store in database instead of JSON file
    // Encrypt sensitive placeholder values
}
```

## Configuração

### applications. json

```json
{
  "AutomaticTranslationSettings": {
    "DefaultLanguage": "en",
    "IgnoredLanguages": ["auto", "detect"],
    "MarkdownRoots": ["/Docs", "/Help"],
    "AutomaticRun": true,
    "CheckingPeriod": 30,
    "WaitingTime": "00:00:00",
    "RequestThrottleMs": 80,
    "RequestTimeoutSeconds": 10
  }
}
```

### Afinação de tempo de corrida

ajuste
|---------|---------|--------|
80
10
3
30

## Estratégia de testes

### Testes de unidade

Cada sub-serviço é independentemente testável:

- Enganar para simular sucesso/fracasso
- Toco para verificar o relatório
- Use diretórios temporários para arquivo I/O
- Verifique o comportamento de poupança por linguagem

### Testes de integração

- Oleoduto completo com real LibreTranslate instância
- Verificar sinal Mensagens R são entregues a clientes conectados
- Teste de prevenção concorrente
- Validar estrutura Markdown após tradução

### Testes de ponta a ponta

- Tradução por API ou agendador
- Verifique se todos os arquivos de idioma de destino são criados/atualizados
- Verifique se os arquivos de metadados contêm o status correto do bloco
- Confirmar que os placeholders estão preservados nas traduções

## Considerações de desempenho

- **Memory**: Per-language saving prevents holding all dictionaries in memory
- ** Disk I/O**: Arquivos de metadados adicionam pequena sobrecarga mas permitem um trabalho incremental
- **Network**: Sequential processing with throttling prevents overwhelming LibreTranslate
- **CPU**: SHA-256 hashing and regex validation are fast relative to translation latency
- **SignalR**: Lightweight messages, no payload compression needed for typical reports

## Migração do projeto monolítico

O original continha toda a lógica em uma classe. O caminho da migração:

1. Extrair a lógica do país →
2. Extrair lógica JSON →
3. Extrair lógica de Markdown →
4. Sinal de extração R publicando →
5. Extrair lógica de repetição →
6. Simplifique orquestrador apenas para delegação

Todas as interfaces existentes permanecem inalteradas. Os consumidores do gasoduto não veem mudanças.
