# Nommé Placeurs dans la localisation

Dita prend en charge **nommé placeholders** dans les chaînes de localisation, permettant d'insérer des valeurs dynamiques à l'exécution tout en préservant la grammaire correcte dans les langues.

## Syntaxe

Placeholders utilise la syntaxe curly-brace à l'intérieur des valeurs du dictionnaire JSON :

```json
{
  "WelcomeMessage": "Hello {userName}, you have {count} new messages",
  "DiskStatus": "Disk {diskName} is {status} with {usagePercent}% used"
}
```

Contrairement aux détenteurs de place (, ), les détenteurs de place nommés sont **langue-agnostique** — les traducteurs peuvent les réorganiser pour correspondre à la grammaire de langue cible sans casser le code.

## Stockage

Les détenteurs de places désignés ont deux sources de valeurs:

### 1. Valeurs temps d ' exécution (recommandé pour les données dynamiques)

Passez les valeurs directement lors de la récupération de la chaîne localisée :

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

### 2. Valeurs stockées (pour la configuration semi-statique)

La gestion d'un fichier dans le répertoire:

```json
{
  "WelcomeMessage": {
    "userName": "Guest",
    "count": "0"
  }
}
```

Les valeurs stockées agissent comme **defaults** et sont dépassées par les valeurs d'exécution.

## Référence API

### JsonStringLocalizer indexer

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

### Service de localisation

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

### Méthodes d'extension

Pour plus de commodité lorsque vous travaillez avec :

```csharp
public static class StringLocalizerExtensions
{
    public static LocalizedString WithPlaceholders(
        this IStringLocalizer localizer,
        string name,
        Dictionary<string, string> placeholders);
}
```

Utilisation :
```csharp
var text = Localizer.WithPlaceholders("WelcomeMessage", new Dictionary<string, string>
{
    ["userName"] = Model.UserName
});
```

## Comportement de traduction

Lorsque le service de traduction automatique rencontre du texte avec des détenteurs de place nommés:

1. **Avant traduction**: Les porte-places sont masqués avec des jetons sûrs () pour empêcher le moteur de traduction de les modifier.
2. **Au cours de la traduction**: Le moteur de traduction ne traite que le texte traduisable.
3. **Après traduction**: Les noms de lieux originaux () sont restaurés dans leurs positions correctes.

### Exemple

Source (anglais):

Préparé pour traduction:

Traduit en tchèque:

Résultat final:

Cela garantit que:
- Les titulaires ne sont jamais traduits ou corrompus
- La grammaire en langue cible peut réorganiser le texte environnant librement
- Le même modèle fonctionne correctement dans toutes les langues

## Meilleures pratiques

1. **Utiliser des noms descriptifs**: est meilleur que ou
2. ** Garder au minimum les détenteurs de place**: Trop de placeholders rendent la traduction plus difficile
3. **Types de documents attendus**: Les commentaires dans le fichier JSON aident les traducteurs à comprendre le contexte
4. **Préférez les valeurs d'exécution**: Pour les données réellement dynamiques (noms d'utilisateur, nombres, dates), passez les valeurs à l'exécution
5. **Utiliser les valeurs stockées pour les valeurs par défaut** : Pour la configuration qui change rarement (nom de l'application, support de l'email)
6. **Titres de valeurs**: Utilisation pour vérifier tous les détenteurs de place prévus sont fournis

## Intégration avec traduction automatique

La gestion automatique de la conservation des emplacements lors des appels LibreTrail. Aucune configuration supplémentaire n'est nécessaire.

Le et les deux utilisent le service de réessayer, donc toutes les traductions du dictionnaire JSON prennent en charge de manière transparente les détenteurs de place nommés.

## Compatibilité arrière

Le code existant en utilisant des placeurs positionnels ou aucun placeurs continue de fonctionner sans changement:

```csharp
// Still works exactly as before
var text = localizer["Hello"];
var formatted = localizer["Value is {0}", 42];
```

L'API de localisation nommée est additive — elle ne brise pas l'utilisation existante.
