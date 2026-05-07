# Arhitectura traducerilor

Acest document descrie arhitectura modulară a sistemului de traducere automată a lui Dita, introdusă pentru a îmbunătăți capacitatea de întreținere, capacitatea de testare și reziliența.

## Obiective de proiectare

Refactorizarea a abordat mai multe preocupări legate de designul monolitic original:

- **Separarea preocupărilor**: Fiecare domeniu de traducere (țări, dicționare JSON, Markdown) este izolat.
- ** Persistenţă creativă**: Fişierele sunt salvate pe limbă imediat după traducere, reducând utilizarea memoriei şi oferind rezultate anterioare.
- **Resilience**: Multiple niveluri de rejudecare manipulează eșecurile tranzitorii fără a bloca întreaga conductă.
- **Observabilitate**: Fiecare operațiune semnificativă este raportată prin SignarR pentru monitorizare în timp real.
- ** Extensibilitate **: Noi obiective de traducere pot fi adăugate prin implementarea unei singure interfețe.

## Descompunerea serviciului

### SuportTranslationService (orchestrator)

** Responsabilitățile**:
- Gestionarea ciclului de viață al conductei (pornire, finalizare, manipulare a erorilor)
- Controlul semaforului asupra convailităților (prevenește suprapunerea rulajelor)
- Validarea serverului (latitudine, disponibilitatea limbii, configurare)
- Delegația la subservicii

** NU conţine **:
- Logica traducerii
- Fișier I/O pentru formate specifice
- Logica remetrică

### Serviciul de Tranzitie Tari

** Responsabilitățile**:
- Citește din dosar
- Sincronizează numele de țară în dicționarul local implicit
- Tradu numele de țară lipsă per limbă țintă
- Salvați fiecare dicționar țintă imediat după traducere

** Comportamente cheie**:
- În cazul în care limba implicită este limba engleză: numele țărilor stocate ca-is
- În cazul în care limba implicită este alta: numele în limba engleză traduse în limba implicită primul
- Fiecare limbă este procesată independent cu propria buclă de rejudecată

### Serviciul de Tranducere Localizare

** Responsabilitățile**:
- Detectează tastele adăugate/modificate prin compararea dicționarului implicit curent cu poza anterioară
- Tradu cheile adăugate în fiecare limbă țintă
- Elimină cheile eliminate din fiecare limbă țintă
- Salvează imaginea pentru următoarea comparație

** Comportamente cheie**:
- Traducerile manuale au întotdeauna prioritate (niciodată suprascrise)
- Tastele adăugate sunt traduse și salvate pe limbă imediat
- Tastele eliminate sunt șterse pe limbă imediat
- Snapshot este salvat numai după ce toate limbile complet cu succes

### Servicii de traducere documente

** Responsabilitățile**:
- Mersul configurat rădăcinile Markdown recursiv
- Detectează fișierele sursă modificate utilizând hașii SHA-256
- Starea traducerii pe bloc în
- Tradu bloc cu bloc cu rejudecare per bloc
- Validarea structurii Markdown după traducere
- Salvează independent fiecare fișier de limbă țintă

** Comportamente cheie**:
- Granulozitate la nivel de bloc: rubrici, paragrafe, elemente de listă sunt traduse separat
- Urme de metadate care au reușit/au eșuat pe limbă
- Blocurile eșuate sunt rejudecate pe rula următoare fără a re-traduce blocuri de succes
- Validarea structurii asigură numărarea rubricilor, liste, blocuri de coduri, etc. sursa meciurilor

## Strategia de rescriere

Sistemul implementează retries la trei niveluri:

### Nivelul 1

- Până la 5 încercări de retragere exponențială (1s, 2s, 3s, 4s, 5s)
- Handles timeouts rețea, 5xx erori, și eșecuri tranzitorii
- Construit în configurația client HTTP

### Nivelul 2

- Până la 3 încercări cu întârzieri de 30 de secunde
- Re-conduce întreaga cerere de traducere după HTTP-nivel retries sunt epuizate
- Incarcator mascare si restaurare se aplica la acest nivel

### Nivelul 3

- Blocuri individuale Markdown care nu sunt marcate în metadate
- Retried automat on the next conduct run
- Blocuri de succes nu sunt niciodată re-traduse

## Fluxul de date

### Traducerea dicţionarului JSON

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

### Traducere Markdown

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

### Traducerea numelui țării

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

## Persistența statului

### Fotografii

- ** JSON**: Stocat într-un fișier lângă dicționarul implicit (numele variază de furnizorul de stocare)
- **Purpose**: Activează sincronizarea incrementală prin urmărirea a ceea ce a fost prezent în rula anterioară

### Fișiere hash

- **Markdown**: lângă fișierul sursă
- **Fallback**: dacă locaţia principală este numai citire
- **Purpose**: Detectează modificările sursei pentru a evita re-transformarea inutilă

### Metadate de traducere

- ** markdown**:
- ** multumiri**:
  - Conţinutul sursei hash
- Statusul blocului per-limbă (array of booleans)
- Ultima dată de actualizare
- **Purpose**: Activează re-transformarea parțială a numai blocuri eșuate

### Depozitarea suportului

- ** File**:
- **Contents**: Dicţionar de taste la locholder nume-valoare pereche
- **Purpose**: Oferă valori implicite pentru titularii de locuri numiți în întreaga aplicație

## Semnal R Raportarea

### Abstractare editor

decuplează servicii de traducere din specificul SignalR:

```csharp
public interface ISignalRPublisher
{
    Task PublishStageAsync<T>(Guid runId, ProcessStage stage, T data, ...);
    Task PublishMessageAsync(Guid runId, LocalizationMessageType type, ProcessStage stage, ...);
}
```

### Garanții privind secvența

- Mesajele dintr-o singură cursă sunt secvențiate monoton
- Numerele de succesiune sunt unice pe run prin
- Clienții pot detecta lacune sau reordonare

### Cartografiere hub

```csharp
app.MapHub<LocalizationHub>("/hubs/localization");
```

## Puncte de extindere

### Adăugarea unei noi ținte de traducere

1. Creează o interfață nouă cu
2. Implementarea interfeței cu logica specifică domeniului
3. Înregistrarea în containerul DI
4. Injectați în constructor
5. Apel după etapele existente

### Politica de rejudecare personalizată

Suprascrie parametrii constructorului:

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

### Manipularea la domiciliu

Punerea în aplicare a sintaxei sau stocării titularului de loc de schimbare:

```csharp
public class CustomPlaceholderService : IPlaceholderService
{
    // Use {{name}} syntax instead of {name}
    // Store in database instead of JSON file
    // Encrypt sensitive placeholder values
}
```

## Configurare

### appsetings.json

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

### Tuning pe timp de execuție

Setare
|---------|---------|--------|
80
10
3
30

## Strategia de testare

### Încercări în unitate

Fiecare subserviciu poate fi testat independent:

- Şoc pentru simularea succesului/eşecului
- Mock pentru a verifica raportarea
- Utilizați directoare temporare pentru fișierul I/O
- Verificați comportamentul de economisire per-limbă

### Teste de integrare

- Full conductle run with real (local) LibreTranslate instance
- Verifică semnalul Mesajele R sunt livrate clienților conectați
- Proba de prevenire concomitentă a alergării (semafor)
- Validarea structurii Markdown după traducere

### Încercările de la un capăt la altul

- Traducere de declanșare prin API sau programator
- Verificați toate fișierele lingvistice țintă sunt create/actualizate
- Verificați fișierele de metadate conțin starea corectă a blocului
- Confirmaţi că titularii de locuri sunt conservaţi prin traduceri

## Considerații privind performanța

- **Memorie**: Salvarea pe limbaj previne păstrarea tuturor dicționarelor în memorie
- **Disk I/O**: Fișierele Metadata adaugă cheltuieli generale mici, dar permit lucrul incremental
- **Network**: Procesare secvenţială cu trepidaţie previne supraîncărcarea libreTranslate
- **CPU**: SHA-256 hashing și validarea regex sunt rapid legate de latență traducere
- **SignalR**: Mesaje ușoare, nu este necesară compresie de sarcină utilă pentru rapoarte tipice

## Migrația de la proiectarea monolitică

Originalul conţinea toată logica într-o singură clasă. Calea migraţiei:

1. Logica țării de extracție →
2. Extragerea logica JSON →
3. Extragerea logica Markdown →
4. Semnal de extragere R publicarea →
5. Logica de retrimitere a extractului →
6. Simplifică orchestratorul doar pentru delegație

Toate interfețele existente () rămân neschimbate. Consumatorii conductei nu văd nicio schimbare.
