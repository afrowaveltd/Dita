# 자동 번역 서비스 변경

## 제품정보

이 문서는 건축 재공장, 새로운 기능, 관측성 개선 및 현지화 향상을 포함한 Dita 자동 번역 서비스로 모든 변경 사항을 요약합니다.

## 건축 변화

### Refactored BackendTranslation서비스

Monolithic는 가벼운 관현관에 의해 협조된 4개의 전문화한 서비스로 decomposed되었습니다:

- **BackendTranslationService** - Pipeline Orchestrator (서버 검증, 단계 위임, 오류 처리)
- **CountriesTranslationService** - 국가 이름 동기화 (영어 → 대상 언어)
- **LocalizationTranslationService** - JSON 사전 동기화 (added/removed 키)
- **DocumentsTranslationService** - Block-level 추적으로 Markdown 문서 번역
- **SignalRPublisher** - SignalR을 통해 실시간 진행 보고
- **TranslationRetryService** - 주주 보전을 통한 단계 수준 리트리

### Benefits

- **문의 분리 **: 각 서비스는 단일 번역 도메인을 취급합니다
- **Maintainability**: 더 작은 클래스는 쉽게 이해 및 테스트
- ** 예외 **: 새로운 번역 대상은 인터페이스 구현을 통해 추가 될 수 있습니다
- **Reliability**: 독립적인 서비스는 더 나은 결함 고립을 제공합니다

## 새로운 기능

### 실시간 번역 모니터

**위치 **:

실시간 가시성을 제공하는 새로운 관리자 페이지 번역 파이프라인:

- 모든 SignalR 이벤트를 표시합니다
- 색상 코드 메시지 유형 (blue=started, green=completed, red=error)
- 자동 연결 상태 배너
- JSON에 메시지 카운터 및 내보내기

### 회사명

로컬라이제이션 시스템은 이제 다른 언어로 향상된 문법성을 위해 placeholders ()라는 이름을 지원합니다

```csharp
// Usage in code
var message = Localizer["WelcomeMessage", new Dictionary<string, string>
{
    ["userName"] = "John",
    ["count"] = "5"
}];
// Result: "Hello John, you have 5 new messages"
```

특징:
- Runtime 또는 저장에 제공된 Placeholder 값
- 자동적인 masking/restoration 번역 도중 corruption를 방지하기 위하여
- 기존의 Positional placeholder와 호환되는 백워드

### 회사연혁

Markdown 파일은 incrementally 번역됩니다

- **Per-language 저축 **: 각 대상 언어는 번역 후 즉시 저장되며, 메모리 압력 감소
- **Block-level tracking**: 블록당 번역 상태를 추적
- **선택적인 리트리**: 실패한 블록은 다음 실행에 다시 번역됩니다
- **Metadata persistence **: 번역 상태는 응용 프로그램을 다시 시작

### 향상된 Retry Logic

탄력의 3개 수준:

1. **HTTP 리트리** (LibreTranslateService): 5개의 시도로 exponential backoff (1s–5s)
2. **Stage retry** (TranslationRetryService): 30s 지연으로 3개의 추가 시도
3. **Block retry** (DocumentsTranslationService): Markdown 블록이 다음 실행에 기여했습니다

### SignalR 보고

모든 파이프라인 가동을 위한 순간 진행 보고:

- 모든 단계 게시 이벤트
- Per-language 진행은 이벤트로 발표
- 오류 이벤트는 상세한 컨텍스트 (source, error code, message)를 포함합니다
- 각 뛰기 내의 주문 보장의 순서 수

## 구성 변경

### 다운로드

끊는 변화 없음. 기존 구성은 계속 작동:

```json
{
  "AutomaticTranslationSettings": {
    "DefaultLanguage": "en",
    "IgnoredLanguages": ["auto", "detect"],
    "MarkdownRoots": ["/Docs"],
    "AutomaticRun": true,
    "CheckingPeriod": 30,
    "WaitingTime": "00:00:00"
  }
}
```

### 새로운 서비스

등록 :

- / 한국어
- `TranslationRetryService`
- / 한국어
- / 한국어
- / 한국어
- / 한국어

SignalR 허브는 클라이언트 연결을 위해 맵핑됩니다.

## 제품정보

### 시험 상태

- **243/244 테스트 통과 ** (1 테스트 환경에서 동시 파일 액세스로 건너 뛰기)
- 추가되는 새로운 시험 적용:
  - PlaceholderService 기능
  - BackendTranslation서비스 오케스트라
  - JsonStringLocalizer placeholder 지수

### Known 제한

- 여러 테스트 인스턴스가 동일한 파일을 공유하기 때문에 병렬에서 실행할 때 테스트가 건너 뛸 수 있습니다. 그것은 고립에서 실행할 때 통과합니다.

## 새 파일 구조

### 회사 소개

- - 파이프 오케스트라
- — 국가 이름 번역
- — JSON 사전 동기화
- — Markdown 번역
- - SignalR 메시지 게시
- — placeholder masking를 가진 Retry 논리
- — 출판사 인터페이스
- - 국가 서비스 인터페이스
- — Localization 서비스 인터페이스
- - 문서 서비스 인터페이스
- — Orchestrator 인터페이스 (업데이트)
- — Per-file 번역 메타데이터

### 업데이트 된 서비스

- — placeholder 지원 추가
- — 새로운 매개 변수에 대한 업데이트
- — Named placeholder 관리
- - Placeholder 인터페이스

### 새로운 관리자 페이지

- - 실시간 모니터링 페이지
- - 페이지 모델

### 새 문서

- — 업데이트된 파이프라인 문서
- - Placeholder 시스템 가이드
- — Dashboard 사용 가이드
- — 기술적인 건축 개요

## Backward 호환성

모든 변화는 첨가물입니다:

- 기존 로컬라이제이션 코드()는 변경되지 않습니다
- Positional formatting ()는 변경되지 않습니다
- Existing JSON 사전 형식은 변경되지 않습니다
- Existing Markdown 구조는 변하지 않습니다
- SignalR 메시지는 동일한 형식을 사용합니다

## 교통수단

관련 기사 refactoring는 내부입니다:

1. 이전은 참고로 보존 된 다음 대체
2. DI 등록은 새로운 인터페이스를 사용하도록 업데이트되었습니다
3. 모든 기존 소비자는 변경하지 않습니다

## 성능 향상

- ** 메모리 사용**: 메모리에서 모든 것을 유지 대신 한 언어에 저장된 파일
- **꽃 증가 **: 변경된 / 실패 Markdown 블록은 다시 번역
- ** 더 나은 가시성 **: 실시간 진행은 느린 단계 진단

## 미래 향상

계획된 개선:

1. **AI Fine-tuning** - 구문에 대한 포스트 기계 번역 검토 > 5 단어
2. **Admin 인증** - 공인된 사용자에게 관리 페이지 제한
3. **Dictionary Editor** - 로컬라이제이션 키 관리를위한 웹 UI
4. **Translation 통계 ** - 번역 수와 오류율을 보여주는 차트
5. **Custom placeholder syntax** — 교체주자 형식 지원

## 제품정보

번역 서비스에 대한 질문이나 문제, 각 모듈의 디렉토리에 대한 상세한 문서를 참조하거나 개발 팀에 문의하십시오.
