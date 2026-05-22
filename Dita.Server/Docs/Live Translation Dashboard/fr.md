# Tableau de bord de la traduction en direct

Le tableau de bord de la traduction en direct est une page d'administration qui fournit une visibilité en temps réel dans le pipeline de traduction automatique. Il se connecte au hub SignalR et affiche tous les événements du pipeline au fur et à mesure qu'ils se produisent.

## URL

`/Admin/LiveTranslation`

> Note: Authentication and authorization are not yet implemented. Future versions will restrict this page to admin users only.

## Caractéristiques

### Flux d'événements en temps réel

Tous les événements SignalR du pipeline de traduction sont affichés dans une table de mise à jour en direct:

- ** Numéro de séquence** — Compteur monotonique dans chaque conduite
- **Timestamp** — Heure locale où l'événement a été reçu
- **ID de course** — GUID raccourci pour corrélation
- **Étage** — Badge de la scène pipeline (CheckServers, TranslateCountries, etc.)
- **Type** — Badge type message (StageStarted, Progress, StageComplet, etc.)
- **Message** — Description lisible par l'homme
- **Détails** — Charge utile complète JSON des données de l'événement

### Codage des couleurs

Couleur
|-------|---------|
Bleu ()
Vert ()
Rouge ()
Blanc (par défaut)

### État de la connexion

Une bannière d'état en haut montre :
- **Connectation** — Établissement de la connexion signalR
- **Connecté** — Réception des événements normalement
- **Reconnecting** — Connexion perdue, essayant de se reconnecter
- **Déconnecté** — Fermeture de la connexion

La connexion utilise un reconnect automatique avec backoff exponentiel: 0s, 2s, 5s, 10s, 30s.

### Contrôles

- **Feed clair** — Supprime tous les messages affichés et réinitialise le compteur
- **Export JSON** — Téléchargements tous les messages reçus en tant que fichier JSON pour analyse
- ** Compteur de messages** — Indique le nombre total d'événements reçus au cours de cette session

## Moyeu signalR

Le tableau de bord se connecte à :

```javascript
const connection = new signalr.HubConnectionBuilder()
    .withUrl("/hubs/localization")
    .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
    .build();
```

### Contrat de message

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

### Types d'événements

Le tableau de bord gère toutes les valeurs :

Type
|------|---------|
Insigne bleu
Insigne vert
Insigne rouge
Insigne vert
Insigne rouge
Insigne d'information
Insigne d'avertissement

## Mise en œuvre technique

### Moteur

- **LocalisationHub** () — Hub SignalR qui diffuse des messages à tous les clients connectés
- **ISignalRPublisher** — Abstraction sur le hub pour utilisation dans les services de traduction
- **SignalRPublisher** — Implémentation par défaut qui incrémente une séquence monotonique et des émissions

### Frontière

- Pure HTML/JS avec style Bootstrap 5
- Utilise la bibliothèque client Microsoft SignalR JavaScript (chargée depuis CDN)
- Aucun rendu côté serveur requis pour le flux d'événement

### Structure des pages

```
Dita.Server/Pages/Admin/
├── LiveTranslation.cshtml      — Razor page markup + JavaScript
└── LiveTranslation.cshtml.cs     — Page model (empty, data comes via SignalR)
```

## Utilisation pendant le développement

1. Démarre la Dita. Application serveur
2. Naviguez vers
3. Déclencher une opération de traduction (ou attendre le programmeur ou appeler l'API)
4. Regarder les événements apparaître en temps réel
5. Utilisez le bouton Exporter pour capturer une trace complète pour le débogage

## Améliorations futures

Améliorations prévues pour le tableau de bord :

- **Authentification** — Restreindre l'accès aux utilisateurs ayant le rôle
- **Filtering** — Filtrer les événements par étape, par type ou par ID d'exécution
- **Exécutions historiques** — Affichage effectué à partir d'une base de données ou d'un fichier journal
- **Statistiques** — Graphiques montrant les nombres de traductions, les taux d'erreur et la latence dans le temps
- **Déclencheurs manuels** — Boutons pour démarrer manuellement des étapes spécifiques du pipeline
- **Configuration** — Modifier directement depuis le tableau de bord
- **Gestion des langues** — Afficher et modifier les langues prises en charge
- **Aperçu des dictionnaires** — Parcourir et rechercher les dictionnaires de localisation

## Dépannage

### Le tableau de bord montre « Impossible de se connecter »

1. Vérifier que le serveur fonctionne et est accessible
2. Vérifiez la console du navigateur pour les erreurs CORS ou réseau
3. Confirmer est présent dans
4. S'assurer qu'aucun pare-feu ne bloque les connexions WebSocket

### Les événements ne sont pas apparus

1. Vérifiez que l'URL du hub SignalR correspond entre le serveur () et le client ()
2. Vérifier que le programmeur est activé dans
3. Regardez les journaux de serveurs pour les erreurs de pipeline de traduction
4. Vérifiez l'onglet réseau du navigateur pour les messages WebSocket

### Les messages sont hors service

Le champ garantit la commande en une seule fois. Si les messages apparaissent hors ordre, ils peuvent indiquer :
- Multiples écoulements de pipeline se chevauchant (ne devrait pas se produire en raison de l'écluse du sémaphore)
- Problèmes de rendu du navigateur (essayer de rafraîchir la page)
