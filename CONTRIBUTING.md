# Contributing to Foundation.AI

Thank you for considering a contribution. Foundation.AI is maintained by a single developer alongside a closed-source product, so a few pieces of context up front:

- **Response time.** Issues and PRs are read within a week, but a substantive reply may take longer. If something is urgent, say so in the issue title.
- **Scope.** Foundation.AI exists to be a pragmatic .NET inference / embedding / RAG / document stack. Contributions that broaden support (more inference providers, more vector backends, more document formats) are very welcome. Contributions that add framework-style ceremony (heavy abstractions, mandatory pipelines, opinionated conventions) usually are not.
- **No CLA.** By contributing you agree your changes are licensed under [Apache-2.0](LICENSE). That's it.

## Reporting issues

A useful issue includes:

- What you tried to do (one sentence).
- What happened (one sentence).
- A minimal repro — ideally a small console program or a failing test.
- Your OS, .NET version (`dotnet --info`), and which inference backend / model you're using.

For LLamaSharp / ONNX / BitNet runtime issues, **please name the exact native package and version** you have referenced. Most "it doesn't work on my GPU" reports trace back to backend-package mismatch (see the README's note on the LLamaSharp Blackwell trap).

## Setting up a dev environment

Prerequisites:

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- Git
- For BitNet: `cmake` and a C++ toolchain if you need to build native binaries locally. Pre-built binaries are not redistributed in this repo.

```bash
git clone https://github.com/demetree/foundation-ai.git
cd foundation-ai
dotnet restore Foundation.AI.sln
dotnet build Foundation.AI.sln --configuration Release
dotnet test Foundation.AI.sln --configuration Release
```

Tests that require native binaries or downloaded models are guarded with `[Trait("RequiresNative", "true")]` or skipped if the model path is unset. CI runs the managed-only subset.

## Pull request process

1. **Open an issue first** for anything larger than a typo, a one-file bug fix, or a docs tweak. A 30-second alignment check saves both of us a 30-minute disagreement after the work is done.
2. **Branch from `main`.** Use a short kebab-case branch name (`feature/sparse-embeddings`, `fix/zvec-segfault-on-arm`).
3. **One concern per PR.** Don't bundle a refactor with a bug fix.
4. **Keep the diff small.** PRs over a few hundred lines tend to languish — a small change merged this week beats a large change still under review next quarter.
5. **Tests.** New behavior needs at least one test. Bug fixes should include a regression test that fails before the fix and passes after.
6. **No new top-level dependencies** without discussion. Each NuGet reference is a maintenance burden.
7. **Run the build locally** before pushing: `dotnet build && dotnet test`. CI will catch what you miss, but every red CI run costs everyone a few minutes.

## Code style

- Follow the conventions already in the file you're editing.
- Public APIs need XML doc comments. Internal helpers don't.
- Prefer `sealed` classes and `readonly` fields by default.
- Prefer interfaces with one or two methods over interfaces with twelve.
- No `async void` except for event handlers.
- No `Thread.Sleep` in production code.

## Adding a new inference provider

Implement `IInferenceProvider` in a project named `Foundation.AI.Inference.<YourProvider>`. Reference `Foundation.AI.Inference` and (if needed) the upstream provider's NuGet package — but keep that package in *your* new project, not bubbled up into the abstraction.

Provide:

1. The provider class itself (sync API at minimum, streaming if the underlying API supports it).
2. A `*ServiceCollectionExtensions` class with an `Add<YourProvider>Inference(this IServiceCollection)` extension.
3. Tests that don't require a live model (mock the network calls). Live-model tests can be `[Trait("RequiresNetwork", "true")]` and excluded from CI.
4. A line in the README's "What's inside" table.

## Adding a new document converter

Implement `IDocumentConverter` in a project named `Foundation.AI.MarkItDown.<Format>`. Same shape as inference providers: own NuGet refs, own DI extension, own tests, README entry.

## Releasing

Releases are cut from `main` by the maintainer. NuGet packages publish via tag-triggered CI. There is no fixed cadence — packages ship when something useful is ready.

## Security

If you find a security issue, **please do not open a public issue.** Email the maintainer or use GitHub's private vulnerability reporting. See [SECURITY.md](SECURITY.md) (TODO).

## Code of Conduct

Be kind. Be specific. Don't make it weird. That's the policy.
