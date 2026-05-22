# Modèles de données

L'espace de noms définit toutes les structures de données utilisées dans le système de localisation et de traduction — des paires de requêtes/réponses d'API aux rapports de pipeline et aux instantanés de tableau de bord.

## Aperçu du modèle

### Configuration

#### Paramètres de traduction automatique

Modèle de configuration lié de . Contrôles LibreRaccordement serveur et comportement du pipeline.

Biens
|---|---|---|---|
URL du serveur LibreTrail
Indique si une clé API est nécessaire
Clé API
Langue par défaut de l'application
Langues à exclure de la traduction
Répertoires racine de documentation
Activer les parcours de pipeline programmés
Retard avant la première manche
Procès-verbal entre les essais
LibreExtrait texte
LibreExtrait du fichier
LibreTraitement des langues
LibreAtteinte de détection
Délai entre les demandes de traduction
Timeout HTTP par requête
Indique si config a été chargé

### Modèles d'API LibreTrail

#### TranslateRequest → TraduireRésultat

**Requête** — appel d'API de traduction de texte:

Biens
|---|---|---|---|
| `Query` | `string` | `"q"` | `""` |
—
—
—
| `ApiKey` | `string?` | `"api_key"` | `null` |
—

**Résultat** — réponse de traduction:

Biens
|---|---|
| `TranslatedText` | `string` |
| `DetectedLanguage` | `Detections` |
| `Alternatives` | `List<string>` |

#### Détecter la demande → Détection

**Demande**: **Réponse**:
**Response**: `{ Language, Confidence }`

#### TranslateFileRequest → TranslateFileRésultat

**Demande**: **Réponse**:
**Response**: `{ TranslatedFileUrl }`

#### Librelangue

Entrée en une seule langue à partir du paramètre:

```json
{ "code": "en", "name": "English", "targets": ["cs", "de", "fr", ...] }
```

### Modèles de rapport de pipeline

#### Rapport de vérification

Résultat de la phase de validation du serveur:

Biens
|---|---|
| `AppsettingsLoaded` | `bool` |
| `TranslationServerReady` | `bool` |
| `DefaultLanguage` | `string` |
| `AvailableLanguages` | `string[]` |
| `ServerLatencyMs` | `int` |

#### Rapport sur les traductions

Résultat des étapes de traduction du dictionnaire/pays:

Biens
|---|---|
| `DefaultDictionaryExists` | `bool` |
| `DefaultDictionaryCount` | `int` |
| `ToTranslateCount` | `int` |
| `AddedCount` | `int` |
| `RemovedCount` | `int` |
| `SkippedCount` | `int` |
| `TranslatedCount` | `int` |
| `ErrorsCount` | `int` |
| `Errors` | `List<TranslationError>?` |

#### Rapport sur les traductions

Résultat de la phase de traduction Markdown:

Biens
|---|---|
| `SourceFilesDetected` | `int` |
| `SourceFilesChanged` | `int` |
| `SavedFiles` | `int` |
| `SkippedFiles` | `int` |
| `TempFallbackWrites` | `int` |
| `Errors` | `List<TranslationError>` |

#### Dépôt du rapport

Agrégation finale des produits persistants:

Biens
|---|---|
| `RunStartedUtc` | `DateTime` |
| `RunCompletedUtc` | `DateTime?` |
| `SavedDictionaryFiles` | `int` |
| `SavedMarkdownFiles` | `int` |
| `SavedHashFiles` | `int` |
| `TempFallbackWrites` | `int` |
| `Errors` | `List<TranslationError>` |

#### Rapport d'étape<T>

Contenant générique qui enveloppe tout type de rapport avec des métadonnées d'étape:

Biens
|---|---|
| `ReportedStage` | `ProcessStage` |
| `StageData` | `T?` |
| `StageStartTime` | `DateTime?` |
| `StageEndTime` | `DateTime?` |
(calculé)

### Modèles de travail de traduction

#### PhraseInQueue

Article de travail pour la file d'attente de traduction:

Biens
|---|---|
| `Target` | `TranslationTarget` |
| `Key` | `string?` |
| `Phrase` | `string` |
| `SourceLanguage` | `string?` |
| `TargetLanguage` | `string` |
| `ChangeRequired` | `PhraseChange` |
| `AddedToList` | `DateTime` |
| `TranslationStart` | `DateTime?` |
| `TranslationEnds` | `DateTime?` |
| `IsTranslated` | `bool` |
| `TranslatedText` | `string?` |

#### TraductionErreur

Enregistrement des erreurs structurées dans tous les rapports :

Biens
|---|---|
(code de langue, chemin de fichier ou nom d'étape)
| `Code` | `ErrorCode` |
| `ErrorMessage` | `string` |

#### Traduction unique

Dictionnaire unique local:

Biens
|---|---|
| `Language` | `string` |
| `Translations` | `Dictionary<string, string>` |

#### MarkdownBlock Translatable

Bloc extrait d'un document Markdown :

Biens
|---|---|
| `Key` | `Guid` |
| `OriginalText` | `string` |
| `TranslatedText` | `string?` |
| `StartLine` | `int` |
| `EndLine` | `int` |
| `BlockType` | `string` |
| `Metadata` | `Dictionary<string, object>` |
| `IsTranslated` | `bool` |

### Modèles de résolution de texte

#### TexteLocalisation Demande → TextLocalisation Réponse

**Demande** — localisation par dictionnaire (écrite):

Biens
|---|---|
| `Text` | `string` |
| `TargetLanguage` | `string?` |
| `Values` | `Dictionary<string, string>?` |

**Réponse**:

Biens
|---|---|
(original)
(localisé)
| `TargetLanguage` | `string` |
| `DefaultLanguage` | `string` |
| `FoundInTargetDictionary` | `bool` |
| `AddedToDefaultDictionary` | `bool` |
| `ResolvedFrom` | `TextResolutionSource` |

#### TexteFrançaisDemande → TexteFrançaisRéponse

**Demande** — traduction dynamique (lecture seule):

Biens
|---|---|
| `Text` | `string` |
| `SourceLanguage` | `string?` |
| `TargetLanguage` | `string?` |
| `Values` | `Dictionary<string, string>?` |

**Réponse**:

Biens
|---|---|
(original)
(traduit)
| `SourceLanguage` | `string` |
| `TargetLanguage` | `string` |
| `FoundInTargetDictionary` | `bool` |
| `TranslationServerUsed` | `bool` |
| `ResolvedFrom` | `TextResolutionSource` |

#### TexteRésolutionSource

Indique où une valeur localisée/translationnelle a été résolue:

Valeur
|---|---|
Trouvé dans le dictionnaire local pour la langue cible
Trouvé dans le dictionnaire de langue par défaut
Non trouvé; ajouté au dictionnaire par défaut
Retourné par LibreTrail
Renvoyé tel quel sans résolution

### Types partagés

#### Définition des pays

Entrée en lecture seule à partir de :

Biens
|---|---|---|
| `Name` | `string` | `"name"` |
| `DialCode` | `string` | `"dial_code"` |
| `Emoji` | `string` | `"emoji"` |
| `Code` | `string` | `"code"` |

#### Conditions de comparaison

État du filtre pour l'évaluation:

Biens
|---|---|
| `Compare` | `Comparison` |
| `Values` | `int[]` |
| `IsOr` | `bool` |

#### ErreurRéponse

Enveloppe d'erreur simple API & #160;:

Biens
|---|---|
| `Error` | `string?` |
