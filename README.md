
# Tower Defense Spiel 
## Gameplay-Ablauf und Tower-Interaktionen

### Spielstart und Initialisierung

Beim Spielstart befindet sich das Spiel im Zustand Preparing. In dieser Phase wird das Spielfeld (Grid) generiert und die Spielsysteme vorbereitet.

Am oberen linken Bildschirmrand sieht der Spieler folgende Informationen:

	•	Aktueller Spielstatus (Preparing, Editing, Playing, GameOver, GameWon)
	•	Verfügbares Gold des Spielers
	•	Verbleibende Lebenspunkte (Health)
	•	Aktueller Level bzw. Wave-Zähler

### Shop-Interaktion und Turmplatzierung

#### Im Zustand Preparing kann der Spieler Türme über den Shop platzieren oder bearbeiten.

	•	Durch Klick auf den Shop-Button (oben rechts) wird eine Liste aller verfügbaren Türme geöffnet.
	•	Die Liste wird zur Laufzeit aus einer JSON-Datei geladen (dynamisch).
	•	Der Spieler kann neue Türme auf das Grid setzen, sofern genügend Gold vorhanden ist.
	•	Alternativ kann ein bestehender Turm ausgewählt und bearbeitet werden (GameState: Editing).

#### Bearbeitungsmöglichkeiten im Editing-Modus:

	•	Turm verschieben: Ein bestehender Turm kann auf ein anderes Grid-Feld bewegt werden.
	•	Turm upgraden: Bei ausreichendem Gold kann der Turm verbessert werden.
	•	Turm löschen: Der ausgewählte Turm wird entfernt.
	•	Bearbeitung abbrechen: Der Editiermodus wird beendet, ohne Änderungen vorzunehmen.

#### Spielgeschwindigkeit und Wellensteuerung

Rechts oben befinden sich zwei weitere Buttons:

	•	Tempo-Button: Ändert die Spielgeschwindigkeit zyklisch (1x, 2x, 4x, 8x). Intern wird dies über Bitshift (_current <<= 1) umgesetzt.
	•	Anfangen-Button: Startet die aktuelle Welle. Dadurch:
	•	Wechselt der Spielzustand von Preparing zu Playing.
	•	Der Shop und anfangen werden automatisch per UI-Binding ausgeblendet.
	•	Die Gegner-Spawns beginnen entsprechend der aktiven Wellenkonfiguration.

### Gegner-Logik und Bewegung

Beim Start einer Welle spawnen Gegner gemäß der aktiven WaveConfig (ScriptableObject-basierte Definitionen).

Es gibt drei Gegnertypen:

	•	Vogel: Ein fliegender Gegner, der das Grid ignoriert und sich direkt zum Ziel bewegt.
	•	Hund und Katze: Bodenbasierte Gegner, die den berechneten Pfad über das Grid entlanglaufen.

Jeder Gegnertyp verwendet eine eigene Controller-Klasse mit spezifischem Verhalten (z. B. Flugbewegung oder Bodenpfad).

Sieg- und Niederlagenbedingungen

Das Spiel kann auf zwei Arten enden:

	•	Sieg (GameWon): Wenn alle Waves abgeschlossen sind (laut WaveDefinitions), wird der Erfolgspanel eingeblendet (UI-Binding).
	•	Niederlage (GameOver): Wenn die Lebenspunkte des Spielers auf 0 fallen (z. B. durch durchgebrochene Gegner).



## Technische Dokumentation und Überblick
### Dieses Projekt ist ein hoch performantes und modulares Tower Defense Spiel, entwickelt mit Unity. Der Fokus liegt auf einer klar strukturierten Architektur, einer effizienten Speicherverwaltung sowie einer robusten und schnellen UI-Bindung mithilfe eines eigenen Binding-Frameworks.

#### Architektur und Systemüberblick
#### 1. GameManager:

Der GameManager ist ein Singleton, das für die zentrale Steuerung des Spiels zuständig ist. Er verwaltet die Szenen, Initialisierungen und globale Einstellungen (z.B. Framerate, API-Simulationen, Touch-Unterstützung).
Lifecycle:
Wird beim Spielstart initialisiert (Awake).
Setzt den Frame Rate-Limiter und initialisiert API und Input-System.
#### 2. BaseScene :
Stellt grundlegende Methoden (Awake, Start, OnEnable, OnDisable, OnDestroy) zur Verfügung, die von spezifischen Szenen implementiert werden können.
#### 2.1 GameScene:
Erbt von BaseScene und implementiert spielrelevante Initialisierungen wie Grid-Setup, Gegner- und Projektil-Pooling sowie Player- und UI-Steuerung.
#### 3. GridManager:
Verwaltung des Spielfelds (Grid), inklusive Zellen-Erzeugung, Wegfindung (Pathfinding) und Zellenzustände (belegt, frei, Pfad).
Besondere Features:
Grid-Erstellung und effizientes Pathfinding.
Optimierte Konvertierung von Weltkoordinaten in Grid-Koordinaten.
#### 4. GridInputHandlerAufgabe:
Verarbeitet Nutzereingaben auf dem Spielfeld. Ermöglicht das Platzieren, Verschieben und Löschen von Türmen.
##### Workflow:
Raycasts werden genutzt, um Positionen zu bestimmen.
Türme werden in Echtzeit auf valide Positionen geprüft und visuell dargestellt.
Validierungen und UI-Feedback bei Interaktionen.

#### 5. TowerPrefabGenerator:
Automatische Erstellung von Turm-Prefabs basierend auf TowerModels. Platziert automatisch Visualisierungs-Cubes für die Footprint-Darstellung der Türme.

Nutzen:

	•	Schnelle Generierung visueller Repräsentationen von Türmen.
	•	Einheitliches Management von Turm-Instanzen.
#### 6. Pooling-System (EnemyPool & BulletPool):
Effiziente Wiederverwendung von Gegner- und Geschossobjekten.

Vorteile:

	•	Reduzierter Speicherverbrauch und Garbage Collection.
	•	Verbesserung der Spielperformance bei hoher Gegner- und Schussrate.
#### 7. EventManager:
Implementiert eine Event-gesteuerte Architektur. Ermöglicht eine lose Kopplung der Komponenten über Ereignisse (GameState-Änderungen, Spieleraktionen).

Nutzen:

	•	Zentrale und übersichtliche Kommunikation zwischen Komponenten.
	•	Einfaches Debugging und Erweiterbarkeit.

# Dokumentation: Binding-Komponenten (UI-Binding System)

#### Überblick

Das UI-Binding-System besteht aus leistungsstarken Komponenten, die eine automatische Aktualisierung der Benutzeroberfläche basierend auf Änderungen in Datenmodellen ermöglichen. Diese Komponenten verwenden den MVVM-Ansatz (Model-View-ViewModel), um UI-Elemente direkt und effizient an Datenquellen (Bindable<T>) zu binden. Dadurch entfällt der Bedarf an manuellem UI-Code und erhöht die Übersichtlichkeit sowie Wartbarkeit des Projekts.
Kernkomponenten
## 1. Bindable<T>

Diese Klasse repräsentiert einen Wert, dessen Änderungen automatisch an alle gebundenen UI-Komponenten gemeldet werden.
#### Vorteile:

	•	Automatische Benachrichtigung der gebundenen Komponenten bei Wertänderungen.
	•	Einfach zu nutzen und klar strukturiert.

## 2. UIBinding (Basisklasse)

Abstrakte Klasse, von der alle spezifischen UI-Bindings erben. Sie bietet grundlegende Mechanismen zur Datenbindung, Ereignisregistrierung und Aktualisierung der UI.

## 3. BindingContextRegistry

BindingContextRegistry ist eine extrem kompakte und hochperformante Registry zur Verwaltung von sogenannten „Binding Contexts“ (IBindingContext) im TowerDefense-Projekt. Diese Registry stellt eine zentrale Sammelstelle dar, über welche UI-Bindings schnell und speichereffizient auf relevante Datenkontexte zugreifen können.


#### Warum Binding Context Registry?

Im Spiel müssen verschiedene UI-Elemente oft auf dieselben Datensätze (z. B. Spielerstatus, Spielstand, Menüzustände) zugreifen. Um diesen Zugriff maximal performant und mit minimaler Laufzeitbelastung zu gestalten, wurde eine zentrale Registry eingeführt, die folgende Eigenschaften besitzt:

	•	O(1)-Zugriffszeit: Dank interner Verwendung eines Dictionaries erfolgt der Abruf jedes Context-Objekts in konstanter Zeit.
	•	Zero-Allocation: Die Registry vermeidet unnötige Speicher-Allokationen durch Vorabreservierung der internen Dictionary-Kapazität.
	•	Inline-Optimierung: Alle Methoden sind mit AggressiveInlining markiert, um sicherzustellen, dass der Compiler diese in kritischen Pfaden direkt inline integriert.

### Code-Optimierungen
#### Vorab reservierte Kapazität (7):
Das interne Dictionary wurde mit einer Anfangskapazität von 7 initialisiert, um bei einer überschaubaren Anzahl von Context-Objekten (MenuScene, PlayerManager, GameScene usw.) eine sofortige und konstante Zugriffszeit zu gewährleisten.

####	Zero-Allocation:
Durch Vermeidung temporärer Objekte (z. B. kein Boxing, keine temporären Listen oder Arrays) arbeitet diese Registry vollkommen GC-neutral und belastet die Garbage Collection somit nicht zusätzlich.


## 4. TextBinder

Bindet einen oder mehrere Werte automatisch an eine TextMeshProUGUI-Komponente.
Core-Vorteile des TextBinder-Systems

#### Automatische UI-Aktualisierung:
UI-Elemente reagieren automatisch und in Echtzeit auf Datenänderungen, sodass manueller UI-Update-Code entfällt. Das spart Entwicklungszeit und reduziert potenzielle Fehlerquellen.

#### Höchste Performance & Minimaler Overhead:
Durch vorab berechnete Formatierungen und aggressive Optimierungen (wie z.B. MethodImplOptions.AggressiveInlining) arbeitet das System extrem effizient. Dies garantiert eine flüssige UI-Darstellung auch in komplexen Szenarien mit zahlreichen Werten und schnellen Updates.

#### Flexible Formatierung (K/M/B):
Große Zahlenwerte werden automatisch kompakt und übersichtlich formatiert. Der Nutzer sieht sofort verständliche Werte wie “1,2K”, “3,4M” oder “5,6B”.

#### Einfache Nutzung & Wartung:
Änderungen der Datenquelle (Bindables) erfordern keinerlei Anpassungen im UI-Code. So können UI-Designer unabhängig von Backend-Entwicklern arbeiten. Durch die klar definierten Bindings sind Änderungen leicht nachvollziehbar und wartbar.

## 5. UIBoolBinding
#### Technische Dokumentation:

Die UIBoolBinding-Komponente ermöglicht eine effiziente, zustandsabhängige Steuerung der Benutzeroberfläche (UI), indem sie zur Laufzeit komplexe boolesche Ausdrücke evaluiert und daraufhin vordefinierte UnityEvents auslöst.

Diese Komponente ist speziell dafür ausgelegt, auf eine Vielzahl unterschiedlicher Bedingungen zu reagieren, die durch dynamisch veränderliche Werte repräsentiert werden.



### Funktionsweise

#### 1. Ausdrucksanalyse und Parsing

Die Komponente nimmt als Eingabe eine Zeichenkette (Expression) entgegen. Diese Expression kann einfache oder verschachtelte logische Bedingungen enthalten, beispielsweise:

	•	Einfache Boolesche Werte:
	•	GameState.IsActive
	•	Numerische Vergleiche:
	•	Player.Gold > 100
	•	Vergleiche zwischen Eigenschaften:
	•	Player.Health <= Player.MaxHealth
	•	Logische Verknüpfungen:
	•	GameState.IsPaused || Player.IsDead
	•	Player.Level > 10 && Player.Gold >= 500
	•	Verschachtelte und komplexe Bedingungen:
	•	(Player.Gold > Player.Bet) && (GameState.Round > GameState.MaxRounds)

Die Expression wird zur Laufzeit einmalig geparst und in einen effizienten internen Strukturbaum umgewandelt (ExpressionTree). Dies geschieht mittels eines rekursiven Parser-Algorithmus, der Klammern, logische Operatoren (&&, ||) sowie Vergleichsoperatoren (>, <, >=, <=, ==, !=) korrekt erkennt und verarbeitet.

#### 2. Automatische Binding-Verwaltung

Für jede Eigenschaft innerhalb der Expression wird automatisch ein Binding erstellt. Dazu verwendet die Komponente Reflection, um die notwendigen Eigenschaften und Events aus den gebundenen Datenobjekten (Bindable<T>) auszulesen.

Sobald sich einer der gebundenen Werte ändert, wird die Expression unmittelbar neu ausgewertet.

#### 3. Evaluierung und Ereignissteuerung

Die Evaluation des internen Expression-Baums erfolgt bei jeder Änderung eines gebundenen Wertes. Je nach Ergebnis (wahr oder falsch) werden anschließend automatisch entsprechende UnityEvents ausgelöst:

	•	_onTrue: Wird ausgelöst, wenn die Expression zu true evaluiert.
	•	_onFalse: Wird ausgelöst, wenn die Expression zu false evaluiert.

Damit lassen sich beispielsweise UI-Elemente dynamisch ein- und ausblenden oder andere Gameplay-bezogene Events triggern.

Effizienz und Performance

	•	Interne Nutzung eines Expression-Baums sorgt dafür, dass nach einmaligem Parsing keine weitere String-Analyse nötig ist.
	•	Reflektionsoperationen werden gecacht, sodass die Zugriffe auf Eigenschaften und Events sehr schnell erfolgen.
	•	Verwendung eines Array-Pools reduziert die Memory-Allocation während Updates auf ein Minimum.

## Third Party Packages

Dieses Projekt verwendet folgende externe Pakete:
- **PrimeTween** – Für performantes Tweening und Animationen

## Authors

- [@halilmentes](https://github.com/hal0000)

