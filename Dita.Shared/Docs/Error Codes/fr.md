# Codes d'erreur

Dita uses a **range-partitioned, unified error code architecture** that provides both domain-specific enums and a single catch-all `ErrorCode` type. Every error in the system — from network failures to disk I/O, from authentication to configuration — is represented by a member of this hierarchy.

## Architecture

### Répartition

Portée
|-------|----------|----------|
1000–1999
2000–2999
3000–3999
4000–4999
5000–5999
6000–6999
7000–7999
8000–8999
9000–9999

### Modèle à double énum

Chaque domaine d'erreur est représenté par **tant** un sous-enum ciblé (par exemple) et des entrées dans l'enum unifié. Les sous-enums utilisent des noms nus; l'enum unifié préfixe des noms avec la catégorie:

```csharp
// Sub-enum — domain-specific context
NetworkError.ConnectionRefused    // 1003

// Unified enum — catch-all reference
ErrorCode.NetworkConnectionRefused  // 1003 — same numeric value
```

Cela permet au code de travailler avec des types spécifiques de domaines lorsque le contexte est connu, tout en prenant en charge la gestion des erreurs génériques qui fonctionne dans tous les domaines.

### sentinelle

Chaque sous-enum définit la valeur de base de sa plage (par exemple). La méthode reconnaît cela et renvoie .

## Classe de code d'erreur

L'enum consolide toutes les valeurs de sous-enum en un seul type avec des plages entières **non-overlaping**. La classe statique d'accompagnement fournit l'humanisation:

```csharp
// Get human-readable text for any error code
string text = ErrorCodeText.ErrorText(ErrorCode.NetworkConnectionRefused);
// → "Network connection refused"

string text2 = ErrorCodeText.ErrorText(5005);
// → "Localization invalid translation format"
```

### Logique d'humanisation

suit une approche de surconfiguration des conventions:

1. PascalLes noms de cas sont divisés en mots via regex
2. Les acronymes connus sont normalisés (Io → I/O, Api → API, Dns → DNS, Http → HTTP, Ssl → SSL, Mfa → MFA, OAuth → OAuth, Sso → SSO, Xml → XML, Json → JSON, Url → URL)
3. Les jetons de toutes les cases (par exemple) sont conservés
4. Valeurs se terminant en retour

## Enums spécifiques au domaine

### Réseau Error (1000–1999)

Couvre les problèmes DNS, SSL/TLS, proxies, passerelles, erreurs de protocole HTTP, connectivité et cycle de vie des requêtes.

Membres à noter
|---|---|
1 000
1001
1002
1003
1004
1005
1006
1007
1008
1009
1010
1019
1020
1021

### Erreur de stockage (2000–2999)

Couvre les connexions à la base de données, les transactions (commit/rollback/timeout), l'intégrité (contraintes, impasses, clés étrangères), la gestion des schémas, la sauvegarde/restauration, la réplication et le quota.

Membres à noter
|---|---|
2000
2003
2004
2007
2010
2012
2013
2018
2023
2029

### Disque Error (3000–3999)

Couvre les erreurs de disque physique de bas niveau et de lecteur: secteurs défectueux, défaillances SMART, dégradation RAID, tables de partition, défaillances matérielles, montage/démontage, format et opérations d'éjection.

Membres à noter
|---|---|
3000
3001
3010
3012
3021
3027
3032

### FichierSystemError (4000–4999)

Couvre les erreurs de fonctionnement du système de fichiers : accès/autorisation, verrouillage des fichiers, compression/décompression/encryptage, problèmes de chemin, liens symboliques, violations de partage et opérations générales d'E/S.

Membres à noter
|---|---|
4 000
4001
4013
4011
4023
4024
4028

### LocalisationErreur (5000–5999)

Couvre les erreurs spécifiques au pipeline de localisation : dictionnaires, encodage, validation locale, formulaires pluriels, API de traduction externe (auth, disponibilité, file d'attente, timeout) et formatage de chaînes.

Membres à noter
|---|---|
5000
5001
5007
5014
5015
5016
5018

### AuthentificationErreur (6000–6999)

Couvre l'authentification et l'autorisation : identifiants, jetons (refraîchissement/accès), sessions, MFA/2FA, biométrie, certificats, OAuth, SSO, et états de compte (désactivés, expirés, verrouillés).

Membres à noter
|---|---|
6000
6001
6004
6015
6024
6026

### ValidationErreur (7000–7999)

Couvre la validation d'entrée : vérifications de format (email, téléphone, URL, JSON, XML, datetime), contraintes de distance/longueur, défaillances de conversion, champs requis, patron/regex, complexité du mot de passe.

Membres à noter
|---|---|
7000
7003
7016
7018

### ConfigurationErreur (8000–8999)

Couvre la configuration et les paramètres : accès aux fichiers, analyse, validation, coffre-fort secret/clé, chaînes de connexion, DI, drapeaux de fonctionnalités, variables d'environnement et erreurs de schéma/version.

Membres à noter
|---|---|
8000
8001
8016
8019

### GénéralErreur (9000–9999)

Catch-all pour les erreurs à l'échelle de l'application: mémoire, concurrence, licence, limitation des taux, filetage, gestion des ressources, support des fonctionnalités et exceptions non traitées.

Membres à noter
|---|---|
9000
9004
9007
9015
9014

## Enums pipeliniers

### Étape du processus

Définit les étapes séquentielles du pipeline de traduction automatique:

Valeur
|-------|------|-------------|
0
1
2
3
4
5

### Type de mesure de localisation

Type de message en temps réel émis par le pipeline:

Valeur
|-------|------|---------|
0
1
2
3
4
5
6

### Traduction Objectif

Spécifie le type de contenu à traduire :

Valeur
|-------|------|---------------|
0
1
2

### Changement de phrase

Tracks État de changement de type CRUD pour les entrées de dictionnaire de localisation :

Valeur
|-------|------|
0
1
2
3

### Comparaison

Opérateurs de comparaison utilisés pour l'évaluation/filtrage des valeurs:

Valeur
|-------|------|----------|
0
1
2
3
4
5
6

### Sexe

Genre grammatical/social pour la localisation :

Valeur
|-------|------|
0
1
2
3

## Utilisation de codes d'erreur

### Rapports en attente

Les erreurs de traduction sont portées dans les dossiers :

```csharp
public class TranslationError
{
    public string Source { get; set; }        // Language code, file path, or stage name
    public ErrorCode Code { get; set; }       // Unified error code
    public string ErrorMessage { get; set; }  // Human-readable text
}
```

### Dans les réponses API

```csharp
var result = Response<TranslateResult>.Fail(
    ErrorCodeText.ErrorText(ErrorCode.NetworkConnectionRefused),
    ErrorCode.NetworkConnectionRefused);
```

### Humaniser tout code

```csharp
// From enum value
string text = ErrorCodeText.ErrorText(ErrorCode.StorageDeadlockDetected);
// → "Storage deadlock detected"

// From raw integer (validates against defined values)
string text2 = ErrorCodeText.ErrorText(2010);
// → "Storage deadlock detected"

// Undefined code
string text3 = ErrorCodeText.ErrorText(99999);
// → "Unknown error (99999)"
```
