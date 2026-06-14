# Дашборд на живоName

На живо превод Dashboard е администратор страница, която осигурява видимост в реално време в автоматичния превод тръбопровод. Той се свързва към SignalR хъб и показва всички събития на тръбопровода в момента.

## АДРЕС

`/Admin/LiveTranslation`

> Note: Authentication and authorization are not yet implemented. Future versions will restrict this page to admin users only.

## Характеристики

### Поток на събития в реално време

Всички събития на SignalR от преводаческия тръбопровод са показани в жива таблица:

- **Начало номер **  горно брояч във всеки тръбопровод
- ** Time невалидно **  год
- **Run ID** .
- **Стаж** Сцена (checkServers, TranslateCountries и др.)
- **Тип**  годежен знак за тип съобщение (StageStarted, Progress, StageCompleted и др.)
- **Съобщение** ..
- **Детайли ** .

### Цветово кодиране

Цвят
|-------|---------|
Синьо ()
Зелено ()
Червено ()
Бял (по подразбиране)

### Състояние на връзката

Банер за статус на върха показва:
- ** Connecting **  год. връзка
- ** Connectioned **
- ** Reconnecting **
- **Разпределено**  това е затворено

Връзката използва автоматична връзка с експоненциално отстъпление: 0s, 2s, 5s, 10s, 30s.

### Контрол

- **Clear Feed **  гони всички показани съобщения и нулира брояча
- **Export JSON** — Downloads all received messages as a JSON file for analysis
- **Message counter** — Shows total number of events received in this session

## Разклонител SignalR

Арматурното табло се свързва към:

```javascript
const connection = new signalr.HubConnectionBuilder()
    .withUrl("/hubs/localization")
    .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
    .build();
```

### Договор за съобщение

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

### Вид събития

Арматурното табло обработва всички стойности:

Тип
|------|---------|
Синя значка
Зелена значка
Червен знак
Зелена значка
Червен знак
Значка за информация
Сигнална значка

## Техническа реализация

### Ядро

- **LocalizationHub** () .
- **ISignalRPublisher** — Abstraction over the hub for use in translation services
- **SignalRPublisher** — Default implementation that increments a monotonic sequence and broadcasts

### Преден

- Чист HTML/JS с Bootstrap 5 стил
- Използва библиотеката на Microsoft SignalR JavaScript клиент (заредена от CDN)
- Не се изисква сървърно предаване за емисията на събития

### Структура на страницата

```
Dita.Server/Pages/Admin/
├── LiveTranslation.cshtml      — Razor page markup + JavaScript
└── LiveTranslation.cshtml.cs     — Page model (empty, data comes via SignalR)
```

## Използване по време на разработването

1. Започни Дита. Приложение на сървъра
2. Навигация на
3. Задействане на рън за превод (или изчакайте графика или се обадете на API)
4. Събитията се появяват в реално време
5. Използвайте бутона Експортиране, за да уловите пълна следа за отстраняване

## Бъдещи подобрения

Планирани подобрения на таблото:

- ** Authentication **  год.Ограничаване на достъпа до потребители с ролята
- **Filtering **  год
- **Historical runs** — View completed runs from a database or log file
- **Statistics** — Charts showing translation counts, error rates, and latency over time
- **Manual triggers** — Buttons to manually start specific pipeline stages
- ** Configuration **  год
- ** Управление на езици**  год
- **Дикционален преглед**  год

## Отстраняване

### Dashboard показва "Неуспешно свързване"

1. Проверка на сървъра работи и достъпно
2. Проверка на конзолата на браузъра за CORS или мрежови грешки
3. Потвърждавам
4. Уверете се, че няма защитна стена блокира WebSocket връзки

### Събитията не се появяват

1. Проверете дали URL-а на SignagerR хъб съвпада между сървър () и клиент ()
2. Проверка на графика в
3. Вижте сървърни дневници за грешки на превод тръбопровод
4. Проверете браузъра Мрежа подпрозорец за WebSocket съобщения

### Съобщенията не работят

Полето гарантира поръчване в рамките на един опит. Ако съобщенията се появят извън строя, те могат да посочат:
- Множество тръбопроводни работи припокриване (не трябва да се случи поради семафор заключване)
- Броузерни въпроси (Опитайте освежаване на страницата)
