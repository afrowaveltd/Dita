# 数据模型

命名空间定义了整个本地化和翻译系统所使用的所有数据结构——从API请求/响应对到管道报告和仪表板快照.

## 模式概览

### 配置

#### 自动翻译安排

配置模式从 . Controls Libre Translate服务器连接和管道行为.

财产
|---|---|---|---|
自由翻译服务器 URL
是否需要 API 密钥
API 密钥
应用程序默认语言
不得翻译的语文
文档根目录
启用已排定的管道运行
第一次运行前的延迟
运行间隔分钟
自由翻译文本终点
自由翻译文件端点
自由翻译语言终点
自由翻译检测终点
翻译请求之间的延迟
每个请求 HTTP 超时
是否装入配置

### 自由翻译 API 模型

#### 翻译请求 · 翻译结果

** 请求**——文本译名API呼叫:

财产
|---|---|---|---|
| `Query` | `string` | `"q"` | `""` |
—— 说
—— 说
—— 说
| `ApiKey` | `string?` | `"api_key"` | `null` |
—— 说

** 成果**——翻译答复:

财产
|---|---|
| `TranslatedText` | `string` |
| `DetectedLanguage` | `Detections` |
| `Alternatives` | `List<string>` |

#### 检测请求 – 检测

** 要求**:** 答复**:
**Response**: `{ Language, Confidence }`

#### 翻译文件请求 翻译文件

** 要求**:** 答复**:
**Response**: `{ TranslatedFileUrl }`

#### 自由语言

结束点的单一语言条目 :

```json
{ "code": "en", "name": "English", "targets": ["cs", "de", "fr", ...] }
```

### 管道报告模型

#### 检查报告

服务器验证阶段的结果 :

财产
|---|---|
| `AppsettingsLoaded` | `bool` |
| `TranslationServerReady` | `bool` |
| `DefaultLanguage` | `string` |
| `AvailableLanguages` | `string[]` |
| `ServerLatencyMs` | `int` |

#### 翻译 报告

词典/国家翻译阶段的结果:

财产
|---|---|
| `DefaultDictionaryExists` | `bool` |
| `DefaultDictionaryCount` | `int` |
| `ToTranslateCount` | `int` |
| `AddedCount` | `int` |
| `RemovedCount` | `int` |
| `SkippedCount` | `int` |
| `TranslatedCount` | `int` |
| `ErrorsCount` | `int` |
| `Errors` | `List<TranslationError>?` |

#### 马克下调翻译报告

Markdown翻译阶段的结果:

财产
|---|---|
| `SourceFilesDetected` | `int` |
| `SourceFilesChanged` | `int` |
| `SavedFiles` | `int` |
| `SkippedFiles` | `int` |
| `TempFallbackWrites` | `int` |
| `Errors` | `List<TranslationError>` |

#### 存储报告

持续产出的最后汇总:

财产
|---|---|
| `RunStartedUtc` | `DateTime` |
| `RunCompletedUtc` | `DateTime?` |
| `SavedDictionaryFiles` | `int` |
| `SavedMarkdownFiles` | `int` |
| `SavedHashFiles` | `int` |
| `TempFallbackWrites` | `int` |
| `Errors` | `List<TranslationError>` |

#### 阶段报告<T>

将任何报告类型与阶段元数据包裹在一起的通用容器 :

财产
|---|---|
| `ReportedStage` | `ProcessStage` |
| `StageData` | `T?` |
| `StageStartTime` | `DateTime?` |
| `StageEndTime` | `DateTime?` |
(计算)

### 翻译工作模式

#### 词组队列

翻译队列的工作项目 :

财产
|---|---|
| `Target` | `TranslationTarget` |
| `Key` | `string?` |
| `Phrase` | `string` |
| `SourceLanguage` | `string?` |
| `TargetLanguage` | `string` |
| `ChangeRequired` | `PhraseChange` |
| `AddedToList` | `DateTime` |
| `TranslationStart` | `DateTime?` |
| `TranslationEnds` | `DateTime?` |
| `IsTranslated` | `bool` |
| `TranslatedText` | `string?` |

#### 翻译错误

所有报告中的结构性错误记录:

财产
|---|---|
(语言代码、文件路径或艺名)
| `Code` | `ErrorCode` |
| `ErrorMessage` | `string` |

#### 单一翻译

单一语言词典 :

财产
|---|---|
| `Language` | `string` |
| `Translations` | `Dictionary<string, string>` |

#### 标记倒数倒数

从 Markdown 文档中取出块 :

财产
|---|---|
| `Key` | `Guid` |
| `OriginalText` | `string` |
| `TranslatedText` | `string?` |
| `StartLine` | `int` |
| `EndLine` | `int` |
| `BlockType` | `string` |
| `Metadata` | `Dictionary<string, object>` |
| `IsTranslated` | `bool` |

### 文本分辨率模型

#### 文本本地化 请求 + 文本本地化 回应

** 请求**——以字典为基础的本地化(可写出):

财产
|---|---|
| `Text` | `string` |
| `TargetLanguage` | `string?` |
| `Values` | `Dictionary<string, string>?` |

** 答复**:

财产
|---|---|
(原始内容)
(当地化)
| `TargetLanguage` | `string` |
| `DefaultLanguage` | `string` |
| `FoundInTargetDictionary` | `bool` |
| `AddedToDefaultDictionary` | `bool` |
| `ResolvedFrom` | `TextResolutionSource` |

#### 文本翻译请求 – 文本翻译回复

** 请求**——动态翻译(只读):

财产
|---|---|
| `Text` | `string` |
| `SourceLanguage` | `string?` |
| `TargetLanguage` | `string?` |
| `Values` | `Dictionary<string, string>?` |

** 答复**:

财产
|---|---|
(原始内容)
(译出)
| `SourceLanguage` | `string` |
| `TargetLanguage` | `string` |
| `FoundInTargetDictionary` | `bool` |
| `TranslationServerUsed` | `bool` |
| `ResolvedFrom` | `TextResolutionSource` |

#### 文本决议来源

确定本地化/翻译值的解决地点:

数值
|---|---|
在目标语言的地语词典中找到
在默认语言词典中找到
未找到; 添加到默认词典
由自由翻译器返回
未解决即返回

### 共享类型

#### 定义

只读条目 :

财产
|---|---|---|
| `Name` | `string` | `"name"` |
| `DialCode` | `string` | `"dial_code"` |
| `Emoji` | `string` | `"emoji"` |
| `Code` | `string` | `"code"` |

#### 比较条件

评价的过滤条件 :

财产
|---|---|
| `Compare` | `Comparison` |
| `Values` | `int[]` |
| `IsOr` | `bool` |

#### 答错

简单的 API 错误信封 :

财产
|---|---|
| `Error` | `string?` |
