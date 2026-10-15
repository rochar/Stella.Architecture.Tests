---
id: PLAN-20261015-fix-type-cache-race
status: in-progress # proposed | in-progress | done | abandoned
date: 2026-10-15
spec: none
adrs: none
summary: Make TypeDependenciesCache safe for concurrent use so consumers can run architecture tests in parallel
---
## Approach
Consumers commonly run several architecture test classes in parallel (xUnit's default), and
those classes often analyse the same assembly. Every rule that inspects dependencies goes
through `TypeDependenciesCache`, a process-wide static `Dictionary<Type, References>`, so
parallel tests read and write it from several threads at once. Today the only workaround is
for the consumer to turn test parallelization off; this plan removes the need for that.

**Root cause.** `EnsureTypeDependencies` does an unlocked `ContainsKey` check, and
`GetInternalReferenceTypes` / `GetExternalReferenceTypes` do an unlocked indexer read
(`_typeDependencies[type]`). Only the final write is inside `lock (_typeDependencies)`.
`Dictionary<TKey, TValue>` is not safe for a read concurrent with a write: a reader that
races a resize can miss an entry that was just added, so `_typeDependencies[type]` throws
`KeyNotFoundException` (surfacing from `DependencyValidator.ShouldBeValid` or
`NamespaceValidator`) for a type whose code never changed. The failure is intermittent and
depends on thread timing, so it shows up as a flaky test.

**Chosen design.** Replace the field with a `ConcurrentDictionary<Type, References>` and
read every entry through `GetOrAdd(type, BuildReferences)`, where `BuildReferences` is the
current reflection walk returning a `References` instead of writing into the dictionary.
Under contention a type may be built twice; that is harmless because the build only reads
reflection metadata and returns immutable sets, and `GetOrAdd` keeps a single winner. The
explicit `lock` goes away. `System.Collections.Concurrent` is part of netstandard2.0, so no
new package dependency is added.

Rejected alternatives:
- Lock around every read as well as the write — correct, but serializes all lookups behind one lock for no gain over `ConcurrentDictionary`.
- `ConcurrentDictionary<Type, Lazy<References>>` to build each entry exactly once — unnecessary, duplicate builds are pure and cheap.
- Consumers warm the cache in a fixture before tests run — relies on library internals and breaks for any type the warm-up misses.
- Regression test as `Parallel.For` over one cold assembly — tried first; 0 of 15 runs failed on the old code, too little write contention to catch the race.
- Consumers disable test parallelization — a valid stopgap, not a fix; it is what this plan makes unnecessary.

## Steps
- [x] 1. Add a concurrency regression test: from a cold cache, start many threads at once behind a gate, several per assembly, each calling `ShouldBeValid()` with a dependency rule (`WithDependencyUsedOnly<Tuna>()`) on one of ten large assemblies no other test analyses (BCL assemblies plus Newtonsoft.Json, NSubstitute, Castle.Core), and assert none throws. Many assemblies keep writes and resizes overlapping with reads; the cache is cold for them regardless of test order.
  - Files: `Stella.Architecture.Tests.Tests/TypeDependenciesCacheConcurrencyTests.cs` (new)
  - Verify: `dotnet test -- --filter-class "Stella.Architecture.Tests.Tests.TypeDependenciesCacheConcurrencyTests"` run repeatedly against the old code fails at least intermittently — observed 1 of 10 runs with `KeyNotFoundException: The given key 'System.Runtime.CompilerServices.NullableContextAttribute' was not present in the dictionary.`
- [x] 2. Switch `_typeDependencies` to `ConcurrentDictionary<Type, References>`, turn `EnsureTypeDependencies` into `private static References BuildReferences(Type type)` with the same reflection walk, make both getters `_typeDependencies.GetOrAdd(type, BuildReferences).Internal` / `.External`, and remove the `lock`.
  - Files: `Stella.Architecture.Tests/TypeDependenciesCache.cs`
  - Verify: `dotnet build` with no new CA* warnings; the step-1 test passes on repeated runs (30 of 30); the full `dotnet test` suite passes (41 tests).
- [ ] 3. Run `dotnet format`, then commit with a patch bump (the GitVersion default on `main`).
  - Files: any files `dotnet format` touches
  - Verify: `dotnet format --verify-no-changes` exits 0; CI on the PR is green.

## Risks
- The regression test is probabilistic and may pass on the old code on some machines — med — confirm it fails against the old code locally before the fix; keep it as a guard even if it only catches the race sometimes.
- Other hidden shared static state in validators could still race — low — a grep of the library finds no other static mutable fields; `DependencyValidator`'s allowed-types set is per instance.
- A slow regression test lengthens the suite — low — bound iterations so it runs in a couple of seconds.

## Verification
- `TypeDependenciesCache` holds no non-thread-safe collection and no read happens outside a thread-safe API.
- The new concurrency test fails (at least intermittently) on the old code and passes consistently, across repeated runs, on the new code.
- The full suite passes with `dotnet test`, the build adds no analyzer warnings, and `dotnet format --verify-no-changes` is clean.
- A consumer test project that analyses one assembly from several parallel test classes runs without `KeyNotFoundException` and without disabling parallelization.
