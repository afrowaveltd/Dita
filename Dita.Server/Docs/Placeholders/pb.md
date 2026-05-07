# Nomeados como placeholders na localização

Dita suporta ** nomes de placeholders** em cordas de localização, permitindo que valores dinâmicos sejam inseridos em tempo de execução, preservando a gramática correta entre as línguas.

## Sintaxe

Os placeholders usam a sintaxe curly-brace dentro dos valores do dicionário JSON:

```json
{
  "WelcomeMessage": "Hello {userName}, you have {count} new messages",
  "DiskStatus": "Disk {diskName} is {status} with {usagePercent}% used"
}
```

Ao contrário de placeholders posicionais (, ), nomeados placeholders são ** linguagem-agnóstico** - tradutores podem reordená-los para combinar gramatical língua-alvo sem quebrar o código.

## Armazenagem

Nomeados placeholders têm duas fontes de valores:

### 1. Valores de tempo de execução (recomendado para dados dinâmicos)

Passe os valores diretamente ao recuperar a corda localizada:

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

### 2. Valores armazenados (para configuração semi-estática)

O gerencia um arquivo no diretório:

```json
{
  "WelcomeMessage": {
    "userName": "Guest",
    "count": "0"
  }
}
```

Valores armazenados agem como **padrões** e são substituídos por valores de tempo de execução.

## Referência da API

### JsonStringIndicedor de localização

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

### Serviço IPlaceholder

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

### Métodos de extensão

Por conveniência ao trabalhar com:

```csharp
public static class StringLocalizerExtensions
{
    public static LocalizedString WithPlaceholders(
        this IStringLocalizer localizer,
        string name,
        Dictionary<string, string> placeholders);
}
```

Uso:
```csharp
var text = Localizer.WithPlaceholders("WelcomeMessage", new Dictionary<string, string>
{
    ["userName"] = Model.UserName
});
```

## Comportamento de tradução

Quando o serviço de tradução automática encontra texto com placeholders nomeados:

1. **Before translation**: Placeholders are masked with safe tokens (`___PH_0___`) to prevent the translation engine from modifying them.
2. **During translation**: The translation engine processes only the translatable text.
3. **After translation**: Original placeholder names (`{name}`) are restored in their correct positions.

### Exemplo

Fonte:

Preparado para tradução:

Traduzido para tcheco:

Resultado final:

Isso garante que:
- Lugares nunca são traduzidos ou corrompidos
- A gramática da língua-alvo pode reorganizar o texto ao redor livremente
- O mesmo modelo funciona corretamente em todas as línguas

## Boas práticas

1. **Use nomes descritivos**: é melhor que ou
2. **Keep placeholders minimal**: Too many placeholders make translation harder
3. **Document expected types**: Comments in the JSON file help translators understand context
4. **Preferir valores de execução**: Para dados verdadeiramente dinâmicos (nomes de usuários, contagens, datas), passe valores em tempo de execução
5. **Use valores armazenados para padrões**: Para configuração que raramente muda (nome do aplicativo, e-mail de suporte)
6. **Validate placeholders**: Use para verificar todos os locais esperados

## Integração com tradução automática

O automaticamente lida com preservação de placeholder durante chamadas LibreTranslate. Nenhuma configuração adicional é necessária.

Os dois usam o serviço de retentação, então todas as traduções do dicionário JSON suportam os placeholders.

## Compatibilidade para trás

Código existente usando placeholders posicionais ou nenhum placeholders continua a trabalhar inalterado:

```csharp
// Still works exactly as before
var text = localizer["Hello"];
var formatted = localizer["Value is {0}", 42];
```

A API chamada placeholder é aditiva, não quebra o uso existente.
