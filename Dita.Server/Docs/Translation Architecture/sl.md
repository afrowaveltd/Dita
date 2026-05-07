# Arhitektura prevajanja

Ta dokument opisuje modularno arhitekturo Ditinega avtomatskega prevajalskega sistema, uvedenega za izboljšanje vzdržljivosti, preizkušnje in odpornosti.

## Cilji oblikovanja

V refaktoriji je bilo obravnavanih več pomislekov glede prvotnega monolitnega načrta:

- ** Ločitev pomislekov**: Vsaka prevajalska domena (države, slovarji JSON, Markdown) je izolirana.
- ** Vztrajnost mišic**: Datoteke so shranjene na jezik takoj po prevodu, zmanjšanje uporabe pomnilnika in zagotavljanje zgodnejših rezultatov.
- **Odpornost**: Večkratni nivoji ponovnih poskusov ne ovirajo celotnega cevovoda.
- ** Opazljivost**: Vsaka pomembna operacija se poroča preko SignalR za spremljanje v realnem času.
- ** Obsežnost**: Nove prevajalske cilje lahko dodamo z uvedbo enotnega vmesnika.

## Razgradnja storitve

### HrbtenicaPrevajanjeService (orgestrator)

** Odgovornosti**:
- Upravljanje življenjskega cikla cevovodov (začetek, dokončanje, ravnanje z napakami)
- Kontrola sočasne izpostavljenosti na osnovi semaforja (preprečuje prekrivanje poteka)
- Potrditev strežnika (latenca, razpoložljivost jezika, konfiguracija)
- Prenos na podstoritve

** NE vsebuje**:
- Logika prevajanja
- Datoteka I/O za posebne oblike
- Znova poskusi logiko

### DržavaPrevajanjeService

** Odgovornosti**:
- Beri iz imenika
- Uskladi imena držav v privzeti slovar krajev
- Prevedi manjkajoča imena držav na ciljni jezik
- Shrani vsak ciljni slovar takoj po prevodu

** Ključno vedenje**:
- Če je privzeti jezik angleščina: country names shranjen as- is
- Če privzeti jezik je drugo: Angleška imena prevedena v privzeti jezik najprej
- Vsak jezik se obdeluje neodvisno z lastno zanko za ponovni preizkus

### LokalizacijaTranslationService

** Odgovornosti**:
- Zaznaj dodane/odstranjene tipke s primerjavo trenutnega privzetega slovarja s prejšnjim posnetekom
- Prevedi dodane tipke v vsak ciljni jezik
- Odstrani izbrisane tipke iz vsakega ciljnega jezika
- Shrani posnetek za naslednjo primerjavo

** Ključno vedenje**:
- Ročni prevodi imajo vedno prednost (nikoli prepisan)
- Dodane tipke so prevedene in shranjene na jezik takoj
- Odvzete tipke se izbrišejo na jezik takoj
- Snapshot je shranjen šele potem, ko so vsi jeziki uspešno zaključeni

### Storitev prevajanja dokumentov

** Odgovornosti**:
- Korak nastavljen Markdown korenine rekurzivno
- Zaznaj spremenjene izvorne datoteke z uporabo SHA-256 hashes
- Stanje prevajanja po bloku v
- Prevedi blok-po-blok z retriiranjem na-blok
- Potrdi strukturo Markdown po prevodu
- Shrani vsako ciljno jezikovno datoteko neodvisno

** Ključno vedenje**:
- Razpredelnica 1
- Metapodatkovne skladbe, ki blokirajo uspelo/neuspelo na jezik
- Neuspeli bloki se ponovno preizkusijo ob naslednjem zagonu brez ponovnega prenosa uspešnih blokov
- Validacija strukture zagotavlja štetje naslovov, sezname, kodne bloke itd

## Znova poskusi strategijo

Sistem izvaja na treh ravneh:

### Raven 1 – HTTP (LibreTranslateService)

- Do 5 poskusov z eksponentnim zaostankom (1s, 2s, 3s, 4s, 5s)
- Ravna z zamiki v omrežju, 5xx napakami in prehodnimi napakami
- Vgrajen v odjemalca HTTP

### Raven 2 – faza (storitev prevajanjaRetryService)

- Do 3 poskusi s 30-sekundnimi zamudami
- Ponovno poganja celoten zahtevek za prevod po HTTP-nivo retries so izčrpani
- Na tej ravni se uporablja prikrivanje in obnova držal

### Raven 3 – blok (DokumentiPrevajanjeService)

- Posamezni Markdown bloki, ki ne uspejo, so označeni v metapodatkih
- Samodejno poženi po naslednjem cevovodu
- Uspešni bloki niso nikoli ponovno prevedeni

## Pretok podatkov

### Prevod slovarja JSON

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

### Prevod Markdown

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

### Prevajanje imena države

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

## Obstojnost stanja

### Posnetki

- **JSON**: Shranjeno v datoteki poleg privzetega slovarja (ime se razlikuje po ponudniku shranjevanja)
- **Purpose**: Omogoča postopno sinhronizacijo s sledenjem tistemu, kar je bilo prisotno v prejšnjem zagonu

### Datoteke Hash

- ** Markdown**: poleg izvorne datoteke
- **Padec**: če je primarna lokacija samo za branje
- **Purpose**: Detects source changes to avoid unnecessary re-translation

### Prevajalski metapodatki

- ** Markdown **:
- ** Vsebine**:
  - Osnovna vsebina hash
- Stanje bloka na jezik (array boolov)
- Zadnji časovni žig posodobitve
- **Utruje**: omogoča delno prevajanje le neuspešnih blokov

### Prostorsko skladiščenje

- **Datoteka**:
- **Vsebine**: Slovar ključev za pare z imenom imetnika
- **Podatki**: Zagotavlja privzete vrednosti za imenovane imetnike po vsej vlogi

## Signal R poročanje

### Abstrakcija založnika

storitve ločenega prevajanja iz specifikacij SignalR:

```csharp
public interface ISignalRPublisher
{
    Task PublishStageAsync<T>(Guid runId, ProcessStage stage, T data, ...);
    Task PublishMessageAsync(Guid runId, LocalizationMessageType type, ProcessStage stage, ...);
}
```

### Jamstva za zaporedje

- Sporočila v enem zagonu so monotonsko sekvenčna
- Zaporedne številke so edinstvene na vožnjo preko
- Odjemalci lahko zaznajo vrzeli ali prerazporeditev

### Kartiranje vozlišča

```csharp
app.MapHub<LocalizationHub>("/hubs/localization");
```

## Razširitvene točke

### Dodajanje novega cilja prevajanja

1. Ustvari nov vmesnik z
2. Izvajati vmesnik z logiko, specifično za domeno
3. Registracija v zabojniku DI
4. Vbrizgajte v konstruktor
5. Klic po obstoječih fazah

### Politika ponovnega poskusa po meri

Povozi parametre konstruktorja:

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

### Obdelava po meri

Izvajati za spremembo sintaksa ali shranjevanje imetnika:

```csharp
public class CustomPlaceholderService : IPlaceholderService
{
    // Use {{name}} syntax instead of {name}
    // Store in database instead of JSON file
    // Encrypt sensitive placeholder values
}
```

## Nastavitev

### appetits.json

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

### Nastavitev časa delovanja

Nastavitev
|---------|---------|--------|
80
10
3
30

## Strategija preskušanja

### Preskusi enote

Vsaka podstoritev je neodvisno preizkušena:

- Mock za simuliranje uspeha/neuspeha
- Opomni za preverjanje poročanja
- Uporabi začasne mape za datoteko I/O
- Preverjanje vedenja varčevanja na jezik

### Testi vključevanja

- Polno delovanje cevovoda z realnim (lokalni) LibrePrevajanje primer
- Preveri signal R sporočila se dostavijo povezanim strankam
- Preskus sočasne preventive s tekom (semafor)
- Potrdi strukturo Markdown po prevodu

### Preskusi med koncema

- Sprožitveno prevajanje preko API ali programerja
- Preveri vse ciljne jezikovne datoteke so ustvarjeni/posodobljeni
- Preveri metapodatkovne datoteke vsebujejo pravilno stanje bloka
- Potrdite, da so vsi prevodi ohranjeni

## Preučevanje učinkovitosti

- **Spomin**: Varčevanje v jeziku preprečuje shranjevanje vseh slovarjev v pomnilniku
- **Disk I/ O**: Metapodatkovne datoteke dodajajo majhne režijske stroške, vendar omogočajo postopna dela
- **Network**: Zaporedna obdelava z gnečo preprečuje veliko LibrePrevajanje
- **CPU**: SHA-256 validacija hashing in regex sta hitra glede na latenco prevajanja
- **SignalR**: Lahka sporočila, za tipična poročila ni potrebno stiskanje tovora

## Migracija iz monolitnega oblikovanja

Original je vseboval vso logiko v enem razredu. Selitvena pot:

1. Izvleci logiko države →
2. Izvlecite JSON logiko →
3. Izvleci logiko Markdown →
4. Ekstraktni signal R založništvo →
5. Ekstraktna logika ponovnega preizkusa →
6. Poenostavitev orkestra na samo delegacijo

Vsi obstoječi vmesniki () ostanejo nespremenjeni. Potrošniki plinovoda ne vidijo nobenih prelomnih sprememb.
