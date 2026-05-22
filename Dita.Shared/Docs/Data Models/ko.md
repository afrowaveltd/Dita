# 데이터 모델

Namespace는 로컬라이제이션 및 번역 시스템에 사용되는 모든 데이터 구조를 정의합니다. API 요청/response 쌍부터 파이프라인 보고서 및 대시보드 스냅샷까지.

## 모델 개요

### 제품 설명

#### 자동번역설정

윤곽 모형 경계에서. LibreTranslate 서버 연결 및 파이프라인 동작을 제어합니다.

제품정보
|---|---|---|---|
LibreTranslate 서버 URL
API 키가 필요한지
API 키
앱 기본 언어
번역을 제외한 언어
문서 루트 디렉토리
계획된 파이프라인 실행
첫 번째 실행 전에 지연
실행 중
LibreTranslate 텍스트 엔드 포인트
LibreTranslate 파일 엔드포인트
LibreTranslate 언어 엔드포인트
LibreTranslate 탐지 endpoint
번역 요청 사이 지연
요청 당 HTTP 타임 아웃
Config가 로드되었는지

### LibreTranslate API 모델

#### TranslateRequest - 번역

**Request** - 텍스트 번역 API 통화:

제품정보
|---|---|---|---|
| `Query` | `string` | `"q"` | `""` |
—
—
—
| `ApiKey` | `string?` | `"api_key"` | `null` |
—

** 결과** — 번역 응답:

제품정보
|---|---|
| `TranslatedText` | `string` |
| `DetectedLanguage` | `Detections` |
| `Alternatives` | `List<string>` |

#### DetectRequest → 탐지

**Request**: `{ Query, ApiKey }`  
**Response**: `{ Language, Confidence }`

#### 번역FileRequest → 번역FileResult

**Request**: `{ File (IFormFile), Source, Target, Api_key }`  
**Response**: `{ TranslatedFileUrl }`

#### 언어 선택

Endpoint의 단일 언어 항목 :

```json
{ "code": "en", "name": "English", "targets": ["cs", "de", "fr", ...] }
```

### Pipeline 보고서 모델

#### 자주 묻는 질문

서버 검증 단계의 결과:

제품정보
|---|---|
| `AppsettingsLoaded` | `bool` |
| `TranslationServerReady` | `bool` |
| `DefaultLanguage` | `string` |
| `AvailableLanguages` | `string[]` |
| `ServerLatencyMs` | `int` |

#### 번역Report

Dictionary/country 번역 단계의 결과:

제품정보
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

#### Markdown운송

Markdown 번역 단계의 결과:

제품정보
|---|---|
| `SourceFilesDetected` | `int` |
| `SourceFilesChanged` | `int` |
| `SavedFiles` | `int` |
| `SkippedFiles` | `int` |
| `TempFallbackWrites` | `int` |
| `Errors` | `List<TranslationError>` |

#### Storing리포트

Persisted 산출의 마지막 집계:

제품정보
|---|---|
| `RunStartedUtc` | `DateTime` |
| `RunCompletedUtc` | `DateTime?` |
| `SavedDictionaryFiles` | `int` |
| `SavedMarkdownFiles` | `int` |
| `SavedHashFiles` | `int` |
| `TempFallbackWrites` | `int` |
| `Errors` | `List<TranslationError>` |

#### 스테이지리포트<T>

단계 metadata를 가진 어떤 보고 유형을 감싸는 일반적인 콘테이너:

제품정보
|---|---|
| `ReportedStage` | `ProcessStage` |
| `StageData` | `T?` |
| `StageStartTime` | `DateTime?` |
| `StageEndTime` | `DateTime?` |
(입력)

### 번역 작업 모델

#### 언어 선택

번역 큐에 대한 작업 항목:

제품정보
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

#### 다운로드

모든 보고서에서 수행 된 구조 오류 기록 :

제품정보
|---|---|
(언어 코드, 파일 경로, 또는 단계 이름)
| `Code` | `ErrorCode` |
| `ErrorMessage` | `string` |

#### 단일 전송

단일 locale 사전:

제품정보
|---|---|
| `Language` | `string` |
| `Translations` | `Dictionary<string, string>` |

#### MarkdownTranslatable블록

Markdown 문서에서 추출된 블록:

제품정보
|---|---|
| `Key` | `Guid` |
| `OriginalText` | `string` |
| `TranslatedText` | `string?` |
| `StartLine` | `int` |
| `EndLine` | `int` |
| `BlockType` | `string` |
| `Metadata` | `Dictionary<string, object>` |
| `IsTranslated` | `bool` |

### 텍스트 해상도 모델

#### 본문 바로가기 요청 → TextLocalization 관련 기사

**Request** - 사전 기반 로컬라이제이션(writable):

제품정보
|---|---|
| `Text` | `string` |
| `TargetLanguage` | `string?` |
| `Values` | `Dictionary<string, string>?` |

** 책임 **:

제품정보
|---|---|
(원래)
(현지화)
| `TargetLanguage` | `string` |
| `DefaultLanguage` | `string` |
| `FoundInTargetDictionary` | `bool` |
| `AddedToDefaultDictionary` | `bool` |
| `ResolvedFrom` | `TextResolutionSource` |

#### TextTranslationRequest → 텍스트TranslationResponse

**Request** - 동적 번역 (읽기 전용):

제품정보
|---|---|
| `Text` | `string` |
| `SourceLanguage` | `string?` |
| `TargetLanguage` | `string?` |
| `Values` | `Dictionary<string, string>?` |

** 책임 **:

제품정보
|---|---|
(원래)
(번역)
| `SourceLanguage` | `string` |
| `TargetLanguage` | `string` |
| `FoundInTargetDictionary` | `bool` |
| `TranslationServerUsed` | `bool` |
| `ResolvedFrom` | `TextResolutionSource` |

#### 텍스트ResolutionSource

로컬/번역된 값이 해결된 곳을 식별합니다:

주요 특징
|---|---|
대상 언어에 대한 Locale dictionary에 발견
기본 언어 사전에서 발견
찾을 수 없음; 기본 사전에 추가
LibreTranslate에 의해 반환
해결책 없는 반환된 as-is

### 공유 유형

#### 국가 정의

에서 읽기 전용 항목 :

제품정보
|---|---|---|
| `Name` | `string` | `"name"` |
| `DialCode` | `string` | `"dial_code"` |
| `Emoji` | `string` | `"emoji"` |
| `Code` | `string` | `"code"` |

#### 비교Condition

평가를 위한 필터 조건:

제품정보
|---|---|
| `Compare` | `Comparison` |
| `Values` | `int[]` |
| `IsOr` | `bool` |

#### 오류 응답

간단한 API 오류 봉투 :

제품정보
|---|---|
| `Error` | `string?` |
