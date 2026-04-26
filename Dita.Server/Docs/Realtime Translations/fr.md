# Traductions en temps réel

Ce document existe comme entrée de test en direct pour le pipeline de traduction automatique.

## Ce que fait le service

Le service fonctionne selon un calendrier et valide le serveur de traduction, la configuration et les langues disponibles avant tout travail de traduction.

Après l'étape de validation, il synchronise les noms de pays du catalogue des pays en lecture seule dans les dictionnaires JSON de localisation standard. Si la langue par défaut de l'application est l'anglais, l'entrée du pays est stockée comme clé égale la valeur. Si la langue par défaut est différente, le nom du pays anglais est d'abord traduit dans la langue par défaut, et seulement alors stocké comme clé égale la valeur dans le dictionnaire par défaut.

Ensuite, le service compare le dictionnaire de localisation par défaut actuel avec l'instantané stocké de l'exécution précédente. Les entrées nouvellement ajoutées ne sont traduites dans les langues cibles que lorsque la clé n'existe pas déjà, de sorte que les traductions manuelles restent prioritaires. Les entrées supprimées sont supprimées de tous les dictionnaires cibles pour garder l'ensemble cohérent.

Enfin, le service scanne les racines de documentation configurées pour les arbres Markdown. Chaque dossier thématique doit contenir un fichier source nommé d'après la langue par défaut, comme en.md. Le service hache ce fichier source, détecte les changements, traduit les fichiers Markdown cibles manquants ou périmés, et stocke le hash courant à côté du fichier source. Si l'écriture du hachage à côté du fichier source n'est pas possible, elle revient à un stockage temporaire.

## L'état d'avancement des rapports du service

Le moteur émet des messages de SignalR généraux à travers le hub de localisation en utilisant une enveloppe de message. Chaque message porte un type de message, l'étape du processus en cours, un horodatage UTC, un résumé de texte et une charge utile optionnelle spécifique à l'étape.

Les étapes actuelles sont les suivantes:

- Vérifier les serveurs
- Pays
- TraduireJsonFiles
- TraduireMarkdownFiles
- Stocker les résultats

Le flux typique de messages est l'étape commencée, l'étape terminée et le pipeline terminé. Si une étape échoue, le message est marqué comme une erreur et inclut des informations d'erreur structurées avec des codes d'erreur unifiés.

## Principes de conception

Les traductions sont traitées successivement pour éviter de surcharger le serveur LibreTrail.

Localisation Les dictionnaires JSON sont toujours stockés avec des clés classées alphabétiquement et formatés JSON pour faciliter la maintenance.

L'instantané de dictionnaire précédent par défaut est stocké de façon persistante afin qu'un redémarrage de l'application ne perde pas le suivi du changement.

**Les traductions manuelles ont toujours priorité sur les ajouts automatiques.**
