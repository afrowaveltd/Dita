# Povzetek sprememb samodejne prevajalske storitve

## Pregled

Ta dokument povzema vse spremembe v Dita avtomatsko prevajanje storitev, vključno z arhitekturo refaktoring, nove značilnosti, izboljšanje opaznosti, in lokalizacije izboljšave.

## Spremembe arhitekture

### Refaktored backendPrevajanjeService

Monolit je razpadel na štiri specializirane storitve, ki jih koordinira lahki orkestrator:

- **BackendTranslationService** — Pipeline Orchestrator (potrjevanje strežnika, odrska delegacija, ravnanje z napakami)
- **Storitve prevajanja** — Sinhronizacija imena države (angleščina → ciljni jezik)
- **LokalizationTranslationService** — Sinhronizacija slovarja JSON (dodane/odvzete tipke)
- **DokumentiPrevajanjeService** — Prevajanje dokumentacije za označevanje s sledenjem ravni blokov
- **SignalRP Publisher** – Poročanje o napredku v realnem času prek SignalR
- **TranslationRetryService** – Stage-level retry with placeholder shranjevanje

### Koristi

- ** Ločitev pomislekov**: Vsaka storitev obravnava eno prevajalsko domeno
- ** Trajnost**: Manjše razrede je lažje razumeti in testirati
- ** Obsežnost**: Novi cilji prevajanja se lahko dodajo z izvajanjem vmesnikov
- **Zanesljivost**: Neodvisne storitve zagotavljajo boljšo izolacijo napak

## Nove lastnosti

### Nadzornik prevajanja v živo

** Kraj**:

Nova admin stran, ki zagotavlja v realnem času vidnost v prevajalski cevovod:

- Prikazuje vse signale r dogodki, kot se pojavijo
- Barvno kodirane vrste sporočil (modra=začeta, zelena=dokončana, rdeča=napaka)
- Napis stanja povezave z samodejno ponovno povezavo
- Števec sporočil in izvoz v JSON

### Imenovani imetniki

Sistem lokalizacije zdaj podpira imenovane imetnike () za izboljšanje slovničnosti v različnih jezikih:

```csharp
// Usage in code
var message = Localizer["WelcomeMessage", new Dictionary<string, string>
{
    ["userName"] = "John",
    ["count"] = "5"
}];
// Result: "Hello John, you have 5 new messages"
```

Značilnosti:
- Vrednosti imetnikov, zagotovljene med izvajanjem ali shranjene v
- Samodejno maskiranje/restavracija med prevajanjem za preprečevanje korupcije
- Nazaj združljiv z obstoječimi pozicijskimi imetniki

### Povzpetniški prevod

Markdown datoteke so prevedene postopoma:

- ** Varčevanje po jeziku**: Vsak ciljni jezik se shrani takoj po prevodu, kar zmanjšuje spominski pritisk
- **Sledenje ravni bloka**: status prevajanja skladb na blok
- **Selektivna ponovna preiskava**: Pri naslednjem zagonu se ponovno prevedejo samo neuspešni bloki
- ** Vztrajnost metapodatkov**: Prevajalsko stanje preživi ponovni zagon programa

### Izboljšana logika ponovnih poskusov

Tri stopnje odpornosti:

1. **HTTP retry** (LibreTranslateService): 5 poskusov z eksponentnim zaostankom (1s-5s)
2. **Stage retry** (translationRetryService): 3 dodatni poskusi s 30-imi zamudami
3. **Block retry** (DocumentsTranslationService): Na naslednjem zaganjanju so se ponovno poskusili bloki Markdown

### Poročanje o signalih

Poročanje o napredku v realnem času za vse cevovodne operacije:

- Vsaka stopnja objavlja dogodke
- Napredek v jeziku, objavljen kot dogodki
- Dogodki napak vključujejo podroben kontekst (vir, koda napake, sporočilo)
- Zaporedne številke zagotavljajo naročanje znotraj vsakega cikla

## Spremembe nastavitev

### appetits.json

Nobenih sprememb. Obstoječa konfiguracija še naprej deluje:

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

### Nove storitve

Registrirano v:

- /
- `TranslationRetryService`
- /
- /
- /
- /

Signal R center je začrtan za povezave strank.

## Testiranje

### Stanje preskusa

- **243/244 testov, ki so opravili ** (1 preskočili zaradi sočasnega dostopa do datoteke v preskusnem okolju)
- Dodana nova preskusna pokritost za:
  - Placeholder Funkcionalnost storitve
  - HrbtenicaPrevajanje Servisna orkestracija
  - JsonStringLocalizer, indekserji

### Znane omejitve

- preskus se med vzporedno vožnjo izpusti, ker si več preskusnih primerov deli isto datoteko. Prehaja, ko teče v izolaciji.

## Nova struktura datotek

### Storitve v

- — Pipeline Orkester
- — Prevod imena države
- — Sinhronizacija slovarja JSON
- – Markdown prevod
- — Signal R objava sporočil
- – Ponovno preizkusite logiko z maskiranjem imetnika
- — Založniški vmesnik
- — Vmesnik državnih storitev
- — Vmesnik storitev lokalizacije
- — Vmesnik storitev dokumentov
- — Orkesterski vmesnik (posodobljen)
- — Metapodatki o prevajanju po datoteki

### Posodobljene storitve

- – Dodano imenovano podporo za imetnike
- — Posodobljen za nov parameter
- — Imenovano upravljanje s sedežem
- — Placeholder vmesnik

### Nova stran za skrbnike

- — Stran za spremljanje v realnem času
- — Model strani

### Nova dokumentacija v

- — Posodobljena dokumentacija o cevovodih
- — Vodnik po sistemu imetnikov
- — Vodnik za uporabo armature
- — Pregled tehnične arhitekture

## Združljivost nazaj

Vse spremembe so aditivne:

- Obstoječa lokacijska koda () dela nespremenjena
- Formatiranje položaja () dela nespremenjeno
- Obstoječa oblika slovarja JSON je nespremenjena
- Obstoječa struktura Markdown je nespremenjena
- Signal Sporočila R uporabljajo isti format

## Migracijska pot

Selitev ni potrebna. Refaktoriranje je notranje:

1. Stara je ohranjena kot referenca, nato pa nadomeščena
2. Registracije DI so bile posodobljene za uporabo novih vmesnikov
3. Vsi obstoječi potrošniki ne vidijo sprememb

## Izboljšanje učinkovitosti

- **Zmanjšana uporaba pomnilnika**: Datoteke shranjene na jezik takoj, namesto da bi imele vse v pomnilniku
- **Pospešek Teki**: Prevedeni so samo spremenjeni/neuspešni Markdown bloki
- ** Boljša vidljivost**: Napredek v realnem času pomaga diagnosticirati počasne faze

## Prihodnje izboljšave

Načrtovane izboljšave:

1. **AI fino uglaševanje** — Pregled prevoda po stroju za fraze > 5 besed
2. **Admin overovitev** – Omeji admin strani pooblaščenim uporabnikom
3. **Dictionary editor** — Web UI za upravljanje ključev lokalizacije
4. **Statistika prevajanja** – grafi, ki prikazujejo število prevodov in stopnje napak skozi čas
5. **Carinska sintaksa imetnika** – Podpora za nadomestne oblike imetnika

## Stik

Za vprašanja ali vprašanja s prevajalsko službo si oglejte podrobno dokumentacijo v imeniku vsakega modula ali se obrnite na razvojno ekipo.
