# Painel de tradução ao vivo

O Painel de Tradução ao Vivo é uma página de administração que fornece visibilidade em tempo real no oleoduto de tradução automática. Ele se conecta ao hub SignalR e exibe todos os eventos do oleoduto enquanto ocorrem.

## URL

`/Admin/LiveTranslation`

> Note: Authentication and authorization are not yet implemented. Future versions will restrict this page to admin users only.

## Características

### Fluxo de eventos em tempo real

Todos os sinais R eventos do oleoduto de tradução são exibidos em uma tabela ao vivo:

- **Sequence number** — Monotonic counter within each pipeline run
- **Timestamp** — Local time when the event was received
- **Run ID** — Shortened GUID for correlation
- ** Stage** - Pipeline emblema de palco (CheckServers, Traduzir Países, etc.)
- **Type** — Message type badge (StageStarted, Progress, StageCompleted, etc.)
- **Message** — Human-readable description
- **Detalhes** - Carga completa JSON dos dados do evento

### Codificação de cores

Cor
|-------|---------|
Azul
Verde ()
Vermelho ()
Branco (padrão)

### Estado da conexão

Uma faixa de status no topo mostra:
- **Connecting** — Establishing SignalR connection
- **Connected** — Receiving events normally
- **Reconectando** - Conexão perdida, tentando reconectar
- **Disconnected** — Connection closed

A conexão usa reconexão automática com retrocesso exponencial: 0s, 2s, 5s, 10s, 30s.

### Controles

- **Clear Feed** — Removes all displayed messages and resets the counter
- **Export JSON** - Downloads todas as mensagens recebidas como um arquivo JSON para análise
- **Message counter** — Shows total number of events received in this session

## Sinal R hub

O painel se conecta a:

```javascript
const connection = new signalr.HubConnectionBuilder()
    .withUrl("/hubs/localization")
    .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
    .build();
```

### Contrato de mensagem

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

### Tipos de eventos

O painel controla todos os valores:

Tipo
|------|---------|
Emblema azul
Emblema verde
Emblema vermelho
Emblema verde
Emblema vermelho
Emblema de informação
Emblema de aviso

## Implementação técnica

### Infraestrutura

- **LocalizationHub** (`/hubs/localization`) — SignalR hub that broadcasts messages to all connected clients
- **ISignalRPublisher** — Abstraction over the hub for use in translation services
- **SignalR Publisher** - Implementação padrão que incrementa uma sequência monotônica e transmite

### Frontend

- Puro HTML/JS com estilo Bootstrap 5
- Usa a biblioteca cliente Microsoft SignalR JavaScript (carregada do CDN)
- Nenhuma renderização do lado do servidor necessária para a transmissão do evento

### Estrutura da página

```
Dita.Server/Pages/Admin/
├── LiveTranslation.cshtml      — Razor page markup + JavaScript
└── LiveTranslation.cshtml.cs     — Page model (empty, data comes via SignalR)
```

## Uso durante o desenvolvimento

1. Comece o Dita. Aplicação de servidor
2. Navegar para
3. Acione uma execução de tradução (ou espere pelo agendador ou ligue para a API)
4. Os eventos aparecem em tempo real
5. Use o botão Exportar para capturar um rastro completo para depuração

## Melhorias futuras

Melhorias planejadas para o painel:

- ** Autenticação** - Restringir o acesso aos usuários com o papel
- **Filtering** — Filter events by stage, type, or run ID
- ** Historical runs** - Vista concluída corre de um banco de dados ou arquivo de registro
- **Statistics** — Charts showing translation counts, error rates, and latency over time
- **Manual triggers** — Buttons to manually start specific pipeline stages
- **Configuração** - Editar diretamente do painel
- ** Gerenciamento de idiomas** - Ver e editar idiomas suportados
- ** Previsão dictionary** - Procurar e procurar dicionários de localização

## Solução de problemas

### O painel mostra "Falhou em se conectar"

1. Verifique se o servidor está em execução e acessível
2. Verifique o console do navegador para erros de rede ou CORS
3. Confirmar está presente em
4. Garanta que nenhum firewall bloqueie conexões WebSocket

### Os eventos não estão aparecendo

1. Verifique se a URL do hub SignalR combina entre servidor () e cliente ()
2. Verifique se o agendador está ativado
3. Veja os registros do servidor para erros de tradução
4. Verifique o navegador Página de rede para mensagens WebSocket

### Mensagens estão fora de ordem

O campo garante ordem em uma única corrida. Se as mensagens aparecerem fora de ordem, pode indicar:
- Vários oleodutos se sobrepõem
- Tradução:
