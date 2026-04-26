# Realtempaj tradukoj

Tiu dokumento ekzistas kiel viva testenigaĵo por la aŭtomata traduko dukto.

## Kion la servo faras

La servo kuras en horaro kaj konfirmas la tradukservilon, konfiguracion, kaj haveblajn lingvojn antaŭ iu traduko laboro komenciĝas.

Post la validigpaŝo, ĝi sinkronigas landnomojn de la leg-restriktitaj landoj katalogo en la norman lokalizon JSON-vortarojn. Se la aplika defaŭlta lingvo estas angla, la landeniro estas stokita kiel ŝlosilo korespondas al valoro. Se la defaŭlta lingvo estas malsama, la angla landnomo unue estas tradukita en la defaŭltan lingvon, kaj nur tiam stokita kiel esenca egala valoro en la defaŭlta vortaro.

Venonta, la servo komparas la nunan defaŭltan lokalizovortaron kun la stokita momentpafo de la antaŭa kuro. Lastatempe aldonitaj kontribuoj estas tradukitaj en cellingvojn nur kiam la ŝlosilo ne jam ekzistas, tiel manlibrotradukoj daŭrigas prioritaton. Forigitaj kontribuoj estas forigitaj de ĉiuj celvortaroj por konservi la tutan aron kongrua.

Finfine, la servo skanas formitajn dokumentarradikojn por Markdown arboj. Ĉiu temofalinto estas atendita enhavi fontdosieron nomitan laŭ la defaŭlta lingvo, kiel ekzemple en.md. La servo havas tiun fontdosieron, detektas ŝanĝojn, tradukas maltrafadon aŭ malmodernan celon Markdown dosieroj, kaj stokas la nunan hah plej proksime al la fontdosiero. Se skribi la hash plej proksime al la fontdosiero ne estas ebla, ĝi falas reen al provizora stokado.

## Kiel la servo raportas progreson

La malantaŭa fino elsendas ĝeneralajn SignalR-mesaĝojn tra la lokaliza nabo uzanta unu mesaĝkoverton. Ĉiu mesaĝo portas mesaĝspecon, la aktualan processtadion, UTC tempstampon, tekstresumon, kaj laŭvolan scen-specifan utilan ŝarĝon.

La aktualaj stadioj estas:

- Kontrolistoj
- Tradukitaj areoj
- tradukintodosieroj
- Translate MarkdownFiles
- Storing Results

Tipa mesaĝfluo estas scenejo komenciĝis, scenejo kompletigis, kaj dukto kompletigis. Se scenejo malsukcesas, la mesaĝo estas markita kiel eraro kaj inkludas strukturitajn erarinformojn kun unuigitaj erarkodoj.

## Dezajnoprincipoj

Tradukoj estas prilaboritaj sinsekve por eviti troŝarĝadon la Libre Traduki servilo.

Lokalizo JSON-vortaroj ĉiam estas stokitaj kun alfabe ordigitaj ŝlosiloj kaj formatitaj JSON por pli facila prizorgado.

La antaŭa defaŭlta vortaro momentfoto estas stokita persiste tiel rekomenco de la aplikiĝo ne perdas ŝanĝi spuradon.

**Multaj tradukoj ĉiam havas prioritaton super aŭtomataj aldonoj**
