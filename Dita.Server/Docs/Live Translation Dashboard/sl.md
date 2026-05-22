# Prevodna plošča v živo

The Live Translation Dashboard je admin stran, ki zagotavlja v realnem času vidljivost v avtomatski prevajalski cevovod. Povezuje se z vozliščem SignalR in prikazuje vse plinovodne dogodke, ko se pojavijo.

## URL

`/Admin/LiveTranslation`

> Note: Authentication and authorization are not yet implemented. Future versions will restrict this page to admin users only.

## Značilnosti

### Tok dogodkov v realnem času

Vsi dogodki SignalR iz prevajalskega cevovoda so prikazani v tabeli za live updating:

- **Sequence number** – monotoni števec znotraj vsakega cevovoda
- **Časovni žig** – krajevni čas prejema dogodka
- **Zaženi ID** – Skrajšan grafični vmesnik za korelacijo
- **Stage** — Pipeline scenska značka (CheckServers, TranslateCountries, etc.)
- **Vrsta** – Značka vrste sporočila (začeta, napredek, dokončana faza itd.)
- **Sporočilo** – opis, ki ga je mogoče prebrati pri človeku
- **Podrobnosti** – Polna korist JSON podatkov o dogodku

### Barvno kodiranje

Barvno
|-------|---------|
Modra ()
Zelena ()
Rdeča ()
Bela (privzeto)

### Stanje povezave

Stanje na vrhu prikazuje:
- ** Povezovanje** – Vzpostavitev povezave SignalR
- **Povezana** – Prejemljivi dogodki
- ** Ponovna povezava** – Povezava izgubljena, poskus ponovne povezave
- **Povezana** – povezava zaključena

Povezava uporablja samodejno ponovno povezavo z eksponentno backoff: 0s, 2s, 5s, 10s, 30s.

### Nadzor

- ** Počisti vir** – odstrani vsa prikazana sporočila in ponastavi števec
- **Izvoz JSON** – Prenese vsa prejeta sporočila kot datoteko JSON za analizo
- ** Message števec** – Prikazuje skupno število dogodkov, prejetih v tej seji

## Vozlišče SignalR

Armaturna plošča je povezana z:

```javascript
const connection = new signalr.HubConnectionBuilder()
    .withUrl("/hubs/localization")
    .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
    .build();
```

### Naročilo sporočila

```typescript
interface LocalizationHubMessage {
    runId: string;        // Guid
    sequence: number;     // long
    type: LocalizationMessageType;
    stage: ProcessStage;
    timestampUtc: string; // ISO 8601
    isError: boolean;
    message: string;
    data: object | null;
}
```

### Vrsta dogodkov

Armaturna plošča upošteva vse vrednosti:

Vrsta
|------|---------|
Modra značka
Zelena značka
Rdeča značka
Zelena značka
Rdeča značka
Značka z informacijami
Opozorilna značka

## Tehnično izvajanje

### Hrbtenica

- **LokalizacijaHub** () — vozlišče SignalR, ki prenaša sporočila vsem povezanim strankam
- **ISignalRP Publisher** – Povzetek o vozlišču za uporabo v prevajalskih storitvah
- **SignalRP Publisher** – Privzeta izvedba, ki povečuje monotonsko zaporedje in oddaje

### Začelje

- Čisti HTML/JS z Bootstrap 5 styling
- Uporablja odjemalno knjižnico Microsoft SignalR JavaScript (naloženo iz CDN)
- Za vir dogodka ni potrebno upodabljanje na strani strežnika

### Struktura strani

```
Dita.Server/Pages/Admin/
├── LiveTranslation.cshtml      — Razor page markup + JavaScript
└── LiveTranslation.cshtml.cs     — Page model (empty, data comes via SignalR)
```

## Uporaba med razvojem

1. Začni Dito. Program strežnika
2. Navigacija za
3. Sprožite prevod teče ( bodisi počakajte na urnik ali pokličite API)
4. Oglejte si dogodke, ki se pojavijo v realnem času
5. Uporabite gumb Izvozi, da zajamete polno sled za razhroščevanje

## Prihodnje izboljšave

Načrtovane izboljšave armaturne plošče:

- **Potrditev** – Omejen dostop do uporabnikov z vlogo
- **Filter** – Filtriranje dogodkov po fazah, vrsti ali zažene ID
- **Zgodovinski teče** – Ogled zaključenih teče iz podatkovne zbirke ali dnevniške datoteke
- **Statistika** – Diagrami, ki prikazujejo število prevodov, stopnje napak in zakasnitev skozi čas
- **Ročni sprožilci** – gumbi za ročni zagon določenih faz cevovoda
- **Nastavitev** – Uredi neposredno z armaturne plošče
- **Upravljanje jezikov** – Ogled in urejanje podprtih jezikov
- **Dictionary predogled** — Brskanje in iskanje lokacijskih slovarjev

## Odpravljanje težav

### Dashboard prikazuje "Neuspelo za povezavo"

1. Preverjanje strežnika teče in je dostopen
2. Preverite konzolo brskalnika za CORS ali napake v omrežju
3. Potrdite je prisoten v
4. Zagotovite, da požarni zid ne blokira povezav WebSocket

### Dogodki se ne pojavijo

1. Preverite, ali se URL vozlišča SignalR ujema med strežnikom () in odjemalcem ()
2. Preverjanje razporeda je omogočeno v
3. Poglejte dnevnike strežnikov za napake v prevajalskem cevovodu
4. Preveri zavihek Omrežje brskalnika za sporočila WebSocket

### Sporočila so neustrezna

Polje zagotavlja naročanje znotraj enega teka. Če so sporočila nepravilna, lahko navede:
- Večkratni pretoki cevovodov se prekrivajo (ne bi smeli biti posledica ključavnice semaforja)
- Brskanje reproduciranja (poskus osveževanja strani)
