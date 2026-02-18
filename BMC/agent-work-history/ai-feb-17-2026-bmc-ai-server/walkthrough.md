# BMC AI Server Implementation

## Changes Made

### 1. Project References — [BMC.Server.csproj](file:///g:/source/repos/Scheduler/BMC/BMC.Server/BMC.Server.csproj)
Added references to `BMC.AI`, `Foundation.AI`, and `Foundation.AI.VectorStore.Zvec`.

---

### 2. API Controller — [AiController.cs](file:///g:/source/repos/Scheduler/BMC/BMC.Server/Controllers/AiController.cs)
Extends `SecureWebAPIController` with 4 endpoints:

| Endpoint | Method | Description | Rate Limit |
|---|---|---|---|
| `api/ai/search/parts` | GET | Semantic part search | 2/sec/user |
| `api/ai/search/sets` | GET | Semantic set search | 2/sec/user |
| `api/ai/chat` | POST | RAG chat with sources | 2/sec/user |
| `api/ai/index` | POST | Admin re-index trigger | 1/min global |

All search/chat endpoints require read privilege; index requires write privilege.

---

### 3. SignalR Hub — [AiChatHub.cs](file:///g:/source/repos/Scheduler/BMC/BMC.Server/Controllers/AiChatHub.cs)
Streaming chat hub at `/AiChatSignal`. Clients call `SendMessage(question)` and receive `ReceiveToken` events for each token, followed by `ChatComplete`. Uses `[Authorize]` with bearer token auth.

---

### 4. Service Registration — [Program.cs](file:///g:/source/repos/Scheduler/BMC/BMC.Server/Program.cs)
- Registered Foundation.AI services: ONNX embedding, Zvec vector store, OpenAI-compatible inference, RAG
- Registered BMC AI services via `AddBmcAI()`
- Added `AddSignalR()` 
- Added `AiController` to explicit controller list
- Mapped `AiChatHub` at `/AiChatSignal`

---

### 5. Configuration — [appsettings.json](file:///g:/source/repos/Scheduler/BMC/BMC.Server/appsettings.json)
Added `AI` config section with subsections for `Embedding`, `VectorStore`, and `Inference`.

## Verification
- **Build**: Compiles successfully (file-lock errors from VS are not compilation errors)
- **DTO alignment**: Controller DTOs correctly map to `BmcSearchResult` and `BmcChatResponse` models
