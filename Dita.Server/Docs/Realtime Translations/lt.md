# @ info: tooltip

Dokumentas yra kaip tiesioginė bandymo įvesties automatinio vertimo vamzdyno.

## Ką paslauga daro

PaslaugA veikia pagal tvarkaraštį ir tvirtina vertimo serverį, konfigūraciją, ir prieinamas kalbas, prieš pradedant bet kokį vertimo darbą.

For the validation step, it sinchronizuoja šalių pavadinimus iš read- only šalių katalogas į standartinį lokalizacijos JSON žodynai. NAME OF TRANSLATORS NAME OF TRANSLATORS.

NAME OF TRANSLATORS Naujai pridėti įrašai į tikslines kalbas verčiami tik tada, kai rakto dar nėra, todėl rankinis vertimas išlieka prioritetas. Pašalinti įrašai yra pašalinti iš visų tikslinių žodynų, kad visas rinkinys būtų nuoseklus.

Galų gale, tarnyba nuskaito sukonfigūruotas dokumentacijos šaknis, Markdown medžių. Kiekvienas temos aplankas turėtų turėti pradinio kodo failą, pavadintą po numatytosios kalbos, pavyzdžiui, en.md. Tarnybos hasses, kad šaltinio failą, aptinka pakeitimus, verčia trūksta arba pasenęs tikslas Markdown failus, ir saugo esamą hash šalia šaltinio failą. @ info: whatsthis.

## Tarnybos ataskaitos apie pažangą

Name @ info: tooltip.

Dabartiniai etapai:

- Kontroliniai serveriai
- Translate countries
- translatechsonfiles
- TranslateMarkdownFiles
- Santraukų rezultatai

Tipinis pranešimų srautas yra pradėtas etapas, etapas baigtas, ir vamzdynas baigtas. @ info: whatsthis.

## Konstrukcijos principai

Vertimai yra tvarkomi iš eilės, siekiant išvengti perkraustymo LibrePlayer serverį.

Lokalizacija JSON žodynai yra visada saugomos su abėcėlės rūšiuoti klavišus ir formatuotas JSON, siekiant palengvinti priežiūrą.

Ankstesnis numatytasis žodyno fotografija yra saugomi nuolat, todėl programos atnaujinimas nepraranda kaita sekimo.

*** Rankinis vertimas visada turi pirmenybę prieš automatinius papildymus. ***
