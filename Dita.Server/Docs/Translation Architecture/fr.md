# Architecture de traduction

Ce document décrit l'architecture modulaire du système de traduction automatique de Dita, introduit pour améliorer la maintenance, la testabilité et la résilience.

## Objectifs de conception

Le remaniement a permis de répondre à plusieurs préoccupations concernant la conception monolithique originale :

- ** Séparation des préoccupations** : Chaque domaine de traduction (pays, dictionnaires JSON, Markdown) est isolé.
- **Constante incrémentale**: Les fichiers sont enregistrés par langue immédiatement après la traduction, réduisant l'utilisation de la mémoire et fournissant des résultats antérieurs.
- **Résilience**: Plusieurs niveaux de ré-essai traitent les défaillances transitoires sans bloquer l'ensemble du pipeline.
- **Observabilité**: Chaque opération importante est signalée via SignalR pour la surveillance en temps réel.
- **Extension**: De nouveaux objectifs de traduction peuvent être ajoutés en mettant en place une interface unique.

## Décomposition des services

### BackendTranslationService (Orchestre)

** Responsabilités** :
- Gestion du cycle de vie des pipelines (démarrage, achèvement, traitement des erreurs)
- Contrôle de la concurrence basé sur le sémaphore (prévenir les chevauchements)
- Validation du serveur (latence, disponibilité linguistique, configuration)
- Délégation aux sous-services

**Ne contient PAS**:
- Logique de traduction
- Fichier E/S pour des formats spécifiques
- Réessayer la logique

### PaysTraductionService

** Responsabilités** :
- Lire depuis le répertoire
- Synchroniser les noms de pays dans le dictionnaire local par défaut
- Traduire les noms de pays manquants par langue cible
- Enregistrer chaque dictionnaire cible immédiatement après la traduction

**Comportements clés**:
- Si la langue par défaut est l'anglais: noms de pays stockés as-is
- Si la langue par défaut est autre : les noms anglais traduits dans la langue par défaut d'abord
- Chaque langue est traitée indépendamment avec sa propre boucle de réessayer

### LocalisationService de traduction

** Responsabilités** :
- Détecter les clés ajoutées/supprimées en comparant le dictionnaire par défaut actuel avec l'instantané précédent
- Traduire les clés ajoutées dans chaque langue cible
- Supprimer les touches supprimées de chaque langue cible
- Enregistrer un instantané pour la prochaine comparaison

**Comportements clés**:
- Les traductions manuelles sont toujours prioritaires (jamais écrasées)
- Les clés ajoutées sont traduites et enregistrées par langue immédiatement
- Les clés supprimées sont supprimées par langue immédiatement
- Snapshot n'est enregistré qu'une fois toutes les langues terminées avec succès

### DocumentsService de traduction

** Responsabilités** :
- Promenez les racines de Markdown configurées de façon récursive
- Détecter les fichiers sources modifiés en utilisant les hachages SHA-256
- Situation de la traduction par bloc dans
- Traduire bloc par bloc avec réessayer par bloc
- Valider la structure Markdown après la traduction
- Enregistrer chaque fichier de langue cible indépendamment

**Comportements clés**:
- Granularité au niveau du bloc: les en-têtes, les paragraphes, les éléments de liste sont traduits séparément
- Les traces de métadonnées qui bloquent ont réussi/ont échoué par langue
- Les blocs échoués sont retriés sur la prochaine course sans re-transcrire les blocs réussis
- La validation de la structure assure le comptage des caps, des listes, des blocs de code, etc

## Stratégie de réessayer

Le système met en œuvre des relevés à trois niveaux:

### Niveau 1 — HTTP (libreservice)

- Jusqu'à 5 tentatives avec un recul exponentiel (1s, 2s, 3s, 4s, 5s)
- Gérez les temps d'arrêt du réseau, les erreurs 5xx et les défaillances transitoires
- Construit dans la configuration du client HTTP

### Niveau 2 — Étape (Service de la traduction)

- Jusqu'à 3 tentatives avec 30 secondes de retard
- Redirige l'ensemble de la requête de traduction après épuisement des requêtes HTTP
- Le masquage et la restauration du porte-place sont appliqués à ce niveau

### Niveau 3 — Bloc (Service de traduction des documents)

- Les blocs de marquage individuels qui échouent sont marqués dans les métadonnées
- Récupéré automatiquement sur le prochain pipeline
- Les blocs réussis ne sont jamais retranscrits

## Débit de données

### Traduction du dictionnaire JSON

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

### Traduction par marquage

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

### Traduction des noms de pays

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

## Résistance des États

### Coups de vue

- **JSON**: Stocké dans un fichier à côté du dictionnaire par défaut (le nom varie selon le fournisseur de stockage)
- **Purpose**: Active la synchronisation progressive en suivant ce qui était présent dans l'exécution précédente

### Fichiers Hash

- **Marquage**: à côté du fichier source
- **Fallback**: si l'emplacement principal est en lecture seule
- **Objet**: Détecte les changements de source pour éviter une retraduction inutile

### Métadonnées de traduction

- **Marquage**:
- **Contenu**:
  - Contenu de la source
- Statut de bloc par langue (pour les booléens)
- Dernière mise à jour horodatage
- **Objet**: Permet une retraduction partielle de blocs seulement échoués

### Stockage des emplacements

- **Dossier**:
- **Contenu**: Dictionnaire des clés pour les paires de noms de domaine
- **But**: Fournit des valeurs par défaut pour les détenteurs de place nommés dans l'application

## SignalR

### Abstraction de l'éditeur

découple les services de traduction des spécificités SignalR:

```csharp
public interface ISignalRPublisher
{
    Task PublishStageAsync<T>(Guid runId, ProcessStage stage, T data, ...);
    Task PublishMessageAsync(Guid runId, LocalizationMessageType type, ProcessStage stage, ...);
}
```

### Garanties de séquence

- Les messages en une seule fois sont séquencés monotoniquement
- Les numéros de séquence sont uniques par run via
- Les clients peuvent détecter des lacunes ou réorganiser

### Cartographie en hub

```csharp
app.MapHub<LocalizationHub>("/hubs/localization");
```

## Points d'extension

### Ajout d'une nouvelle cible de traduction

1. Créer une nouvelle interface avec
2. Implémenter l'interface avec la logique propre au domaine
3. Inscription dans le conteneur DI
4. Injecter dans le constructeur
5. Appel après les étapes existantes

### Politique de rétry sur mesure

Paramètres du constructeur de dépassement:

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

### Gestion personnalisée du détenteur de place

Mettre en œuvre pour modifier la syntaxe ou le stockage du détenteur de place:

```csharp
public class CustomPlaceholderService : IPlaceholderService
{
    // Use {{name}} syntax instead of {name}
    // Store in database instead of JSON file
    // Encrypt sensitive placeholder values
}
```

## Configuration

### appsettings.json

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

### Réglage des temps d'exécution

Réglage
|---------|---------|--------|
80
10
3
30

## Stratégie d'essai

### Essais unitaires

Chaque sous-service est testable de manière indépendante:

- Mock pour simuler succès/échec
- Mock pour vérifier les rapports
- Utiliser des répertoires temporaires pour le fichier I/ O
- Vérifier le comportement de sauvegarde par langue

### Essais d'intégration

- Plein pipeline avec réel (local) Libre instance
- Vérifier que les messages SignalR sont livrés aux clients connectés
- Essai de prévention simultanée (sémaphore)
- Valider la structure Markdown après la traduction

### Essais de bout en bout

- Trigger traduction via API ou planificateur
- Vérifier que tous les fichiers de langue cible sont créés/mise à jour
- Vérifier que les fichiers de métadonnées contiennent l'état correct du bloc
- Confirmer que les détenteurs de place sont conservés dans les traductions

## Considérations relatives aux résultats

- **Mémorie**: L'enregistrement par langue empêche de garder tous les dictionnaires en mémoire
- **Disk I/O**: Les fichiers de métadonnées ajoutent de petits frais généraux mais permettent des travaux supplémentaires
- **Réseau**: Le traitement séquentiel avec le throttling empêche l'écrasante Libre
- **CPU**: Le hachage SHA-256 et la validation du régex sont rapides par rapport à la latence de traduction
- **SignalR**: Messages légers, aucune compression de charge utile nécessaire pour les rapports types

## Migration de la conception monolithique

L'original contenait toute la logique dans une classe. La voie migratoire:

1. Extraire la logique du pays →
2. Extraire la logique JSON →
3. Extraire la logique de balisage →
4. Extraire la publication SignalR →
5. Extraire la logique de réessayer →
6. Simplifiez l'orchestreur à la délégation seulement

Toutes les interfaces existantes () restent inchangées. Les consommateurs du pipeline ne voient aucun changement de rupture.
