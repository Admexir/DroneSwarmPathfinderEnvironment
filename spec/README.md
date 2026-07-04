# Specification of the final project for relevant C# courses

## C# Courses selection

- [x] NPRG035 (Programming in C# language | Programování v jazyce C#)
- [x] NPRG038 (Advanced C# Programming | Pokročilé programování v jazyce C#)
- [ ] NPRG057 (Advanced .NET Programming II | Pokročilé programování pro .NET II)
- [ ] NPRG064 (Programming user interfaces in .NET | Programování uživatelských rozhraní v .NET)

## Specification

### Simulační prostředí pro navigaci rojů dronů

Cílem projektu je vytvořit robustní simulační a vizualizační prostředí
v herním enginu Unity (C#) pro simulaci pathfindingu roje dronů.
Aplikace umožňuje načítání ukládání konfigurací do JSON souborů
a poskytuje API pro napojení pathfinding algoritmů, které budou
primárním předmětem navazující bakalářské práce. 


## 1. Základní informace

### 1.1. Popis a zaměření softwarového díla
Jedná se o simulační prostředí vyvíjené v jazyce C# s využitím enginu Unity, které slouží k vizualizaci a testování algoritmů pro navigaci rojů dronů (např. kvadrokoptér). Software je primárně zaměřen na vytvoření *"pískoviště" (sandboxu)* – aktuální iterace se soustředí výhradně na funkční vizualizaci, správu stavů a přípravu API pro externí skripty. Samotné komplexní navigační algoritmy budou implementovány až v rámci navazující bakalářské práce. Cílovou skupinou jsou vývojáři 3D pathfinding algoritmů.

### 1.2. Použité technologie
* **Jazyk:** C#
* **Unity Engine:** (2022 LTS nebo novější)
* **Serializace dat:** JSON (nebo podobný datový formát)

### 1.3. Konvence tohoto dokumentu
Požadavky označené jako **[Rozšíření]** představují funkcionalitu, která bude přidána v případě dostatku času nebo při vypracovávání bakalářské práce.

---

## 2. Stručný popis softwarového díla

### 2.1. Důvod vzniku softwarového díla a jeho základní části a cíle řešení
Dílo vzniká jako příprava pro bakalářskou práci. Cílem je vyvinout nástroj k vývoji pathfinding algoritmů spolu s vizualizací a simulací v unifikovaném prostředí. Základními částmi systému jsou:
* **Jádro (Core):** Definice rozhraní pro agenty a navigační algoritmy (čistý C#, nezávislý na Unity).
* **Simulátor (Unity):** 3D scéna, která interpretuje data z Jádra a vizualizuje je.
* **Správce stavů:** Modul pro načítání, ukládání a interaktivní úpravu počátečních/cílových konfigurací roje.

### 2.2. Hlavní funkce
* Vizualizace pohybu agentů ve 3D mřížce (**[Rozšíření]** s architekturou připravenou na přechod do spojitého prostoru).
* Ukládání a načítání stavů simulace do/z lidsky čitelného formátu (JSON).
* Vizuální editor scény umožňující ve 3D prostředí přidávat/odebírat agenty, měnit jejich pozici a nastavovat jejich specifické vlastnosti.
* Zprostředkování API pro napojení externích C# algoritmů pro pathfinding.
* Správa specializovaných vlastností agentů (skupiny, barvy, fyzikální omezení,…).

### 2.3. Motivační příklad užití
Uživatel ve vizuálním editoru vytvoří cílovou konfiguraci přidáním 10 dronů na levé a pravé straně místnosti. Dronům na levé straně nastaví modrou barvu a na pravé straně červenou barvu. Stav uloží do JSON souboru a vytvoří počáteční konfiguraci tak, aby si modré a červené drony musely vyměnit pozice. Uživatel spustí aplikaci, načte JSON soubor s cílovou konfigurací a vybere ze seznamu "Triviální Demo Algoritmus". Po stisknutí tlačítka "Simulovat" aplikace spočítá trasy a plynule vizualizuje přelet dronů přes sebe ve 3D prostoru až do cílové destinace.

### 2.4. Prostředí aplikace
Aplikace bude zkompilována jako samostatný spustitelný program pro operační systém Windows 10/11, popřípadě OS X nebo Linux.

### 2.5. Omezení díla
* Aplikace v této fázi nebude obsahovat pokročilé algoritmy pro hledání cesty (pouze triviální demo ukázku).
* Fyzika bude zjednodušená (využití Unity Physics, simulace detekce rigidbody kolizí, setrvačnosti, akcelerace,… Nebude simulovat odpor vzduchu, přízemní efekt, turbulence, apod.).

### 2.6. Dokumentace díla
Kromě standardní uživatelské dokumentace (jak program ovládat) bude vytvořena *Vývojová dokumentace API*, která přesně popíše, jaké rozhraní (např. `IPathfinder`) musí budoucí algoritmus implementovat, aby mohl být načten a spuštěn v tomto vizualizačním prostředí.

---

## 3. Vnější rozhraní

### 3.1. Uživatelské rozhraní, vstupy a výstupy
UI bude implementováno pomocí Unity Canvas. Bude obsahovat:
* Tlačítka pro načtení a uložení konfigurace (vyvolá systémový dialog pro výběr souboru).
* Ovládací prvky simulace: Play, Pause, Stop, posuvník pro rychlost přehrávání a možnost krokování.
* Nástroje pro editaci scény: Výběr dronů, přesun, panel pro změnu vlastností, přidání a odebrání dronů.

**Vstupem** je základní konfigurace, ve které drony začínají, algoritmus (nebo algoritmy), který mají použít a cílová konfigurace.

**Výstupem** je vizuální 3D reprezentace letu, volitelně exportovaný JSON soubor s výslednou vypočítanou trasou a volitelně exportovaný JSON soubor se stávající konfigurací simulace.

### 3.2. Rozhraní s hardware
Aplikace nevyžaduje žádný specifický hardware. Ovládání probíhá pomocí standardní myši a klávesnice (navigace ve 3D scéně).

### 3.3. Rozhraní se software
Dílo bude komunikovat s externími `.dll` knihovnami (nebo `.cs` skripty) s navigačními algoritmy pomocí mechanismu Reflection nebo předem definovaných C# rozhraní. Tato architektura zajistí, že algoritmy bude možné vyvíjet zcela odděleně od Unity projektu.

### 3.4. Komunikační rozhraní
Aplikace využívá pouze lokální souborový systém pro čtení a zápis datových formátů (JSON).

---

## 4. Detailní popis funkcionality

### 4.1. Vizualizace a pohyb ve 3D prostoru
Jádro simulace bude pracovat na 3D mřížce, ale vnitřní datové struktury pro reprezentaci pozic a samotná implementace v Unity jsou navrženy tak, aby je bylo možné plynule a bez nutnosti přepisu vizualizačního jádra rozšířit na spojitý prostor (libovolný úhel a vzdálenost).

Uživatel se bude moci v simulačním prostoru pohybovat ve všech směrech pomocí klávesnice a myši.

### 4.2. Serializace a deserializace stavů
Systém dokáže exportovat aktuální rozložení agentů ve scéně do formátu JSON a naopak z něj scénu postavit. Tyto soubory jsou lidsky čitelné, což umožňuje uživateli vytvořit počáteční (konfigurace 1) a cílový (konfigurace 2) stav manuálně v textovém editoru (ale preferované je využití vizuálního editoru). Ukládají se pozice, orientace a unikátní ID agentů.

### 4.3. API pro externí pathfinding skripty
Základní stavební kámen pro budoucí bakalářskou práci. Vizualizace obsahuje můstek, který přijímá objekty implementující společné rozhraní. Do tohoto rozhraní Unity předá počáteční (nebo stávající) a cílový stav a očekává vrácení sady tras pro jednotlivé drony. Nyní bude implementován pouze triviální demo algoritmus.

### 4.4. Extenzibilní vlastnosti agentů
Struktura agenta bude navržena polymorfně, aby umožňovala přiřazení speciálních vlastností a skupinového chování. Podporované vlastnosti zahrnují:
* **Identifikace:** Nerozlišitelní agenti vs. rozlišitelní (např. 10 modrých dronů může letět na libovolný z 10 modrých cílů).
* **[Rozšíření] Fyzikální limity:** Omezení zrychlení, maximálního výkonu, nebo vytvoření zakázaných zón (např. agent s vysokým výkonem vytváří pod sebou turbulenci, která znemožňuje průlet jiného agenta).

---

## 5. Ostatní (mimofunkční) požadavky

### 5.1. Požadavky na výkon
Simulace a vizualizace musí běžet plynule (minimálně 30 FPS) pro roj do 50 dronů v prostoru do 100x100x100 jednotek. Cena vizualizace by měla být opominutelná v porovnání s cenou běhu navigačních algoritmů. Algoritmy pro hledání cesty mohou běžet asynchronně na pozadí, aby neblokovaly vykreslovací vlákno enginu.

### 5.2. Požadavky na rozšiřitelnost a začlenitelnost
Datové struktury gridu musí být snadno nahraditelné za spojitý prostor a způsob předávání dat mezi vizualizací a výpočetními skripty musí být oddělen rozhraním tak, aby v budoucnu nebyl nutný zásah do vizualizačního kódu při změně algoritmu.

---

## 6. Negativní vymezení

Součástí tohoto díla není:
* Vývoj samotného optimálního navigačního algoritmu pro bakalářskou práci (řeší se pouze testovací prostředí a triviální demo).
* Export dat do fyzických dronů.
* Fyzikálně přesná simulace prostředí (např. přesný odpor vzduchu nebo jiné aerodynamické jevy, počasí).
