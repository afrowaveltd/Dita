# Localization에 있는 Named Placeholders

Dita는 로컬라이제이션 문자열에 있는 **named placeholders**를 지원하며, 언어의 올바른 문법을 보존하면서 런타임에 삽입될 동적 값을 허용한다.

## 옵션 정보

Placeholders는 JSON 사전 값 안쪽에 curly-brace 문법을 사용합니다

```json
{
  "WelcomeMessage": "Hello {userName}, you have {count} new messages",
  "DiskStatus": "Disk {diskName} is {status} with {usagePercent}% used"
}
```

Positional placeholders (, )와는 달리, placeholders는 **language-agnostic ** - 번역자는 부호를 끊기 없이 대상 언어 문법과 일치할 수 있습니다.

## 제품 정보

Named placeholders에는 2개의 값이 있습니다:

### 1. Runtime 값 (동적 데이터에 대한 권장)

Localized 문자열을 retrieving 할 때 패스 값:

```csharp
// In a Razor page or controller
@inject JsonStringLocalizer Localizer

var message = Localizer["WelcomeMessage", new Dictionary<string, string>
{
    ["userName"] = "John",
    ["count"] = "5"
}];
// Result: "Hello John, you have 5 new messages"
```

### 2. 저장된 값 (반전적인 윤곽을 위해)

디렉토리의 파일 관리:

```json
{
  "WelcomeMessage": {
    "userName": "Guest",
    "count": "0"
  }
}
```

**defaults**로 저장된 값은 실행 시간 값으로 overridden 입니다.

## API 참조

### JsonStringLocalizer 지수

```csharp
// Without placeholders (backward compatible)
LocalizedString text = localizer["SomeKey"];

// With positional formatting (backward compatible)
LocalizedString text = localizer["SomeKey", "arg1", "arg2"];

// With named placeholders (new)
LocalizedString text = localizer["SomeKey", new Dictionary<string, string>
{
    ["name"] = "value"
}];
```

### IPlaceholder서비스

```csharp
public interface IPlaceholderService
{
    // Get stored placeholders for a key
    Dictionary<string, string> GetPlaceholders(string key);
    
    // Set a stored placeholder value
    void SetPlaceholder(string key, string placeholderName, string value);
    
    // Remove all stored placeholders for a key
    void RemoveKey(string key);
    
    // Format a template with placeholders
    string Format(string template, Dictionary<string, string>? values = null);
    
    // Extract placeholder names from template
    string[] ExtractPlaceholders(string template);
    
    // Check if template contains placeholders
    bool HasPlaceholders(string template);
    
    // Prepare text for translation (mask placeholders)
    (string preparedText, Func<string, string> restore) PrepareForTranslation(string template);
    
    // Persist/load from disk
    Task SaveAsync();
    Task LoadAsync();
}
```

### 확장 방법

일할 때 편익을 위해:

```csharp
public static class StringLocalizerExtensions
{
    public static LocalizedString WithPlaceholders(
        this IStringLocalizer localizer,
        string name,
        Dictionary<string, string> placeholders);
}
```

사용법:
```csharp
var text = Localizer.WithPlaceholders("WelcomeMessage", new Dictionary<string, string>
{
    ["userName"] = Model.UserName
});
```

## 번역 행동

자동 번역 서비스가 name placeholders와 텍스트를 만날 때:

1. **번역은 **: placeholders are masked with safe token () 번역 엔진을 수정하여.
2. **번역 중 **: 번역 엔진은 번역 가능한 텍스트만 처리합니다.
3. **번역 후 **: Original placeholder name ()은 올바른 위치에 복원됩니다.

### Example

근원 (영어):

번역 준비:

체코어 번역:

최종 결과:

이 보증:
- 주주는 결코 번역하거나 손상되지 않습니다
- Target-language 문법은 주변 텍스트를 자유롭게 배열할 수 있습니다
- 같은 템플릿은 모든 언어로 올바르게 작동합니다

## 좋은 관행

1. ** descriptive name 사용 **: 보다 나거나
2. ** 최소 주주 **: Too many placeholders 만들기 번역 harder
3. **Document 예상 유형 **: JSON 파일에 대한 의견은 번역자를 이해합니다
4. **Prefer 런타임 값**: 진정한 동적 데이터 (사용자 이름, 수, 날짜)의 경우, 실행 시간에 값을 전달합니다
5. **기본값 사용**: 거의 변화하는 구성 (app name, support email)
6. **일부 주주 **: 모든 예상된 주주를 확인하기 위한 사용

## 자동 번역과 통합

자동으로 LibreTranslate 통화 중 placeholder 보존을 처리합니다. 추가 구성이 필요하지 않습니다.

그리고 둘 다 retry 서비스를 사용 하 여, 그래서 모든 JSON 사전 번역 투명 하 게 위주인 이름 지원.

## Backward 호환성

Positional placeholders 또는 placeholders를 사용하는 기존 코드는 계속 변경되지 않습니다

```csharp
// Still works exactly as before
var text = localizer["Hello"];
var formatted = localizer["Value is {0}", 42];
```

Name placeholder API는 첨가제입니다 - 기존의 사용을 방해하지 않습니다.
