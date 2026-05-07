# Painel de tradução ao vivo

O Live Translation Dashboard é uma página de administração que fornece visibilidade em tempo real no oleoduto de tradução automática. Ele se conecta ao hub SignalR e exibe todos os eventos de pipeline à medida que ocorrem.

## URL

`/Admin/LiveTranslation`

> Note: Authentication and authorization are not yet implemented. Future versions will restrict this page to admin users only.

## Características

### Fluxo de eventos em tempo real

Todos os sinais Os eventos R do oleoduto de tradução são exibidos em uma tabela de atualização ao vivo:

- ** Número de sequência** — Contador monotónico dentro de cada canalização
- ** Timestamp** — Hora local em que o evento foi recebido
- ** ID de execução** — GUID abreviado para correlação
- ** Stage** — Pipeline emblema de palco (CheckServers, TraduzirPaíses, etc.)
- **Type** — Emblema de tipo de mensagem (StageStarted, Progress, StageCompleted, etc.)
- **Mensagem** — Descrição legível pelo Homem
- **Detalhes** — Carga útil completa da JSON dos dados do evento

### Codificação de cores

Cor
|-------|---------|
Azul ()
Verde ()
Vermelho ()
Branco (por omissão)

### Estado da ligação

Um banner de status no topo mostra:
- ** Conectando** — Estabelecendo conexão SignalR
- ** Conectado** — Recebendo eventos normalmente
- **Reconectando** — Ligação perdida, tentando reconectar
- ** Desligado** — Ligação fechada

A conexão utiliza reconexão automática com backoff exponencial: 0s, 2s, 5s, 10s, 30s.

### Controlos

- **Limpar Feed** — Remove todas as mensagens exibidas e reinicia o contador
- **Export JSON** — Downloads todas as mensagens recebidas como arquivo JSON para análise
- ** Contador de mensagens** — Mostra o número total de eventos recebidos nesta sessão

## Sinal R hub

O painel liga-se a:

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

O painel lida com todos os valores:

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

- ** Localização Hub** () — SignalR hub que transmite mensagens para todos os clientes conectados
- **ISignalR Publisher** — Abstração sobre o hub para uso em serviços de tradução
- **SignalRPublisher** — Implementação padrão que incrementa uma sequência monotônica e transmissões

### interface

- Puro HTML/JS com estilo Bootstrap 5
- Usa a biblioteca cliente Microsoft SignalR JavaScript (carregada do CDN)
- Nenhuma renderização do lado do servidor necessária para a fonte de eventos

### Estrutura da página

```
Dita.Server/Pages/Admin/
├── LiveTranslation.cshtml      — Razor page markup + JavaScript
└── LiveTranslation.cshtml.cs     — Page model (empty, data comes via SignalR)
```

## Utilização durante o desenvolvimento

1. Liga o Dita. Aplicação do servidor
2. Navegar para
3. Activar uma execução de tradução (ou esperar pelo programador ou chamar a API)
4. Observar os eventos aparecerem em tempo real
5. Use o botão Exportar para capturar um traço completo para depuração

## Melhorias futuras

Melhorias planejadas para o painel:

- **Autenticação** — Limitar o acesso aos utilizadores com o papel
- **Filtragem** — Filtrar eventos por fase, tipo ou ID de execução
- **Historic runs** — View completed runs from a database or log file
- ** Estatísticas** — Gráficos mostrando contagens de traduções, taxas de erro e latência ao longo do tempo
- **Ativadores manuais** — Botões para iniciar manualmente estágios específicos de tubulação
- **Configuração** — Editar diretamente do painel
- ** Gerenciamento de idiomas** — Ver e editar idiomas suportados
- ** Visualização dictionary** — Dicionários de localização de navegação e pesquisa

## Resolução de Problemas

### O painel mostra "Não foi possível ligar"

1. Verificar se o servidor está em execução e acessível
2. Verificar o console do navegador para erros de rede ou CORS
3. Confirmar está presente em
4. Garantir que nenhum firewall está bloqueando conexões WebSocket

### Os eventos não estão aparecendo

1. Verifique se o URL do hub SignalR corresponde entre servidor () e cliente ()
2. Verificar se o escalonador está activo
3. Veja os registros do servidor para erros de tradução
4. Verificar navegador Página de rede para mensagens WebSocket

### As mensagens estão fora de ordem

O campo garante a ordenação dentro de uma única corrida. Se as mensagens aparecerem fora de ordem, pode indicar:
- Multiple pipeline corre sobreposição (não deve acontecer devido ao bloqueio semáforo)
- Problemas de renderização do navegador (tentar atualizar a página)
