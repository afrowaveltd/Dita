# 活口翻譯板

在自動翻譯管道中提供实时能見度。 它能接通信號R中枢并顯示出所發生的所有管道事件.

## 網址

`/Admin/LiveTranslation`

> Note: Authentication and authorization are not yet implemented. Future versions will restrict this page to admin users only.

## 地貌

### 即時事件流

所有信號 R出自翻譯管道的事件被顯示在活的更新表:

- ** 序号**-每通管所收取的收音机
- **Timestamp** — Local time when the event was received
- ** 跑取 ID ** 相關相關相關相關的簡短介面
- **Stage** — Pipeline stage badge (CheckServers, TranslateCountries, etc.)
- ** Type ** — 信件類型徽章 (StageStarted, Progress, Stage Cupleted等)
- ** 信**-人可讀取的描述
- ** 详细信息** -- -- 事件資料的全部JSON有效载荷

### 顏色編碼

顏色
|-------|---------|
藍 ()
綠 ()
紅 ()
白 (默认)

### 連接狀態

在最上面顯示的狀態標籤:
- **相接**-建立信號連接
- ** 接通** -- -- 接通正常事件
- **再接**-接通已輸出,想再接通
- ** 已斷接**-已關接

二相接相接相接相接相接相接相接相接相接相接相接相接相接相接相接相接相接相接相接相接相接相接相接相接相接相接相接相接相接相接相接相接相接相接相接相接相接相接相接相接相接相接相接相接相接相接相接相接相接相接相接相接相接相接相接相接相接相接相接相接相接相接相接相接相接相接相接相接相接相接相接相接相接相接相接相接相接相接相接相接相接相接相接相接相接相接相接相接相接相接相接相接相接相接相接相接相接相接相接相接相接相接相接相相接相相接相接相相接相接相接相接相接相接相接相接相接相接相接相接相接相接相相相接相接相相相相相相.

### 控制

- ** 清除 feed ** —取出所顯示的所有信件并重放收取器
- ** 匯出 JSON **- 下載所有收取的信件作 JSON 檔案以作分析
- **Message counter** — Shows total number of events received in this session

## 信號 R中枢

有:

```javascript
const connection = new signalr.HubConnectionBuilder()
    .withUrl("/hubs/localization")
    .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
    .build();
```

### 信件合同

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

### 事件型態

儀表板處理所有值:

類型
|------|---------|
有藍色徽章
綠色徽章
有紅相接相接相接相接相接相接相接相接相接相接相接相接相接相接相接相接相接相接相接相接相接相接相接相接相接相接相接相接相接相接相接相接相接相接相接相接相接相接相接相接相接相接相接相接相接相接相接相接相接相接相接相接相接相相接相相接相相接相相接相接相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相
綠色徽章
有紅相接相接相接相接相接相接相接相接相接相接相接相接相接相接相接相接相接相接相接相接相接相接相接相接相接相接相接相接相接相接相接相接相接相接相接相接相接相接相接相接相接相接相接相接相接相接相接相接相接相接相接相接相接相相接相相接相相接相相接相接相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相相
信息徽章
警示徽章

## 技術

### 后端

- ** 就地化 Cub** ()- 向相關客戶通訊的 SignalR 中枢
- ** ISignalRPublisher**-收錄了中心站供翻譯用
- ** SignalRPublisher **- 增加一單音序列并放送的預設實作

### 前端

- 纯 HTML/JS 有 Bootstrap 5 外形
- 使用 Microsoft SignalR JavaScript 客戶端文庫 (由 CDN 載入)
- 事件 feed 不需要伺服器端的渲染

### 頁面结构

```
Dita.Server/Pages/Admin/
├── LiveTranslation.cshtml      — Razor page markup + JavaScript
└── LiveTranslation.cshtml.cs     — Page model (empty, data comes via SignalR)
```

## 二. 开发用法

1. 起起起起起起起起起起起起起起起起起起起起起起起起起起起起起起起起起起起起起起起起起起起起起起起起起起起起起起起起起起起起起起起起起起起起起起起起起起起起起起起起起起起起起起起起起起起起起起起起起起起起起起起起起起起起起起起起起起起起起起起起起起起起起起起起起起起起起起起起起起起起起起起起起起起起起起起起起起起起起起起起起起起起起起起起起起起起起起起起起起起起起起起起起起起起起起起起起起起起起起起起起起起起起起起起起起起起起起起起起起起起起起起起起起起起起起起起起起起起起起起起起起起起起起起起起起起起起起起 伺服器應用程式
2. 通航到
3. 触发翻譯跑( 等排程器或呼叫 API)
4. 二. 就地出事了
5. 使用匯出按鈕取回去除錯的全部追蹤

## 未來的增强

二. 程序表

- ** 校正** —— 有角色限制使用者存取
- ** Filtering ** — 按相關相關相關相關相關相關相關相關相關相關相關相
- ** 歷史跑道 **- 从數據庫或紀錄檔取出已完成的檢視
- ** 统计数据** -- -- 表明翻譯數量、出錯率和相去不遠的圖
- **Manual triggers** — Buttons to manually start specific pipeline stages
- **相接**-由仪表板直接編輯
- ** 語言管理**-查看并編輯所支持的語言
- ** 字典預覽**-瀏覽和搜尋地區字典

## 有麻煩了

### Dashboard 顯示"未能連接"

1. 檢查伺服器已執行并可存取
2. 为 CORS 或網路錯誤檢查瀏覽器控制台
3. 有確認
4. 确保沒有防火牆阻擋 WebSocket 連接

### 有出事了

1. 檢查 signalR 中枢 URL 在伺服器 () 與客戶端 ()相匹配
2. 檢查排程器已開啟于
3. 查看伺服器紀錄中翻譯管道出錯
4. 檢查瀏覽器 WebSocket 信件的網路分頁

### 有訊息出錯

就一跑一跑 就讓地盤出事了 如果信件出錯,可以指:
- 有多條管線相接 (由於 semaphore 鎖定而起)
- 瀏覽器渲染出問題 (試取刷新頁面)
