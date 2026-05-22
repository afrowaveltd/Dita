# 翻译结构

本文介绍了为增强可维护性,可测试性和韧性而引入的Dita自动翻译系统的模块化架构.

## 设计目标

重新构思解决了最初的单体设计中的几个问题:

- ** 消除关切**: 每个翻译域(countries,JSON词典,Markdown)都是孤立的.
- ** 递增持久性**: 文件在翻译后立即被每个语言保存,减少了内存使用并提供了更早的结果.
- ** 反应**:多重试级处理瞬态故障而不妨碍整个管道.
- ** 可观测性**:每次重大操作均通过信号R报告,用于实时监测.
- ** 延期**: 可以通过实施单一接口来增加新的翻译目标.

## 服务分解

### 后端 翻译服务( orchestrator)

** 责任**:
- 管道生命周期管理(启动、完成、错误处理)
- 基于Semaphore的货币控制(防止重叠运行)
- 服务器验证( 相关性、 语言可用性、 配置)
- 分处授权

** 不包含**:
- 翻译逻辑
- 特定格式的文件 I/O
- 重试逻辑

### 翻译服务

** 责任**:
- 从目录读取
- 将国家名称同步到默认的语言词典
- 按目标语言翻译缺失的国家名称
- 翻译后立即保存每个目标词典

**关键行为**:
- 如果默认语言是英语: country names survey as-is
- 如果默认语言是其它语言: 英语名称先被翻译为默认语言
- 每种语言都用自己的重试循环独立处理

### 本地化 翻译服务

** 责任**:
- 通过比较当前默认字典和先前的快照来检测添加/删除的密钥
- 将添加的密钥翻译到每种目标语言
- 从每个目标语言中删除已删除的密钥
- 保存快照供下次比较

**关键行为**:
- 手工翻译总是优先(从未覆盖)
- 立即翻译并保存每个语言的添加密钥
- 删除的密钥立即被逐个语言删除
- 在所有语言成功完成后才保存快照

### 文件翻译服务

** 责任**:
- 逆向行走已配置的 Markdown 根
- 使用 SHA-256 散列探测已更改的源文件
- 每个块的翻译状态
- 逐块翻译,每块重试
- 翻译后验证马克下架结构
- 独立保存每个目标语言文件

**关键行为**:
- 块级颗粒性:标题,段落,列表项目单独翻译
- 每个语言块成功/失败的元数据音轨
- 失败的块会在下次运行时重审, 而不重新翻译成功的块
- 结构验证确保标题计数、列表、代码块等匹配源

## 重试策略

该系统在三个层面进行重试:

### 1级 — HTTP( Libre Translate Services)

- 最多5次指数回落的尝试(1s, 2s, 3s, 4s, 5s)
- 处理网络超时、 5xx 错误和瞬态故障
- 建入 HTTP 客户端配置

### 第二级——阶段(翻译服务)

- 最多3次尝试,拖延30秒
- 在 HTTP 级别重试结束后重新驱动整个翻译请求
- 占位符遮盖和修复适用于这一级别

### 第三级-块(文件翻译服务)

- 失败的单个Markdown块被标记为元数据
- 在下一次输油管运行时自动重试
- 成功块永远不会被重新翻译

## 数据流动

### JSON 词典翻译

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

### 马克下调翻译

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

### 国家名称翻译

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

## 国家的持久性

### 抓图

- **JSON**: Stored in a file next to the default dictionary (name varies by storage provider)
- ** 目的**:通过跟踪前一次运行中的内容,启用递增同步

### 散列文件

- ** Markdown **: 源文件旁边
- ** Fallback**:如果主位置只读
- ** 目的**:检测源变化以避免不必要的再翻译

### 翻译元数据

- ** 马可达**:
- ** 内容**:
  - 来源内容散列h
- 每语区块状态(布尔斯阵列)
- 上次更新时间戳
- ** 目的**:只允许部分重译失败的块

### 占位符存储

- ** 文件**:
- ** 术语**:占位符名称值对的密钥词典
- ** 目标**:在整个应用程序中为指定占位符提供默认值

## 信号报告

### 出版者抽象

从 SignalR 细节中去除翻译服务:

```csharp
public interface ISignalRPublisher
{
    Task PublishStageAsync<T>(Guid runId, ProcessStage stage, T data, ...);
    Task PublishMessageAsync(Guid runId, LocalizationMessageType type, ProcessStage stage, ...);
}
```

### 序列保障

- 单个运行中的信件是单调序列
- 每个运行的序列数是独一无二的
- 客户端可以发现漏洞或重新排序

### 枢纽绘图

```csharp
app.MapHub<LocalizationHub>("/hubs/localization");
```

## 延长点

### 添加一个新的翻译目标

1. 创建新接口
2. 执行与特定域逻辑的接口
3. 在 DI 容器中注册
4. 注入构造器
5. 现有阶段之后的呼叫

### 自定义重试策略

覆盖构造参数 :

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

### 自定义占位符处理

用于更改占位符语法或存储 :

```csharp
public class CustomPlaceholderService : IPlaceholderService
{
    // Use {{name}} syntax instead of {name}
    // Store in database instead of JSON file
    // Encrypt sensitive placeholder values
}
```

## 配置

### 应用程序.json

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

### 运行时间调试

设置
|---------|---------|--------|
80个
10个
3个
30个

## 测试战略

### 单位测试

每个次级服务可独立测试:

- 模拟成功/失败
- 装模作样核查报告
- 文件 I/ 使用临时目录 欧
- 验证每个语言的保存行为

### 融合测试

- 用真实的(当地)自由翻译实例进行全管运行
- 校验信号R消息发送给连接客户端
- 试验同时运行的预防(血肿)
- 翻译后验证马克下架结构

### 端到端测试

- 通过 API 或调度器进行触发翻译
- 校验所有目标语言文件的创建/更新
- 检查元数据文件包含正确的块状态
- 在翻译中保存确认的占位符

## 业绩考虑

- ** 记忆**: 语言保存防止将所有字典保存在记忆中
- ** Disk I/O**:元数据文件添加了小的管理费用,但允许增量工作
- ** 网络**: 用节奏进行顺序处理 防止压倒性的自由翻译
- ** CPU**:SHA-256散列和正则验证与翻译延迟相比速度快
- ** SignalR**:轻量级信息,典型报告不需要有效载荷压缩

## 单体设计的迁移

原作在一个类中包含所有逻辑. 迁移路径 :

1. 抽取国家逻辑
2. 摘录 JSON 逻辑 ~
3. 提取 Markdown 逻辑 —
4. 摘录信号R 发布
5. 提取重试逻辑 ~
6. 将管弦乐器简化为仅限代表团使用

所有现有接口()保持不变。 输油管的消费者没有出现突破性变化.
