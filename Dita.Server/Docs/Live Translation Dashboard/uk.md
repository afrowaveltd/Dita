# Живий переклад Dashboard

Dashboard - це сторінка адміністратора, яка забезпечує в режимі реального часу видимість в автоматичному перекладі. Підключається до концентрату SignalR і відображає всі заходи трубопроводів, як вони відбуваються.

## КОНТАКТИ

`/Admin/LiveTranslation`

> Note: Authentication and authorization are not yet implemented. Future versions will restrict this page to admin users only.

## Особливості

### Поточний час

Усі події SignalR з перехідного трубопроводу відображаються в живому столі:

- **Sequence number** — Monotonic counter within each pipeline run
- **Timestamp** — Local time when the event was received
- **Run ID** — Shortened GUID for correlation
- **Stage** — Pipeline stage badge (CheckServers, TranslateCountries, etc.)
- **Type** — Message type badge (StageStarted, Progress, StageCompleted, etc.)
- **Message** — Human-readable description
- **Details** — Full JSON payload of the event data

### Колір кодування

Колір
|-------|---------|
Синій ()
Зелений ()
Червоний ()
Білий (за замовчуванням)

### Статус на сервери

Статус банера вгорі показує:
- **Connecting** — Establishing SignalR connection
- **Connected** — Receiving events normally
- **Відключення** — З’єднання втрачено, спробує від’єднатись
- **Disconnected** — Замкнено підключення

З'єднання використовує автоматичне відключення з відключенням: 0s, 2s, 5s, 10s, 30s.

### Контроль

- **Clear Feed** — Removes all displayed messages and resets the counter
- **Export JSON** — Завантаження всіх отриманих повідомлень як файл JSON для аналізу
- **Message counter** — Shows total number of events received in this session

## Навігація

З'єднує панель:

```javascript
const connection = new signalr.HubConnectionBuilder()
    .withUrl("/hubs/localization")
    .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
    .build();
```

### Договір повідомлення

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

### Типи подій

Дашборд ручає всі значення:

Тип
|------|---------|
Синій значок
Зелений значок
Червоний значок
Зелений значок
Червоний значок
Новини
Попередження значка

## Технічна реалізація

### Зареєструватися

- **LocalizationHub** () — SignalR hub, який веде повідомлення до всіх підключених клієнтів
- **ISignalRPublisher** — Abstraction over the hub for use in translation services
- **SignalRPublisher** — Default implementation that increments a monotonic sequence and broadcasts

### Фронт

- Чистий HTML / JS з Bootstrap 5 укладання
- Русский EnglishРусскийУкраїнськаPolskiItalianoEspañol汉语Bahasa Indonesiaहिन्दीPortuguês日本語DeutschFrançaisภาษาไทยελληνικά اللغة العربية
- Не потрібно натиснути на сервер

### Структура сторінки

```
Dita.Server/Pages/Admin/
├── LiveTranslation.cshtml      — Razor page markup + JavaScript
└── LiveTranslation.cshtml.cs     — Page model (empty, data comes via SignalR)
```

## Використання при розробці

1. Почати Діту. Статус на сервери
2. Навігація
3. Перевірити переклад (написатися на графік або виклик API)
4. Перегляд подій в режимі реального часу
5. Використовуйте кнопку Експорт, щоб захопити повний слід для відбілювання

## Майбутні добавки

Плановані поліпшення для приладової панелі:

- **Authentication** — Restrict access to users with the `Admin` role
- **Filtering** — Filter events by stage, type, or run ID
- **Historical runs** — View completed runs from a database or log file
- **Statistics** — Charts showing translation counts, error rates, and latency over time
- **Універсальні тригери** — кнопки для ручного запуску конкретних етапів трубопроводів
- **Конфігурація** — Редагувати безпосередньо з панелі інструментів
- **Language Management** — Перегляд та редагування підтримуваних мов
- **Dictionary preview** — Browse and search localization dictionaries

## Виправлення несправностей

### Дешборд показує "Не вдалося підключитися"

1. Перевірити сервер працює і доступний
2. Перевірити консолі браузера для корпоративних або мережевих помилок
3. Підтвердити присутні
4. Забезпечити відсутність брандмауера блокує підключення WebSocket

### Події не з'являються

1. Перевірте, що URL-адреса SignalR між сервером () і клієнтом ()
2. Перевірити графік ввімкнено
3. Дивитися на серверних колодах для помилок конвеєра
4. Перевірити вкладку мережі браузера для повідомлень WebSocket

### Повідомлення з замовлення

Поле гарантує замовлення в межах одного ходу. Якщо повідомлення з'являються з замовлення, це може вказувати:
- Кілька трубопроводів перекриття (нав'язується з замком смофору)
- Випадкові проблеми з відновленням сторінок
