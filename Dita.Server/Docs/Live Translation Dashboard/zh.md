# 现场翻译 Dashboard

Live Craw Dashboard是一款可实时收发到自动翻译管道的管理员页面. 它连接到SignalR中枢,并在发生时显示所有管道事件.

## 網址

`/Admin/LiveTranslation`

> Note: Authentication and authorization are not yet implemented. Future versions will restrict this page to admin users only.

## 特征

### 实时事件流

所有信号 来自翻译管道的 R 事件显示在一个实时更新的表格中:

- ** 序列号**——每条输油管内的Monotonic计数器
- ** 时间戳**——收到活动的当地时间
- ** 运行 ID ** – 关联性图形界面缩短
- **Stage** — 管道舞台徽章(检查工、翻译等)
- ** Type ** — 信件类型徽章( StageStarted, Progress, Stage Conflected等)
- ** Message**——人可读描述
- ** 详细情况**——事件数据的全部JSON有效载荷

### 颜色编码

颜色
|-------|---------|
蓝色( )
绿色( )
红色( )
白 (默认)

### 连接状态

顶级显示的状态横幅 :
- ** 连接**——建立信号R连接
- ** 连接** - 通常接收事件
- **重新连接** — 连接丢失, 试图重新连接
- ** 已断开** — 连接已关闭

连接使用以指数回放的自动重联:0s,2s,5s,10s,30s.

### 控件

- ** 清除 Feed ** — 删除所有显示的信件并重放计数器
- ** Export JSON** — 将所有收到的消息下载为JSON文件进行分析
- ** 信箱** 显示本届会议收到的活动总数

## 信号 R 中枢

仪表板连接到:

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

### 事件类型

仪表盘处理所有值:

类型
|------|---------|
蓝色徽章
绿色徽章
红色警徽
绿色徽章
红色警徽
信息标记
警示徽章

## 技术实施

### 后端

- ** 当地化 枢纽**()——向所有连接客户发送信息的信号R中枢
- ** ISignalRPublisher**——翻译服务中心摘要
- ** SignalRPublisher** - 默认执行,增加单音序列和广播

### 前端

- 纯 HTML/JS 有靴子5型
- 使用 Microsoft SignalR JavaScript 客户端库(从 CDN 装入)
- 事件种子不需要服务器侧渲染

### 页结构

```
Dita.Server/Pages/Admin/
├── LiveTranslation.cshtml      — Razor page markup + JavaScript
└── LiveTranslation.cshtml.cs     — Page model (empty, data comes via SignalR)
```

## 开发过程中的使用

1. 开始Dita。 服务器应用程序
2. 导航到
3. 触发翻译运行( 要么等待调度器, 要么呼叫 API)
4. 实时观察事件
5. 使用导出按钮捕获调试的全部跟踪

## 未来的加强

计划改进仪表板:

- ** 核证**——限制用户使用该功能
- ** 过滤 ** — 按阶段、 类型或运行标识过滤事件
- ** 历史运行** — 从数据库或日志文件中查看完成
- ** 统计数据**——显示翻译数、误差率和长期性的图表
- ** 手动触发器** — 手动启动特定管道阶段的按钮
- ** 构图** — 从仪表板直接编辑
- ** 语言管理** — 查看和编辑所支持的语言
- ** 词典预览**-浏览和搜索本地化词典

## 解决问题

### Dashboard 显示“ 连接失败 ”

1. 验证服务器正在运行和可访问
2. 检查浏览器控制台中的 CORS 或网络出错
3. 确认在
4. 确保没有防火墙屏蔽 WebSocket 连接

### 事件没有出现

1. 请检查signalR中枢 URL 在服务器( ) 和客户端( ) 之间匹配
2. 验证调度器启用于
3. 查看翻译管道错误的服务器日志
4. 检查浏览器 WebSocket 信件的网络标签

### 信件有异常

现场保证单程内订购. 如果信息出现异常,可以表示:
- 多个管道运行重叠(不应因semaphore锁而发生)
- 浏览器渲染问题( 尝试刷新页面)
