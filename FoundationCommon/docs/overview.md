# FoundationCommon — Overview

FoundationCommon is a **.NET Standard 2.1** class library providing cross-platform utilities used by all Foundation projects. It targets .NET Standard to remain compatible with both .NET Core and .NET Framework consumers (if any).

---

## Contents

### Concurrent Collections (`Concurrent/`)

| Class | Purpose |
|-------|---------|
| `ConcurrentList<T>` | Thread-safe list implementation |
| `ConcurrentQueueWithRecent<T>` | Thread-safe queue that also tracks the most recent items |
| `ExpiringCache<TKey, TValue>` | Thread-safe cache with automatic time-based expiration |
| `ThreadSafeBool` | Lock-free boolean for cross-thread signaling |

### Utilities (`Utility/`)

| Class | Purpose |
|-------|---------|
| `Basics` | Fundamental helper methods |
| `Constants` | Platform-wide constants |
| `DateTime` | Date/time formatting, parsing, and manipulation helpers |
| `DoubleMetaphone` | Phonetic algorithm for fuzzy name matching |
| `Encryption` | AES encryption/decryption utilities |
| `Extensions` | General .NET extension methods |
| `Network` | Network utility methods |
| `StringUtility` | String manipulation and formatting |

---

## Usage

Referenced transitively by all Foundation projects via **FoundationCore → FoundationCommon**. Application projects rarely reference FoundationCommon directly.

```
FoundationCore.Web → FoundationCore → FoundationCommon
```
