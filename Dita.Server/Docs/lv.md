# Automātiskās tulkošanas dienesta izmaiņu kopsavilkums

## Pārskats

Šajā dokumentā apkopotas visas izmaiņas, kas veiktas Dita automātiskās tulkošanas dienestā, ieskaitot arhitektūras refaktūru, jaunas funkcijas, novērošanas uzlabojumus un lokalizācijas uzlabojumus.

## Arhitektūras izmaiņas

### Pārstrādāts aizmugures tulkošanas pakalpojums

Monolīts ir sadalīts četros specializētos pakalpojumos, kurus koordinē vieglais orķestrators:

- **Atpakaļtulkošanas pakalpojums** – Cauruļvadu orķestris (servera apstiprināšana, estrādes deleģēšana, kļūdu apstrāde)
- **CountriesTranslationService** – Valodu nosaukumu sinhronizācija (angļu → mērķvaloda)
- **LocalizationTranslationService** — JSON vārdnīcas sinhronizācija (pievienotie/atceltie taustiņi)
- **DokumentiTulkošanas pakalpojums** – Novilkumu dokumentēšanas tulkojums ar bloka līmeņa izsekošanu
- **SignalRPublizer** – reāllaika progresa ziņojumi, izmantojot SignalR
- **TulkojumsRetryService** – Stage-level retritting with placeholder seservation

### Ieguvumi

- ** Bažu nošķiršana**: Katrs pakalpojums apstrādā vienu tulkošanas domēnu
- ** Noturība**: Mazākas klases ir vieglāk saprast un pārbaudīt
- **Terminalitāte**: Jaunus tulkošanas mērķus var pievienot, izmantojot saskarnes īstenošanu
- **Uzticamība**: Neatkarīgi pakalpojumi nodrošina labāku defektu izolāciju

## Jaunas iespējas

### Tulkošanas monitors

**Atrašanās vieta**:

Jauna admin lapa, kas nodrošina reāllaika redzamību tulkošanas cauruļvadā:

- Rāda visu signālu R notikumi, kad tie notiek
- Krāsu kodificēti ziņojumu tipi (zils=startēts, zaļš=pabeigts, sarkans=kļūds)
- Savienojuma statusa baneris ar automātisko savienojumu
- Ziņu skaitītājs un eksports uz JSON

### Nosaukti vietturi

Lokalizācijas sistēma tagad atbalsta nosauktos vietturus (), lai uzlabotu gramatiku dažādās valodās:

```csharp
// Usage in code
var message = Localizer["WelcomeMessage", new Dictionary<string, string>
{
    ["userName"] = "John",
    ["count"] = "5"
}];
// Result: "Hello John, you have 5 new messages"
```

Iezīmes:
- Vietas turētāja vērtības, kas sniegtas darba laikā vai uzglabātas
- Automātiska maskēšana/atjaunošana tulkošanas laikā, lai novērstu korupciju
- Atpakaļ savietojams ar esošajiem vietas turētājiem

### Iztulkošana

Atzīmēšanas faili tiek tulkoti pakāpeniski:

- **Ietaupījums uz vienu valodu**: Katra mērķa valoda tiek saglabāta uzreiz pēc tulkošanas, samazinot atmiņas spiedienu
- **Bloklīmeņa izsekošana**: celiņi tulkošanas statusu par bloku
- ** Selektīva pārskatīšana**: Nākamās palaišanas laikā atkārtoti tiek tulkoti tikai neveiksmīgie bloki
- **Metadatu noturība**: Tulkošanas stāvoklis pārdzīvo programmu pārstartēšanu

### Uzlabota atkārtošana

Trīs izturētspējas līmeņi:

1. **HTTP atkārtojums** (LibreTranslateService): 5 mēģinājumi ar eksponenciālu dublējumu (1s–5s)
2. **Stage retritry** (TulkojumsRestryService): 3 papildu mēģinājumi ar 30s aizkavēšanos
3. ** Block retritry** (Dokumentu tulkošanas dienests): Neizdevās novilkt blokus, kas tika retridetēti pēc nākamās palaišanas

### SignāluR ziņošana

Reālā laika progresa ziņojumi par visām cauruļvadu operācijām:

- Katrā posmā publicē notikumus
- Progress katrā valodā, kas publicēts kā notikums
- Kļūdas notikumi ietver detalizētu kontekstu (avots, kļūdas kods, ziņojums)
- Kārtas numuri garantē pasūtījumu katrā braucienā

## Konfigurācijas izmaiņas

### appsetings.json

Bez pārrāvuma izmaiņām. Esošā konfigurācija turpina darboties:

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

### Jauni pakalpojumi

Reģistrēta:

- /
- `TranslationRetryService`
- /
- /
- /
- /

Signāls R mezgls tiek kartēts klientu pieslēgumiem.

## Testēšana

### Testa stāvoklis

- **243/244 testi iziet** (1 izlaists sakarā ar vienlaicīgu piekļuvi failiem testa vidē)
- Jauns testa pārklājums pievienots:
  - Vietas turētājs Pakalpojumu funkcionalitāte
  - AizmugureTulkojums Pakalpojumu organizēšana
  - JsonStringLocalizer vietturu indeksētāji

### Zināmie ierobežojumi

- tests ir izlaists, ja darbojas paralēli, jo vairākas testa instances ir kopīgs pats fails. Tas iet, kad skrien izolēti.

## Jauna faila struktūra

### Pakalpojumi

- — Cauruļvadu orķestris
- — Valsts nosaukuma tulkojums
- — JSON vārdnīca sinhronizācija
- — Marķējuma tulkošana
- — Signāls R ziņojumu izdošana
- — Mēģināt vēlreiz loģiku ar vietturu maskēšanu
- — Publicētāja saskarne
- — Valsts pakalpojumu saskarne
- — Lokalizācijas pakalpojuma saskarne
- — Dokumentu dienesta saskarne
- — Orķestra saskarne (papildināts)
- — Par failu tulkošanas metadati

### Pakalpojumu atjaunināšana

- — Pievienotais vietturu atbalsts
- — Atjaunots jaunam parametram
- — Norādīta viettura vadība
- — viettura saskarne

### Jauna administratora lapa iekš

- — Reālā laika monitoringa lapa
- — Lappušu modelis

### Dokumentācija

- — Atjaunināta cauruļvadu dokumentācija
- — viettura sistēmas rokasgrāmata
- — Dashboard lietošanas rokasgrāmata
- — Tehniskās arhitektūras pārskats

## Atpakaļgaitas savietojamība

Visas izmaiņas ir papildinošas:

- Esošais lokalizācijas kods () darbi netiek mainīti
- Pozicionālais formatējums () darbojas nemainīgs
- Esošais JSON vārdnīcas formāts nav mainīts
- Esošā iezīmēšanas struktūra nemainās
- Signāls R ziņojumi lietot to pašu formātu

## Migrācijas ceļš

Migrācija nav nepieciešama. Refaktors ir iekšējais:

1. Vecais tika saglabāts kā atsauce un pēc tam aizstāts
2. DI reģistrācijas tika atjauninātas, lai izmantotu jaunas saskarnes
3. Visi esošie patērētāji neredz nekādas izmaiņas

## Darbības uzlabojumi

- ** Reduced memory use**: Faili, kas saglabāti katrai valodai nekavējoties, nevis visu tur atmiņā
- ** Pamata pakāpe trases**: Pārtulkoti tiek tikai mainīti/neizslēgti iezīmēšanas bloki
- **Labāka redzamība**: Reālā laika progress palīdz diagnosticēt lēnas stadijas

## Turpmākie uzlabojumi

Plānotie uzlabojumi:

1. **AI precizēšana** — frāžu post-mašīntulkošana > 5 vārdi
2. **Admin autentifikācija** – Ierobežojiet admin lapas pilnvarotiem lietotājiem
3. **Dictionary redaktor** – Web UI lokalizācijas atslēgu pārvaldībai
4. **Tulkojumu statistika** – Tabulas, kurās redzams tulkojumu skaits un kļūdu īpatsvars laika gaitā
5. **Atvieglota viettura sintakse** – Atbalsts aizvietojošiem viettura formātiem

## Kontaktinformācija

Jautājumus vai problēmas ar tulkošanas pakalpojumu, lūdzu, skatiet detalizētu dokumentāciju katra moduļa direktorijā vai sazināties ar izstrādes komandu.
