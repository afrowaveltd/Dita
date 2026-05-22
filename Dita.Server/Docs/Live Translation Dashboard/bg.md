# Live превод Dashboard

The Live Translation Dashboard е администраторска страница, която осигурява видимост в реално време в автоматичния превод тръбопровод. Той се свързва към SignalR хъб и показва всички събития, свързани с тръбопровода.

## АДРЕС

`/Admin/LiveTranslation`

> Note: Authentication and authorization are not yet implemented. Future versions will restrict this page to admin users only.

## Характеристики

### Поток на събития в реално време

Всички събития на SignalR от преводаческия тръбопровод са показани в жива таблица:

- **Sequence number** — Monotonic counter within each pipeline run
- ** Time knight**   годежно време, когато събитието е получено
- **Run ID** .
- **Stage** Сценична значка (checkServers, TranslateCountries и др.)
- **Тип**  годежен знак за тип съобщение (StageStarted, Progress, StageCompleted и др.)
- **Съобщение**
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
- ** Connecting **  год
- ** Connectioned **
- ** Reconnecting **
- **Разпределено**  годежът е затворен

Връзката използва автоматична връзка с експоненциално отстъпление: 0s, 2s, 5s, 10s, 30s.

### Контрол

- **Clear Feed **  гофрира всички показани съобщения и нулира брояча
- **Export JSON** — Downloads all received messages as a JSON file for analysis
- **Message counter** — Shows total number of events received in this session

## Функционален модул за сигнализация

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

- **LocalizationHub ** () . .
- **ISignalRPublisher** — Abstraction over the hub for use in translation services
- **SignalRPublisher** — Default implementation that increments a monotonic sequence and broadcasts

### Преден

- Чист HTML/JS с Bootstrap 5 стил
- Използва библиотеката на Microsoft SignalR JavaScript клиент (заредена от CDN)
- Не се изисква сървърно предаване за захранването на събитието

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
5. Използвайте бутона Export, за да уловите пълна следа за отстраняване

## Бъдещи подобрения

Планирани подобрения на таблото:

- ** Authentication **  год
- **Filtering **  год
- **Historical runs** — View completed runs from a database or log file
- ** Статии **   по-долу показва броя на преводите, процентите на грешки и латентност с течение на времето
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

1. Проверете дали URL адреса на SignerR съвпада между сървър () и клиент ()
2. Проверка на графика в
3. Вижте сървърни дневници за грешки на превод тръбопровод
4. Проверка на браузъра Мрежа за WebSocket съобщения

### Съобщенията не работят

Полето гарантира поръчване в рамките на един опит. Ако съобщенията се появят извън строя, те могат да посочат:
- Множество тръбопроводи се припокриват (не трябва да се случва поради семафор заключване)
- Броузър превод въпроси (Опитайте освежаване на страницата)
