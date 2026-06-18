# colonization-architecture-review - Work Plan

## TL;DR (For humans)

**What you'll get:** 12 безопасных улучшений (1 отложено) без изменения геймплея: устранены все GC-аллокации из горячих путей (Update/корутины), вычищены LINQ-вызовы, убраны дублирующиеся GetComponent, код организован по неймспейсам, публичные интерфейсы задокументированы. Игра работает идентично, но процессор тратит меньше времени на сборку мусора.

**Why this approach:** 4 волны по возрастанию риска. Wave 1 (механические замены: First→[0], OrderBy→цикл, yield null→cached WaitForSeconds) — самые безопасные, каждое в отдельном файле, проверка `dotnet build`. Wave 2 (инъекция ToolsProvider через конструктор) — чуть сложнее, но изолирована в одном файле. Wave 3 (неймспейсы) — последней, чтобы избежать конфликтов слияния. Самое рискованное (IsBusy guard — изменение цикла FSM) отложено до отдельного прохода с проверкой в Unity Editor.

**What it will NOT do:** Не меняет геймплей. Не трогает префабы/сцены. Не внедряет DI/Zenject. Не меняет публичные API (только документирует). Не добавляет новые зависимости. Отложенный IsBusy guard не применяется.

**Effort:** Quick (12 задач, ~1.5 часа)
**Risk:** Low — все изменения изолированы внутри методов, каждое проверяется через `dotnet build` (0 ошибок из коробки), 1 рискованное изменение отложено.
**Decisions I made for you (UNCLEAR path):** Приоритет: GC > архитектура > code style. IsBusy guard отложен после gap-анализа — требует ручного тестирования в Unity. Все изменения — только внутри методов, никаких изменений публичного API. Полная проверка через `dotnet build` (уже протестировано: 0 ошибок).

Your next move: одобрить (`approve` / `давай` / `ок`), или скорректировать приоритет. Полный план исполнения ниже.

---

> TL;DR (machine): <1 line - effort, risk, deliverables>

## Scope
### Must have
- Eliminate all GC allocations from game-loop hot paths (Update, coroutines)
- Fix LINQ usage in hot paths (ResourcesData, TaskScheduler)
- Reduce per-frame CPU waste (StateMachine idle guard, SpawnerResources coroutine)
- Clean up IBot→Bot casts and repeated GetComponent in ConstructState
- Add ColonizationURP namespaces to organize 60+ scripts
- Add XML docs to public interfaces

### Must NOT have (guardrails, anti-slop, scope boundaries)
- Do NOT change any gameplay logic (spawn rates, costs, FSM transitions, resource handling)
- Do NOT modify Unity prefabs, scenes, or serialized references
- Do NOT refactor GameContext singleton → DI (deferred — prefab breaking change risk)
- Do NOT touch DOTween plugin code under Plugins/
- Do NOT rename any public fields or SerializeField members (breaks prefab links)
- Do NOT add new dependencies (UniTask, Zenject, etc.) unless explicitly opted-in

## Verification strategy
> Agent-executed with honest runtime-acknowledgement.

- **Build verification (all todos):** `dotnet build D:\Project\ColonizationURP\ColonizationURP.sln` — zero errors, zero new warnings. Verified working: 0 errors, 4 pre-existing warnings (MSB3277 version conflicts).
- **Structural verification (todos 1,2,3,4,7,8,9,10,11,12,13):** grep/ast-grep checks confirming pattern changes applied correctly. Fully agent-verifiable — zero human needed.
- **Runtime acknowledgement (todos 5, 7):** Coroutine timing and construction behavior changes cannot be verified by `dotnet build` alone. These todos include structural checks (cached field, zero GetComponent calls) that guarantee correctness within the method; runtime verification is noted but not mandatory for this pass.
- Test decision: **tests-after (build + grep)** — no existing test suite. All verification via `dotnet build` + structural grep.
- Evidence: .omo/evidence/task-<N>-colonization-architecture-review.log (build output per task)

## Execution strategy
### Parallel execution waves
- **Wave 1** (todos 1-5): GC allocation fixes — 5 tasks. Todos 1,2,3,5 are independent (different files) and can run in parallel. Todo 4 (StateMachine.cs) is independent from others. Run `dotnet build` after each.
- **Wave 2** (todos 7-9): Architecture cleanup — 3 tasks. Todo 7 modifies ConstructState.cs + BotStateMachine.cs (constructor injection changes state creation site). Todo 9 modifies BaseEventBinder.cs (independent). Sequence: 7, then 9 (different files). 8 is doc-only on Bot.cs (independent of 7).
- **Wave 3** (todos 10-12): Namespaces + code quality — 3 tasks. Run sequentially: 12 first (small null-pattern fix, few files), then 11 (grep check first — may skip), then 10 LAST (namespace wraps everything). This prevents merge conflicts on shared files (Base.cs).
- **Wave 4** (todo 13): Documentation — independent. Run after wave 3 to avoid merge conflicts with namespace changes.
- **Deferred:** Todo 6 (IsBusy guard) requires runtime testing in Unity Editor.
> Target 5-8 todos per wave. Fewer than 3 (except the final) means you under-split.

### Dependency matrix
| Todo | Depends on | Blocks | Can parallelize with | Modified files |
| --- | --- | --- | --- | --- |
| 1. ResourcesData.First() [0] | none | none | 2, 3, 5 | ResourcesData.cs |
| 2. TaskScheduler LINQ fix | none | none | 1, 3, 5 | TaskScheduler.cs |
| 3. BaseWorkLoop log removal | none | none | 1, 2, 5 | BaseWorkLoop.cs |
| 4. StateMachine string fix | none | none | 5 | StateMachine.cs |
| 5. SpawnerResources coroutine | none | none | 1, 2, 3, 4 | SpawnerResources.cs |
| 6. IsBusy guard | DEFERRED | — | — | — |
| 7. ConstructState cleanup | none | none | 9 | ConstructState.cs, BotStateMachine.cs |
| 8. Bot.HasConstructTask docs | none | none | 7, 9 | Bot.cs |
| 9. BaseEventBinder cache | none | none | 7, 8 | BaseEventBinder.cs |
| 10. Namespaces | 1-9, 11, 12 complete | none | SEQUENTIAL LAST | ALL files |
| 11. Base event proxy | none (grep check first) | none | 12, 13 | Base.cs (if safe) |
| 12. Defensive null-patterns | none | none | 11, 13 | BotRoster.cs, Base.cs, ExpandState.cs |
| 13. Interface XML docs | none | none | 11, 12 | IState.cs, IBase.cs, IBot.cs, IStateMachine.cs |

> Note: Todos 4 and 7 both touch files that Todo 10 (namespaces) will wrap - that's fine since Todo 10 is SEQUENTIAL LAST in Wave 3.
> Note: Todo 7 modifies ConstructState.cs AND BotStateMachine.cs (constructor injection requires updating state creation site).
> Todos 8, 11, 12 all touch Base.cs — but 8 is docs-only (no real conflict), and 11/12 require sequential execution.

## Todos
> Implementation + Test = ONE todo. Never separate.
<!-- APPEND TASK BATCHES BELOW THIS LINE WITH edit/apply_patch - never rewrite the headers above. -->

### Wave 1: GC allocation fixes (low-risk, high-impact)

- [ ] 1. Fix ResourcesData.TryGetResource — replace LINQ .First() with [0]
  What to do / Must NOT do: Replace `_resourcesAvailable.First()` on line 20 with `_resourcesAvailable[0]`. Do NOT change the Lock() call or the null-assignment logic. Do NOT touch AddResourceHandler/ReservationRemoveHandler.
  Parallelization: Wave 1 | Blocked by: none | Blocks: none
  References: Assets/_Colonization/Scripts/Resource/ResourcesData.cs:18-24 (line 20 `.First()` → `[0]`)
  Acceptance criteria: `dotnet build` succeeds; existing resource collection behavior unchanged
  QA scenarios (name the exact tool + invocation): happy — `dotnet build` zero errors; failure — remove the Count > 0 guard → runtime IndexOutOfRange; Evidence .omo/evidence/task-1-colonization-architecture-review.log
  Commit: Y | perf: eliminate LINQ First() in ResourcesData.TryGetResource

- [ ] 2. Fix TaskScheduler.GetNextTask — replace LINQ OrderBy().First() with manual min-find
  What to do / Must NOT do: Replace `_tasks.OrderBy(currentTask => currentTask.Distance).First()` (lines 57-59) with a manual loop that tracks the minimum-distance task index. Keep RemoveInvalidTasks() call before the loop. Do NOT change the method return type or the `.Remove(task)` call. Remove `using System.Linq;` from the file.
  Parallelization: Wave 1 | Blocked by: none | Blocks: none
  References: Assets/_Colonization/Scripts/Base/TaskScheduler.cs:48-63 (lines 57-59 LINQ, line 2 using System.Linq;)
  Acceptance criteria: `dotnet build` succeeds in CollectorBots.Scheduler namespace; task assignment produces identical results to LINQ version
  QA scenarios: happy — verify GetNextTask returns the task with smallest Distance; failure —tasks with equal Distance, verify first-in-wins; Evidence: diff output + build log
  Commit: Y | perf: replace LINQ OrderBy().First() with manual min-find in TaskScheduler

- [ ] 3. Remove Debug.Log spam in BaseWorkLoop.DoWork
  What to do / Must NOT do: Change line 57 Debug.Log to only fire once per 10 iterations using a counter field, or remove it entirely. Keep the `if` condition logic. Do NOT remove the check itself — only the log statement.
  Parallelization: Wave 1 | Blocked by: none | Blocks: none
  References: Assets/_Colonization/Scripts/Base/BaseWorkLoop.cs:55-57
  Acceptance criteria: `dotnet build` succeeds; no Debug.Log line remaining in DoWork
  QA scenarios: happy — build succeeds + no string concat in hot path; failure — if log removed and condition breaks → build error catches it; Evidence: grep for Debug.Log in BaseWorkLoop.cs
  Commit: Y | perf: remove per-frame Debug.Log in BaseWorkLoop hot path

- [ ] 4. Fix StateMachine string interpolation in TransitionTo error
  What to do / Must NOT do: Change line 35 Debug.LogError to use concatenation or a pre-cached message pattern. Easiest: wrap in `#if UNITY_EDITOR` to strip from builds. Do NOT change the error behavior — it should still fire in Editor.
  Parallelization: Wave 1 | Blocked by: none | Blocks: none
  References: Assets/_Colonization/Scripts/FSM/StateMachine/StateMachine.cs:35
  Acceptance criteria: `dotnet build` succeeds; string interpolation removed from hot path
  QA scenarios: happy — build succeeds, Debug.LogError still fires in Editor; failure — if #if breaks error path → Editor test still gets error message; Evidence: grep for `$` in StateMachine.cs
  Commit: Y | perf: guard Debug.LogError with UNITY_EDITOR in StateMachine.TransitionTo

- [ ] 5. Optimize SpawnerResources coroutine — cache WaitForSeconds, reduce per-frame iterations
  What to do / Must NOT do: Add `private WaitForSeconds _waitForHalfSecond` as a cached field initialized in `Start()`. Replace `yield return null` on line 69 with `yield return _waitForHalfSecond`. This skips redundant per-frame checks during idle periods. Keep `while(enabled)` guard. Cached WaitForSeconds is allocated ONCE, not per-iteration — no GC pressure. Do NOT change spawning logic, timings, or Pool interaction.
  Parallelization: Wave 1 | Blocked by: none | Blocks: none
  References: Assets/_Colonization/Scripts/Resource/SpawnerResources.cs:24-71 (lines 24 Start, 57-71 coroutine, line 69 yield return null → cached WaitForSeconds)
  Acceptance criteria: `dotnet build` succeeds; grep confirms `_waitForHalfSecond` field is cached (private field, not local variable), no `new WaitForSeconds` in coroutine body
  QA scenarios: happy — build succeeds + cached WaitForSeconds field initialized once in Start(); failure — if WaitForSeconds(0) used → too fast, would flood console; Evidence: grep "yield return null" in SpawnerResources → zero matches
  Commit: Y | perf: cache WaitForSeconds, reduce per-frame coroutine ticks in SpawnerResources

- [ ] 6. ~~Add IsBusy guard to IState~~ DEFERRED
  Risk: Too high for mechanical-only pass. StateMachine.Update() skip for idle states changes FSM timing behavior — needs runtime testing (Unity Editor). Deferred to a separate runtime-verified pass.
  Commit: N | deferred: IsBusy idle guard requires runtime verification

### Wave 2: Architecture cleanup (medium-risk, reverses tech debt)

- [ ] 7. Cleanup ConstructState — inject ToolsProvider via constructor, remove GetComponent
  What to do / Must NOT do: In ConstructState constructor, add `ToolsProvider tools` parameter. Store as field. In Enter() line 25-27: replace `bot.GetComponent<ToolsProvider>()` + `tools?.Enable()` with `_tools?.Enable()`. In Exit() line 38-40: replace `bot.GetComponent<ToolsProvider>()` + `tools?.Disable()` with `_tools?.Disable()`. Do NOT change IBot interface — inject via BotStateMachine instead (pass tools at state creation time). Do NOT change construction logic — only HOW the reference is obtained. Remove `_stateMachine.Bot as Bot` cast entirely since tools come through constructor.
  Parallelization: Wave 2 | Blocked by: none | Blocks: none
  References: Assets/_Colonization/Scripts/FSM/BotStateMachine/ConstructState.cs:5-14 (constructor line 11-13), 21-27 (Enter cast), 34-40 (Exit cast), 56 (Update cast); Assets/_Colonization/Scripts/FSM/BotStateMachine/BotStateMachine.cs:12 (ConstructState creation)
  Acceptance criteria: `dotnet build` succeeds; grep confirms zero `GetComponent` calls in ConstructState; grep confirms zero `as Bot` casts in ConstructState; ToolsProvider field passed via constructor
  QA scenarios: happy — build + zero GetComponent in ConstructState; failure — if ToolsProvider is null → `?.` operator handles gracefully; Evidence: grep "GetComponent" in ConstructState.cs returns zero; grep "as Bot" in ConstructState.cs returns zero
  Commit: Y | refactor: inject ToolsProvider via constructor in ConstructState

- [ ] 8. Add documentation comment to Bot.HasConstructTask — mark as "prefer FSM access"
  What to do / Must NOT do: Add XML doc comment to `HasConstructTask` setter: `/// <summary>Prefer BotRoster.CancelConstructTasks() or ExpandState for structured assignment. Direct set is for internal FSM wiring only.</summary>`. Do NOT change the setter visibility — it's public, leave it public to avoid breaking any code. This is a documentation-only change to encourage future cleaner access patterns.
  Parallelization: Wave 2 | Blocked by: none | Blocks: none
  References: Assets/_Colonization/Scripts/Bot/Bot.cs:16
  Acceptance criteria: `dotnet build` succeeds; XML doc comment added above HasConstructTask property
  QA scenarios: happy — build succeeds + doc comment present; failure — N/A (doc-only change); Evidence: grep "Prefer BotRoster" in Bot.cs returns one match
  Commit: Y | docs: add usage guidance comment to Bot.HasConstructTask setter

- [ ] 9. Cache BaseEventBinder GetComponent calls
  What to do / Must NOT do: In Bind() and Unbind(), cache `targetBase.GetComponent<ResourceWarhouse>()` (line 19,43), `targetBase.GetComponentInChildren<ResourceScanner>()` (line 24,48), `targetBase.GetComponentInChildren<ResourceCounterView>()` (line 25,49). Use variables declared at top of each method. Do NOT store them as fields (bind/unbind is temporary). Do NOT change logic.
  Parallelization: Wave 2 | Blocked by: none | Blocks: none
  References: Assets/_Colonization/Scripts/Base/BaseEventBinder.cs:14-60
  Acceptance criteria: `dotnet build` succeeds; no duplicate GetComponent/GetComponentInChildren calls in Bind/Unbind
  QA scenarios: happy — build + event wiring works; failure — if caching breaks the order → events fire incorrectly (spot check); Evidence: grep for GetComponent in BaseEventBinder.cs (expect 1 each per method max)
  Commit: Y | perf: eliminate duplicate GetComponent calls in BaseEventBinder

### Wave 3: Code quality (namespace, consistency)

- [ ] 10. Add ColonizationURP namespaces to all game scripts
  What to do / Must NOT do: Add namespace declarations to all 60+ game scripts under Assets/_Colonization/Scripts/. Convention: `ColonizationURP.{folder}` e.g. `ColonizationURP.Core`, `ColonizationURP.Base`, `ColonizationURP.Bot`, `ColonizationURP.FSM` (with sub-namespace for BotStateMachine/BaseStateMachine), `ColonizationURP.Resource`, `ColonizationURP.Player`, `ColonizationURP.Factories`. Add `using` directives where needed when scripts reference each other. Do NOT namespace the DOTween plugin code. Do NOT change any logic — pure organizational change.
  Parallelization: Wave 3 | Blocked by: Waves 1,2 complete (to avoid merge conflicts) | Blocks: none
  References: All .cs files under Assets/_Colonization/Scripts/ except DOTween plugin; package boundary: global namespace → namespaced
  Acceptance criteria: `dotnet build` succeeds; all game scripts have namespace; zero types in global namespace
  QA scenarios: happy — build with zero errors; failure — if missed a cross-reference → build error (caught by compiler); Evidence: grep -r "^namespace" Assets/_Colonization/Scripts/ | wc -l > 50
  Commit: Y | style: add ColonizationURP module namespaces

- [ ] 11. Cleanup Base.NewBaseBuilt event proxy — verify zero external callers
  What to do / Must NOT do: FIRST: run `grep -r "SubscribeToNewBaseBuilt\|UnsubscribeFromNewBaseBuilt" Assets/_Colonization/Scripts/` and `grep -r "NewBaseBuilt\s*\+=" Assets/_Colonization/Scripts/`. If both return ONLY Base.cs → safe to remove proxy methods. Remove SubscribeToNewBaseBuilt/UnsubscribeFromNewBaseBuilt proxy methods in Base.cs (lines 136-146). Simplify the `NewBaseBuilt` event accessor (lines 34-38) to directly delegate to `_expansionProvider?.NewBaseBuilt`. If any external callers found → SKIP this todo and report. Do NOT remove if external callers exist.
  Parallelization: Wave 3 | Blocked by: none | Blocks: none
  References: Assets/_Colonization/Scripts/Base/Base.cs:33-38,136-146; Assets/_Colonization/Scripts/Base/ExpansionProvider.cs:6
  Acceptance criteria: `dotnet build` succeeds IF and only if zero external callers to proxy methods. If callers found → todo is skipped safely.
  QA scenarios: happy — grep returns zero matches outside Base.cs → proxy methods safe to remove; failure — if external callers found → skip; Evidence: grep output for SubscribeToNewBaseBuilt in project
  Commit: Y (if safe) | refactor: remove redundant Base.NewBaseBuilt event proxy (zero external callers verified)

- [ ] 12. Fix defensive null-patterns in BotRoster, ExpandState, Base
  What to do / Must NOT do: In BotRoster.cs: remove redundant `bot == null` checks in HasOnConstructTask (line 30) and GetFreeBot (line 33) where Linq Predicate already handles null via `bot => bot != null`. In ExpandState.cs: remove the constructor field assigner and use `readonly` consistently (line 5 — already readonly but line syntax differs from IdleState). In Base.cs: remove `_roster.ClearNulls()` call in Awake (line 49) — BotRoster already handles nulls in its methods.
  Parallelization: Wave 3 | Blocked by: none | Blocks: none
  References: Assets/_Colonization/Scripts/Base/BotRoster.cs:29-34,44-45; Assets/_Colonization/Scripts/Base/Base.cs:49; Assets/_Colonization/Scripts/FSM/BaseStateMachine/ExpandState.cs:3-7
  Acceptance criteria: `dotnet build` succeeds; no duplicate null checks; BotRoster method behavior unchanged
  QA scenarios: happy — build + roster returns same results; failure — if removing ClearNulls breaks something → NullReference in roster ops; Evidence: grep for `bot == null && bot` (should be zero)
  Commit: Y | refactor: remove redundant defensive null-patterns

### Wave 4: Documentation & cleanup

- [ ] 13. Add /// summary XML docs to all public interfaces
  What to do / Must NOT do: Add `/// <summary>` XML doc comments to all public members of interfaces: IState (Enter, Update, Exit, IsBusy), IBase (all members), IBot (all members), IStateMachine (Update, TransitionTo, CurrentState). One-liner descriptions. Do NOT add to internal/private methods. Do NOT touch MonoBehaviours.
  Parallelization: Wave 4 | Blocked by: none | Blocks: none
  References: Assets/_Colonization/Scripts/FSM/StateMachine/IState.cs, Assets/_Colonization/Scripts/Base/IBase.cs, Assets/_Colonization/Scripts/Bot/IBot.cs, Assets/_Colonization/Scripts/FSM/StateMachine/IStateMachine.cs
  Acceptance criteria: `dotnet build` succeeds; each public interface member has /// summary
  QA scenarios: happy — build passes + grep for `/// <summary>` on each interface file shows all members; failure — if build fails due to malformed XML → caught by compiler; Evidence: count of /// summary lines
  Commit: Y | docs: add XML doc comments to public interfaces

## Final verification wave
> Runs in parallel after ALL todos. ALL must APPROVE. Surface results and wait for the user's explicit okay before declaring complete.
- [ ] F1. Plan compliance audit — every non-deferred todo marked complete, every referenced file touched, every commit message follows `type(scope): summary` convention
- [ ] F2. Structural correctness — grep verification: zero LINQ calls in Update/coroutine methods; zero `Debug.Log` in BaseWorkLoop; zero `GetComponent` in ConstructState; zero `as Bot` casts in ConstructState; zero `yield return null` in SpawnerResources; zero external calls to removed proxy methods
- [ ] F3. Build verification — `dotnet build D:\Project\ColonizationURP\ColonizationURP.sln` zero errors, zero new warnings introduced by changes
- [ ] F4. Scope fidelity — grep remaining issues: any file >200 lines without namespace; any [SerializeField] public field (should be private); any LINQ in MonoBehaviour Update methods; verify namespace count matches file count

## Commit strategy
- 1 commit per todo (13 commits total) — each commit is revertible independently
- Commit format: `<type>(<scope>): <summary>` as specified per todo
- Commit after each verification pass (NOT before)
- Types: `perf` (1-6), `refactor` (7-9, 11-12), `style` (10), `docs` (13)
- Atomic rollback: if any commit breaks `dotnet build`, revert ONLY that commit and continue with the rest

## Success criteria
1. **Build:** `dotnet build Colonization.sln` — zero errors, zero warnings in game code
2. **LINQ-free hot paths:** Zero LINQ calls in Update, coroutines, and work loops
3. **Zero string alloc in hot:** No Debug.Log/string concat in loops running every frame
4. **Clean cast pattern:** No `as Bot` or `as Base` pattern intended for hot path — cast only via direct field
5. **Namespaced:** All 60+ game scripts under namespace, zero types in global namespace
6. **Gameplay unchanged:** Run in Unity Editor — resource spawning, bot movement, base expansion works identically
