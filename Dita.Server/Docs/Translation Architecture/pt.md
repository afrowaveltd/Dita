# Arquitetura de Tradução

Este documento descreve a arquitetura modular do sistema de tradução automática de Dita, introduzido para melhorar a manutenção, testabilidade e resiliência.

## Objectivos de concepção

O refatoramento abordou várias preocupações com o projeto monolítico original:

- ** Separação de preocupações**: Cada domínio de tradução (países, dicionários JSON, Markdown) é isolado.
- ** Persistência incremental**: Os arquivos são salvos por idioma imediatamente após a tradução, reduzindo o uso da memória e fornecendo resultados anteriores.
- ** Resiliência**: Múltiplos níveis de repetição manuseiam falhas transitórias sem bloquear todo o oleoduto.
- **Observabilidade**: Todas as operações significativas são relatadas via SignalR para monitoramento em tempo real.
- **Extensibilidade**: Novos alvos de tradução podem ser adicionados implementando uma única interface.

## Descomposição do serviço

### Infra- EstruturaTranslationService (orquestrator)

**Responsabilidades**:
- Gestão do ciclo de vida da tubulação (início, conclusão, manipulação de erros)
- Controle de congruência baseado em semáforo (preveni corridas sobrepostas)
- Validação do servidor (latência, disponibilidade do idioma, configuração)
- Delegação aos sub-serviços

** NÃO contém **:
- Lógica de tradução
- Ficheiro I/O para formatos específicos
- Repetir a lógica

### Serviço de Tradução de Países

**Responsabilidades**:
- Ler da pasta
- Sincronizar os nomes dos países no dicionário local padrão
- Traduzir nomes de países em falta por idioma alvo
- Gravar cada dicionário de destino imediatamente após a tradução

** Comportamentos-chave**:
- Se o idioma padrão for Inglês: nomes de países armazenados como-is
- Se o idioma padrão for outro: Nomes em inglês traduzidos para o idioma padrão primeiro
- Cada idioma é processado independentemente com seu próprio loop de repetição

### Serviço de Tradução de Localização

**Responsabilidades**:
- Detectar chaves adicionadas/removidas comparando o dicionário padrão atual com o instantâneo anterior
- Traduzir chaves adicionadas em cada idioma alvo
- Remover chaves apagadas de cada idioma alvo
- Salvar instantâneo para a próxima comparação

** Comportamentos-chave**:
- As traduções manuais têm sempre prioridade (nunca sobrescritas)
- Chaves adicionadas são traduzidas e salvas por idioma imediatamente
- Chaves removidas são apagadas por idioma imediatamente
- O snapshot só é salvo depois de todos os idiomas terminarem com sucesso

### Serviço de Tradução de Documentos

**Responsabilidades**:
- Caminhar as raizes de Markdown configuradas recursivamente
- Detecta ficheiros de código alterados com o SHA-256 hashes
- Acompanhar o estado da tradução por bloco
- Traduzir bloco a bloco com repetição por bloco
- Validar a estrutura de marcação após a tradução
- Gravar cada ficheiro de idioma de destino independentemente

** Comportamentos-chave**:
- Granularidade em bloco: rubricas, parágrafos, itens da lista são traduzidos separadamente
- Faixas de metadados que bloqueiam ou falham por idioma
- Blocos falhados são tentados novamente na próxima execução sem re-traduzir blocos bem sucedidos
- A validação da estrutura garante a contagem de cabeçalhos, listas, blocos de código, etc

## Estratégia de repetição

O sistema implementa repetições a três níveis:

### Nível 1 — HTTP (LibreTranslateService)

- Até 5 tentativas com retrocesso exponencial (1s, 2s, 3s, 4s, 5s)
- Lida com timeouts de rede, erros 5xx e falhas transitórias
- Compilada na configuração do cliente HTTP

### Nível 2 — Fase (Serviço de tradução)

- Até 3 tentativas com 30 segundos de atraso
- Re-drives toda a requisição de tradução depois de HTTP-level retries estão esgotados
- O mascaramento e a restauração do placeholder são aplicados neste nível

### Nível 3 — Bloco (DocumentosTranslationService)

- Blocos de marcação individuais que falham estão marcados em metadados
- Repetiu automaticamente na próxima execução do gasoduto
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

### Tradução de marcação para baixo

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

- **JSON**: Armazenado em um arquivo ao lado do dicionário padrão (nome varia pelo provedor de armazenamento)
- **Purpose**: Permite a sincronização incremental rastreando o que estava presente na execução anterior

### Ficheiros de hash

- **Markdown**: ao lado do arquivo fonte
- ** Fallback**: se a localização primária é somente leitura
- **Purpose**: Detecta alterações na fonte para evitar re-tradução desnecessária

### Metadados de tradução

- **Markedown**:
- **Conteúdo**:
  - Hash de conteúdo fonte
- Status do bloco por idioma (array de booleanos)
- Hora da última atualização
- **Purpose**: Activa a re-tradução parcial de apenas blocos falhados

### Armazenagem no local

- ** Ficheiro**:
- **Conteúdo**: Dicionário de chaves para pares de nomes de placeholder
- **Purpose**: Fornece valores padrão para placeholders nomeados em toda a aplicação

## Sinal Relatório R

### Abstração do editor

dissocia os serviços de tradução dos específicos do SignalR:

```csharp
public interface ISignalRPublisher
{
    Task PublishStageAsync<T>(Guid runId, ProcessStage stage, T data, ...);
    Task PublishMessageAsync(Guid runId, LocalizationMessageType type, ProcessStage stage, ...);
}
```

### Garantias de sequência

- Mensagens dentro de uma única execução são sequenciadas monotonicamente
- Números de sequência são únicos por execução via
- Os clientes podem detectar lacunas ou reordenar

### Mapeamento do Hub

```csharp
app.MapHub<LocalizationHub>("/hubs/localization");
```

## Pontos de extensão

### Adicionando um novo alvo de tradução

1. Criar uma nova interface com
2. Implementar a interface com a lógica específica do domínio
3. Registo no recipiente DI
4. Injectar no construtor
5. Chamada após as fases existentes

### Política de repetição personalizada

Sobrescrever os parâmetros do construtor:

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

### Manuseamento personalizado de espaços

Implementar para alterar a sintaxe ou armazenamento do placeholder:

```csharp
public class CustomPlaceholderService : IPlaceholderService
{
    // Use {{name}} syntax instead of {name}
    // Store in database instead of JSON file
    // Encrypt sensitive placeholder values
}
```

## Configuração

### appsets.json

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

### Ajuste de tempo de execução

Configuração
|---------|---------|--------|
80
10
3
30

## Estratégia de ensaio

### Ensaios unitários

Cada sub-serviço é independentemente testável:

- Mock para simular sucesso/fracasso
- Mock para verificar o relatório
- Usar diretórios temporários para o arquivo E/S
- Verificar o comportamento de gravação por linguagem

### Testes de integração

- O gasoduto completo é executado com uma instância LibreTranslate real (local)
- Verificar o Sinal Mensagens R são entregues a clientes conectados
- Teste de prevenção simultânea (semáforo)
- Validar a estrutura de marcação após a tradução

### Ensaios de ponta a ponta

- Ativar a tradução via API ou agendador
- Verificar todos os arquivos de idioma de destino são criados/atualizados
- Verificar se os ficheiros de meta- dados contêm o estado correcto do bloco
- Confirmar os placeholders são preservados nas traduções

## Considerações sobre o desempenho

- ** Memória**: Por-language saving evita manter todos os dicionários na memória
- **Disk E/O**: Arquivos de metadados adicionam pequena sobrecarga, mas habilitam o trabalho incremental
- **Rede**: O processamento sequencial com estrangulamento evita a esmagadora LibreTranslate
- **CPU**: validação de hashing e regex SHA-256 são rápidas em relação à latência da tradução
- **SignalR**: Mensagens leves, sem compressão de carga útil necessária para relatórios típicos

## Migração do desenho monolítico

O original continha toda a lógica em uma classe. O caminho da migração:

1. Extrair a lógica do país →
2. Extrair lógica JSON →
3. Extrair lógica de Markdown →
4. Sinal de Extração R publicando →
5. Extrair lógica de repetição →
6. Simplificar orquestrador apenas para delegação

Todas as interfaces existentes () permanecem inalteradas. Os consumidores do gasoduto não vêem mudanças de ruptura.
