# Copilot Instructions

## Pokyny k projektu

- V tomto repozitáři preferují použití nejnovějších stabilních NuGet balíčků, pokud je to kompatibilní se solution.
- V tomto projektu chtějí mít konzolové logy v debug režimu blokový, víceřádkový a barevný Serilog výpis pro lepší čitelnost; JSON se používá pro ukládání do souborů a dalších perzistentních úložišť.
- Neodstraňovat nevyužité veřejné metody ve službách; zachovat veřejné API pro kompatibilitu.
- Při čištění projektu odstraňovat zejména nevyužité NuGet balíčky, nevyužité privátní metody/properties a staré dočasné soubory (např. Copilot/temp).
