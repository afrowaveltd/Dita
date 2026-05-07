# Lugares Nomeados na Localização

Dita suporta ** nomes de placeholders** em strings de localização, permitindo que valores dinâmicos sejam inseridos em tempo de execução, preservando a gramática correta entre os idiomas.

## Sintaxe

Os placeholders usam a sintaxe curly-brace dentro dos valores do dicionário JSON:

```json
{
  "WelcomeMessage": "Hello {userName}, you have {count} new messages",
  "DiskStatus": "Disk {diskName} is {status} with {usagePercent}% used"
}
```

Ao contrário dos placeholders posicionais (, ), os placeholders nomeados são ** language-agnóstico** — tradutores podem reordená-los para combinar gramática de língua-alvo sem quebrar o código.

## Armazenamento

Os placeholders nomeados têm duas fontes de valores:

### 1. Valores de tempo de execução (recomendado para dados dinâmicos)

Passe os valores diretamente ao recuperar a string localizada:

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

Valores armazenados atuam como **defaults** e são substituídos por valores de tempo de execução.

## Referência da API

### Indexador JsonStringLocalizer

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

### IPlaceholderService

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

Para conveniência ao trabalhar com :

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

1. ** Antes da tradução**: Os placeholders são mascarados com fichas seguras () para evitar que o motor de tradução os modifique.
2. **Durante a tradução**: O motor de tradução processa apenas o texto translatável.
3. ** Após a tradução**: Nomes de placeholder originais () são restaurados em suas posições corretas.

### Exemplo

Fonte (inglês):

Preparado para tradução:

Traduzido para checo:

Resultado final:

Isto garante que:
- Lugares nunca são traduzidos ou corrompidos
- Gramática de língua-alvo pode reorganizar o texto circundante livremente
- O mesmo modelo funciona corretamente em todos os idiomas

## Boas práticas

1. **Use nomes descritivos**: é melhor que ou
2. ** Mantenha os espaços mínimos**: Muitos placeholders tornam a tradução mais difícil
3. **Documento tipos esperados**: Comentários no arquivo JSON ajudam tradutores a entender o contexto
4. **Preferir valores de tempo de execução**: Para dados verdadeiramente dinâmicos (nomes de usuário, contagens, datas), passe valores em tempo de execução
5. **Use valores armazenados para padrões**: Para configuração que raramente muda (nome do aplicativo, e-mail de suporte)
6. ** Validação de espaços**: Utilização para verificar todos os espaços esperados

## Integração com tradução automática

O automaticamente lida com a preservação do placeholder durante as chamadas LibreTranslate. Nenhuma configuração adicional é necessária.

O e ambos usam o serviço de repetição, então todas as traduções do dicionário JSON suportam de forma transparente placeholders.

## Compatibilidade para trás

Código existente que utiliza substitutos posicionais ou nenhum placeholders continua a funcionar inalterado:

```csharp
// Still works exactly as before
var text = localizer["Hello"];
var formatted = localizer["Value is {0}", 42];
```

A API chamada placeholder é aditiva — não quebra o uso existente.
