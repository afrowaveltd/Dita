# Rezumatul modificărilor aduse serviciului de traducere automată

## Prezentare generală

Acest document rezumă toate modificările aduse serviciului de traducere automată Dita, inclusiv refactorarea arhitecturii, noi caracteristici, îmbunătăţiri de observabilitate şi îmbunătăţiri de localizare.

## Modificări de arhitectură

### Refactored backendTranslation Service

Monoliticul a fost descompus în patru servicii specializate coordonate de un orchestrator usor:

- **BackendTranslationService**
- **CountriesTranslationService**
- **LocalizareTranslationService**
- **DocumenteTranslationService**
- **SignalRPublisher**
- **TranslationRetryService**

### Beneficii

- **Separarea preocupărilor**: Fiecare serviciu se ocupă de un singur domeniu de traducere
- ** Mentenabilitate**: Clasele mai mici sunt mai ușor de înțeles și de testat
- ** Extensibilitate **: Noi obiective de traducere pot fi adăugate prin implementarea interfeței
- ** Fiabilitate**: Serviciile independente asigură o izolare mai bună a defectelor

## Caracteristici noi

### Monitor traducere live

**Locaţia**:

O nouă pagină admin care oferă vizibilitate în timp real în conducta de traducere:

- Afișează toate evenimentele SignalR așa cum apar
- Tipuri de mesaje codate în culori (albastru=pornit, verde=completat, roșu=eroare)
- Banner de stare conexiune cu reconectare automată
- Contorul de mesaje și exportul către JSON

### Deţinătorii numiţi

Sistemul de localizare suportă acum persoanele numite () pentru ameliorarea gramaticalității în diferite limbi:

```csharp
// Usage in code
var message = Localizer["WelcomeMessage", new Dictionary<string, string>
{
    ["userName"] = "John",
    ["count"] = "5"
}];
// Result: "Hello John, you have 5 new messages"
```

Caracteristici:
- Valorile titularului locului furnizate la rulare sau stocate în
- Mascarea/restaurarea automată în timpul traducerii pentru prevenirea corupției
- Înapoi compatibil cu deținătorii de poziții existenți

### Traducere creativă

Fișierele Markdown sunt traduse treptat:

- **Per-language saving**: Each target language is saved immediately after translation, reducing memory pressure
- **Block-nivel de urmărire**: piese de traducere starea pe bloc
- ** Rejudecarea selectivă **: Numai blocuri eșuate sunt re-traduse pe următoarea cursă
- ** Persistenţa datelor**: Starea de traducere supravieţuieşte reluării aplicaţiei

### Logica remetrică îmbunătățită

Trei niveluri de reziliență:

1. **HTTP retry** (LibreTranslateService): 5 încercări cu exponențial backoff (1s
2. **Retroducere de fază** (TranslationRetryService): 3 încercări suplimentare cu întârzieri de 30 de ani
3. **Block retry** (DocumentsTranslationService): Blocurile Markdown eșuate au fost rejudecate pe următoarea cursă

### Raportarea semnalizării

Raportarea în timp real a progreselor înregistrate pentru toate operațiunile de conducte:

- Fiecare etapă publică evenimente
- Progrese pe limbaj publicate ca evenimente
- Evenimentele de eroare includ context detaliat (sursa, codul de eroare, mesaj)
- Numerele de secventa garanteaza comandarea in fiecare rula

## Modificări de configurare

### appsetings.json

Fără schimbări de rupere. Configurația existentă continuă să funcționeze:

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

### Servicii noi

Înregistrată în:

- /
- `TranslationRetryService`
- /
- /
- /
- /

Conexiunea SignalR este cartografiată pentru conexiunile clienţilor.

## Testare

### Starea încercării

- **243/244 teste care trec** (1 omise din cauza accesului simultan la fișiere în mediul de testare)
- O nouă acoperire de încercare adăugată pentru:
  - Funcţionalitate PlaceholderServicii
  - Name
  - Indexeri JsonStringLocalizer

### Limite cunoscute

- testul este omis atunci când rulează în paralel, deoarece mai multe cazuri de testare împărtășesc același fișier. Trece când e izolat.

## Structura fișierului nou

### Servicii în

- — Pipeline orchestrator
- Traducerea numelui de țară
- Sincronizarea dicţionarului JSON
- Traducerea Markdown
- editare mesaj semnal
- — Retry logic with placeholder masking
- Interfaţa editorului
- Interfața cu serviciul de țară
- Interfaţa serviciului de localizare
- Interfața de serviciu document
- Interfață orchestrator (actualizată)
- — Per-file translation metadata

### Servicii actualizate în

- — Added named placeholder support
- Actualizat pentru un nou parametru
- — Named placeholder management
- — Placeholder interface

### Pagina de administrare nouă în

- Pagina de monitorizare în timp real
- Modelul paginii

### Documentație nouă în

- — Updated pipeline documentation
- Ghid de sistem al titularului locului
- Ghid de utilizare a tabloului de bord
- Prezentare generală a arhitecturii tehnice

## Compatibilitatea înapoi

Toate modificările sunt aditive:

- Codul de localizare existent () funcționează neschimbat
- Formatarea pozițională () funcționează neschimbată
- Format dicţionar JSON existent este neschimbat
- Structura de marcare existentă este neschimbată
- Mesajele SignalR folosesc același format

## Calea migrației

Nu este necesară migrarea. Refactorizarea este internă:

1. Vechi a fost păstrat ca o referință și apoi înlocuit
2. Înregistrările DI au fost actualizate pentru a utiliza noi interfețe
3. Toți consumatorii existenți nu văd nicio schimbare

## Îmbunătățiri ale performanței

- **Redusă utilizarea memoriei**: Fișiere salvate imediat pe limbă în loc să dețină toate în memorie
- **Faster incremental ruleaza**: Doar blocurile Markdown modificate/retrase sunt retraduse
- **O mai bună vizibilitate**: Progresul în timp real ajută la diagnosticarea etapelor lente

## Îmbunătăţiri viitoare

Îmbunătăţiri planificate:

1. **AI fine- tuning**
2. **Admin Autentificare**
3. **Dictionary editor**
4. **Statistici de traducere**
5. **Custom placeholder sintaxa**

## Contact

Pentru întrebări sau probleme legate de serviciul de traducere, vă rugăm să consultați documentația detaliată din directorul fiecărui modul sau să contactați echipa de dezvoltare.
