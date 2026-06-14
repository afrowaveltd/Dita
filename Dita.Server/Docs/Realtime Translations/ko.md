# 실시간 번역

이 문서는 자동 번역 파이프라인의 실시간 테스트 입력으로 존재합니다. 이 파일에 대한 변경은 다음 예정된 실행의 모든 대상 언어 파일의 재 번역.

## 건축 개요

번역 파이프라인은 경량 관현관에 의해 협조된 4개의 전문화한 sub-services를 가진 모듈 구조로 재건축되었습니다:

- **BackendTranslationService** - 전체 파이프라인을 오케스트라하고 서버 검증을 처리하고 하위 서비스 작업을 위임합니다.
- **CountriesTranslationService** — 언어 사전으로 국가의 이름을 동기화합니다.
- **LocalizationTranslationService** - 기본 JSON 사전의 추가/제거 키를 감지하고 대상 언어로 번역합니다.
- **DocumentsTranslationService** - 탭 블록 추적 및 메타데이터로 Markdown 문서 파일을 번역합니다.

각 서브 서비스는 독립적으로 작동하고 실시간으로 SignalR을 통해 진행 상황을 보고합니다.

## 어떤 서비스는

서비스는 일정에 실행하고 5단계 파이프라인을 실행합니다. 서버 검증, 국가 동기화, JSON 사전 동기화, Markdown 파일 번역 및 결과를 지속합니다. 각 단계는 SignalR에 구조화된 실시간 진행 사건을 방출하므로 연결된 클라이언트는 작업 진행으로 따라 진행될 수 있습니다.

## 파이프 단계

### 1 단계 — CheckServers

어떤 번역 작업이 시작되기 전에, 서비스는 모든 사전 조건이 만족한다는 것을 확인합니다:

- 구성 섹션은 현재와 유효해야합니다.
- LibreTranslate 서버는 수락가능한 대기 시간 안에 응답해야 합니다.
- 번역 서버에서 사용할 수있는 언어의 목록은 fetched.
- 설정된 기본 언어는 해당 목록에 있습니다.
- 지원되는 모든 언어에 대한 locale JSON 파일을 자동으로 생성합니다.

어떤 체크가 실패하면, 파이프라인은 즉시 멈추고 메시지가 방출됩니다.

### 단계 2 — TranslateCountries

국가 이름은 read-only 카탈로그 ()에서 로컬화 JSON 사전으로 동기화됩니다.

- 응용 프로그램 기본 언어가 영어 인 경우, 각 국가 이름은 번역없이 저장됩니다.
- 기본 언어가 다른 언어 인 경우, 영어 국가 이름은 그 언어로 처음 번역되고, 결과는 기본 사전에 항목이됩니다.
- 기본 사전이 업데이트 된 후, 각 대상 언어 사전의 각 누락 된 국가 입국은 번역되고 저장됩니다 ** 언어 당 즉시 **.
- Already-translated 항목은 수정없이 보존됩니다.
- 번역이 실패한 경우, 다음 언어로 이동하기 전에 30 초 지연으로 최대 3 배의 서비스를 제공합니다.

### 단계 3 - 번역JsonFiles

이 서비스는 이전 실행에서 저장된 스냅 샷으로 현재 기본 로컬라이제이션을 비교합니다

- **추가된 키** — 현재 기본 항목에 제시된 항목은 스냅샷에서 부당하지만, 이미 그 키에 대한 수동 항목을 가지고 있지 않는 모든 대상 언어로 번역됩니다.
- **제거 키** — 스냅샷에 제시된 항목은 현재 기본값에서 부당하지만, 모든 대상 언어 사전에서 삭제됩니다.
- 수동 번역은 항상 우선 순위를 가지고. 대상 사전이 이미 열쇠에 대한 값을 포함하면, 해당 항목은 소스가 말하는 것에 관계없이 변경되지 않습니다.
- **각 대상 언어 사전은 번역 완료 후 즉시 저장됩니다 **, 오히려 모든 언어가 완료 될 때까지 기다리는 것보다.
- 번역이 특정 언어에 실패한 경우, 서비스 retries는 자동으로. 단속 오류(예: 지원되지 않은 언어)는 해당 언어가 건너 뛰기 때문입니다.
- 실행 후, 현재 기본 사전은 다음 비교에 대한 새로운 스냅 샷으로 저장됩니다.

모든 사전은 항상 알파벳으로 정렬 된 키와 인간의 읽을 수없는 JSON으로 저장됩니다.

### 단계 4 - TranslateMarkdownFiles

이 서비스는 구성 된 문서 루트 (과태: )를 걸어 각 소스 파일이 반복적으로 처리합니다

1. 소스 파일 내용이 읽고 SHA-256 해시가 계산됩니다.
2. 소스 트랙과 같은 파일 per-language, per-block translation status, enable **incremental re-translation** of only failed block.
3. 이전 실행에서 저장 된 해시 (소스 파일 옆에 파일에서, 또는 임시 낙하 위치에)는 현재 해시와 비교됩니다.
4. 각 대상 언어의 경우, 해당 파일은 구조적 무결성을 검사합니다.
5. 누락 된 모든 대상 파일, outdated hash, 실패 구조 유효성, 또는 다시 번역에 대 한 비공식 블록을 포함.
6. **Each 대상 언어는 독립적으로 번역되고 저장됩니다 ** - 체코가 성공하지만 프랑스어가 실패하면 체코 파일은 여전히 디스크에 기록됩니다.
7. 성공적으로 번역된 파일들은 소스(equal heading counts, list items, code block, blockquotes, links, bold/italic markers, HTML tags)와 구조적 패성을 위해 검증됩니다.
8. 소스의 모든 대상 파일이 성공하면 새로운 해시가 소스 옆에 저장됩니다. 소스 옆에 쓰기가 실패하면 (읽기 전용 배포에서 예를 들어), hash는 임시 디렉토리로 돌아갑니다.
9. 어떤 타겟 번역이 유효하지 않은 경우, 메타 데이터는 그 블록을 번역하지 않고 다음 실행에 대한 재향.

### 5 단계 - StoringResults

통합은 조립 및 출판됩니다. 다음을 포함합니다:

- UTC 실행 시작 및 완료 타임스탬프.
- 저장된 locale JSON 파일의 수, 저장된 Markdown 파일, 저장된 hash 파일 및 fallback hash 쓰기.
- 실행 중에 수집된 모든 저장 오류.
- Per-language 번역 통계 (번역된 수, Skipped count, 오류 수).

## SignalR 메시지 봉투

각 진행 이벤트는 다음과 같은 필드로 전달됩니다

제품정보
|-------|------|-------------|
현재 파이프라인 실행에 대한 상관 식별자
1에서 시작되는 런 내에서 Monotonic 카운터
메시지의 Semantic 유형
Pipeline 단계 메시지는
메시지가 방출되었을 때 UTC 시간
메시지가 오류 상태를 나타냅니다
인간 읽기 쉬운 요약
단계별 페이로드 (report object 또는 null)

### 메시지 유형

주요 특징
|-------|------|---------|
0 댓글
1명
2개
3개
4개
55,000원
6개

### 파이프 단계

주요 특징
|-------|------|-------------|
0 댓글
1명
2개
3개
4개
55,000원

### 일반적인 메시지 흐름

```text
StageStarted  / CheckServers
Progress / CheckServers — Server latency: 42ms
StageCompleted / CheckServers
StageStarted  / TranslateCountries
Progress / TranslateCountries — Found 195 country names
Progress / TranslateCountries — Starting translations for 'cs'...
Progress / TranslateCountries — Saved dictionary for 'cs' (198 entries)
StageCompleted / TranslateCountries
StageStarted  / TranslateJsonFiles
Progress / TranslateJsonFiles — Detected 3 added and 0 removed keys
Progress / TranslateJsonFiles — Starting JSON translations for 'cs'...
Progress / TranslateJsonFiles — Saved dictionary for 'cs' (201 entries)
StageCompleted / TranslateJsonFiles
StageStarted  / TranslateMarkdownFiles
Progress / TranslateMarkdownFiles — Scanning 2 source files in '/Docs'
Progress / TranslateMarkdownFiles — File 'en.md' has 12 translatable blocks
Progress / TranslateMarkdownFiles — Translating 'en.md' to 'cs'...
Progress / TranslateMarkdownFiles — Saved 'cs' translation for 'en.md' (12/12 blocks)
StageCompleted / TranslateMarkdownFiles
StageCompleted / StoringResults
PipelineCompleted / StoringResults
```

어떤 단계가 실패하면 나머지 단계는 건너뛰고, 메시지가 방출되고, 마지막으로 메시지가 실행됩니다.

## 번역 retry 논리

파이프라인은 탄력의 2개의 수준을 실행합니다:

### 단계 수준의 리트리 (TranslationRetryService)

- 번역 요청이 LibreTranslate의 내부 retries 후 실패하면 30 초 지연으로 최대 3 개의 추가 단계 수준의 retries가 수행됩니다.
- Placeholder Masking: Named placeholders () 텍스트는 일시적으로 안전한 토큰으로 대체됩니다 () 번역 및 복원 후, 대상 언어로 정확한 문법을 보장합니다.

### 언어 검증

- 대상 언어로 번역하기 전에, 언어는 번역 서버에 의해 지원됩니다.
- 지원되지 않은 언어는 경고로 건너 뛰고 반복된 실패한 시도를 방지합니다.

### Markdown 블록 레벨 리트리

- Markdown 번역은 Block-by-block (headings, 단락, 목록 항목)을 수행하고 있습니다.
- 개별 블록이 번역을 실패하면 메타 데이터 파일에서 번역되지 않고 다음 파이프 라인 실행에 기여합니다.
- 서비스 트랙 per-language, 각 소스 Markdown 파일 옆에 파일의 per-block 상태.

## 오류 코드

오류는 범위로 분류되지 않은 enum을 사용하여보고됩니다

제품정보
|-------|----------|
1000-1999년
2000-2999년
3000-3999년
4000-4999년
5000-5999년

보고서의 각 오류는 소스 식별자 (언어 코드, 파일 경로, 또는 단계 이름), 오류 코드 및 인간의 읽기 메시지가 나타납니다.

## 라이브 번역 Dashboard

Server 프로젝트는 SignalR 허브에 연결하고 실시간으로 모든 파이프라인 이벤트를 표시합니다.

- 연결 상태, 메시지 카운트 및 모든 이벤트의 라이브 업 테이블을 표시합니다.
- 색상 코드 행 : 단계 시작을위한 파란색, 완료 녹색, 오류에 대한 빨간색.
- JSON에 모든 메시지를 전달하는 피드를 삭제합니다.
- 연결이 떨어지면 exponential backoff를 가진 자동 연결.

## 디자인 원칙

- **Modularity**: 각 번역 우려는 유지보수성 및 시험성에 대한 자체 서비스에 격리되어 있습니다.
- **Incremental 지속 **: Dictionaries 및 Markdown 파일은 번역 후 즉시 저장되며 메모리 압력을 줄이고 이전 피드백을 제공합니다.
- ** 탄력**: 다중 구호 수준 (HTTP, 단계, 블록)은 일시적인 실패가 파이프라인을 막지 않습니다.
- **State tracking**: Per-file metadata () 및 hash 파일은 연속 실행에 정확한 incremental 작업을 가능하게 합니다.
- ** 실시간 가시성**: 모든 중요한 가동은 감시와 벌레잡기를 위한 SignalR를 통해 보고됩니다.
- ** 수동 번역은 항상 자동 추가에 우선 순위가 있습니다. **
