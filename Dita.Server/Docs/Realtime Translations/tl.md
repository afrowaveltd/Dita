# Mga salin sa Real-time

Ang dokumentong ito ay umiiral bilang isang buháy na test input para sa awtomatikong transaksyon na tubo.

## Kung ano ang ginagawa ng paglilingkod

Ang serbisyo ay nasa iskedyul at nagpapatunay sa pagsasaling server, pagsasaayos, at makukuhang mga wika bago magsimula ang anumang gawaing pagsasalin.

Pagkatapos ng hakbang na eksistensiya, pinagtutugma nito ang mga pangalan ng bansa mula sa talahuluganang-lamang na mga bansa sa pamantayang lokalisasyong JSON na mga diksiyunaryo. Kung ang application default language ay Ingles, ang entry ng bansa ay iniimbak bilang key equals value. Kung ang default language ay naiiba, ang pangalan ng lalawigan sa Ingles ay unang isinasalin sa default language, at sa gayon lamang iniimbak bilang susi na katumbas ng halaga sa distributor ng default.

Pagkatapos, inihahambing ng serbisyo ang kasalukuyang default localization dictionary sa nakaimbak na litrato mula sa naunang run. Ang bagong idinagdag na mga sulat ay isinasalin lamang sa mga wikang puntirya kapag hindi pa umiiral ang susi, kaya ang mga salin ng manwal ang inuuna. Ang inalis na mga sulat ay inaalis sa lahat ng tinatarget na mga diksiyunaryo upang panatilihing hindi nagbabago ang buong set.

Sa wakas, ang service scan ay nagsasaayos ng mga dokumento para sa mga puno ng Markdown. Ang bawat folder ng paksa ay inaasahang naglalaman ng source file na ipinangalan sa default language, tulad ng en.md. Ang service hashes na nagpo-host ng source file, nakadetek ng mga pagbabago, nagsasalin ng nawawala o laos na target Markdown files, at nag-iimbak ng kasalukuyang hash sa tabi ng source file. Kung ang pagsulat ng hash sa tabi ng source file ay hindi posible, ito ay bumabagsak pabalik sa pansamantalang imbakan.

## Kung paano sumusulong ang mga ulat sa paglilingkod

Ang backend ay naglalabas ng pangkalahatang mga mensahe ng SignalR sa pamamagitan ng lokalisasyong sentro na ginagamit ang isang sobre ng mensahe. Ang bawat mensahe ay nagdadala ng isang uri ng mensahe, ang kasalukuyang yugto ng proseso, isang UTC timestamp, isang buod ng teksto, at opsyonal na stage-specific payload.

Ang kasalukuyang mga yugto ay:

- Mga Tagapagsuri
- Mga Koleksiyon sa Pagsasalin
- Mga TranslateJsonFile
- Translate MartdownFiles
- Mga Pag - urong

Karaniwan nang nagsisimula ang daloy ng mensahe, natatapos ang entablado, at natatapos ang mga tubo. Kung ang isang yugto ay nabigo, ang mensahe ay minarkahan bilang isang pagkakamali at kinabibilangan ng organisadong maling impormasyon na may nagkakaisang mga kodigo ng pagkakamali.

## Magdisenyo ng mga simulain

Ang mga salin ay prinosesong sequential upang maiwasan ang labis na pag-iwas sa LibreTranslate server.

Ang lokalisasyong mga diksiyunaryong JSON ay laging iniimbak sa pamamagitan ng mga susing inuuri ayon sa abakada at binubuo ng mga palay para sa mas madaling pagmamantini.

Ang naunang default dictionary ay patuloy na iniimbak upang ang muling paglalagay ay hindi mawalan ng pagsubaybay sa pagbabago.

** Ang mga salin sa wikang Manual ay laging may priyoridad kaysa awtomatikong pagdaragdag.***
