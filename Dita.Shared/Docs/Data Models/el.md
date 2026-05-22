# Μοντέλα δεδομένων

Ο χώρος ονομάτων ορίζει όλες τις δομές δεδομένων που χρησιμοποιούνται σε όλο το σύστημα εντοπισμού και μετάφρασης — από API αίτημα / απόκριση ζεύγη μέχρι αναφορές αγωγών και στιγμιότυπα ταμπλό.

## Υπόδειγμα επισκόπησης

### διαμόρφωση

#### Αυτόματες ρυθμίσεις μεταφράσεων

Μοντέλο ρύθμισης που δεσμεύεται από . Ελέγχει τη σύνδεση του εξυπηρετητή LibreTranslate και τη συμπεριφορά του αγωγού.

Ιδιότητα
|---|---|---|---|
URL εξυπηρετητή Libre Translate
Αν απαιτείται ένα κλειδί API
Κλειδί API
Προεπιλεγμένη γλώσσα εφαρμογής
Γλώσσες που αποκλείονται από τη μετάφραση
Καταλόγους ρίζας τεκμηρίωσης
Ενεργοποίηση προγραμματισμένων δρομών αγωγών
Καθυστέρηση πριν την πρώτη εκτέλεση
Λεπτά μεταξύ των εργασιών
LibreTranslate τελικό σημείο κειμένου
LibreTranslate αρχείο τελικό σημείο
LibreTranslate languages τελικό σημείο
LibreTranslate τελικό σημείο ανίχνευσης
Καθυστέρηση μεταξύ των αιτήσεων μετάφρασης
Χρονικό όριο HTTP ανά αίτηση
Αν φορτώθηκε η ρύθμιση

### LibreTranslate μοντέλα API

#### Μετάφρασε την απαίτηση → Μεταφράζει το αποτέλεσμα

**Αίτηση ** — μετάφραση κειμένου API κλήση:

Ιδιότητα
|---|---|---|---|
| `Query` | `string` | `"q"` | `""` |
—
—
—
| `ApiKey` | `string?` | `"api_key"` | `null` |
—

**Αποτελέσματα ** — ανταπόκριση στη μετάφραση:

Ιδιότητα
|---|---|
| `TranslatedText` | `string` |
| `DetectedLanguage` | `Detections` |
| `Alternatives` | `List<string>` |

#### ΑνίχνευσηΑίτηση → Ανίχνευση

**Αίτηση **: **Απάντηση **:
**Response**: `{ Language, Confidence }`

#### ΜετάφρασεΑίτηση αρχείου → ΜεταφράστηκεΑρχείοResult

**Αίτηση **: **Απάντηση **:
**Response**: `{ TranslatedFileUrl }`

#### LibreΓλώσσα

Ενιαία καταχώρηση γλώσσας από το τελικό σημείο:

```json
{ "code": "en", "name": "English", "targets": ["cs", "de", "fr", ...] }
```

### Μοντέλα αναφοράς αγωγών

#### Έλεγχος αναφοράς

Αποτέλεσμα του σταδίου επικύρωσης του εξυπηρετητή:

Ιδιότητα
|---|---|
| `AppsettingsLoaded` | `bool` |
| `TranslationServerReady` | `bool` |
| `DefaultLanguage` | `string` |
| `AvailableLanguages` | `string[]` |
| `ServerLatencyMs` | `int` |

#### Αναφορά μεταφράσεων

Αποτέλεσμα σταδίων μετάφρασης λεξικού/χώρας:

Ιδιότητα
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

#### Έκθεση για τις μεταγλώττιση σημάτων

Αποτέλεσμα του μεταφραστικού σταδίου Markdown:

Ιδιότητα
|---|---|
| `SourceFilesDetected` | `int` |
| `SourceFilesChanged` | `int` |
| `SavedFiles` | `int` |
| `SkippedFiles` | `int` |
| `TempFallbackWrites` | `int` |
| `Errors` | `List<TranslationError>` |

#### Αποθήκευση έκθεσης

Τελική συγκέντρωση των συνεχιζόμενων εκροών:

Ιδιότητα
|---|---|
| `RunStartedUtc` | `DateTime` |
| `RunCompletedUtc` | `DateTime?` |
| `SavedDictionaryFiles` | `int` |
| `SavedMarkdownFiles` | `int` |
| `SavedHashFiles` | `int` |
| `TempFallbackWrites` | `int` |
| `Errors` | `List<TranslationError>` |

#### Αναφορά σταδίου <T>

Γενόσημο δοχείο που τυλίγει κάθε τύπο αναφοράς με μεταδεδομένα στάδιο:

Ιδιότητα
|---|---|
| `ReportedStage` | `ProcessStage` |
| `StageData` | `T?` |
| `StageStartTime` | `DateTime?` |
| `StageEndTime` | `DateTime?` |
(υπολογίστηκε)

### Μεταφραστικά μοντέλα εργασίας

#### ΦράσηInQueue

Αντικείμενο εργασίας για την ουρά μετάφρασης:

Ιδιότητα
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

#### ΜετάφρασηError

Δομημένο αρχείο σφαλμάτων που μεταφέρεται σε όλες τις εκθέσεις:

Ιδιότητα
|---|---|
(κωδικός γλώσσας, διαδρομή αρχείου ή όνομα σταδίου)
| `Code` | `ErrorCode` |
| `ErrorMessage` | `string` |

#### Ενιαία μετάφραση

Ένα τοπικό λεξικό:

Ιδιότητα
|---|---|
| `Language` | `string` |
| `Translations` | `Dictionary<string, string>` |

#### Σημείωση προς τα κάτωTranslatableBlock

Απόσπασμα από έγγραφο Markdown:

Ιδιότητα
|---|---|
| `Key` | `Guid` |
| `OriginalText` | `string` |
| `TranslatedText` | `string?` |
| `StartLine` | `int` |
| `EndLine` | `int` |
| `BlockType` | `string` |
| `Metadata` | `Dictionary<string, object>` |
| `IsTranslated` | `bool` |

### Μοντέλα ανάλυσης κειμένου

#### Εντοπισμός κειμένου Αίτηση → Εντοπισμός κειμένου Ανταπόκριση

**Αίτηση ** — εντοπισμός με βάση το λεξικό (εγγράψιμο):

Ιδιότητα
|---|---|
| `Text` | `string` |
| `TargetLanguage` | `string?` |
| `Values` | `Dictionary<string, string>?` |

** Απάντηση **:

Ιδιότητα
|---|---|
(πρωτότυπο)
(εντοπίστηκε)
| `TargetLanguage` | `string` |
| `DefaultLanguage` | `string` |
| `FoundInTargetDictionary` | `bool` |
| `AddedToDefaultDictionary` | `bool` |
| `ResolvedFrom` | `TextResolutionSource` |

#### Αίτηση μετάφρασης κειμένου → Απάντηση μετάφρασης κειμένου

**Αίτηση ** — δυναμική μετάφραση (μόνο ανάγνωση):

Ιδιότητα
|---|---|
| `Text` | `string` |
| `SourceLanguage` | `string?` |
| `TargetLanguage` | `string?` |
| `Values` | `Dictionary<string, string>?` |

** Απάντηση **:

Ιδιότητα
|---|---|
(πρωτότυπο)
(μεταφρασμένο)
| `SourceLanguage` | `string` |
| `TargetLanguage` | `string` |
| `FoundInTargetDictionary` | `bool` |
| `TranslationServerUsed` | `bool` |
| `ResolvedFrom` | `TextResolutionSource` |

#### Πηγή ανάλυσης κειμένου

Αναγνωρίζει από πού λύθηκε μια τοπική/μεταφρασμένη τιμή:

Τιμή
|---|---|
Βρέθηκε στο λεξικό locale για τη γλώσσα προορισμού
Βρέθηκε στο προεπιλεγμένο λεξικό γλωσσών
Δε βρέθηκε· προστέθηκε στο προεπιλεγμένο λεξικό
Επιστροφή από LibreTranslate
Επιστρέφεται χωρίς ανάλυση

### Κοινοί τύποι

#### ΧώραΟρισμός

Είσοδος μόνο για ανάγνωση από:

Ιδιότητα
|---|---|---|
| `Name` | `string` | `"name"` |
| `DialCode` | `string` | `"dial_code"` |
| `Emoji` | `string` | `"emoji"` |
| `Code` | `string` | `"code"` |

#### ΣύγκρισηΣυνθήκη

Κατάσταση φίλτρου για αξιολόγηση:

Ιδιότητα
|---|---|
| `Compare` | `Comparison` |
| `Values` | `int[]` |
| `IsOr` | `bool` |

#### Σφάλμα Απάντηση

Απλός φάκελος σφάλματος API:

Ιδιότητα
|---|---|
| `Error` | `string?` |
