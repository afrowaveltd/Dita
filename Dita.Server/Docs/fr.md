# Résumé des modifications apportées au Service de traduction automatique

## Aperçu général

Ce document résume toutes les modifications apportées au service de traduction automatique de Dita, y compris la refacturation de l'architecture, les nouvelles fonctionnalités, les améliorations de l'observabilité et les améliorations de localisation.

## Changements d'architecture

### service de traduction en backend refactoré

Le monolithique a été décomposé en quatre services spécialisés coordonnés par un orchestre léger :

- **BackendTranslationService** — Orchestra pipeline (validation du serveur, délégation de scène, gestion des erreurs)
- **PaysTranslationService** — Synchronisation des noms de pays (anglais → langue cible)
- **LocalisationService de traduction** — Synchronisation du dictionnaire JSON (clés ajoutées/supprimées)
- **DocumentsTranslationService** — Traduire la documentation avec suivi au niveau des blocs
- **SignalRPublisher** — Rapport d'avancement en temps réel via SignalR
- **TranslationRetryService** — Réessayer au niveau de l'étape avec la préservation du détenteur de place

### Avantages

- ** Séparation des préoccupations** : Chaque service gère un seul domaine de traduction
- ** Maintenabilité** : Les classes plus petites sont plus faciles à comprendre et à tester
- **Extension**: De nouveaux objectifs de traduction peuvent être ajoutés via l'implémentation de l'interface
- **Fiabilité**: Les services indépendants offrent une meilleure isolation des défauts

## Nouvelles fonctionnalités

### Moniteur de traduction en direct

**Emplacement**:

Une nouvelle page d'administration qui fournit une visibilité en temps réel dans le pipeline de traduction:

- Affiche tous les signaux Les événements de R au fur et à mesure qu'ils se produisent
- Types de messages codés en couleur (bleu=démarré, vert=complété, rouge=error)
- Bannière d'état de connexion avec connexion automatique
- Compteur de messages et exportation vers JSON

### Titulaires désignés

Le système de localisation prend désormais en charge les placeholders nommés () pour améliorer la grammaticalité dans différentes langues:

```csharp
// Usage in code
var message = Localizer["WelcomeMessage", new Dictionary<string, string>
{
    ["userName"] = "John",
    ["count"] = "5"
}];
// Result: "Hello John, you have 5 new messages"
```

Caractéristiques:
- Valeurs de placement fournies à l'exécution ou stockées dans
- Masquage/restauration automatique pendant la traduction pour prévenir la corruption
- Compatibilité avec les positions existantes

### Traduction différentielle

Les fichiers Markdown sont traduits progressivement :

- **Enregistrement par langue**: Chaque langue cible est enregistrée immédiatement après la traduction, réduisant la pression de mémoire
- **Suivi au niveau du bloc**: état de la traduction par bloc
- **Répétition sélective**: Seuls les blocs échoués sont retranscrits à la prochaine manche
- **Constante des métadonnées**: L'état de traduction survit aux redémarrages d'applications

### Logique de réessayer améliorée

Trois niveaux de résilience :

1. **HTTP retry** (Liberservice): 5 tentatives avec un retour exponentiel (1s–5s)
2. **Répétition de l'étape** (TranslationRetryService): 3 tentatives supplémentaires avec des retards de 30s
3. **Block retry** (Service de traduction des documents): Blocs de Markdown échoués réajustés au prochain tirage

### SignalR Signalisation

Rapports d'étape en temps réel pour toutes les opérations pipelinières :

- Chaque étape publie des événements
- Progrès par langue publiés comme événements
- Les événements d'erreur incluent le contexte détaillé (source, code d'erreur, message)
- Les numéros de séquence garantissent la commande dans chaque course

## Modifications de configuration

### appsettings.json

Pas de changement. La configuration actuelle continue de fonctionner:

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

### Nouveaux services

Enregistré dans :

- /
- `TranslationRetryService`
- /
- /
- /
- /

Le signal R hub est cartographié pour les connexions client.

## Essais

### État de l'essai

- **243/244 essais réussis** (1 échappé en raison de l'accès simultané au fichier dans l'environnement d'essai)
- Nouvelle couverture de test ajoutée pour:
  - Titulaire Fonctionnalité du service
  - BackendTraduction Orchestration de service
  - JsonStringLocalizer placeholder indexers

### Limitations connues

- test est ignoré lors de l'exécution en parallèle parce que plusieurs instances de test partagent le même fichier. Il passe en courant isolé.

## Nouvelle structure de fichier

### Services en

- — orchestre pipelinier
- — Traduction des noms de pays
- — Synchronisation du dictionnaire JSON
- — Traduction par marquage
- — Signal Publication de messages R
- — Réessayez la logique avec le masquage de l'emplacement
- — Interface éditeur
- — Interface de service par pays
- — Interface de service de localisation
- — Interface de service de documents
- — Interface Orchestrator (mise à jour)
- — métadonnées de traduction par fichier

### Services mis à jour en

- — Ajout d'un support nominatif
- — Mise à jour pour le nouveau paramètre
- — Gestion nominative des lieux
- — Interface de localisation

### Nouvelle page d'administration dans

- — Page de suivi en temps réel
- — Modèle de page

### Nouvelle documentation

- — Mise à jour de la documentation relative aux pipelines
- — Guide du système de localisation
- — Guide d'utilisation du tableau de bord
- — Aperçu de l'architecture technique

## Compatibilité en aval

Tous les changements sont additifs:

- Le code de localisation existant () fonctionne inchangé
- Formatage positionnel () fonctionne sans changement
- Le format actuel du dictionnaire JSON est inchangé
- La structure actuelle du marquage est inchangée
- Signal Les messages R utilisent le même format

## Voie migratoire

Aucune migration requise. La refacturation est interne:

1. Old a été conservé comme référence puis remplacé
2. Les enregistrements des DI ont été mis à jour pour utiliser de nouvelles interfaces
3. Tous les consommateurs actuels ne voient aucun changement

## Amélioration des performances

- **Utilisation réduite de la mémoire**: Fichiers enregistrés par langue immédiatement au lieu de garder tout en mémoire
- **Création progressive Essais**: Seuls les blocs de balisage modifiés ou échoués sont retranscrits
- ** Meilleure visibilité**: Le progrès en temps réel aide à diagnostiquer les étapes lentes

## Améliorations futures

Améliorations prévues :

1. **AI finissage** — Revue de traduction post-machine pour les phrases > 5 mots
2. **Authentification administrative** — Restreindre les pages administratives aux utilisateurs autorisés
3. ** Éditeur de dictionnaire** — UI Web pour la gestion des clés de localisation
4. **Statistiques sur la traduction** — Graphiques montrant les nombres de traductions et les taux d'erreur dans le temps
5. ** Syntaxe personnalisée du détenteur de place** — Prise en charge des formats de remplacement du détenteur de place

## Personne à contacter

Pour des questions ou des problèmes avec le service de traduction, veuillez consulter la documentation détaillée dans le répertoire de chaque module ou contacter l'équipe de développement.
