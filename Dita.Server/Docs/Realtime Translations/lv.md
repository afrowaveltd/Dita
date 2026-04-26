# Reālā laika tulkojumi

Šis dokuments pastāv kā reālā testa ieeja automātiskajam tulkošanas cauruļvadam.

## Ko dara dienests

Pakalpojums darbojas uz grafiku un apstiprina tulkošanas serveri, konfigurācija, un pieejamās valodas pirms jebkura tulkošanas darbu sāk.

Pēc apstiprināšanas posma tas sinhronizē valstu nosaukumus no tikai lasāmo valstu kataloga standarta lokalizācijas JSON vārdnīcās. Ja lietotnes noklusējuma valoda ir angļu valoda, valsts ieraksts tiek saglabāts kā atslēga ir vienāda ar vērtību. Ja noklusējuma valoda ir atšķirīga, angļu valsts nosaukums vispirms tiek tulkots noklusējuma valodā, un tikai pēc tam saglabāts kā atslēga vienāds vērtību noklusējuma vārdnīcā.

Nākamais, serviss salīdzina pašreizējo noklusējuma lokalizācijas vārdnīcu ar iepriekšējo palaisto momentuzņēmumu. Jaunie pievienotie ieraksti tiek tulkoti mērķa valodās tikai tad, ja atslēga jau neeksistē, tāpēc manuālie tulkojumi saglabā prioritāti. Izņemtie ieraksti tiek dzēsti no visām mērķa vārdnīcām, lai saglabātu visu iestatīto.

Visbeidzot, pakalpojums skenē konfigurēta dokumentācijas saknes iezīmēšanas kokiem. Katrai tēmas mapei ir jāsatur avota fails, kas nosaukts pēc noklusējuma valodas, piemēram, en.md. Pakalpojums hashes ka avota failu, konstatē izmaiņas, tulko trūkstošo vai novecojušu mērķa iezīmēšanas failus, un saglabā pašreizējo hash blakus avota failu. Ja hash rakstīšana blakus avota failam nav iespējama, tas atkrīt atpakaļ uz pagaidu glabāšanu.

## Kā dienests ziņo par progresu

Aizmugure izdod vispārīgus SignalR ziņojumus caur lokalizācijas centrmezglu, izmantojot vienu ziņojuma aploksni. Katrs ziņojums nes ziņojuma tipu, pašreizējo procesa posmu, UTC laika zīmogu, teksta kopsavilkumu un pēc izvēles uz konkrēto posmu attiecinātu derīgo slodzi.

Pašreizējie posmi ir šādi:

- PārbaudītServers
- TulkotCountries
- TulkotJsonFiles
- TulkotMarkdownFiles
- SaglabātRezultātus

Tipiska ziņojumu plūsma ir posms sākas, posms pabeigts, un cauruļvadu pabeigta. Ja posms neizdodas, ziņojums tiek atzīmēts kā kļūda un ietver strukturētu kļūdu informāciju ar vienotiem kļūdu kodiem.

## Projektēšanas principi

Tulkojumi tiek apstrādāti secīgi, lai izvairītos no pārslodzes LibreTranslate serveri.

Lokalizācija JSON vārdnīcas vienmēr tiek glabāti ar alfabētiski sakārtoti taustiņi un formatēts JSON vieglāku apkopi.

Iepriekšējais noklusējuma vārdnīcas momentuzņēmums tiek saglabāts pastāvīgi, lai programmas pārstartēšana nezaudētu izmaiņu izsekošanu.

** Manuālie tulkojumi vienmēr ir prioritāte pār automātiskajiem papildinājumiem.**
