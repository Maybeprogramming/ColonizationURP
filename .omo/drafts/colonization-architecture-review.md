# ColonizationURP — Architecture Review Draft

- **slug:** colonization-architecture-review
- **intent:** UNCLEAR (fuzzy outcome: "исследуй и дай план улучшений")
- **status:** awaiting-approval
- **pending action:** write .omo/plans/colonization-architecture-review.md (DONE — plan written)
- **approach:** 12 safe mechanical refactors in 4 waves. 1 change deferred (IsBusy guard — too risky for this pass). All verified via dotnet build + structural grep. No gameplay changes.

## Research summary

### How it works
60+ game scripts under `Assets/_Colonization/Scripts/` with clear folder split:
- `Core/` — GameContext (singleton service locator)
- `Base/` — Base, BaseWorkLoop, BotRoster, TaskScheduler, ExpansionProvider, BaseEventBinder
- `Bot/` — Bot, Mover, Inventory, ToolsProvider, interfaces (IBot, IMover, IGatherer, etc.)
- `FSM/` — StateMachine (abstract), BaseStateMachine (NormalState, ExpandState), BotStateMachine (Idle, Walk, Gathering, Drop, Construct)
- `Resource/` — Resource, ResourcesData, ResourceWarhouse, SpawnerResources, Scanner
- `Player/` — FlagPlacer, BaseSelector, GroundRaycaster, SelectionRectRenderer, FlagVisualProvider
- `Factories/` — BaseFactory, BotFactory
- `Camera/` — CameraMover, CameraRotator
- `InputSystem/` — PlayerInputSystem
- `Editor/` — Property drawers

### Good patterns
1. FSM architecture — decoupled states with IState interface
2. Interfaces all around (IState, IBase, IBot, IMover, IGatherer, IConstructor, IInventory, IStateMachine)
3. Event-driven communication (ResourceAdded, ResourceFound, BotCreated, StateChanged, etc.)
4. Factory pattern (BaseFactory, BotFactory)
5. Object pool via BaseResourcePool<T> for resources
6. Proper OnDestroy cleanup with -= unsubscribe in most files
7. [SerializeField] private for inspector exposure (Unity best practice)
8. URP render pipeline, DOTween integration

### Architecture weaknesses (severity-ordered)
CE1 GameContext static singleton service locator, all modules coupled to it
CE2 No namespaces (all scripts in global namespace)
HE3 ConstructState casts IBot→Bot, calls GetComponent on every Enter/Exit
HE4 StateMachine.Update() runs every frame on ALL state machines, even idle states
HE5 TaskScheduler.GetNextTask() — LINQ OrderBy().First() allocated per assignment
ME6 ResourcesData.TryGetResource() — LINQ .First() on List
ME7 Bot.HasConstructTask with public setter — external mutation bypassing FSM
ME8 BaseWorkLoop.DoWork() — Debug.Log every iteration when no free bots
ME9 FlagPlacer monolithic — input + selection + visual feedback in one class
LE10 BaseEventBinder calls GetComponent 3 times each in Bind/Unbind
LE11 SpawnerResources.Spawning() — yield return null every frame for a timer
LE12 ResourceScanner.Physics.OverlapSphere allocates Collider[] per scan
LE13 StateMachine.TransitionTo — StateChanged?.Invoke(typeof(T)) boxes Type
LE14 No UniTask or async — coroutines only
LE15 ExpandState/BotRoster/ResourcesData — defensive null-pattern code scattered

### Open-assumptions (UNCLEAR path — adopted as defaults)
OA1 Scope: non-destructive refactors only — no gameplay changes, no prefab restructuring
OA2 Priority: GC allocation fixes > architecture decoupling > code quality
OA3 Namespace: adopt `ColonizationURP.{Module}` namespace convention
OA4 UniTask: adopt where coroutines don't need frame-by-frame yield (scanner timer, spawner timer)
OA5 GameContext: replace direct static access with field-cached references in Awake (micro-fix), full DI deferred
OA6 ConstructState: inline ToolsProvider into IBot interface as property
OA7 StateMachine idle guard: add `_isIdle` flag to skip Update when state is IdleState
OA8 Risk: low — all changes are mechanical refactors with zero gameplay impact
