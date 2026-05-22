# Тирүү котормо панели

Live Translation Dashboard - бул автоматтык котормо түтүктөрүнө реалдуу убакыт режиминде көрүнүүчү администратордук баракча. Ал SignalR борборуна туташтырылат жана бардык түтүктөр окуяларын пайда болгондо көрсөтөт.

## URL ДАРЕГИ

`/Admin/LiveTranslation`

> Note: Authentication and authorization are not yet implemented. Future versions will restrict this page to admin users only.

## Анын өзгөчөлүктөрү

### Чыныгы убакыттагы иш-чаралардын агымы

Котормо түтүктөрүндөгү бардык SignalR окуялары жандуу жаңыртуу таблицасында көрсөтүлгөн:

- **Sequence number** — Monotonic counter within each pipeline run
- ** Убакыттын өтүшү** Иш-чара кабыл алынган жергиликтүү убакыт
- **Run ID** — Shortened GUID for correlation
- **Stage** — Pipeline stage badge (CheckServers, TranslateCountries, etc.)
- **Type** — Message type badge (StageStarted, Progress, StageCompleted, etc.)
- ** Билдирүү** Адамдар окуй турган сүрөттөмө
- **Details** — Full JSON payload of the event data

### Түстүү коддоо

Түстүүсү
|-------|---------|
Көк ()
Жашыл ()
Кызыл (кызыл)
Ак (дефолт)

### Байланыш абалы

Жогорку деңгээлдеги абал белгиси төмөнкүлөрдү көрсөтөт:
- ** Байланыш** SignalR байланышын түзүү
- **Connected** — Receiving events normally
- **Reconnecting** — Connection lost, attempting to reconnect
- ** Байланышсыз** Байланыш жабылган

Бул байланыш экспоненциалдык резервдик байланыш менен автоматтык түрдө кайра туташууну колдонот: 0s, 2s, 5s, 10s, 30s.

### Башкаруулар

- **Clear Feed** — Removes all displayed messages and resets the counter
- **Export JSON** — Downloads all received messages as a JSON file for analysis
- ** Билдирүү эсептегичи** Бул сессияда алынган иш-чаралардын жалпы санын көрсөтөт

## SignalR борбору

Капталдагы такта төмөнкүлөргө туташтырылат:

```javascript
const connection = new signalr.HubConnectionBuilder()
    .withUrl("/hubs/localization")
    .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
    .build();
```

### Билдирүү келишими

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

### Окуялардын түрлөрү

Панель бардык маанилерди иштетет:

Тип түрү
|------|---------|
Көк белги
Жашыл белги
Кызыл белги
Жашыл белги
Кызыл белги
Маалымат белгиси
Эскертүү белгиси

## Техникалык ишке ашыруу

### Кайра кайтарып берүү

- **LocalizationHub** (`/hubs/localization`) — SignalR hub that broadcasts messages to all connected clients
- **ISignalRPublisher** — Abstraction over the hub for use in translation services
- **SignalRPublisher** — Default implementation that increments a monotonic sequence and broadcasts

### Фронт-энд

- HTML-JS менен Bootstrap 5 стили
- Microsoft SignalR JavaScript клиенттик китепканасы (CDNден жүктөлгөн)
- Окуялар үчүн сервердик рендеринг талап кылынбайт

### Баракчанын түзүлүшү

```
Dita.Server/Pages/Admin/
├── LiveTranslation.cshtml      — Razor page markup + JavaScript
└── LiveTranslation.cshtml.cs     — Page model (empty, data comes via SignalR)
```

## Өнүктүрүү учурунда колдонуу

1. Дитаны баштагыла. Server тиркемеси
2. Навигат
3. Котормону баштоо (пластинаторду күтүү же APIге чалуу)
4. Көрүү окуялары реалдуу убакытта пайда болот
5. Экспорттук баскычты колдонуп, ката кетирүү үчүн толук изин алыңыз

## Келечектеги жакшыртуулар

Машина тактасын жакшыртуу пландаштырылган:

- **Authentication** — Restrict access to users with the `Admin` role
- ** Фильтрлөө** Фильтрлөө окуялары сахна, тип же иштөө идентификатору боюнча
- **Historical runs** — View completed runs from a database or log file
- **Statistics** — Charts showing translation counts, error rates, and latency over time
- **Manual triggers** — Buttons to manually start specific pipeline stages
- **Configuration** — Edit `AutomaticTranslationSettings` directly from the dashboard
- **Language management** — View and edit supported languages
- **Dictionary preview** — Browse and search localization dictionaries

## Кыйынчылыктарды чечүү

### Дашборддо "байланышсыз" деген жазуу бар

1. Сервердин иштеп жатканын жана жеткиликтүү экендигин текшериңиз
2. Браузердин консолун CORS же тармактык каталар үчүн текшериңиз
3. Тастыктоо
4. Firewall WebSocket байланыштарын тосуп албашын камсыз кылуу

### Окуялар көрүнбөйт

1. SignalR хабынын URL дареги сервер менен клиенттин ортосундагы дал келгенин текшериңиз
2. Пландаштыруучунун иштетилгендигин текшериңиз
3. Котормо түтүктөрүнүн каталары үчүн сервердик журналдарды караңыз
4. WebSocket билдирүүлөрү үчүн браузердин тармактык баракчасын текшериңиз

### Билдирүүлөр туура эмес болуп жатат

Талаа бир эле мөөнөттө буйрутма берүүгө кепилдик берет. Эгерде билдирүүлөр туура эмес болсо, анда төмөнкүлөр көрсөтүлүшү мүмкүн:
- Бир нече түтүктөр бири-бирине дал келиши керек (семафордун кулпусунан улам болбошу керек)
- Браузердин рендеринг маселелери (баракчаны жаңыртууга аракет кылыңыз)
