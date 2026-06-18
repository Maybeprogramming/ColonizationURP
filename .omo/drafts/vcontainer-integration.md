# vcontainer-integration — Draft

- **slug:** vcontainer-integration
- **intent:** CLEAR (user wants educational step-by-step VContainer migration)
- **status:** awaiting-approval
- **pending action:** write .omo/plans/vcontainer-integration.md
- **approach:** 7-phase educational migration. Each phase explains VContainer concepts, then applies them. All phases are sequential — each builds on the previous. User does the implementation themselves; this plan is a study guide + roadmap.

## Key decisions
- VContainer 1.18.0 already installed via local package
- `BankResolver` approach for factories (standard VContainer pattern for runtime creation)
- `InjectGameObject` for prefab-based objects (bots, bases)
- Keep existing architecture (FSM, interfaces, events) — only replace service resolution

## Project context (what we're migrating FROM)
GameContext is a MonoBehaviour singleton that:
- Holds serialized references to 8 services: ResourcesData, SpawnerResources, BotFactory, BaseFactory, Base, ResourceCounterView, LayerMask, Bounds
- Exposes them as static properties
- Wires BaseEventBinder and ResourceWarhouse.Changed in Awake
- Creates FlagPlacer component if missing

These services are consumed throughout the codebase:
- `Base.cs` → `GameContext.ResourcesData`, `GameContext.BaseFactory`
- `BaseFactory.cs` → `GameContext.ResourcesData`, `GameContext.Spawner`
- `Bot.cs` → `GameContext.MapBounds` (for movement clamping — TODO verify)
- `FlagPlacer.cs` → `GameContext.BaseLayer`, `GameContext.GroundLayer`, `GameContext.MapBounds`
- Other scripts via the same static properties

## What VContainer brings
- **Lifetime Scope:** controls object lifetime (Singleton = one per scene; Transient = new each time)
- **[Inject] attribute:** marks where dependencies go
- **Constructor Injection:** prefer this over [Inject] on properties — it's more explicit
- **Method Injection:** for Unity objects that can't use constructor injection (MonoBehaviours)
- **Factory Pattern:** `BankResolver` for creating objects at runtime with DI
