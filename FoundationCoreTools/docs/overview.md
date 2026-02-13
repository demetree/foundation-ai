# FoundationCoreTools — Overview

FoundationCoreTools is a **.NET 10** console application providing developer utility tools for the Foundation platform.

---

## Contents

| File | Size | Purpose |
|------|------|---------|
| `Program.cs` | ~14 KB | Interactive console menu with developer actions |
| `DatabaseScriptGenerator.cs` | ~10 KB | Generates SQL scripts from Foundation database definitions |

---

## Purpose

This tool provides a **menu-driven console UI** for developers to perform administrative and code generation tasks:

- Reset user passwords
- Generate database SQL scripts for the Security and Auditor databases
- Run Foundation-level code generation
- Other developer-facing platform utilities

---

## Usage

```powershell
cd FoundationCoreTools
dotnet run
```

Then follow the interactive menu prompts.

---

## Dependencies

- `CodeGenerationCore` — code generation engine
- `CodeGenerationCommon` — `DatabaseGenerator` base
- `FoundationCore` — Security/Auditor modules
- Database generator projects (Security, Auditor) from `DatabaseGenerators/`
