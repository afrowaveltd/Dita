# Prevodi v realnem času

Ta dokument obstaja kot vhodni preskus v živo za cevovod za avtomatsko prevajanje.

## Kaj stori služba

Storitev teče po urniku in potrdi prevajalski strežnik, konfiguracijo in razpoložljive jezike, preden se začne kakršno koli prevajalsko delo.

Po validacijskem koraku sinhronizira imena držav iz držav, ki samo berejo, katalog v slovarje standardne lokalizacije JSON. Če je privzeti jezik uporabe angleščina, je vnos države shranjen kot ključ enak vrednosti. Če je privzeti jezik drugačen, se ime angleške države najprej prevede v privzeti jezik in šele nato shrani kot ključ enako vrednosti v privzetem slovarju.

Nato storitev primerja trenutni privzeti slovar lokalizacije s shranjenim posnetek iz prejšnjega zagona. Novo dodani vnosi so prevedeni v ciljne jezike samo, če ključ že ne obstaja, zato imajo ročni prevodi prednost. Odstranjeni vnosi se izbrišejo iz vseh ciljnih slovarjev, da se ohrani skladnost celotnega nabora.

Končno, servisni pregledi so nastavili korenine dokumentacije za drevesa Markdown. Vsaka tematska mapa naj bi vsebovala izvorno datoteko, imenovano po privzetem jeziku, kot je en.md. Storitev hashes, da izvorna datoteka, zazna spremembe, prevaja manjkajoče ali zastarele ciljne Markdown datoteke, in shrani trenutni hash poleg izvorne datoteke. Če pisanje hašiša ob izvorni datoteki ni mogoče, pade nazaj v začasno hrambo.

## Kako služba poroča o napredku

Hrbtenica oddaja splošna sporočila SignalR preko centra za lokalizacijo z uporabo ene ovojnice sporočila. Vsako sporočilo vsebuje vrsto sporočila, trenutno fazo procesa, časovni žig UTC, povzetek besedila in neobvezen koristni tovor za posamezne faze.

Trenutne stopnje so:

- Pregledovalniki
- prevajalske države
- Prevedi datoteke Json
- PrevediDatoteke
- ShranjevanjeResults

Tipični tok sporočil se je začel, stopnja končana in cevovod končan. Če faza ne uspe, je sporočilo označeno kot napaka in vključuje strukturirane informacije o napakah z enotnimi kodami napak.

## Načela projektiranja

Prevodi se obdelujejo zaporedoma, da se izognemo preobremenitvi strežnika LibreTranslate.

Lokalizacija JSON slovarji so vedno shranjeni z abecedno razvrščenimi ključi in formatirani JSON za lažje vzdrževanje.

Prejšnja privzeta slika slovarja se trajno shrani, tako da ponovni zagon programa ne izgubi sledenja spremembam.

** Ročni prevodi imajo vedno prednost pred samodejnimi dodatki.**
