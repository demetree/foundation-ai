# My Shift Page - On-Call Status View

A dedicated page showing the current user's on-call schedule status, organized for mobile-first responder use.

## Proposed Changes

### [NEW] MyShiftComponent

#### [NEW] [my-shift.component.ts](file:///g:/source/repos/Scheduler/Alerting/Alerting.Client/src/app/components/my-shift/my-shift.component.ts)

**Core Logic:**
- Get current user from `AuthService`
- Load schedules where user is a participant via `OnCallScheduleService`
- Calculate "Am I on-call now?" by checking rotations against current time
- Load active overrides affecting user via `ScheduleOverrideService`
- Load escalation policies referencing user

**Key Methods:**
- `calculateCurrentShift()` - Determine if user is currently on-call
- `getNextShift()` - Find when user's next on-call period starts
- `getUpcomingShifts()` - List next 7 days of on-call periods

#### [NEW] [my-shift.component.html](file:///g:/source/repos/Scheduler/Alerting/Alerting.Client/src/app/components/my-shift/my-shift.component.html)

**Layout:**
```
┌─────────────────────────────────────┐
│  ← Back             My Shift    🔄  │
├─────────────────────────────────────┤
│  ┌───────────────────────────────┐  │
│  │  🟢 YOU ARE ON-CALL          │  │  ← Status hero
│  │  Primary Support Schedule     │  │
│  │  Until 8:00 AM tomorrow      │  │
│  └───────────────────────────────┘  │
├─────────────────────────────────────┤
│  📅 Upcoming Shifts                 │
│  ┌───────────────────────────────┐  │
│  │ Mon Feb 3  • 8:00 AM - 5:00 PM│  │
│  │ Primary Support               │  │
│  └───────────────────────────────┘  │
│  ┌───────────────────────────────┐  │
│  │ Tue Feb 4  • 8:00 AM - 5:00 PM│  │
│  └───────────────────────────────┘  │
├─────────────────────────────────────┤
│  ⚠️ Active Overrides               │
│  ┌───────────────────────────────┐  │
│  │ Coverage for John Doe         │  │
│  │ Feb 5, 2:00 PM - 6:00 PM     │  │
│  └───────────────────────────────┘  │
├─────────────────────────────────────┤
│  My Escalation Policies (3)        │
│  • API Gateway - Level 1           │
│  • Payment Service - Level 2       │
│  • Auth Service - Level 1          │
└─────────────────────────────────────┘
```

#### [NEW] [my-shift.component.scss](file:///g:/source/repos/Scheduler/Alerting/Alerting.Client/src/app/components/my-shift/my-shift.component.scss)

**Mobile-First Styling:**
- Status hero with green (on-call) / gray (off-duty) gradient
- Card-based shift list with clear date/time formatting
- Override cards with warning styling
- Responsive breakpoints matching Responder Console

---

### Routing

#### [MODIFY] [app-routing.module.ts](file:///g:/source/repos/Scheduler/Alerting/Alerting.Client/src/app/app-routing.module.ts)

Add route:
```typescript
{ path: 'my-shift', component: MyShiftComponent, ... }
```

---

### Module Registration

#### [MODIFY] [app.module.ts](file:///g:/source/repos/Scheduler/Alerting/Alerting.Client/src/app/app.module.ts)

- Import and declare `MyShiftComponent`

---

### Wire Bottom Nav

#### [MODIFY] [responder-console.component.ts](file:///g:/source/repos/Scheduler/Alerting/Alerting.Client/src/app/components/responder-console/responder-console.component.ts)

Add navigation method:
```typescript
goToMyShift(): void {
    this.router.navigate(['/my-shift']);
}
```

#### [MODIFY] [responder-console.component.html](file:///g:/source/repos/Scheduler/Alerting/Alerting.Client/src/app/components/responder-console/responder-console.component.html)

Enable the My Shift button and wire click handler.

---

## Verification Plan

### Build Verification
```powershell
npm run build
```

### Manual Testing
1. Navigate to `/my-shift`
2. Verify on-call status calculation
3. Check upcoming shifts display
4. Test mobile viewport
