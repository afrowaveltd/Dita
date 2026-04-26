# Reaaliaikaiset käännökset

Tämä asiakirja on olemassa suorana testinä automaattiselle käännösputkelle.

## Mitä palvelu tekee

Palvelu toimii aikataulussa ja validoi käännöspalvelimen, konfiguroinnin ja käytettävissä olevat kielet ennen käännöstyötä.

Validoinnin jälkeen se synkronoi maiden nimet luku-vain maiden luettelosta vakio lokalisointi JSON sanakirjoja. Jos sovelluksen oletuskieli on englanti, maamerkintä tallennetaan avaimena yhtä suuri arvo. Jos oletuskieli on erilainen, englanninkielinen maan nimi on ensin käännetty oletuskieli, ja vasta sitten tallennetaan avain vastaa arvoa oletussanakirjassa.

Seuraavaksi palvelu vertaa nykyistä oletuslokalisointi sanakirjaa edelliseltä ajolta tallennettuun tilannekuvaan. Äskettäin lisätty merkinnät käännetään kohdekielille vain silloin, kun avainta ei ole vielä olemassa, joten manuaaliset käännökset ovat etusijalla. Poistetut tietueet poistetaan kaikista kohdesanakirjoista, jotta koko sarja pysyy yhtenäisenä.

Lopuksi palvelu skannaa konfiguroituja dokumentointi juuria Markdown puita. Kunkin aihekansion odotetaan sisältävän lähdetiedoston nimetty oletuskielen, kuten en.md. Palvelu häiritsee lähdetiedostoa, havaitsee muutoksia, kääntää puuttuvat tai vanhentuneet kohde Markdown-tiedostot ja tallentaa nykyisen hash-tiedoston vieressä. Jos lähdetiedoston vieressä olevan hasan kirjoittaminen ei ole mahdollista, se palautetaan väliaikaiseen tallennustilaan.

## Miten palveluraportit edistyvät

Takaosa lähettää yleisiä SignalR-viestejä lokalisointikeskuksen kautta käyttäen yhtä viestikuorta. Jokaisessa viestissä on viestityyppi, nykyinen prosessivaihe, UTC-aikaleima, tekstitiivistelmä ja valinnainen vaihekohtainen hyötykuorma.

Nykyiset vaiheet ovat seuraavat:

- Tarkastukset
- KäännäKulutteet
- Käännä JsonFiles
- Käännä MarkdownFiles
- tulosten tallentaminen

Tyypillinen viestin virtaus on vaihe aloitettu, vaihe valmis, ja putkijohto valmis. Jos vaihe epäonnistuu, viesti on merkitty virheeksi ja sisältää jäsenneltyjä virhetietoja, joissa on yhtenäiset virhekoodit.

## Suunnitteluperiaatteet

Käännöksiä käsitellään peräkkäin LibreTranslate-palvelimen ylikuormituksen välttämiseksi.

Lokalisointi JSON sanakirjat tallennetaan aina aakkosjärjestyksessä lajiteltu avaimet ja muotoiltu JSON helpompaa huoltoa.

Edellinen oletussanakirjakuva tallennetaan pysyvästi, joten sovelluksen uudelleenkäynnistys ei menetä muutosseurantaa.

**Manuaaliset käännökset ovat aina etusijalla automaattisiin lisäyksiin nähden.**
