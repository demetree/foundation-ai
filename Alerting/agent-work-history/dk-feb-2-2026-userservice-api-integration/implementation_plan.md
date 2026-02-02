# Escalation Policy Editor - Full-Page Implementation

Implement a premium full-page editor for configuring escalation policies with their rules.

## Proposed Changes

### Frontend - New Component

#### [NEW] [escalation-policy-editor.component.ts](file:///g:/source/repos/Scheduler/Alerting/Alerting.Client/src/app/components/escalation-policy-editor/escalation-policy-editor.component.ts)

Full-page editor component with:

**Header Section:**
- Back button → returns to `/escalation-policy-management`
- Policy name (editable inline or in form)
- Status badge (Active/Inactive)
- Save & Cancel buttons

**Policy Metadata Card:**
- Name input
- Description textarea
- Active toggle

**Escalation Rules Timeline:**
- Visual step-by-step timeline showing escalation flow
- Each rule displayed as a "step" card with:
  - Step number with delay indicator (e.g., "After 5 min")
  - Target type dropdown: `User`, `Schedule`, or `All Users`
  - Target selector (user picker / schedule picker based on type)
  - Repeat settings: count + delay
  - Remove button
- "Add Step" button at end of timeline
- Drag-to-reorder support (using Angular CDK DragDrop)

**Linked Services Section:**
- List of services using this policy
- Link to Service Management for configuration

---

#### [NEW] [escalation-policy-editor.component.html](file:///g:/source/repos/Scheduler/Alerting/Alerting.Client/src/app/components/escalation-policy-editor/escalation-policy-editor.component.html)

Template structure:
```html
<div class="policy-editor-container">
  <!-- Hero Header -->
  <div class="editor-header">
    <button (click)="goBack()"><i class="fa-solid fa-arrow-left"></i></button>
    <h1>{{ policy?.name || 'New Policy' }}</h1>
    <div class="header-actions">
      <button class="btn btn-secondary" (click)="cancel()">Cancel</button>
      <button class="btn btn-primary" (click)="save()">Save Changes</button>
    </div>
  </div>

  <!-- Policy Metadata -->
  <div class="metadata-card">...</div>

  <!-- Escalation Rules Timeline -->
  <div class="rules-section">
    <h2>Escalation Timeline</h2>
    <div cdkDropList (cdkDropListDropped)="onRuleDrop($event)">
      <div *ngFor="let rule of rules" cdkDrag class="rule-step">
        <!-- Step visualization -->
      </div>
    </div>
    <button (click)="addRule()">+ Add Escalation Step</button>
  </div>

  <!-- Linked Services (read-only) -->
  <div class="services-section">...</div>
</div>
```

---

#### [NEW] [escalation-policy-editor.component.scss](file:///g:/source/repos/Scheduler/Alerting/Alerting.Client/src/app/components/escalation-policy-editor/escalation-policy-editor.component.scss)

Premium styling following existing patterns from `integration-management.component.scss`.

---

### Frontend - Routing Update

#### [MODIFY] [app-routing.module.ts](file:///g:/source/repos/Scheduler/Alerting/Alerting.Client/src/app/app-routing.module.ts)

Add new route after line 157:
```typescript
{ path: 'escalation-policy-management/:id/edit', component: EscalationPolicyEditorComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Escalation Policy' },
```

---

### Frontend - Management Component Update

#### [MODIFY] [escalation-policy-management.component.ts](file:///g:/source/repos/Scheduler/Alerting/Alerting.Client/src/app/components/escalation-policy-management/escalation-policy-management.component.ts)

- Inject `Router`
- Modify `openEditModal` to navigate to editor page instead:
```typescript
openEditModal(policy: EscalationPolicyData): void {
  this.router.navigate(['/escalation-policy-management', policy.id, 'edit']);
}
```

---

### Data Flow

**Loading:**
1. Editor reads `:id` from route params
2. Fetches policy via `EscalationPolicyService.GetEscalationPolicy(id, includeRelations=true)`
3. Fetches rules via `EscalationRuleService.GetEscalationRuleList({ escalationPolicyId: id })`
4. Loads available users and schedules for target selection

**Saving:**
1. Update policy metadata via `EscalationPolicyService.UpdateEscalationPolicy()`
2. Sync rules using Relational Synchronizer pattern:
   - Compare current rules to original
   - POST new rules
   - PUT updated rules  
   - Soft-delete removed rules (set `deleted=true`)

---

### Target Types

The `targetType` field supports:
- `User` - Individual user (objectGuid = user's objectGuid)
- `Schedule` - On-call schedule (objectGuid = schedule's objectGuid)
- `AllUsers` - Notify all users (objectGuid = null)

---

## Verification Plan

### Build Verification
```powershell
cd g:\source\repos\Scheduler\Alerting\Alerting.Client
npm run build
```

### Manual Testing
1. Navigate to Escalation Policy Management
2. Click Edit on a policy → should open full-page editor
3. Modify policy name/description
4. Add/edit/remove/reorder escalation rules
5. Save changes → verify persistence
6. Cancel → verify no changes saved
