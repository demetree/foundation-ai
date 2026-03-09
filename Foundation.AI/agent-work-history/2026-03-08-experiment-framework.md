# Foundation.AI.Experiment — Autonomous Experimentation Framework

**Date:** 2026-03-08

## Summary

Created a new `Foundation.AI.Experiment` project inspired by Karpathy's [autoresearch](https://github.com/karpathy/autoresearch). It provides a reusable autonomous experimentation orchestrator that uses `IInferenceProvider` to have an LLM propose code modifications, runs experiments via subprocess execution, manages git history, and logs results in autoresearch-compatible TSV format.

## Changes Made

### New Files (Foundation.AI.Experiment/)

- **Foundation.AI.Experiment.csproj** — net10.0 class library, references `Foundation.AI.Inference` and `Microsoft.Extensions.Logging.Abstractions`
- **Models.cs** — `ExperimentConfig`, `ExperimentResult`, `CodeModification` records and `MetricDirection`/`ExperimentStatus` enums
- **IExperimentRunner.cs** — Interface for executing a single experiment
- **IExperimentAgent.cs** — Interface for LLM-driven code modification proposals
- **IExperimentLogger.cs** — Interface for persisting experiment results
- **GitManager.cs** — Git operations (branch, commit, revert) via `Process.Start("git", ...)`
- **MetricParser.cs** — Parses `key: value` metrics from script output with automatic MB→GB conversion
- **ProcessExperimentRunner.cs** — Runs scripts as child processes with time budget enforcement, output capture, and metric parsing
- **TsvExperimentLogger.cs** — Tab-separated results logging compatible with autoresearch's `results.tsv`
- **LlmExperimentAgent.cs** — Proposes code changes via `IInferenceProvider.ChatAsync`, builds structured prompts from code and history
- **ExperimentOrchestrator.cs** — Main experiment loop: propose → commit → run → keep/discard → log
- **ExperimentServiceExtensions.cs** — DI registration (`AddExperiment()`) with three overloads

### Modified Files

- **Foundation.AI/Foundation.AI.csproj** — Added `ProjectReference` to Experiment project
- **Foundation.AI/FoundationAIServiceExtensions.cs** — Added `using` and `AddExperiment()` to builder doc comments

## Key Decisions

- **Three clean interfaces** (`IExperimentRunner`, `IExperimentAgent`, `IExperimentLogger`) for independent testability and swappability
- **Git via CLI** rather than LibGit2Sharp — avoids a NuGet dependency; git is always available on dev machines
- **TSV format** (not CSV) for results — matches autoresearch convention and avoids comma conflicts in descriptions
- **MetricParser** uses regex for `key: value` lines — matches autoresearch's output format and is easily extensible
- **LlmExperimentAgent** requests the full modified file (not diffs) from the LLM — simpler and more reliable than patch-based approaches
- **ExperimentOrchestrator** separates `SetupAsync` (baseline) from `RunLoopAsync` (experiment loop) for flexibility

## Testing / Verification

- Built `Foundation.AI.Experiment.csproj` standalone — succeeded
- Built the umbrella `Foundation.AI.csproj` (all 9 projects) — all succeeded with no errors
- No runtime integration test yet (would require a git repo + a runnable script); interfaces support easy mocking for unit tests
