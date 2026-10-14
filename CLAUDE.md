# CLAUDE.md

## What this is

`Stella.Architecture.Tests` is a NuGet library (no test-framework dependency) that provides a fluent, reflection-based API for writing architecture tests in .NET: namespace isolation, forbidden assembly references, restricted type dependencies, and type/method conventions.

## Commands

```bash
dotnet test -- --filter-class "Stella.Architecture.Tests.Tests.SolutionArchitectureBuilderTests"
dotnet test -- --filter-method "*ShouldBeValidWhenNoRulesAreDefined"
```

- Tests use **xUnit v3 on Microsoft Testing Platform** (`UseMicrosoftTestingPlatformRunner`), so filters are passed after `--` as MTP options (`--filter-class`, `--filter-method`, `--filter-namespace`), not VSTest `--filter`.
- **Before every commit, run `dotnet format`** and include any resulting changes in the commit.
- Both projects set `EnforceCodeStyleInBuild` and `AnalysisMode=Recommended`, so analyzer warnings (CA*) appear in normal builds.

## Target frameworks

- Library: `netstandard2.0` with `LangVersion=latest`. Modern C# features work only via polyfills (e.g. `IsExternalInit.cs` for records/init). Avoid APIs not available in netstandard2.0 (e.g. `HashCode`, newer `string`/LINQ overloads); `System.Collections.Immutable` is the only runtime dependency.

## Architecture

Entry points are fluent builders; rules are collected during configuration and only evaluated in `ShouldBeValid()`, which throws `AssertArchitectureException` wrapping all individual violations (`AssertExceptions` array).

- `ShouldBeValid()` short-circuits once 15 violations accumulate, to keep error output bounded.
- `TypeDependenciesCache` (static, process-wide) computes a type's dependencies from reflection signatures only — base type, interfaces, fields, properties, constructor parameters, method return/parameter types — split into same-assembly ("internal") and other-assembly ("external") sets. Method bodies are **not** inspected, so dependencies only used inside method bodies are invisible to the rules.
- Violation exception types live in `Exceptions/` (`AssertTypeDependencyException`, `AssertAssembyDependencyException` [sic], `AssertTypeInvalidException`, `AssertMethodInvalidException`).

## Tests

The test project validates itself: `Stella.Architecture.Tests.Tests/App/` contains fixture types (e.g. `Tuna`, `Sardine`, `Salmon` namespaces, `AForbiddenDependencyClass` referencing Newtonsoft.Json) that tests assert against using `Assembly.GetExecutingAssembly()`. Adding types under `App/` can change the outcome of existing namespace/dependency tests.

## Versioning & release

- Versions come from **GitVersion** (`GitVersion.yml`, trunk-based). Commit messages can bump versions with `+semver: major|minor|patch|none`; default on `main` is patch.
- CI (`.github/workflows/ci.yml`) builds, tests, and packs on push/PR to `main`; pushes to `main` publish to NuGet.org via `publish.yml`.
- `README.md` is packed into the NuGet package — keep its usage examples in sync with the public API.
