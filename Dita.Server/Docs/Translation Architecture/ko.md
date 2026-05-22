# 번역 건축

이 문서는 Dita의 자동 번역 시스템의 모듈 아키텍처를 설명하고, 유지 보수성, 능력 및 탄력을 개선하기 위해 도입되었습니다.

## 디자인 목표

Refactoring는 본래 monolithic 디자인에 몇몇 관심사를 해결했습니다:

- **문의 분리 **: 각 번역 도메인(countries, JSON dictionaries, Markdown)은 격리되어 있습니다.
- **Incremental 지속 **: 파일은 번역 후 즉시 저장되며 메모리 사용량을 줄이고 이전 결과를 제공합니다.
- **Resilience**: 전체 파이프라인을 차단하지 않고 여러 가지 재량 레벨을 처리하십시오.
- **Observability**: 모든 중요한 가동은 실시간 모니터링을 위한 SignalR을 통해 보고됩니다.
- ** 예외 **: 새로운 번역 대상은 단일 인터페이스를 구현하여 추가 할 수 있습니다.

## 서비스 decomposition

### BackendTranslation서비스(orchestrator)

** 책임 **:
- Pipeline Lifecycle 관리 (시작, 완료, 오류 처리)
- Semaphore 기반 암호화 제어 (prevents overlapping run)
- Server validation (latency, 언어 가용성, 구성)
- 하위 서비스 위임

**Does NOT contain**:
- 번역 논리
- 파일 I/O 특정 형식
- Retry 논리

### 국가TranslationService

** 책임 **:
- 본문 바로가기
- 기본 Locale dictionary에 국가 이름을 동기화
- 대상 언어의 누락 된 국가 이름 번역
- 번역 후 각 타겟 사전을 즉시 저장

** 키 동작 **:
- 기본 언어는 영어로 된 경우 : 국가 이름은 as-is에 저장됩니다
- 기본 언어가 다른 경우: 기본 언어로 번역된 영어 이름
- 각 언어는 독립적으로 자신의 리트리 루프로 처리됩니다

### 현지화Translation서비스

** 책임 **:
- 이전 스냅 샷으로 현재 기본 사전을 비교하여 추가 / 제거 키를 감지
- 각 대상 언어에 추가된 키
- 각 대상 언어에서 삭제 된 키를 제거
- 다음에 대한 snapshot 저장

** 키 동작 **:
- 수동 번역은 항상 우선 순위를 가지고 (모든 overwritten)
- 추가 키는 즉시 번역되고 저장됩니다
- 제거된 키는 즉시 삭제된 per-language입니다
- Snapshot은 모든 언어가 성공적으로 완료 한 후만 저장됩니다

### 문서TranslationService

** 책임 **:
- 설정된 Markdown 루트 recursively
- SHA-256 hashes를 사용하여 변경된 소스 파일을 검색
- Per-block 번역 상태 추적
- Per-block retry와 block-by-block 번역
- 번역 후 Markdown 구조 검증
- 각 대상 언어 파일을 독립적으로 저장

** 키 동작 **:
- Block-level granularity: headings, 단락, 목록 항목은 별도로 번역됩니다
- Metadata는 언어 당 성공 / 실패
- 실패 블록은 성공적인 블록을 다시 번역하지 않고 다음 실행에 의존합니다
- 구조 검증은 계산, 목록, 코드 블록 등을 보장

## Retry 전략

시스템은 세 가지 수준에서 retries를 구현합니다

### 레벨 1 - HTTP (LibreTranslateService)

- 최대 5개의 시도는 exponential 백오프 (1s, 2s, 3s, 4s, 5s)
- 네트워크 타임아웃, 5xx 오류 및 일시 장애 처리
- HTTP 클라이언트 구성에 내장

### 레벨 2 - 단계 (TranslationRetryService)

- 30초 지연으로 최대 3개의 시도
- Re-drives HTTP-level retries 이후 전체 번역 요청이 배출됩니다
- Placeholder masking와 restoration는 이 수준에 적용됩니다

### Level 3 - 블록 (DocumentsTranslationService)

- 개인 Markdown 블록은 metadata에 표시되어 있습니다
- 다음 파이프 라인 실행에서 자동으로 검색
- 성공적인 블록은 결코 다시 번역되지 않습니다

## 데이터 흐름

### JSON 사전 번역

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

### Markdown 번역

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

### 국가 이름 번역

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

## 국가 지속

### 스냅샷

- **JSON **: 기본 사전 옆에 파일에 저장 (이름은 저장 공급자에 따라 다릅니다)
- **Purpose**: 이전 실행에 존재하는 것을 추적하여 incremental sync를 활성화

### Hash 파일

- **Markdown**: 소스 파일 옆에
- **Fallback**: 기본 위치가 읽기 전용이라면
- **Purpose**: 불필요한 재번역을 피하기 위해 소스 변경을 감지

### 번역 메타데이터

- ** Markdown**:
- ** 내용**:
  - 소스 내용 hash
- Per-language 블록 상태 (booleans의 레이)
- 최근 업데이트 타임스탬프
- **Purpose**: 부분적인 재번역만 실패한 블록을 활성화

### 회사소개

- ** 파일**:
- **콘텐츠**: 키의 사전을 위주로 name-value 쌍
- **Purpose**: application을 통하여 name placeholders의 기본값을 제공합니다

## SignalR 보고

### 게시자 요약

signalR specifics의 decouples 번역 서비스:

```csharp
public interface ISignalRPublisher
{
    Task PublishStageAsync<T>(Guid runId, ProcessStage stage, T data, ...);
    Task PublishMessageAsync(Guid runId, LocalizationMessageType type, ProcessStage stage, ...);
}
```

### Sequence 보증

- 단일 실행 내에서 메시지는 단색적으로 순서
- Sequence 숫자는 통해 유일한 per-run입니다
- 클라이언트는 간격 또는 reordering를 검출할 수 있습니다

### 허브 mapping

```csharp
app.MapHub<LocalizationHub>("/hubs/localization");
```

## 연장 점

### 새로운 번역 대상 추가

1. 새로운 인터페이스 만들기
2. 도메인 별 논리와 인터페이스 구현
3. DI 컨테이너에 등록
4. 생성자에 주입
5. 기존 단계에서 호출

### 주문 retry 정책

Override constructor 매개변수:

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

### 회사소개

Placeholder syntax 또는 저장을 바꾸기 위하여 실행하십시오:

```csharp
public class CustomPlaceholderService : IPlaceholderService
{
    // Use {{name}} syntax instead of {name}
    // Store in database instead of JSON file
    // Encrypt sensitive placeholder values
}
```

## 제품 설명

### 다운로드

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

### 실행 시간 튜닝

설치하기
|---------|---------|--------|
80명
10대
3개
30 분

## 시험 전략

### 단위 시험

각 sub-service는 자주적으로 시험할 수 있습니다:

- 성공 / 실패 시뮬레이션
- 보고 확인하기
- 파일 I/ ₢ 킹
- Per-language 저축 행동을 검증

### 통합 시험

- 전체 파이프라인은 실제 (현지) LibreTranslate 인스턴스로 실행됩니다
- Verify SignalR 메시지는 연결된 클라이언트에 전달됩니다
- 시험 동시 뛰기 예방 (semaphore)
- 번역 후 Markdown 구조 검증

### 시험 종료

- API 또는 Scheduler를 통해 더 큰 번역
- 모든 대상 언어 파일을 생성/업데이트
- Metadata 파일 확인은 올바른 블록 상태를 포함합니다
- Placeholders는 번역을 통해 보존됩니다

## 성과 고려사항

- **Memory**: Per-language 저축은 기억에 있는 모든 사전을 붙드는 것을 막습니다
- **Disk I/O**: Metadata 파일은 작은 오버헤드를 추가하지만 증가 작업을 활성화
- **네트워크**: throttling을 사용한 순차 처리는 압도적인 LibreTranslate를 방지합니다
- **CPU**: SHA-256 해싱 및 regex 유효성 검사는 번역 지연시 빠른 상대입니다
- **SignalR**: 경량 메시지, 일반적인 보고서에 필요한 페이로드 압축 없음

## Monolithic 디자인에서 Migration

원래는 모든 논리를 하나의 클래스에 포함. 이동 경로:

1. 추출 국가 논리 →
2. JSON 논리 추출 →
3. 추출물 Markdown 논리 →
4. SignalR 출판 →
5. 추출물 retry 논리 →
6. 관현악을 간단히 합니다

모든 기존 인터페이스 ()는 변경되지 않습니다. 파이프라인의 소비자는 끊는 변화를 볼 수 없습니다.
