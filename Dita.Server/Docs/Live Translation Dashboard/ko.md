# 라이브 번역 Dashboard

Live Translation Dashboard는 자동 번역 파이프라인에 실시간 가시성을 제공하는 관리자 페이지입니다. SignalR 허브에 연결하고 모든 파이프라인 이벤트를 표시합니다.

## 사이트맵

`/Admin/LiveTranslation`

> Note: Authentication and authorization are not yet implemented. Future versions will restrict this page to admin users only.

## 제품 정보

### 실시간 이벤트 스트림

번역 파이프라인의 모든 SignalR 이벤트는 생방송 테이블에 표시됩니다

- **Sequence number** - 각 파이프라인 내의 Monotonic 카운터
- **Timestamp** — 이벤트가 수신될 때 현지 시간
- **Run ID** - 상관관계를 위한 단축 GUID
- **Stage** - Pipeline 단계 배지 (CheckServers, TranslateCountries 등)
- **Type** - 메시지 유형 배지 (StageStarted, Progress, StageCompleted 등)
- **Message** - 인간 읽기 쉬운 설명
- **Details** - 이벤트 데이터의 전체 JSON 페이로드

### 색깔 기호화

색깔: 회색
|-------|---------|
블루 ()
녹색 ()
레드 ()
백색 (과태)

### 연결 상태

상단의 상태 배너 :
- **Connecting** — SignalR 연결 설정
- ** 연결** — 이벤트를 일반적으로 수신
- ** 연결 ** - 연결 손실, 연결 시도
- **Disconnected** - 연결 닫기

연결은 exponential backoff를 가진 자동적인 재연결을 이용합니다: 0s, 2s, 5s, 10s, 30s.

### 제품정보

- ** CLear Feed** - 모든 표시된 메시지를 제거하고 카운터를 재설정
- **Export JSON ** - 분석을위한 JSON 파일로 모든 수신 된 메시지를 다운로드
- **Message 카운터 ** -이 세션에서받은 총 이벤트 수를 표시합니다

## SignalR 허브

대시보드는 다음과 같습니다:

```javascript
const connection = new signalr.HubConnectionBuilder()
    .withUrl("/hubs/localization")
    .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
    .build();
```

### 메시지 계약

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

### 이벤트 유형

대시보드는 모든 값을 처리합니다:

제품정보
|------|---------|
블루 배지
녹색 배지
레드 배지
녹색 배지
레드 배지
정보 배지
공지사항

## 기술 구현

### 기타

- **LocalizationHub** () - 모든 연결된 클라이언트에 메시지를 방송하는 SignalR 허브
- **ISignalRPublisher** — 번역 서비스에 대한 허브에 대한 애정
- **SignalRPublisher** - 모노토닉 시퀀스와 방송을 증가하는 기본 구현

### 회사연혁

- Pure HTML/JS 와 부트 스트랩 5 스타일링
- Microsoft SignalR JavaScript 클라이언트 라이브러리를 사용하여 (CDN에서로드 됨)
- 이벤트 피드에 필요한 서버 측 렌더링 없음

### 페이지 구조

```
Dita.Server/Pages/Admin/
├── LiveTranslation.cshtml      — Razor page markup + JavaScript
└── LiveTranslation.cshtml.cs     — Page model (empty, data comes via SignalR)
```

## 사용법 개발 도중

1. Dita를 시작합니다. Server 응용
2. 바로가기
3. 번역 실행을 방아쇠 (스케일러를 기다리거나 API를 호출)
4. 이벤트가 실시간으로 표시됩니다
5. 벌레잡기를 위한 가득 차있는 추적을 붙잡기 위하여 수출 단추를 사용하십시오

## 미래 향상

대시보드의 계획된 개선:

- ** 인증** - 역할과 사용자에 대한 제한
- **Filtering** - 단계별 필터 이벤트, 유형, 또는 ID를 실행
- **Historical run** - 데이터베이스 또는 로그 파일에서 완료된 실행
- **Statistics** - 번역 카운트, 오류율 및 대기 시간을 보여주는 차트
- **Manual triggers** — Buttons to manually start specific pipeline stages
- **Configuration** - 대시보드에서 직접 편집
- **Language Management** — 지원된 언어 보기 및 편집
- **Dictionary 미리보기 ** - 검색 및 현지화 사전 검색

## 문제 해결

### 대시보드는 "연결에 실패"

1. 서버가 실행되고 접근 가능
2. CORS 또는 네트워크 오류에 대한 브라우저 콘솔 확인
3. 자주 묻는 질문
4. 방화벽이 WebSocket 연결을 차단하지 않습니다

### 이벤트는 나타나지 않습니다

1. SignalR 허브 URL이 서버()과 클라이언트() 사이 일치한다는 것을 확인합니다
2. 스케줄러를 검증합니다
3. 서버 로그에서 번역 파이프라인 오류
4. WebSocket 메시지에 대한 브라우저 네트워크 탭 확인

### 메시지는 주문 중입니다

필드는 단일 실행 내에서 주문을 보장합니다. 메시지가 순서로 나타낸 경우, 그것은 나타냅니다:
- 다중 파이프라인은 overlapping를 달립니다 (semaphore 자물쇠 때문에 일어나지 마십시오)
- 브라우저 렌더링 문제 (페이지 상쾌)
