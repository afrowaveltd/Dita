# Traductions en temps réel

Ce document existe comme entrée de test en direct pour le pipeline de traduction automatique. Toute modification de ce fichier déclenche la retraduction de tous les fichiers de langue cible lors de la prochaine exécution programmée.

## Aperçu de l'architecture

Le pipeline de traduction a été restructuré en une architecture modulaire avec quatre sous-services spécialisés coordonnés par un orchestre léger:

- **BackendTranslationService** — Organise l'ensemble du pipeline, gère la validation du serveur et les délégués travaillent aux sous-services.
- **PaysTranslationService** — Synchronise les noms de pays des dictionnaires par langue.
- **LocalisationTranslationService** — Détecte les clés ajoutées ou supprimées dans le dictionnaire JSON par défaut et les traduit dans les langues cibles.
- **DocumentsTranslationService** — Traduit les fichiers de documentation Markdown avec le suivi par bloc et les métadonnées.

Chaque sous-service fonctionne de manière indépendante et rend compte de ses progrès via SignalR en temps réel.

## Ce que fait le service

Le service fonctionne selon un calendrier et exécute un pipeline en cinq étapes: validation du serveur, synchronisation du pays, synchronisation du dictionnaire JSON, traduction du fichier Markdown et persistance des résultats. Chaque étape émet des événements structurés de progression en temps réel sur Signal R afin que les clients connectés puissent suivre le déroulement du travail.

## Étapes du pipeline

### Étape 1 — Serviteurs de contrôle

Avant tout travail de traduction, le service vérifie que toutes les conditions préalables sont remplies :

- La section de configuration doit être présente et valide.
- Le serveur LibreTrail doit répondre dans une latence acceptable.
- La liste des langues disponibles sur le serveur de traduction est récupérée.
- La langue par défaut configurée doit être présente dans cette liste.
- Les fichiers JSON locaux manquants pour n'importe quelle langue prise en charge sont créés automatiquement.

Si une vérification échoue, le pipeline s'arrête immédiatement et un message est émis.

### Étape 2 — Pays de traduction

Les noms de pays sont synchronisés depuis un catalogue en lecture seule () vers les dictionnaires JSON de localisation.

- Si la langue par défaut de l'application est l'anglais, chaque nom de pays est stocké comme sans traduction.
- Si la langue par défaut est n'importe quelle autre langue, le nom du pays anglais est d'abord traduit dans cette langue, et le résultat devient l'entrée dans le dictionnaire par défaut.
- Après la mise à jour du dictionnaire par défaut, chaque entrée manquante dans chaque dictionnaire de langue cible est traduite et enregistrée **immédiatement par langue**.
- Les entrées déjà traduites sont conservées sans modification.
- En cas d'échec d'une traduction, le service repart jusqu'à 3 fois avec des retards de 30 secondes avant de passer à la langue suivante.

### Étape 3 — TraduireJsonFiles

Le service compare le dictionnaire de localisation par défaut actuel avec un instantané stocké depuis l'exécution précédente :

- **Les clés ajoutées** — entrées présentes dans la valeur par défaut actuelle mais absentes de l'instantané — sont traduites dans chaque langue cible qui n'a pas déjà d'entrée manuelle pour cette clé.
- **Les touches supprimées** — entrées présentes dans l'instantané mais absentes de la valeur par défaut actuelle — sont supprimées de chaque dictionnaire de langue cible.
- Les traductions manuelles sont toujours prioritaires. Si un dictionnaire cible contient déjà une valeur pour une clé, cette entrée reste inchangée, peu importe ce que dit la source.
- **Chaque dictionnaire de langue cible est sauvegardé immédiatement après sa traduction complète**, plutôt que d'attendre la fin de toutes les langues.
- Si une traduction échoue pour une langue spécifique, le service est automatiquement récupéré. Seules les erreurs persistantes (p. ex., un langage non pris en charge) font que ce langage est ignoré.
- Après l'exécution, le dictionnaire par défaut actuel est enregistré comme nouveau snapshot pour la prochaine comparaison.

Tous les dictionnaires sont toujours stockés avec des clés classées par ordre alphabétique et JSON pour la lisibilité humaine.

### Étape 4 — Traduire les fichiers Markdown

Le service guide les racines de documentation configurées (par défaut : ) et traite chaque fichier source de façon récursive :

1. Le contenu du fichier source est lu et un hachage SHA-256 est calculé.
2. Un fichier à côté des pistes source par langue, statut de traduction par bloc, permettant **rétraduction progressive** de blocs échoués.
3. Le hash stocké de l'exécution précédente (conservé dans un fichier à côté du fichier source, ou dans un emplacement de repli temporaire) est comparé au hash actuel.
4. Pour chaque langue cible, le fichier correspondant est également vérifié pour l'intégrité structurelle.
5. Tout fichier cible manquant, ayant un hachage dépassé, ne valide pas la structure, ou contenant des blocs non traduits est en attente pour une nouvelle traduction.
6. **Chaque langue cible est traduite et sauvegardée indépendamment** — si le tchèque réussit mais que le français échoue, le fichier tchèque est toujours écrit sur disque.
7. Les fichiers traduits avec succès sont validés pour la parité structurelle avec la source (nombres de titres égaux, éléments de liste, blocs de code, blockquotes, liens, marqueurs gras/italiques et balises HTML) avant qu'ils soient écrits sur le disque.
8. Si tous les fichiers cibles d'une source réussissent, le nouveau hachage est stocké à côté de la source. Si l'écriture à côté de la source échoue (par exemple en lecture seule), le hash revient dans le répertoire temporaire.
9. Si une traduction cible échoue à la validation, les métadonnées marquent ces blocs comme non traduits de sorte qu'ils sont retriés à la prochaine exécution.

### Étape 5 — Stocker les résultats

Une synthèse est assemblée et publiée. Il comprend:

- Temps de démarrage et d'achèvement des essais UTC.
- Compte des fichiers JSON locaux enregistrés, des fichiers Markdown enregistrés, des fichiers hash sauvegardés, et des écritures hash de retour.
- Toute erreur de stockage collectée pendant l'exécution.
- Statistiques de traduction par langue (nombre traduit, nombre omis, nombre d'erreurs).

## Signal enveloppe de message R

Chaque événement de progrès est livré dans les domaines suivants :

Champ
|-------|------|-------------|
Identificateur de corrélation pour le fonctionnement actuel du pipeline
Compteur monotonique dans une course, à partir de 1
Type sémantique du message
Étape pipeline le message appartient à
Heure UTC où le message a été émis
Indique si le message représente une condition d'erreur
Résumé lisible par l'homme
Charge utile spécifique à l'étape (objet de rapport ou null)

### Types de messages

Valeur
|-------|------|---------|
0
1
2
3
4
5
6

### Étapes du pipeline

Valeur
|-------|------|-------------|
0
1
2
3
4
5

### Flux typique de messages

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

Si une étape échoue, les étapes restantes sont ignorées, un message est émis, et enfin un message ferme la course.

## Logique de réessayer la traduction

Le pipeline met en œuvre deux niveaux de résilience:

### Recours à l'étape (Service de la traduction)

- Si une demande de traduction échoue après les relevés internes de LibreTrail, la réalisation jusqu'à 3 relevés supplémentaires de niveau étape avec 30 secondes de retard.
- Masquage du porte-place: Les détenteurs de place nominés () dans le texte sont temporairement remplacés par des jetons sûrs () avant la traduction et restaurés après, assurant une grammaire correcte dans les langues cibles.

### Validation linguistique

- Avant de traduire dans une langue cible, le service vérifie que la langue est prise en charge par le serveur de traduction.
- Les langues non soutenues sont ignorées par un avertissement, empêchant les tentatives répétées ratées.

### Repérer le niveau du bloc

- Les traductions Markdown sont effectuées bloc par bloc (en-têtes, paragraphes, éléments de liste).
- Si un bloc individuel échoue à la traduction, il est marqué comme non traduit dans le fichier de métadonnées et réédité sur le prochain pipeline.
- Le service suit l'état par langue, par bloc dans les fichiers à côté de chaque fichier Markdown source.

## Codes d'erreur

Les erreurs sont signalées à l'aide d'un enum unifié groupé en plages :

Portée
|-------|----------|
1000–1999
2000–2999
3000–3999
4000–4999
5000–5999

Chaque erreur d'un rapport porte l'identifiant source (code de langue, chemin de fichier ou nom d'étape), le code d'erreur et un message lisible par l'homme.

## Tableau de bord de la traduction en direct

Le projet Server inclut une page d'administration à laquelle se connecte le hub SignalR et affiche tous les événements de pipeline en temps réel.

- Affiche l'état de la connexion, le nombre de messages et une table de mise à jour en direct de tous les événements.
- Lignes codées en couleurs: bleu pour le démarrage de la scène, vert pour l'achèvement, rouge pour les erreurs.
- Prend en charge la compensation du flux et l'exportation de tous les messages vers JSON.
- Auto-reconnecte avec un retour exponentiel si la connexion tombe.

## Principes de conception

- **Modularité**: Chaque souci de traduction est isolé dans son propre service pour la maintenance et la testabilité.
- **Constante incrémentale**: Les dictionnaires et les fichiers Markdown sont enregistrés par langue immédiatement après la traduction, ce qui réduit la pression de la mémoire et fournit des commentaires plus tôt.
- **Resilience**: Plusieurs niveaux de ré-essai (HTTP, étape, bloc) garantissent que les défaillances transitoires ne bloquent pas le pipeline.
- ** Suivi de l'État** : Les métadonnées par fichier () et les fichiers de hachage permettent un travail progressif précis sur les séries subséquentes.
- ** Visibilité en temps réel**: Chaque opération importante est signalée via SignalR pour la surveillance et le débogage.
- **Les traductions manuelles ont toujours priorité sur les ajouts automatiques. **
