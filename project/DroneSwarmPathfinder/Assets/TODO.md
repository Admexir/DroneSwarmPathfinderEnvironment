# TODO List: Simulační prostředí pro roje dronů

## 1. Inicializace projektu a architektury

* **1.1. Vytvoření Unity projektu**
* Založit 3D Core projekt
* Nastavit verzovací systém (Git, `.gitignore` pro Unity)

* **1.2. Základní 3D scéna**
* Vytvořit ohraničený prostor (např vizuální reprezentace mřížky 100x100x100)
* Nastavit základní nasvícení



## 2. Jádro (Core) – Datové struktury a rozhraní

Tato část musí být zcela nezávislá na `UnityEngine`, aby šla snadno testovat a používat v externích knihovnách

* **2.1. Datové modely prostoru a pozice**
* Vytvořit strukturu `Vector3D` (nebo použít System.Numerics.Vector3), aby nebyla závislost na `UnityEngine.Vector3`
* Vytvořit třídu pro reprezentaci Mřížky (Grid / Voxel space)


* **2.2. Datový model Agenta (Drona)**
* Definovat třídu/strukturu s vlastnostmi: `ID` (unikátní), `Position` (výchozí/cílová), `Color`/`Team` (pro rozlišení skupin)
* Implementovat logiku pro rozlišitelné vs. nerozlišitelné agenty (např boolean flag nebo dědičnost)


* **2.3. Definice rozhraní `IPathfinder` (API)**
* Navrhnout metodu, která přijme počáteční stav a cílový stav a vrátí vypočítané trasy
* *Návrh signatury:* `Task<SimulationResult> CalculatePathsAsync(SimulationContext context);`


* **2.4. Datový model výsledku (Trasy)**
* Vytvořit strukturu pro uložení série pozic (a času/kroku) pro každého drona



## 3. Unity Vizualizace a Ovládání

* **3.1. Kamera (Flycam)**
* Napsat skript pro volný pohyb kamery ve 3D prostoru (WASD pohyb, myš pro rotaci, posun nahoru/dolů)


* **3.2. Reprezentace Drona ve scéně**
* Vytvořit 3D model/Prefab drona (stačí jednoduchý mesh, např kapsle nebo stažený low-poly model)
* Napsat skript, který z `Core` dat drona aplikuje barvu na materiál v Unity


* **3.3. Pohybový kontroler dronů**
* Napsat skript pro interpolaci pohybu (Lerp) mezi dvěma body mřížky, aby vizualizace byla plynulá



## 4. Vizuální editor scény (Sandbox)

* **4.1. Uživatelské rozhraní (UI Canvas)**
* Vytvořit hlavní panel nástrojů (Tlačítka: Přidat drona, Odebrat drona)
* Vytvořit panel vlastností (Změna barvy, ID, skupiny aktuálně vybraného drona)


* **4.2. Interakce ve 3D prostoru**
* Implementovat `Raycasting` z kamery na kurzor myši pro výběr drona
* Implementovat "Drag & Drop" (přesouvání) vybraného drona v mřížce



## 5. Serializace a Deserializace (Ukládání stavů)

* **5.1. Ukládání (Export)**
* Implementovat mapování aktuálního stavu scény do datových struktur `Core`
* Vytvořit funkci pro uložení do JSON na lokální disk


* **5.2. Načítání (Import)**
* Vytvořit funkci pro přečtení JSON souboru
* Napsat logiku pro vymazání aktuální scény a instanciování dronů podle dat z JSON


* **5.3. Napojení na UI**
* Tlačítka "Load Cfg 1", "Load Cfg 2", "Save Cfg"
* Otevření systémového dialogu pro výběr souboru (např přes `StandaloneFileBrowser` nebo podobnou knihovnu)



> ### 💡 Možnosti implementace: Serializace do JSON
> 
> 
> * **Možnost A: Unity `JsonUtility**`
> * *Výhody:* Extrémně rychlý, vestavěný v Unity, žádné externí závislosti
> * *Nevýhody:* Nepodporuje složitější struktury (Dictionary, polymorfismus), umí serializovat jen objekty dědící z `MonoBehaviour` nebo tagované `[Serializable]`
> 
> 
> * **Možnost B: `Newtonsoft.Json` (Json.NET) - DOPORUČENO**
> * *Výhody:* Průmyslový standard, plná podpora polymorfismu (důležité pro budoucí extensibilní vlastnosti agentů), snadná ignorace nepotřebných dat
> * *Nevýhody:* Nutné přidat jako balíček přes Unity Package Manager, nepatrně pomalejší (pro tento projekt zanedbatelné)
> 
> 
> 
> 

## 6. Napojení Pathfinding Algoritmů a Simulátor

* **6.1. Správce simulace (Simulation Manager)**
* UI prvky: Play, Pause, Stop, Posuvník rychlosti, Tlačítko pro krokování (Step)
* Logika běhu času v simulaci (přepínání stavů Idle -> Computing -> Playing -> Paused)


* **6.2. Triviální demo algoritmus**
* Implementovat `IPathfinder` – např algoritmus, který pošle drony přímo k cíli a ignoruje kolize, jen pro ověření datového toku


* **6.3. Asynchronní spuštění výpočtu**
* Napojit vybraný algoritmus na tlačítko "Simulovat"
* Zajistit, aby výpočet nezasekl vizualizaci (Unity musí dál běžet plynule)



> ### 💡 Možnosti implementace: Integrace externích algoritmů
> 
> 
> * **Možnost A: Přes .DLL (System.Reflection)**
> * *Výhody:* Splňuje definici "externí" do puntíku. Algoritmus je kompilován jinde, Unity načte DLL za běhu. Maximální oddělení
> * *Nevýhody:* Těžší na ladění (debugování chyb v algoritmu uvnitř Unity je složité)
> 
> 
> * **Možnost B: Unity Assembly Definitions (.asmdef) - DOPORUČENO (pro začátek)**
> * *Výhody:* Skripty algoritmů jsou přímo v projektu, ale striktně kompilované do oddělené knihovny. Brání to "špagetovému kódu", ale zachovává plný komfort Unity debuggeru
> * *Nevýhody:* Vyžaduje základní znalost toho, jak Unity kompiluje kód
> 
> 
> 
> 

> ### 💡 Možnosti implementace: Asynchronní výpočet
> 
> 
> * **Možnost A: Unity Coroutines (`IEnumerator`)**
> * *Výhody:* Snadné na naučení, bezpečné pro Unity API (běží na hlavním vlákně)
> * *Nevýhody:* Těžké matematické výpočty (jako A* pro 50 dronů) způsobí zásek (frame drop), protože reálně to **není** multithreading
> 
> 
> * **Možnost B: C# `Task` (TPL - Task Parallel Library) - DOPORUČENO**
> * *Výhody:* Skutečný multithreading. Výpočet běží na jádrech CPU na pozadí, vizualizace poběží stále na 60+ FPS
> * *Nevýhody:* Nelze z vlákna na pozadí volat Unity API (např `Transform.position`). Výpočet musí probíhat čistě na datech z `Core` (což specifikace stejně vyžaduje, takže je to ideální řešení)
> 
> 
> 
> 

## 7. Optimalizace a testování

* **7.1. Zátěžový test**
* Vytvořit testovací scénu s 50 drony v mřížce 100x100x100
* Změřit FPS (cíl je stabilních 30+)


* **7.2. Fyzikální interakce (Unity Physics)**
* Přidat dronům komponenty `Rigidbody` (Kinematic = true během pathfindingu, pokud je řídíme striktně skriptem, nebo nastavit fyzikální detekce kolizí, pokud algoritmus selže)
* Implementovat logiku pro detekci "srážky" dronů (pokud algoritmus vymyslel špatnou cestu, scéna to musí vizuálně ohlásit - např drony zčervenají a spadnou)


* **7.3. Úprava pro build**
* Nastavit parametry kompilace (Windows 10/11 x64)
* Vytvořit testovací build a ověřit fungování ukládání/načítání mimo editor



## 8. Dokumentace

* **8.1. Vývojová dokumentace (API)**
* Zdokumentovat rozhraní `IPathfinder`, `SimulationContext` a strukturu JSON souboru
* Vytvořit krátký návod: "Jak přidat vlastní algoritmus do simulátoru"


* **8.2. Uživatelská dokumentace**
* Návod na ovládání kamery, editoru a simulace



## 9. Rozšíření (Bonus / Příprava na BP)

* **9.1. Přechod na spojitý prostor**
* Refaktorovat datové modely tak, aby pozice nebyla vázána na celá čísla (int), ale mohla přijímat přesné `float` hodnoty pro libovolný úhel a pozici


* **9.2. Fyzikální omezení agentů**
* Do `Core` přidat datovou strukturu pro omezení (max. rychlost, akcelerace)
* Upravit simulační smyčku, aby tyto limity zohledňovala (pokud algoritmus navrhne nemožný pohyb, vizualizace to odmítne vykonat)