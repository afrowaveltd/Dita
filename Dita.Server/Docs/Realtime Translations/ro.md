# Traduceri în timp real

Acest document există ca o intrare de testare live pentru conducta de traducere automată.

## Ce face serviciul

Serviciul rulează pe un program și validează serverul de traducere, configurarea și limbile disponibile înainte de începerea oricărei lucrări de traducere.

După etapa de validare, sincronizează numele de țară din catalogul țărilor citite în dicționarele standard de localizare JSON. În cazul în care limba implicită a aplicației este engleză, intrarea țării este stocată ca cheie egală cu valoarea. În cazul în care limba implicită este diferită, numele de țară în limba engleză este tradus pentru prima dată în limba implicită, și numai apoi stocate ca cheie este egală cu valoarea în dicționarul implicit.

Apoi, serviciul compară dicţionarul curent implicit de localizare cu instantaneuul stocat din rula anterioară. Noile intrări adăugate sunt traduse în limbile țintă numai atunci când cheia nu există deja, astfel încât traducerile manuale păstrează prioritatea. Înregistrările eliminate se elimină din toate dicționarele țintă pentru a menține întregul set consecvent.

În cele din urmă, serviciul scanează rădăcinile de documentare configurate pentru copacii Markdown. Fiecare dosar subiect este de așteptat să conțină un fișier sursă numit după limba implicită, cum ar fi en.md. Serviciul hashes că fișierul sursă, detectează modificări, traduce lipsă sau depășite fișiere țintă Markdown, și stochează hash curent lângă fișierul sursă. Dacă scrierea hash lângă fișierul sursă nu este posibilă, aceasta cade înapoi la depozitare temporară.

## Cum raportează serviciul progresele

Platforma emite mesaje de semnal general prin intermediul hubului de localizare folosind un plic de mesaj. Fiecare mesaj poartă un tip de mesaj, stadiul actual al procesului, o marcă de timp UTC, un rezumat al textului și o sarcină utilă opțională specifică etapei.

Etapele actuale sunt:

- controlere
- TraducereCounties
- Tradu Dosarele Json
- traducemarkdownfiles
- Rezultate de stocare

Fluxul tipic de mesaje este început etapa, etapa finalizată, și conducta finalizată. Dacă o etapă eşuează, mesajul este marcat ca o eroare şi include informaţii de eroare structurate cu coduri de eroare unificate.

## Principii de proiectare

Traducerile sunt prelucrate secvenţial pentru a evita supraîncărcarea serverului LibreTraduce.

Localizare dicționare JSON sunt întotdeauna stocate cu chei sortate alfabetic și formatate JSON pentru întreținere mai ușoară.

Snapshot dicționar implicit anterior este stocat persistent astfel încât o repornire a aplicației nu pierde de urmărire schimbare.

** Traducerile manuale au întotdeauna prioritate față de adăugările automate.**
