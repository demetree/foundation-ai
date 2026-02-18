# BMC AI — Web API + Client

## Server (BMC.Server)
- [x] Add BMC.AI project reference to BMC.Server.csproj
- [x] Create `AiController` (search parts/sets, chat, index endpoints)
- [x] Create `AiChatHub` (SignalR streaming chat)
- [x] Register services + controller + hub in Program.cs
- [x] Add Foundation.AI config to appsettings.json

## Client (BMC.Client)
- [ ] Install `@microsoft/signalr` package
- [ ] Create `ai.service.ts` (HTTP endpoints)
- [ ] Create `ai-chat-signalr.service.ts` (streaming chat)
- [ ] Create `ai-assistant` component (search + chat tabs)
- [ ] Add route and module registration
- [ ] Add nav link to sidebar/header

## Verification
- [ ] Server build passes
- [ ] Client build passes
- [ ] All existing tests still pass
