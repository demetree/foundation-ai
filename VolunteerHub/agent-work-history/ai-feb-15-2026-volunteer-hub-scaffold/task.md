# Volunteer Self-Service Hub

## Phase 3 (Complete)
- [x] Feature 6: Dashboard Alerts
- [x] Feature 7: CSV Export
- [x] Feature 8: Availability Calendar
- [x] Feature 9: Smart Suggestions

## Phase A: Hub Skeleton + Auth

### Database Schema
- [x] Add `linkedUserGuid` to VolunteerProfile
- [ ] Re-run database generator

### Server API
- [x] Create `VolunteerHubController` (auth + data endpoints)

### Client App
- [x] Scaffold Angular SPA (`VolunteerHub.Client`)
- [x] Auth service + OTP login flow
- [x] Auth guard + session management
- [x] Hub shell layout (bottom tabs mobile)
- [x] Login page (email/phone + code entry)
- [x] Dashboard shell page
- [x] Schedule shell page
- [x] Hours shell page
- [x] Profile shell page

### Admin Integration
- [ ] Add "Linked User" field to volunteer add/edit

### Verification
- [x] Server build verified (exit code 0)
- [x] Angular client build verified (dist generated)
- [ ] Test OTP login flow end-to-end
