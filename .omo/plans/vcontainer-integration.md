# vcontainer-integration - Work Plan

## TL;DR (For humans)

**Что:** Пошаговое образовательное внедрение VContainer в твой проект — от создания контейнера до полной замены GameContext. Ты делаешь всё сам, этот план — твой roadmap и учебник. 7 фаз, каждая объясняет концепцию и показывает код.

**Почему так:** VContainer — стандарт DI для Unity (от автора UniTask). Сейчас все модули жёстко привязаны через `GameContext.Instance`. С VContainer каждый модуль знает только свои зависимости, а контейнер связывает их в одном месте.

**Что НЕ делает:** Не меняет геймплей, FSM, фабрики, префабы. Не добавляет UniTask. Всё заменяется механически — та же функциональность, другой способ получения зависимостей.

**Effort:** Large (7 фаз, ~60 файлов на финальной фазе)
**Risk:** Medium — требует тестирования в Unity Editor после каждой фазы
**Формат:** Каждый шаг = объяснение концепции + код + команда проверки

### Концепции VContainer (шпаргалка)

| Концепция | Что делает | Аналог из обычной жизни |
|-----------|-----------|------------------------|
| `LifetimeScope` | Регистрирует сервисы в контейнере | Каталог: "вот все доступные сервисы" |
| `builder.Register<T>(Lifetime.Singleton)` | Один экземпляр на всё время | "Дайте мне тот же самый экземпляр" |
| `builder.Register<T>(Lifetime.Transient)` | Новый экземпляр каждый раз | "Создайте новый" |
| `[Inject]` | Поле/метод заполняется контейнером | "Внедрите зависимость сюда" |
| `Container.Resolve<T>()` | Получить вручную | "Дайте мне T" |
| `BankResolver<T>` | Фабрика: создаёт через DI | "Фабрика, которая пользуется контейнером" |
| `InjectGameObject` | Компонент на префабе для VContainer | "Этот префаб управляется VContainer" |

---

## Scope
### Must have
- Полная замена `GameContext.Instance` на VContainer
- Понимание каждой концепции (объясняется в каждом шаге)
- Рабочая игра на каждом этапе (не ломаем, а мигрируем)
- Фабрики (BotFactory, BaseFactory) работают через DI

### Must NOT have
- Не меняем префабы без крайней необходимости
- Не меняем поведение игры
- Не удаляем GameContext до последней фазы (держим как fallback)
- Не добавляем новые паттерны (всё как сейчас, только через DI)

## Verification strategy
- **После каждой фазы:** `dotnet build` 0 ошибок + запуск в Unity Editor
- **После каждой инъекции:** проверить что боты ходят, ресурсы собираются
- **Финальная проверка:** удалить GameContext, запустить игру — всё работает

---

## Todos

### Фаза 1: Знакомство — чтение документации + песочница

- [ ] 1. Изучить документацию VContainer
  Что делать: Открыть https://vcontainer.hadashikick.jp/ (официальный сайт). Прочитать разделы: "Quick Start", "Registering", "Resolving", "Lifetime". Это займёт 20-30 минут. Цель: понять базовые концепции до того как начать писать код.
  Параллелизация: Phase 1 | Блокирует: все остальные фазы
  Критерий: Ты можешь объяснить разницу между Singleton и Transient своими словами
  Commit: N (чтение)

- [ ] 2. Создать тестовый MonoBehaviour с [Inject] в отдельной сцене
  Что делать / Must NOT do: Создать новую пустую сцену (не Demo.unity!). Добавить пустой GameObject → `VContainer LifetimeScope`. Создать тестовый скрипт `TestDI.cs`:
  ```csharp
  public class TestDI : MonoBehaviour
  {
      public class Config { public string Message = "Hello VContainer!"; }
      [Inject] private Config _config;
      private void Start() => Debug.Log(_config.Message);
  }
  ```
  В LifetimeScope зарегистрировать: `builder.Register<Config>(Lifetime.Singleton)`. Повесить TestDI на любой объект. Запустить сцену — в консоли должно появиться "Hello VContainer!". Do NOT трогать Demo.unity или любой другой код проекта.
  Параллелизация: Phase 1 | Блокирует: все остальные фазы
  Критерий: Debug.Log выводит сообщение из инжектированного Config
  Commit: N (песочница, не в основном проекте)

---

### Фаза 2: Перенос сервисов без зависимостей (SpawnerResources, ResourcesData, ResourceWarhouse)

- [ ] 3. Создать GameLifetimeScope (замена GameContext как регистратора)
  **Концепция:** `LifetimeScope` — это MonoBehaviour, который знает обо всех сервисах. Он регистрирует их в контейнере. Когда объект в сцене хочет получить сервис, контейнер его находит и внедряет.
  
  Что делать: Создать `D:\Project\ColonizationURP\Assets\_Colonization\Scripts\Core\GameLifetimeScope.cs`:
  ```csharp
  using VContainer;
  using VContainer.Unity;
  
  public class GameLifetimeScope : LifetimeScope
  {
      [Header("Services")]
      [SerializeField] private ResourcesData _resourcesData;
      [SerializeField] private SpawnerResources _spawnerResources;
      
      protected override void Configure(IContainerBuilder builder)
      {
          // Register existing scene-instanced services as singletons
          builder.RegisterInstance(_resourcesData);
          builder.RegisterInstance(_spawnerResources);
          
          // Полный список сервисов — будет добавляться по ходу миграции
      }
  }
  ```
  Must NOT do: Не регистрируй BotFactory/BaseFactory пока. Не регистрируй Base. Не подключай события. Только голый контейнер с двумя сервисами.
  Параллелизация: Phase 2 | Блокирует: всё что использует DI для сервисов
  Критерий: `dotnet build` 0 ошибок. GameLifetimeScope висит на GameObject в Demo.unity.
  Commit: Y | feat: add GameLifetimeScope with ResourcesData and SpawnerResources registration

- [ ] 4. Инжектировать ResourcesData в Base (через метод)
  **Концепция:** `[Inject]` на методе — VContainer вызывает этот метод ПОСЛЕ Awake, передавая туда зависимости. Для MonoBehaviour это единственный способ инжекции (конструктор недоступен).

  Что делать: В `Base.cs` заменить строку 45:
  ```csharp
  // Было:
  _resourcesData = GameContext.ResourcesData;
  // Стало:
  private ResourcesData _resourcesData; // убрать инициализацию из Awake
  
  [Inject]
  private void Construct(ResourcesData resourcesData)
  {
      _resourcesData = resourcesData;
  }
  ```
  Параллелизация: Phase 2 | Блокирует: ничего
  Критерий: `dotnet build` 0 ошибок. После запуска в Unity: база видит ресурсы (ResourceCount меняется при сборе).
  Commit: Y | refactor: inject ResourcesData into Base via VContainer

- [ ] 5. Инжектировать SpawnerResources в BaseEventBinder (через конструктор)
  Что делать: `BaseEventBinder` — не MonoBehaviour, поэтому используем **конструкторную инжекцию**. В `BaseFactory.Awake()` (строка 19): сейчас создаётся `new BaseEventBinder(_resourcesData, _spawner)`. Переделать так:
  ```csharp
  // BaseFactory.cs — добавить в конструктор или [Inject] метод
  [Inject]
  private void Construct(ResourcesData resourcesData, SpawnerResources spawner)
  {
      _resourcesData = resourcesData;
      _spawner = spawner;
      _eventBinder = new BaseEventBinder(_resourcesData, _spawner);
  }
  ```
  Параллелизация: Phase 2 | Блокирует: ничего
  Критерий: `dotnet build` 0 ошибок. BaseEventBinder получает SpawnerResources через DI.
  Commit: Y | refactor: inject into BaseEventBinder via BaseFactory constructor injection

---

### Фаза 3: Перенос фабрик (BotFactory, BaseFactory)

- [ ] 6. Инжектировать BotFactory в Base (правильная отписка)
  **Концепция:** `[Inject]` заменяет `GameContext.BotFactory` и кэширование в `Awake`. Но есть нюанс: `Base` подписывается на `BotFactory.BotCreated`. Эта подписка сейчас в `OnEnable/OnDisable`. Нужно аккуратно перенести инжекцию так, чтобы подписка не порвалась.

  Что делать: В `Base.cs`:
  ```csharp
  // Убрать строку 43: _botFactory ??= GetComponentInChildren<BotFactory>();
  // Убрать строку 46: _baseFactory = GameContext.BaseFactory;
  
  [Inject]
  private void Construct(
      ResourcesData resourcesData,
      BaseFactory baseFactory,
      BotFactory botFactory)
  {
      _resourcesData = resourcesData;
      _baseFactory = baseFactory;
      _botFactory = botFactory;
  }
  ```
  Важно: теперь `_botFactory` приходит из инжекции, а не из `GetComponentInChildren`. Подписка в `OnEnable` остаётся без изменений (она ссылается на `_botFactory` которое уже установлено).

  В `GameLifetimeScope.Configure()` добавить:
  ```csharp
  builder.RegisterComponent(_baseFactory); // для стартовой базы
  builder.RegisterComponent(_botFactory);  // её BotFactory
  ```
  Параллелизация: Phase 3 | Блокирует: ничего
  Критерий: `dotnet build` 0 ошибок. Base получает BotFactory и BaseFactory через DI. Спавн ботов работает.
  Commit: Y | refactor: inject BotFactory and BaseFactory into Base

- [ ] 7. Инжектировать зависимости в Base (через прямое получение сервисов)
  Что делать: В `Base.cs` заменить строки:
  ```csharp
  // Было:
  _resourcesData = GameContext.ResourcesData;      // строка 45
  _baseFactory = GameContext.BaseFactory;           // строка 46
  
  // Стало: (если [Inject] метод уже добавлен из шага 4)
  // Ничего — уже инжектировано!
  ```
  Убедиться что строка 42 (`_stateMachine ??= GetComponent<BaseStateMachine>()`) остаётся — это нормально, `RequireComponent` гарантирует что компонент есть, а `GetComponent` в Awake кэширует.
  Параллелизация: Phase 3 | Блокирует: ничего
  Критерий: `dotnet build` 0 ошибок. Все зависимости Base приходят через DI.
  Commit: Y | refactor: remove GameContext.BaseFactory/ResourcesData access from Base

---

### Фаза 4: Перенос Player-модулей (FlagPlacer, PlayerMover, Camera)

- [ ] 8. Инжектировать LayerMask и Bounds в FlagPlacer и PlayerMover
  **Концепция:** VContainer умеет регистрировать не только объекты, но и структуры (LayerMask, Bounds, Vector3). Это через `builder.RegisterInstance()`. Значения всё ещё выставляются в `GameLifetimeScope` через SerializeField — контейнер просто передаёт их кто запросит.

  Что делать: В `GameLifetimeScope.Configure()`:
  ```csharp
  [SerializeField] private LayerMask _groundLayer;
  [SerializeField] private LayerMask _baseLayer;
  [SerializeField] private Bounds _mapBounds;
  
  protected override void Configure(IContainerBuilder builder)
  {
      // ... существующие регистрации ...
      builder.RegisterInstance(_groundLayer);
      builder.RegisterInstance(_baseLayer);
      builder.RegisterInstance(_mapBounds);
  }
  ```

  В `FlagPlacer.cs` заменить строки 20-22:
  ```csharp
  // Было:
  _camera = Camera.main;
  _selector = new BaseSelector(GameContext.BaseLayer);
  _groundRaycaster = new GroundRaycaster(GameContext.GroundLayer, GameContext.MapBounds);
  
  // Стало:
  [Inject]
  private void Construct(LayerMask groundLayer, LayerMask baseLayer, Bounds mapBounds)
  {
      _camera = Camera.main;
      _selector = new BaseSelector(baseLayer);
      _groundRaycaster = new GroundRaycaster(groundLayer, mapBounds);
  }
  ```
  Параллелизация: Phase 4 | Блокирует: ничего
  Критерий: `dotnet build` 0 ошибок. FlagPlacer получает слои и границы через DI. Клик по базе + установка флага работают.
  Commit: Y | refactor: inject LayerMask and Bounds into FlagPlacer

- [ ] 9. Инжектировать зависимости в остальные Player/Сamera скрипты
  Что делать: Проверить все скрипты которые сейчас используют `GameContext.*`:
  ```csharp
  // PlayerMover.cs — проверь, использует ли GameContext.MapBounds
  // CameraMover.cs / CameraRotator.cs — вряд ли используют GameContext
  ```
  Если используют — заменить на `[Inject]` метод. Если нет — пропустить.

  Параллелизация: Phase 4 | Блокирует: ничего
  Критерий: grep "GameContext\." в Player/ и Camera/ папках возвращает 0
  Commit: Y | refactor: inject remaining Player/Camera dependencies

---

### Фаза 5: Фабрики через VContainer (самое сложное)

- [ ] 10. Настроить BotFactory через VContainer (BankResolver)
  **Концепция:** `BankResolver` — это фабрика, которая создаёт объекты через DI. Она регистрируется в контейнере и когда кто-то вызывает `Resolve<IObjectResolver>()`, получает доступ к созданию объектов. Это нужно потому что BotFactory создаёт ботов в рантайме — контейнер должен знать как их создать.

  Что делать: `BotFactory` сейчас создаёт бота через `Instantiate(_botPrefab)` и публикует событие. С VContainer:

  ```csharp
  // BotFactory.cs — добавить InjectGameObject на префаб (в Unity Editor!)
  // ИЛИ: использовать IObjectResolver для создания
  
  // В GameLifetimeScope:
  builder.Register<BotFactory>(Lifetime.Singleton);
  
  // Для создания ботов в рантайме: использовать IObjectResolver
  // BotFactory получает доступ к контейнеру:
  [Inject] private IObjectResolver _resolver;
  
  public void Spawn()
  {
      Bot bot = Instantiate(_botPrefab);
      _resolver.InjectGameObject(bot.gameObject); // внедряет зависимости в бота
      BotCreated?.Invoke(bot);
  }
  ```
  Параллелизация: Phase 5 | Блокирует: создание ботов через DI
  Критерий: `dotnet build` 0 ошибок. BotFactory.Spawn() создаёт бота с инжектированными зависимостями. Bot получает свои зависимости через `[Inject]`.
  Commit: Y | feat: configure BotFactory with IObjectResolver for runtime DI

- [ ] 11. Инжектировать зависимости в Bot
  **Концепция:** `Bot` сейчас получает зависимости через `Start()` (строки 26-31: `GetComponent<Mover>()`, `GetComponent<Inventory>()`, `GetComponent<BotStateMachine>()`). VContainer может внедрить их через `[Inject]` метод — замена `GetComponent` на DI.

  Что делать: В `Bot.cs`:
  ```csharp
  // Убрать строки 27-31 из Start()
  
  [Inject]
  private void Construct(Mover mover, Inventory inventory, BotStateMachine stateMachine)
  {
      _mover = mover;
      _botInventory = inventory;
      _stateMachine = stateMachine;
  }
  ```
  `GetComponent` ВСЕ РАВНО работает (компоненты на том же GameObject), но VContainer внедряет их явно — это читаемее и тестируемее.

  Параллелизация: Phase 5 | Блокирует: ничего
  Критерий: `dotnet build` 0 ошибок. Bot получает компоненты через DI вместо GetComponent.
  Commit: Y | refactor: inject Bot components via VContainer instead of GetComponent

- [ ] 12. Настроить BaseFactory через VContainer (самый сложный)
  **Концепция:** `BaseFactory.Spawn()` создаёт новую базу в рантайме (расширение). Она клонирует префаб, уничтожает встроенных ботов, клонирует BotFactory. Это сложный процесс, и VContainer должен управлять им.
  
  **Подход:** Оставить `BaseFactory` как есть (она создаёт объекты через `Instantiate`), но дать ей доступ к контейнеру через `IObjectResolver` чтобы она могла внедрить зависимости в новую базу:

  ```csharp
  // BaseFactory.cs — добавить:
  [Inject] private IObjectResolver _resolver;
  
  public Base Spawn(Vector3 position)
  {
      Base newBase = InstantiateBase(position);
      _resolver.InjectGameObject(newBase.gameObject); // внедряет ВСЕ зависимости в новую базу
      ConfigureBaseChildren(newBase);
      _eventBinder.Bind(newBase);
      return newBase;
  }
  ```
  Параллелизация: Phase 5 | Блокирует: создание новых баз через DI
  Критерий: `dotnet build` 0 ошибок. Создание новой базы через флаг работает. Новая база получает все зависимости через DI.
  Commit: Y | feat: inject into new Base via IObjectResolver in BaseFactory

---

### Фаза 6: Удаление GameContext — финальный шаг

- [ ] 13. Проверить grep на все оставшиеся `GameContext.` вызовы
  Что делать: Запустить `grep -r "GameContext\." Assets/_Colonization/Scripts/`. Каждый найденный вызов должен быть заменён на `[Inject]` или удалён (если сервис больше не нужен).
  Параллелизация: Phase 6 | Блокирует: удаление GameContext
  Критерий: grep возвращает 0 результатов во ВСЕХ не-Editor скриптах
  Commit: Y | refactor: remove all remaining GameContext.* references

- [ ] 14. Удалить GameContext.cs и заменить на GameLifetimeScope
  Что делать: 
  1. Убедиться что `GameLifetimeScope` висит на GameObject в Demo.unity (на том же объекте где был GameContext)
  2. Перенести все `[SerializeField]` поля из GameContext в GameLifetimeScope (ResourcesData, SpawnerResources, BotFactory, BaseFactory, Base, слои, границы, флаг-префаб)
  3. Перенести логику из GameContext.Awake() в GameLifetimeScope.Configure():
     - `BaseEventBinder` — создавать и биндить в Configure() или оставить в отдельном компоненте
     - `FlagPlacer` — заменить `AddComponent<FlagPlacer>()` на `builder.RegisterComponent<FlagPlacer>(Lifetime.Singleton)`
  4. Удалить `GameContext.cs`
  5. Запустить Demo.unity — игра должна работать идентично
  6. Profit. 🎉
  
  Параллелизация: Phase 6 (последний шаг!) | Блокирует: ничего
  Критерий: `dotnet build` 0 ошибок. GameContext.cs удалён. Игра работает идентично. Все зависимости приходят через VContainer.
  Commit: Y | refactor: replace GameContext with VContainer GameLifetimeScope

---

### Фаза 7: Финальная проверка и чистка

- [ ] 15. Финальная верификация: запуск игры и проверка всех механик
  Что делать: Запустить Demo.unity и проверить каждую механику:
  - ✅ Ресурсы спавнятся на карте
  - ✅ Боты ходят к ресурсам и собирают их
  - ✅ Счётчик ресурсов обновляется
  - ✅ База тратит 3 ресурса на нового бота
  - ✅ Флаг ставится по клику (ЛКМ на базу → ЛКМ на землю)
  - ✅ Флаг переставляется (повторный клик)
  - ✅ База отправляет бота строить новую базу (5 ресурсов + флаг)
  - ✅ Новая база появляется, бот переходит к ней
  - ✅ Камера двигается (WASD + средняя кнопка мыши)
  - ✅ ПКМ отменяет выбор базы
  
  Параллелизация: Phase 7 | Блокирует: объявление готовности
  Критерий: Все 10 пунктов выше работают
  Commit: N (верификация)

- [ ] 16. Очистка: удалить неиспользуемые using и закомментированный код
  Что делать: Удалить `using` директивы которые стали не нужны (например `using CollectorBots.Scheduler;` в файлах где раньше был GameContext). Удалить закомментированные строки с `// Было: GameContext.*`.
  Параллелизация: Phase 7 | Блокирует: ничего
  Критерий: `dotnet build` 0 ошибок. Код чистый, без мусора.
  Commit: Y | chore: remove unused usings and commented code

---

## Final verification wave
- [ ] F1. Сборка: `dotnet build` 0 ошибок
- [ ] F2. GameContext: `grep -r "GameContext" Assets/_Colonization/Scripts/` → 0 результатов
- [ ] F3. DI-правильность: `grep -r "\[Inject\]" Assets/_Colonization/Scripts/` → все инжекции соответствуют регистрациям в GameLifetimeScope
- [ ] F4. Геймплей: запустить Demo.unity, проверить все механики из шага 15

## Commit strategy
- 1 коммит на шаг (шаги 1-16)
- Формат: `feat(vcontainer): <описание>` для новых регистраций, `refactor(vcontainer): <описание>` для миграции
- После каждой фазы — запускать игру и проверять что ничего не сломано

## Success criteria
1. `dotnet build` 0 ошибок после каждого шага
2. Игра работает идентично на каждом шаге
3. `GameContext.cs` полностью удалён
4. Все зависимости приходят через `[Inject]`, ни одного `GameContext.Instance.*`
5. Ты понимаешь каждую концепцию VContainer и можешь объяснить её
