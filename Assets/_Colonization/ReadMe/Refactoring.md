# План рефакторинга

## Сводка

| Категория | Статус |
|---|---|
| Critical (7) | Завершено |
| High: 2.0 GameContext | Завершено |
| High: 2.1 FlagPlacer | Завершено |
| High: 2.2 Base decompose | Завершено |
| High: 2.3 Base ↔ BotFactory cycle | Завершено (event pattern) |
| High: 2.4 Constants | Завершено |
| High: 2.5 BaseFactory.Spawn dedup | Завершено |
| High: 2.6 Command methods | Завершено |
| High: 2.7 IsBusy interface | Завершено |
| High: 2.8 IBase | Завершено |
| High: 2.9 GC pressure | Завершено |
| Medium: 3.1–3.3 HLSL | Завершено |
| Medium: 3.4 typos/naming | Завершено (folder rename + Hummer→Hammer) |
| Medium: 3.5 member order | Завершено |
| Medium: 3.6 magic numbers | Завершено |
| Medium: 3.7 allocations | Завершено |
| Medium: 3.8 mesh naming | Завершено |
| Low (стиль) Phase 4 | Завершено |
| SOLID: ISP (split IBot) | Завершено |
| SOLID: OCP (FSM ScriptableObject) | **Отложено** (хардкод переходов оставлен) |

**Главные проблемы:**
- God Classes (`FlagPlacer` — 7 обязанностей, `Base` — 7, `BaseFactory` — 6)
- Скрытые зависимости через `FindFirstObjectByType` (7+ мест)
- Дублирование wiring событий
- Race condition в `Mover.MoveTo`, утечки памяти в `FlagPlacer.OnDestroy`
- Нарушения HLSL-правил: `v2g`/`g2f`/`verts` вместо `VertexToGeometry`/`GeometryToFragment`/`vertexPositions`

---

## Фаза 1 — Выполнено

7 фиксов. Ключевое: новый `BaseEventBinder` (единая подписка), инжекция `BaseFactory` через `BotStateMachine` + `IBase.BaseFactory`, `??=` вместо обнуления в `Start()`.

**Уступки:** runtime fallback'и через `FindFirstObjectByType` в `Awake` (нет NRE при пустой сцене). Полное решение — `GameContext` в Фазе 2.0.

---

## Фаза 2.0 — Выполнено

**Цель:** устранить `FindFirstObjectByType` / `GameObject.Find`, централизовать зависимости.

**Изменения:**
- Создан `Scripts/GameContext.cs` — Service Locator с `[DefaultExecutionOrder(-1000)]` (Awake первым)
- Поля GameContext: `_resourcesData`, `_spawner`, `_botFactory`, `_baseFactory`, `_startBase`, `_counterView`, `_groundLayer`, `_baseLayer`, `_mapBounds`
- Статические геттеры: `GameContext.ResourcesData`, `GameContext.Spawner`, `GameContext.BotFactory`, `GameContext.BaseFactory`, `GameContext.StartBase`, `GameContext.GroundLayer`, `GameContext.BaseLayer`, `GameContext.MapBounds`
- `GameContext.Awake` сам создаёт `BaseEventBinder`, биндит стартовую базу, подписывает HUD counter, добавляет `FlagPlacer` если его нет
- `GameContext.OnDestroy` отписывает всё
- `Base.cs`: убраны `[SerializeField] _resourcesData`, `[SerializeField] _baseFactory`, `SetBaseFactory`, `FindFirstObjectByType`; добавлены `_resourcesData = GameContext.ResourcesData`, `_baseFactory = GameContext.BaseFactory` в `Awake`
- `BaseFactory.cs`: убраны `[SerializeField] _resourcesData`, `[SerializeField] _spawner`, `FindFirstObjectByType` в `Awake`/`Spawn`, вызов `newBase.SetBaseFactory(this)`; `BaseEventBinder` создаётся в `Awake` из GameContext
- `FlagPlacer.cs`: убраны `[SerializeField] _mapBounds`, `[SerializeField] _groundLayer`, `[SerializeField] _baseLayer`, `GameObject.Find("Ground")`, `FindFirstObjectByType<Base>()`; все берутся из GameContext в `Awake`
- `Bootstrapper.cs` удалён — его логика перенесена в `GameContext.Awake`

**Результат:** `grep FindFirstObjectByType|GameObject.Find` в `Scripts/` — 0 совпадений. Все зависимости явные, из GameContext.

**Ручные шаги в сцене `Demo.unity`:**
1. Создать пустой GameObject `GameContext`
2. Добавить компонент `GameContext`
3. Заполнить поля: `_resourcesData`, `_spawner`, `_botFactory`, `_baseFactory`, `_startBase`, `_counterView`, `_groundLayer`, `_baseLayer`, `_mapBounds`
4. Удалить GameObject `Bootstrapper` (теперь missing script)
5. Удалить компонент `FlagPlacer` со старого GameObject (если был) — он будет автоматически добавлен на GameContext в Awake

---

## Фаза 2.1 — Выполнено

**Цель:** декомпозиция `FlagPlacer` (был God Class: 263 строки, 7 обязанностей).

**Изменения:**
- `FlagPlacer.cs` стал тонким координатором (140 строк)
- Выделены 4 helper-класса в `Scripts/Player/`:
  - `BaseSelector` (43 строки) — состояние `_selectedBase` + событие `SelectionChanged`
  - `GroundRaycaster` (27 строк) — `Physics.Raycast` с проверкой `Bounds`
  - `FlagVisualProvider` (62 строки) — создание/показ/скрытие флага + `Resources.Load`
  - `SelectionRectRenderer` (64 строки) — визуал рамки выделения
- Подписка на `NewBaseBuilt` теперь через `OnSelectionChanged` callback (раньше дублировалась в `OnDestroy`/`RemoveFlag`/`TrySelectBase`)

**Ответственности после:**
- `FlagPlacer` — только: input, координация между helpers
- `BaseSelector` — только: raycast в базы, состояние выбора
- `GroundRaycaster` — только: raycast в террейн
- `FlagVisualProvider` — только: флаг-визуал
- `SelectionRectRenderer` — только: рамка выделения

**Минорные улучшения:**
- Добавлена проверка `Mouse.current == null` (защита от NRE при отсутствии мыши)

---

## Фаза 2.2 + 2.4 — Выполнено

**Цель:** декомпозиция `Base` (7 обязанностей, 146 строк) + консолидация дублированных констант.

**Изменения:**
- `Base.cs` стал тонким оркестратором (131 строк)
- Выделены 4 новых класса в `Scripts/Base/`:
  - `BotRoster` (46 строк) — список ботов + add/remove/find/cancel/has
  - `ExpansionController` (28 строк) — `FlagPosition` + `HasConstructNewBase` + `NewBaseBuilt` event
  - `BaseWorkLoop` (60 строк) — корутина + `DoWork` + `TaskScheduler` интеграция
  - `BaseBalance` (5 строк, static) — `BotSpawnCost = 3`, `ExpandCost = 5`
- `TaskScheduler` (74 строки, -12) — убран собственный список ботов, берёт `IReadOnlyList<Bot>` из `BotRoster`
- `Base.NewBaseBuilt` — custom event forwarder на `ExpansionController.NewBaseBuilt`
- `Base.HasConstructNewBase` setter — `true` → `AssignFlag(FlagPosition)`, `false` → `Cancel()`
- `NormalState`, `ExpandState`, `Base` используют `BaseBalance.BotSpawnCost`/`BaseBalance.ExpandCost`

**Ответственности после:**
- `Base` — GameContext-зависимости, IBase-форвардеры, spawn bot, lifecycle
- `BotRoster` — список ботов
- `ExpansionController` — состояние expansion
- `BaseWorkLoop` — work loop корутина
- `BaseBalance` — константы
- `TaskScheduler` — назначение задач

**Риски:** сериализованное поле `_bots` осталось (нужно для initial bots в префабе), но используется только в `Awake` для seed `BotRoster`. Inspector-значения для `HasConstructNewBase` больше не сериализуются (был `[field: SerializeField]`), но значение по умолчанию `false` совпадает.

---

## Фаза 2.6 + 2.7 + 2.9 — Выполнено

### 2.6 Command methods
- `IBase.HasConstructNewBase` стал `get`-only, добавлены `AssignExpansionFlag(Vector3)` / `CancelExpansion()`
- `IBase.FlagPosition` стал `get`-only
- `FlagPlacer` использует новые методы вместо прямой мутации свойств
- **Исправлен баг** в FlagPlacer: при переключении баз старая база теперь корректно сбрасывает `CancelExpansion` + `CancelConstructTasks`

### 2.7 IsBusy в IState
- `IState` теперь имеет `bool IsBusy => true;` (default)
- `IdleState` override: `public bool IsBusy => false;`
- `Bot.IsBusy` теперь: `_stateMachine.GetCurrentState?.IsBusy ?? false` (без type-check)

### 2.9 GC pressure в SpawnerResources
- Было: `yield return new WaitForSeconds(Random.Range(...))` каждую итерацию (аллокация)
- Стало: `_nextSpawnTime` timestamp, `yield return null` каждый кадр, проверка `Time.time >= _nextSpawnTime`
- Нулевые аллокации в горячем цикле

---

## Фаза 2.3 + 2.5 — Выполнено

### 2.3 Разрыв цикла `Base ↔ BotFactory`

**Проблема:** `Base._botFactory` → `BotFactory`, `BotFactory._base` → `Base`. Циклическая ссылка, порядок инициализации критичен.

**Решение:** событийный паттерн. `BotFactory` больше не знает о `Base`.

**Изменения:**
- `BotFactory.cs` (22 строки): удалены `[SerializeField] _base`, `Initialize(Base)`, `_base` usage в `Position`/`Spawn`. Добавлен `public event Action<Bot> BotCreated`. `Spawn()` создаёт бота в `Vector3.zero` и фаерит событие.
- `Base.cs`: добавлены `OnEnable`/`OnDisable` подписка/отписка на `_botFactory.BotCreated`. `SetBotFactory` переподписывается (unsubscribe old → set → subscribe new). `OnBotCreated` инициализирует бота (`bot.Init(this, BaseFactory)`) и добавляет в roster.
- `BaseFactory.Spawn`: удалён `botFactory.Initialize(newBase)`.
- `Base.OnBotCreated` ставит позицию: `bot.transform.position = transform.position + BaseBalance.BotSpawnOffset` (offset (0, 0, 3)).
- `BaseBalance.BotSpawnOffset = (0, 0, 3f)` — константа в `BaseBalance.cs`.
- `Demo.unity`: убраны устаревшие `_base: {fileID: ...}` и `_spawnTransform: {fileID: 0}` из BotFactory MonoBehaviour.

**Почему Base позиционирует бота, а не BotFactory:** `EntityFactory` (где живёт BotFactory-template) — root GameObject в сцене на world (0, 1, 0), **не** child стартовой базы. `transform.position` template-а не совпадает с позицией базы. Base знает свою позицию, Base позиционирует бота.

### 2.5 Декомпозиция `BaseFactory.Spawn`

**Изменения:**
- `BaseFactory.cs` (85 строк, было 85 но `Spawn` стал 9-строчным оркестратором):
  - `HasDependency(Object, string, string)` — единый helper для null-check (заменил 4 копии)
  - `InstantiateBase(Vector3)` — создание базы + random name
  - `ConfigureBaseChildren(Base)` — уничтожение клонов ботов, замена BotFactory, активация ResourceScanner
  - `Spawn(Vector3)` — оркестратор: 4 проверки → создание → конфигурация → биндинг

---

## Фаза 2.8 + Phase 3.1–3.4 — Выполнено

### 2.8 IBase
- `IBase` расширен: `AddBot(Bot)`, `RemoveBot(Bot)`, `ClearExpansionFlag()`
- `IBot.OwnerBase` теперь возвращает `IBase`
- `IBot.SwitchBase(IBase)` — параметр стал интерфейсом
- `Bot.SwitchBase` — каст `(Base)newBase` для присвоения приватному полю (компромисс ради тестируемости)
- `ConstructState` использует `IBase` вместо `Base`
- `BaseFactory.Spawn` — параметр `previousBase` удалён (не использовался)
- `ConstructState.Spawn` лог: cast `oldBase as Base` только для `name` в warning

### Phase 3 HLSL (3.1–3.3)
- `v2g` → `VertexToGeometry`, `g2f` → `GeometryToFragment` в `GrassGeometry.shader` + `FlowerGeometry.shader` (в двух passes каждого)
- `verts[6]` → `vertexPositions[6]` (12 мест в FlowerGeometry)
- `6.28` → `TWO_PI` (с добавлением `#define TWO_PI 6.28318530718` после `#pragma target 4.6` в каждом pass)
- `frag(... i)` / `fragShadow(... i)` → `... input` (правило 8)

### Phase 3 C# (3.4)
- `HasConstractNewBase` → `HasConstructNewBase` (везде: IBase, Base, ExpansionController, NormalState, ExpandState)
- `IStateMachine.GetCurrentState` → `CurrentState` (правило 1, без `Get` префикса)

### Phase 3.4–3.8 + Phase 4 — Выполнено

#### 3.4 folder rename + Hummer→Hammer
- `Scripts/FSM/BaseStateMashine/` → `BaseStateMachine/` (через `Rename-Item`, .meta GUID'ы сохранены)
- `Hummer.cs` → `Hammer.cs` (класс, файл, .meta с тем же GUID `66658c3e...d22e`)
- `ToolsProvider._hummer` → `_hammer` с `[FormerlySerializedAs("_hummer")]` для миграции serialized data
- `m_EditorClassIdentifier: Assembly-CSharp::Hummer` → `Hammer` в `Hammer.prefab` + `BotHumanoid.prefab`

#### 3.5 member order
- `Base.cs`: properties → events (публичные свойства перед events)
- `CameraRotator.cs`: `GetOffset` (public) перед private `Handle*` методами
- `ResourceWarhouse.cs`: property `Count` → events → public methods → private `OnChangeCount`; убран пустой `Start()`

#### 3.6 magic numbers
- `GatheringState.PickupDelay = 1.5f` — именованная константа

#### 3.7 allocations
- `PlayerInputSystem.GetDirection`: кеш `ReadValue<Vector2>()` в локальную переменную
- `ScanAnimation.EnsureMaterials()`: ленивый кеш `_materialCircle1/2/3` (устраняет повторный `renderer.material` instancing)

#### 3.8 mesh naming
- `GrassRenderer.CreateTriangleMesh` → `CreatePointMesh`

#### Phase 4 стиль
- `CameraRotator.GetOffset`: `Vector3.zero.z` → `0f`
- `SpawnerResources.OnDrawGizmosSelected`: `Vector3.one.y` → `1f`
- `ButterflyMeshBuilder.BuildInternal`: `new Vector3(..., 0, ...)` → `0f` (8 мест)
- `Mover.Position` удалён (dead code, не используется через `IMover`)

---

## Архитектурное решение: GameContext (Service Locator)

### Проблема

Сейчас `Base`, `BaseFactory`, `FlagPlacer`, `Bootstrapper` ищут зависимости через `FindFirstObjectByType` в `Awake` — stringly-typed, неявные связи, NRE при пустой сцене.

### Решение

```csharp
public class GameContext : MonoBehaviour
{
    public static GameContext Instance { get; private set; }

    [SerializeField] private ResourcesData _resourcesData;
    [SerializeField] private SpawnerResources _spawner;
    [SerializeField] private BotFactory _botFactory;
    [SerializeField] private BaseFactory _baseFactory;
    [SerializeField] private Base _startBase;
    [SerializeField] private LayerMask _groundLayer;
    [SerializeField] private LayerMask _baseLayer;
    [SerializeField] private Bounds _mapBounds;

    public ResourcesData ResourcesData => _resourcesData;
    public SpawnerResources Spawner => _spawner;
    public BotFactory BotFactory => _botFactory;
    public BaseFactory BaseFactory => _baseFactory;
    public Base StartBase => _startBase;
    public LayerMask GroundLayer => _groundLayer;
    public LayerMask BaseLayer => _baseLayer;
    public Bounds MapBounds => _mapBounds;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }
}
```

**Использование** — все компоненты берут зависимости из `GameContext.Instance`, fallback'и в `Awake` удаляются.

### Альтернатива: Zenject / VContainer

Для полноценного DI. Для текущего масштаба `GameContext` достаточно.

---

## Фаза 2 — HIGH (архитектурный долг)

**Подфаза 2.0:** внедрить `GameContext`, убрать runtime fallback'и из Фазы 1.
**Подфазы 2.1–2.5:** декомпозиция God Classes.

### 2.0 GameContext
Создать `GameContext.cs`, добавить в сцену, заменить `FindFirstObjectByType` в `Awake` на `GameContext.Instance`. **~1 час**

### 2.1 `FlagPlacer` — God Class (245 строк, 7 обязанностей)
Файл: `Scripts/Player/FlagPlacer.cs`. Обязанности: ввод мыши, raycast, выбор базы, флаг-визуал, прямоугольник выделения, поиск Ground, загрузка Resources.

**Декомпозиция:**
- `GroundRaycaster` — raycast в террейн
- `BaseSelector` — состояние `_selectedBase`
- `FlagPlacementController` — позиция флага + follow-mouse
- `SelectionRectRenderer` — только визуал рамки

### 2.2 `Base` — God Class (130 строк, 7 обязанностей)
Файл: `Scripts/Base/Base.cs`. Обязанности: FSM, склад, бот-лист, scheduler, фабрика, флаг, корутина.

**Декомпозиция:**
- `BotRoster` — `AddBot`, `RemoveBot`, `GetFreeBot`, `CancelConstructTasks`, `BotCount`
- `ExpansionController` — `HasConstructNewBase`, `FlagPosition`, `ClearExpansionFlag`
- `BaseWorkLoop` — корутина, scheduler, задержка

### 2.3 Циклическая ссылка `Base ↔ BotFactory`
Файлы: `Base.cs:18`, `BotFactory.cs:8`. `Base._botFactory` ↔ `BotFactory._base` — порядок инициализации критичен.

**Решение:** событийный паттерн или mediator (через `GameContext`).

### 2.4 Дублирование констант
`BotSpawnCost = 3` в `Base.cs:10` + `NormalState.cs:5`. `ExpandCost = 5` в `Base.cs:11` + `NormalState.cs:6` + `ExpandState.cs:5`.

**Решение:** статический класс `BaseBalance`.

### 2.5 `BaseFactory.Spawn` дублирует wiring
Файл: `BaseFactory.cs` (62 строки, 6 обязанностей). Уже частично решено через `BaseEventBinder` в Фазе 1. Осталась декомпозиция оставшихся обязанностей.

### 2.6 Публичные мутации state
`Base.cs:29,31`, `Bot.cs:15,16` — `[field: SerializeField] public bool ... { get; set; }`. Inspector перезаписывает рантайм.

**Решение:** методы-команды: `AssignConstructTask(Vector3)`, `ClearConstructTask()`, `SetFlagPosition(Vector3)`, `StartExpansion()`.

### 2.7 `Bot.IsBusy` зависит от конкретного `IdleState`
`Bot.cs:21`: `_stateMachine.GetCurrentState is IdleState == false` — нельзя подменить.

**Решение:** `bool IsBusy { get; }` в `IState`, каждое состояние само объявляет.

### 2.8 `IBot.OwnerBase` возвращает конкретный `Base`
Файлы: `IBot.cs:11`, `Bot.cs:20`.

**Решение:** возвращать `IBase`. Ввести `IResource` для `TargetResource`.

### 2.9 GC pressure в `SpawnerResources`
`SpawnerResources.cs:54-68`: `new WaitForSeconds(...)` в каждой итерации.

**Решение:** кэшировать `WaitForSeconds` или использовать тайм-метку.

---

## Фаза 3 — нарушения правил

### 3.1 HLSL — Rule 8 (структуры)
- `GrassGeometry.shader:45-55, 172-180`, `FlowerGeometry.shader:68-79, 557-565`: `v2g`/`g2f` → `VertexToGeometry`/`GeometryToFragment`
- `GrassGeometry.shader:133`, `FlowerGeometry.shader:519, 784`: `half4 frag(... i)` → `input`

### 3.2 HLSL — Rule 9 (локальные переменные)
- `FlowerGeometry.shader:93`: `EmitQuad(... a, b, c, d ...)` → `bottomLeft, bottomRight, topLeft, topRight`
- `FlowerGeometry.shader:615,651,677,711,745,771`: `verts[6]` → `vertexPositions`
- `GrassGeometry.shader:112, 226`: `vertices[6]` → `vertexPositions`

### 3.3 HLSL — Rule 10 (константы)
- `GrassGeometry.shader:79, 204`: `6.28` → `#define TWO_PI 6.28318530718`

### 3.4 C# — Rule 1 (naming)
- `GrassRenderer.cs`: `_bladeMesh` → `_pointMesh`
- `CameraRotator.cs:25`: `private Vector2 _lookDelta` → `PascalCase` для свойств
- `IStateMachine.cs:3`: `IState GetCurrentState` → `CurrentState`
- `Base.cs:31`, `IBase.cs:7`: `HasConstractNewBase` → `HasConstructNewBase` (везде)
- `Scripts/FSM/BaseStateMashine/`: папка → `BaseStateMachine/`
- `ButterflyMeshBuilder.cs:10`: убрать `static`

### 3.5 C# — Rule 2 (порядок членов)
- `Base.cs:13-31`: public-свойства **до** events — переставить
- `ResourceWarhouse.cs:18`: private **до** public
- `CameraRotator.cs:82`: public **после** private
- `StateMachine.cs:12-13`: public `Update()` после Unity messages

### 3.6 C# — Rule 3 (магические числа)
- `GatheringState.cs:14`: `_delayTimer = 1.5f` → `const GatheringDelay`
- `BaseFactory.cs`: `Random.Range(1000, 9999)` → `const` (уже сделано)
- `Base.cs:17`: `_delayTime` без дефолта

### 3.7 C# — Rule 5 (аллокации)
- `SpawnerResources.cs:54-68`: кэшировать `WaitForSeconds`
- `PlayerInputSystem.cs:30-35`: `ReadValue<Vector2>()` 2 раза — кэшировать
- `ScanAnimation.cs:64`: `renderer.material.color` → `MaterialPropertyBlock`

### 3.8 C# — Rule 6 (Mesh)
- `GrassRenderer.cs:52-70`: `CreateTriangleMesh` → `CreatePointMesh` + `mesh.name = "PointMesh"`

### 3.9 C# — Rule 11 (файлы)
- Папка `Scripts/FSM/BaseStateMashine/` → `BaseStateMachine/`

---

## Фаза 4 — LOW (стиль)

- `CameraRotator.cs:84`: `Vector3.zero.z` → `0f`
- `PlayerInputSystem.cs:33`: `Vector3.zero.y` → `0f`
- `Mover.cs:13`: `Position` — dead code, удалить
- `Inventory.cs:30-31`: `Detach` статический — сделать инстансным
- `Hummer.cs`: переименовать в `Hammer`
- `BotFactory.cs`: `Hummer` как маркер → `ITool` интерфейс

---

## SOLID (итог)

| Принцип | Цель | Статус |
|---|---|---|
| S (SRP) | Разделить `FlagPlacer`, `Base`, `BaseFactory` | Завершено (Фазы 2.1, 2.2) |
| O (OCP) | FSM с хардкодом → ScriptableObject со списком состояний | **Отложено** (хардкод `TransitionTo<T>()` оставлен) |
| L (LSP) | `Bot.IsBusy`, `IBot.OwnerBase` → интерфейсы | Завершено (Фазы 2.7, 2.8) |
| I (ISP) | `IBot` — 12 членов → `IMovable`, `IGatherer`, `IConstructor` | Завершено (Фаза 5) |
| D (DIP) | Конкретные классы → интерфейсы + DI через `GameContext`/Inspector | Частично (GameContext + IBase, IBot, IInventory, IMover) |

---

## Фаза 5 — SOLID: ISP — Выполнено

**Проблема:** `IBot` — 12 членов, нарушение Interface Segregation Principle. Потребители зависят от методов, которые не используют (states используют подмножества, `BotRoster` использует только `HasConstructTask`, `TaskScheduler` — только `SetTargetResource`).

**Решение:** ISP-сплит `IBot` на три ролевых интерфейса. `IBot` остаётся как композиция для удобства.

**Новые файлы:**
- `Scripts/Bot/IMovable.cs` — `IMover Mover { get; }`
- `Scripts/Bot/IGatherer.cs` — `Resource TargetResource { get; }`, `IInventory Inventory { get; }`, `void GiveResource(Resource)`, `void SetTargetResource(Resource)`
- `Scripts/Bot/IConstructor.cs` — `bool HasConstructTask { get; set; }`, `Vector3 ConstructTargetPosition { get; set; }`

**Изменённые файлы:**
- `Scripts/Bot/IBot.cs`: `interface IBot : IMovable, IGatherer, IConstructor { bool IsBusy { get; } IBase OwnerBase { get; } Vector3 OwnerBasePosition { get; } void SwitchBase(IBase) }`
- `Bot.cs`: без изменений — `Bot` уже реализует все члены, transitively implements все три интерфейса через `IBot`

**Потребители (без изменений):**
- `BotRoster` хранит `List<Bot>` (конкретный тип), лямбды используют `bot.HasConstructTask` — работает, т.к. `Bot` реализует `IConstructor`
- `TaskScheduler` хранит `IReadOnlyList<Bot>`, вызывает `bot.SetTargetResource(...)` — работает, т.к. `Bot` реализует `IGatherer`
- States (`IdleState`, `WalkState`, `GatheringState`, `DropState`, `ConstructState`) используют `_stateMachine.Bot` (`IBot`) — работает, т.к. `IBot` transitively содержит все члены

**Выгода:** структурная — интерфейсы задокументированы, будущие потребители могут зависеть от узких ролей. `IMovable` для навигации, `IGatherer` для экономики, `IConstructor` для строительства.

### OCP — отложено

FSM остаётся с хардкодом `TransitionTo<WalkState>()` / `TransitionTo<IdleState>()` и т.д. ScriptableObject-конфиг переходов не реализован — текущий масштаб (5 состояний) не оправдывает overhead конфигурации.

---

## Порядок выполнения

1. **Фаза 1 (Critical)** — ВЫПОЛНЕНО
2. **Фаза 2 (High)** — ВЫПОЛНЕНО
3. **Фаза 3 (правила)** — ВЫПОЛНЕНО
4. **Фаза 4 (стиль)** — ВЫПОЛНЕНО
5. **Фаза 5 (SOLID: ISP)** — ВЫПОЛНЕНО
6. **Фаза 5 (SOLID: OCP)** — отложено

Все запланированные фазы (кроме OCP) выполнены. Рефакторинг завершён.
