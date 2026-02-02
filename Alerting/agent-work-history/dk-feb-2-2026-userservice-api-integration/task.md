# Expose UserService API & Update Editor

## Backend
- [x] Register `IUserService` in DI (Program.cs) - already existed
- [x] Create `UsersController` with endpoints
  - [x] GET /api/Users - List all applicable users
  - [x] GET /api/Users/{userGuid} - Get specific user
  - [x] GET /api/Teams - List all teams
  - [x] GET /api/Teams/{teamGuid} - Get specific team
  - [x] GET /api/Teams/{teamGuid}/Users - Get team members

## Frontend
- [x] Create Angular `AlertingUserService` to call new endpoints
- [x] Update `EscalationPolicyEditorComponent` to:
  - [x] Load users from new service
  - [x] Replace text input with user dropdown
  - [x] Display user name in timeline

## Verification
- [x] Backend build succeeds
- [x] Frontend build succeeds
