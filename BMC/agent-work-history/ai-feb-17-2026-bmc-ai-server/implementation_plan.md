# BMC AI — Web API + Angular Client

Add HTTP endpoints and Angular UI to expose the BMC.AI core library (semantic search + RAG chat) to users.

## Proposed Changes

### Server — BMC.Server

#### [MODIFY] [BMC.Server.csproj](file:///g:/source/repos/Scheduler/BMC/BMC.Server/BMC.Server.csproj)
- Add `ProjectReference` to `BMC.AI`
- Add `Microsoft.AspNetCore.SignalR` if not already included (should be in-box for ASP.NET Core)

---

#### [NEW] [AiController.cs](file:///g:/source/repos/Scheduler/BMC/BMC.Server/Controllers/AiController.cs)
Extends `SecureWebAPIController` (matching `PartsCatalogController` pattern):

| Endpoint | Method | Description |
|----------|--------|-------------|
| `api/ai/search/parts` | GET | `?query=...&topK=10` → semantic part search |
| `api/ai/search/sets` | GET | `?query=...&topK=10` → semantic set search |
| `api/ai/chat` | POST | `{ question }` → RAG chat response |
| `api/ai/index` | POST | Admin-only: triggers `BmcSearchIndex.IndexAllAsync()` |

DTOs as inner classes. Uses `DoesUserHaveReadPrivilegeSecurityCheckAsync` for read endpoints and write-level for index.

---

#### [NEW] [AiChatHub.cs](file:///g:/source/repos/Scheduler/BMC/BMC.Server/Controllers/AiChatHub.cs)
SignalR hub for streamed chat responses. Following the Compactica `DashboardSignalRService` pattern:
- Client invokes `SendMessage(question)` 
- Hub streams back `ReceiveToken(token)` fragments via `ChatStreamAsync`
- Sends `ChatComplete` when done
- Uses bearer token auth via `accessTokenFactory`

---

#### [MODIFY] [Program.cs](file:///g:/source/repos/Scheduler/BMC/BMC.Server/Program.cs)
- Register `AddBmcAI()` + Foundation.AI services (`AddFoundationEmbedding`, `AddFoundationVectorStore`, `AddFoundationRag`)
- Add `controllers.Add(typeof(AiController))`
- Add `app.MapHub<AiChatHub>("/AiChatSignal")`
- Add `builder.Services.AddSignalR()`

> [!IMPORTANT]
> Foundation.AI services need configuration (API key, model, vector store path). These will be added to `appsettings.json`. We'll need to determine which embedding provider and inference provider to configure.

---

### Client — BMC.Client

#### [NEW] [ai.service.ts](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/services/ai.service.ts)
Angular service wrapping the AI API endpoints:
- `searchParts(query, topK)` → `Observable<AiSearchResult[]>`
- `searchSets(query, topK)` → `Observable<AiSearchResult[]>`  
- `chat(question)` → `Observable<AiChatResponse>`
- Uses `AuthService.GetAuthenticationHeaders()` pattern

---

#### [NEW] [ai-chat-signalr.service.ts](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/services/ai-chat-signalr.service.ts)
SignalR service for streaming chat (follows Compactica `DashboardSignalRService` pattern):
- `connect()` / `disconnect()` with ref-counting
- Exposes `chatResponse$` as `BehaviorSubject<string>`
- `sendMessage(question)` invokes hub method
- `accessTokenFactory` for auth

---

#### [NEW] [ai-assistant/](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/components/ai-assistant/) component
Full-page AI assistant with two tabs:

**Search Tab:**
- Toggle between Parts / Sets search
- Search input with debounce
- Results grid with part/set cards showing name, ID, score, metadata

**Chat Tab:**
- Chat message list with user/assistant bubbles
- Input field + send button
- Streaming tokens via SignalR displayed in real-time
- LEGO-themed styling consistent with BMC's `--bmc-*` design tokens

---

#### [MODIFY] [app-routing.module.ts](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/app-routing.module.ts)
- Add route: `{ path: 'ai', component: AiAssistantComponent }`

#### [MODIFY] [app.module.ts](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/app.module.ts)
- Declare `AiAssistantComponent`

#### [MODIFY] [package.json](file:///g:/source/repos/Scheduler/BMC/BMC.Client/package.json)
- Add `@microsoft/signalr` dependency

---

## User Review Required

> [!IMPORTANT]
> **Foundation.AI provider configuration**: The AI controller needs an embedding provider and inference provider configured in `appsettings.json`. Which providers should we use?
> - **Embedding**: OpenAI `text-embedding-3-small`? Local? Other?
> - **Inference**: OpenAI `gpt-4o`? Gemini? Other?
> - **Vector store path**: Where should the Zvec data directory live? (e.g. `./ai-data/vectors`)

> [!IMPORTANT]
> **Navigation**: Should we add an "AI Assistant" link to the BMC sidebar/header nav? If so, what icon/position?

## Verification Plan

### Build Verification
- `dotnet build BMC\BMC.Server\BMC.Server.csproj` — zero errors
- `cd BMC\BMC.Client && npm install && ng build` — zero errors

### Functional Testing
- Verify search endpoints return results via Swagger
- Verify SignalR hub connects and streams tokens
- Verify Angular components render and interact correctly
