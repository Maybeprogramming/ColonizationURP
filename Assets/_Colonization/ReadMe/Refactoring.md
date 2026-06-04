# План рефакторинга

Создан на основе архитектурного аудита проекта. Цель: привести кодовую базу к соответствию `rules.md` и принципам SOLID перед расширением функционала.

## Сводка

| Категория | Кол-во | Статус |
|---|---|---|
| Critical | 7 | Фаза 1 |
| High | 9 | Фаза 2 |
| Medium (правила) | 14+ | Фаза 3 |
| Low (стиль) | 6+ | Фаза 4 |

**Главные проблемы:**
- God Classes (`FlagPlacer` — 7 обязанностей, `Base` — 7, `BaseFactory` — 6)
- Скрытые зависимости через `??=` / `FindFirstObjectByType` (7+ мест)
- Дублирование wiring событий между `Bootstrapper` и `BaseFactory.Spawn`
- Потеря Inspector-данных в `Start()` (ресурсы и счётчик)
- Утечки памяти в `FlagPlacer.OnDestroy` (материал, прямоугольник)
- Race condition в `Mover.MoveTo` (параллельные корутины)
- Нарушения HLSL-правил: `v2g`/`g2f`/`verts`/`vertices` вместо `VertexToGeometry`/`GeometryToFragment`/`vertexPositions`

---

## Фаза 1 — CRITICAL (блокирует прогресс)

### 1.1 Потеря Inspector-данных в `Start()`
Файлы: `Scripts/ResourcesData.cs:10-14`, `Scripts/ResourceWarhouse/ResourceWarhouse.cs:13-16`.

**Проблема:** `Start()` безусловно перезаписывает сериализованные поля новыми инстансами, теряя дизайнерские настройки.

**Решение:** использовать `??=`, не пересоздавать список/значение если оно уже есть.

### 1.2 Дублирование wiring событий
Файлы: `Scripts/Bootstrapper.cs:19-23`, `Scripts/EntityFactories/BaseFactory.cs:46-55`.

**Проблема:** `ResourceAdded`, `Changed`, `ResourceFound` подписываются в двух местах. При динамической замене базы — двойная подписка.

**Решение:** вынести wiring в один класс `BaseEventBinder` и вызывать его из обоих мест. Альтернатива: подписка в `Base.Awake`.

### 1.3 Утечка `BotFactory` из префаба
Файл: `Scripts/EntityFactories/BaseFactory.cs:31-38`.

**Проблема:** `Base.Awake` находит существующий `BotFactory` через `GetComponentInChildren`, затем `BaseFactory.Spawn` создаёт новый и перезаписывает ссылку. Старый `BotFactory` остаётся в иерархии.

**Решение:** уничтожать старый `BotFactory.gameObject` перед созданием нового, либо убрать `BotFactory` из префаба базы.

### 1.4 Утечка памяти в `FlagPlacer.OnDestroy`
Файл: `Scripts/Player/FlagPlacer.cs:81-88`.

**Проблема:** уничтожается `_flagVisual`, но не `_selectionRect` (создан `CreatePrimitive`) и не `_selectionMaterial` (создан `new Material(Shader.Find(...))`).

**Решение:** добавить `Destroy(_selectionRect)` и `Destroy(_selectionMaterial)` в `OnDestroy`.

### 1.5 Race condition в `Mover.MoveTo`
Файл: `Scripts/Bot/Mover.cs:18-23`.

**Проблема:** новый `StartCoroutine` без остановки предыдущего. Два `Moving()` мутируют `transform.position` — бот идёт в неверную цель.

**Решение:** перед запуском корутины вызывать `if (_moving != null) StopCoroutine(_moving);`.

### 1.6 Скрытые зависимости через `??=` и `FindFirstObjectByType`
Файлы (7+ мест):
- `Scripts/Base/Base.cs:35,38`
- `Scripts/EntityFactories/BaseFactory.cs:13,16,19,22`
- `Scripts/Bootstrapper.cs:14`
- `Scripts/Player/FlagPlacer.cs:34,49`
- `Scripts/FSM/BotStateMachine/ConstructState.cs:21`

**Проблема:** рантайм-поиск объектов в сцене. Недетерминированный порядок инициализации, NRE в тестах, проект нельзя тестировать.

**Решение:** заменить `??=` на обязательные `[SerializeField]`-ссылки (с интерфейсами где возможно). В крайнем случае — централизованный ServiceLocator MonoBehaviour.

### 1.7 `ConstructState` ищет `BaseFactory` при каждом входе
Файл: `Scripts/FSM/BotStateMachine/ConstructState.cs:21`.

**Проблема:** `Object.FindFirstObjectByType<BaseFactory>()` выполняется на каждый `Enter` ConstructState.

**Решение:** инжектировать `BaseFactory` через `BotStateMachine` или `Bot.OwnerBase`.

---

## Фаза 2 — HIGH (архитектурный долг)

### 2.1 `FlagPlacer` — God Class (245 строк, 7 обязанностей)
Файл: `Scripts/Player/FlagPlacer.cs`.

**Обязанности для разделения:**
1. Чтение ввода мыши
2. Raycast по террейну
3. Управление выбором базы
4. Спавн/управление флаг-визуалом
5. Прямоугольник выделения
6. Авто-определение `Ground`
7. Загрузка ресурсов из `Resources`

**Декомпозиция:**
- `GroundRaycaster` — raycast → точка на террейне
- `BaseSelector` — состояние `_selectedBase`, выбор/смена
- `FlagPlacementController` — позиция флага, визуал, follow-mouse
- `SelectionRectRenderer` — только визуал рамки
- `GroundBoundsResolver` — авто-определение `_mapBounds`

### 2.2 `Base` — God Class (130 строк, 7 обязанностей)
Файл: `Scripts/Base/Base.cs`.

**Обязанности:** FSM, склад, список ботов, планировщик задач, фабрика, флаг, корутина-цикл.

**Декомпозиция:**
- `BotRoster` — `AddBot`, `RemoveBot`, `GetFreeBot`, `CancelConstructTasks`, `BotCount`
- `ExpansionController` — `HasConstructNewBase`, `FlagPosition`, `ClearExpansionFlag`
- `BaseWorkLoop` — корутина, scheduler, задержка

### 2.3 Циклическая ссылка `Base ↔ BotFactory`
Файлы: `Scripts/Base/Base.cs:18`, `Scripts/EntityFactories/BotFactory.cs:8`.

**Проблема:** `Base._botFactory` ↔ `BotFactory._base` — порядок инициализации критичен.

**Решение:** событийный паттерн (`Base` публикует `BotNeeded`, `BotFactory` подписывается) или mediator.

### 2.4 Дублирование констант в 3 местах
Файлы: `Base.cs:10-11`, `FSM/BaseStateMashine/NormalState.cs:5-6`, `FSM/BaseStateMashine/ExpandState.cs:5`.

**Решение:** создать `BaseBalance` статический класс:
```csharp
public static class BaseBalance
{
    public const int BotSpawnCost = 3;
    public const int ExpandCost = 5;
}
```

### 2.5 `BaseFactory.Spawn` дублирует wiring событий
Файл: `Scripts/EntityFactories/BaseFactory.cs` (62 строки, 6 обязанностей).

**Решение:** вынести wiring в `BaseEventBinder`, инжектировать зависимости через интерфейсы.

### 2.6 Публичные мутации state (corruption risk)
Файлы:
- `Base.cs:29,31` — `FlagPosition { get; set; }`, `HasConstractNewBase { get; set; }`
- `Bot.cs:15,16` — `HasConstructTask { get; set; }`, `ConstructTargetPosition { get; set; }`

**Проблема:** `[field: SerializeField] public bool ... { get; set; }` — Inspector перезаписывает рантайм.

**Решение:** заменить на методы-команды:
- `AssignConstructTask(Vector3 target)`
- `ClearConstructTask()`
- `SetFlagPosition(Vector3)`
- `StartExpansion()`

### 2.7 `Bot.IsBusy` зависит от конкретного `IdleState`
Файл: `Scripts/Bot/Bot.cs:21`.

**Проблема:** `_stateMachine.GetCurrentState is IdleState == false` — нельзя подменить реализацию.

**Решение:** добавить `bool IsBusy { get; }` в интерфейс `IState` (или отдельный `IStateStatus`), каждое состояние само объявляет занятость.

### 2.8 `IBot.OwnerBase` возвращает конкретный `Base`
Файлы: `Scripts/Bot/IBot.cs:11`, `Scripts/Bot/Bot.cs:20`.

**Решение:** возвращать `IBase` (а не `Base`). Ввести `IResource` для `TargetResource`.

### 2.9 GC pressure в `SpawnerResources`
Файл: `Scripts/Spawner/SpawnerResources.cs:54-68`.

**Проблема:** `new WaitForSeconds(...)` на каждой итерации корутины.

**Решение:** закэшировать `WaitForSeconds` или использовать `WaitForSecondsRealtime` с временной меткой.

---

## Фаза 3 — нарушения правил (массовые исправления)

### 3.1 HLSL — Rule 8 (структуры)
- `Resources/Grass/GrassGeometry.shader:45-55, 172-180`: `v2g` -> `VertexToGeometry`, `g2f` -> `GeometryToFragment`
- `Resources/Flawers/FlowerGeometry.shader:68-79, 557-565`: то же самое
- `Resources/Grass/GrassGeometry.shader:133`, `Resources/Flawers/FlowerGeometry.shader:519, 784`: `half4 frag(... i)` -> `input`

### 3.2 HLSL — Rule 9 (локальные переменные)
- `Resources/Flawers/FlowerGeometry.shader:93`: `EmitQuad(... a, b, c, d ...)` -> `bottomLeft, bottomRight, topLeft, topRight`
- `Resources/Flawers/FlowerGeometry.shader:615,651,677,711,745,771`: `verts[6]` -> `vertexPositions`
- `Resources/Grass/GrassGeometry.shader:112, 226`: `vertices[6]` -> `vertexPositions`

### 3.3 HLSL — Rule 10 (константы)
- `Resources/Grass/GrassGeometry.shader:79, 204`: магическое `6.28` -> `#define TWO_PI 6.28318530718`

### 3.4 C# — Rule 1 (naming)
- `Resources/Grass/GrassRenderer.cs:13,26,27,40`: `_bladeMesh` -> `_pointMesh` (аббревиатура + Rule 6)
- `Scripts/Camera/CameraRotator.cs:25`: `private Vector2 _lookDelta` -> `PascalCase` для свойств
- `Scripts/FSM/StateMachine/IStateMachine.cs:3`: `IState GetCurrentState` -> `CurrentState`
- `Base.cs:31`, `IBase.cs:7`: опечатка `HasConstractNewBase` -> `HasConstructNewBase` (везде)
- `Scripts/FSM/BaseStateMashine/`: папка `Mashine` -> `Machine` (Rule 11)
- `Resources/FlyEntities/ButterflyMeshBuilder.cs:10`: убрать `static` (Rule 1)

### 3.5 C# — Rule 2 (порядок членов)
- `Scripts/Base/Base.cs:13-31`: public-свойства **до** events — переставить
- `Scripts/ResourceWarhouse/ResourceWarhouse.cs:18`: private `OnChangeCount` **до** public методов — переставить
- `Scripts/Camera/CameraRotator.cs:82`: public `GetOffset` **после** private — переставить
- `Scripts/FSM/StateMachine/StateMachine.cs:12-13`: public `Update()` должен быть после Unity messages

### 3.6 C# — Rule 3 (магические числа)
- `Scripts/FSM/BotStateMachine/GatheringState.cs:14`: `_delayTimer = 1.5f` -> `const GatheringDelay`
- `Scripts/EntityFactories/BaseFactory.cs:29`: `Random.Range(1000, 9999)` -> `const BaseNameMin`, `BaseNameMax`
- `Scripts/Base/Base.cs:17`: `_delayTime` без дефолта -> `const DefaultDelayTime = 1f` или атрибут

### 3.7 C# — Rule 5 (аллокации)
- `Scripts/Spawner/SpawnerResources.cs:54-68`: кэшировать `WaitForSeconds` (см. 2.9)
- `Scripts/InputSystem/PlayerInputSystem.cs:30-35`: `ReadValue<Vector2>()` вызывается 2 раза — кэшировать в local
- `Scripts/ResourceScanner/ScanAnimation.cs:64`: `renderer.material.color` — заменить на `MaterialPropertyBlock`

### 3.8 C# — Rule 6 (Mesh)
- `Resources/Grass/GrassRenderer.cs:52-70`: `CreateTriangleMesh` -> `CreatePointMesh` + `mesh.name = "PointMesh"`

### 3.9 C# — Rule 11 (файлы)
- Папка `Scripts/FSM/BaseStateMashine/` -> `BaseStateMachine/`

---

## Фаза 4 — LOW (стиль, мелочи)

- `Scripts/Camera/CameraRotator.cs:84`: `Vector3.zero.z` -> `0f`
- `Scripts/InputSystem/PlayerInputSystem.cs:33`: `Vector3.zero.y` -> `0f`
- `Scripts/Bot/Mover.cs:13`: `Position` — **dead code**, удалить
- `Scripts/Bot/Inventory.cs:30-31`: `Detach` статический без причины — сделать инстансным
- `Scripts/Entity/Entity/Hummer.cs`: переименовать в `Hammer` (русская транслитерация)
- `Scripts/EntityFactories/BotFactory.cs` — не использовать `Hummer` как маркер, использовать интерфейс `ITool`

---

## Принципы SOLID (итог)

| Принцип | Состояние | Цель |
|---|---|---|
| S (SRP) | Нарушен | Разделить `FlagPlacer`, `Base`, `BaseFactory` |
| O (OCP) | Нарушен | FSM с хардкодом -> ScriptableObject со списком состояний |
| L (LSP) | Нарушен | `Bot.IsBusy`, `IBot.OwnerBase` -> интерфейсы |
| I (ISP) | Нарушен | `IBot` — 15 членов -> `IMovable`, `IGatherer`, `IConstructor` |
| D (DIP) | Нарушен | Повсеместно конкретные классы -> интерфейсы + DI через инспектор |

---

## Порядок выполнения

1. Фаза 1 (Critical) — фиксы багов и утечек, ~30 минут
2. Фаза 2 (High) — декомпозиция God Classes, ~2-3 часа
3. Фаза 3 (правила) — массовые rename и перестановки, ~1 час
4. Фаза 4 (стиль) — косметика, по возможности

Каждый этап — отдельный коммит с пометкой `[REFACTOR]`. Тесты — после каждой фазы (включая ручную проверку в Unity Editor).
