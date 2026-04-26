# Real- time fordítások

Ez a dokumentum az automatikus fordítóvezeték élő vizsgálati bemeneteként létezik.

## Mit tesz a szolgálat

A szolgáltatás fut a menetrend, és érvényesíti a fordítás szerver, konfiguráció, és a rendelkezésre álló nyelvek, mielőtt bármilyen fordítás munka kezdődik.

A validálási lépés után szinkronizálja az országneveket a csak olvasott országok katalógusából a szabványos lokalizációs JSON szótárakba. Ha az alkalmazás alapértelmezett nyelve angol, az ország bejegyzés tárolja a kulcs egyenlő érték. Ha az alapértelmezett nyelv más, az angol ország nevét először lefordítják az alapértelmezett nyelvre, és csak akkor tárolja kulcsként egyenlő érték az alapértelmezett szótárban.

Következő, a szolgáltatás összehasonlítja az aktuális alapértelmezett lokalizációs szótár a tárolt pillanatfelvétel az előző futás. Az újonnan hozzáadott bejegyzéseket csak akkor fordítják célnyelvekre, ha a kulcs még nem létezik, így a manuális fordítás megőrzi a prioritást. A törölt bejegyzéseket törölni kell az összes célszótárból, hogy az egész készlet következetes maradjon.

Végül, a szolgáltatás letapogatja a beállított dokumentáció gyökereit a Marklown fák. Minden téma mappa várhatóan tartalmaz egy forrás fájl után az alapértelmezett nyelv, mint például az en.md. A szolgáltatás kihasználja a forrásfájlt, érzékeli a változásokat, lefordítja az eltűnt vagy elavult céljelölési fájlokat, és tárolja az aktuális hashist a forrásfájl mellett. Ha a hash írása a forrásfájl mellett nem lehetséges, akkor az ideiglenes tárolásra kerül vissza.

## Hogyan halad a szolgálat jelentése

A backend általános SignalR üzeneteket bocsát ki a lokalizációs csomóponton keresztül egy üzenetborítékkal. Minden üzenet tartalmaz egy üzenettípust, az aktuális folyamatszakaszt, egy UTC időbélyegzőt, egy szöveges összefoglalót és egy opcionális szakaszspecifikus hasznos terhet.

A jelenlegi szakaszok a következők:

- ellenőrző szerverek
- Tranzakt országok
- transzlatejsonfiles
- TranslateMarkdownName
- elemzési eredmények

Tipikus üzenetáramlás indult, befejeződött, és csővezeték befejeződött. Ha egy szakasz nem működik, az üzenet hibaként van megjelölve, és egységes hibakódokkal rendelkező strukturált hibainformációkat tartalmaz.

## Tervezési elvek

A fordításokat egymás után dolgozzák fel, hogy elkerüljék a LibreTranslate szerver túlterhelését.

Lokalizáció JSON szótárak mindig tárolt ábécésorrendben válogatott kulcsok és formázott JSON könnyebb karbantartás.

Az előző alapértelmezett szótár pillanatfelvétel tárolja tartósan, így egy újraindítása az alkalmazás nem veszít változás követő.

*** Kézi fordítás mindig elsőbbséget élvez az automatikus kiegészítésekkel szemben. ***
