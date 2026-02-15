# Finish Volunteer & Volunteer Group Custom Components

Complete the remaining gaps in the volunteer and volunteer-group custom UI components to bring them to the same level of maturity as `client-custom` and `crew-custom`.

## Proposed Changes

### Volunteer Profile Detail — History Tab

Currently the History tab shows placeholder text: *"Change history will be loaded here."*

#### [MODIFY] [volunteer-custom-detail.component.ts](file:///d:/source/repos/scheduler/Scheduler/Scheduler.Client/src/app/components/volunteer-custom/volunteer-custom-detail/volunteer-custom-detail.component.ts)
- Add `auditHistory: any[] | null = null` and `isLoadingHistory = false` fields
- Inject `VolunteerProfileService` (already injected) and `Router` + `ActivatedRoute`
- Add `loadHistory()` method following the `client-custom-detail` pattern:
  ```ts
  loadHistory(): void {
      if (this.auditHistory != null || !this.volunteer) return;
      this.isLoadingHistory = true;
      this.volunteerProfileService.GetVolunteerProfileAuditHistory(this.volunteer.id as number, true).subscribe({
          next: (data) => { this.auditHistory = data || []; this.isLoadingHistory = false; },
          error: () => { this.auditHistory = []; this.isLoadingHistory = false; }
      });
  }
  ```
- Update `onTabChange()` to call `loadHistory()` when `activeTab === 'history'`
- Reset `auditHistory = null` in `onVolunteerChanged()` so it re-fetches after edits

#### [MODIFY] [volunteer-custom-detail.component.html](file:///d:/source/repos/scheduler/Scheduler/Scheduler.Client/src/app/components/volunteer-custom/volunteer-custom-detail/volunteer-custom-detail.component.html)
- Replace History tab placeholder with:
  ```html
  <app-change-history-viewer
      [auditHistory]="auditHistory"
      [isLoading]="isLoadingHistory"
      [entityName]="'Volunteer Profile'"
      [excludeFields]="['objectGuid', 'avatarData', 'avatarFileName', 'avatarSize', 'avatarMimeType']">
  </app-change-history-viewer>
  ```

---

### Volunteer Group Detail — History Tab

Same placeholder issue as above.

#### [MODIFY] [volunteer-group-custom-detail.component.ts](file:///d:/source/repos/scheduler/Scheduler/Scheduler.Client/src/app/components/volunteer-group-custom/volunteer-group-custom-detail/volunteer-group-custom-detail.component.ts)
- Add `auditHistory` / `isLoadingHistory` fields
- Add `loadHistory()` method using `GetVolunteerGroupAuditHistory`
- Update `onTabChange()` to trigger on 'history'
- Reset `auditHistory = null` in `onGroupChanged()`

#### [MODIFY] [volunteer-group-custom-detail.component.html](file:///d:/source/repos/scheduler/Scheduler/Scheduler.Client/src/app/components/volunteer-group-custom/volunteer-group-custom-detail/volunteer-group-custom-detail.component.html)
- Replace History tab placeholder with `<app-change-history-viewer>` (same pattern)

---

### Volunteer Group Members Tab — Add/Remove Workflow

Currently the Members tab is read-only — no way to add or remove members from the UI.

#### [NEW] [volunteer-group-add-member-modal.component.ts](file:///d:/source/repos/scheduler/Scheduler/Scheduler.Client/src/app/components/volunteer-group-custom/volunteer-group-add-member-modal/volunteer-group-add-member-modal.component.ts)
New modal component modeled on `crew-add-to-crew-modal`:
- Form fields: Resource (dropdown), Assignment Role (dropdown), Sequence, Joined Date
- Submits via `VolunteerGroupMemberService.PostVolunteerGroupMember()`
- Closes modal with newly created member on success

#### [NEW] [volunteer-group-add-member-modal.component.html](file:///d:/source/repos/scheduler/Scheduler/Scheduler.Client/src/app/components/volunteer-group-custom/volunteer-group-add-member-modal/volunteer-group-add-member-modal.component.html)
Modal template with resource dropdown, role dropdown, sequence, and joined date fields.

#### [NEW] [volunteer-group-add-member-modal.component.scss](file:///d:/source/repos/scheduler/Scheduler/Scheduler.Client/src/app/components/volunteer-group-custom/volunteer-group-add-member-modal/volunteer-group-add-member-modal.component.scss)
Minimal styling consistent with existing modal patterns.

#### [MODIFY] [volunteer-group-members-tab.component.ts](file:///d:/source/repos/scheduler/Scheduler/Scheduler.Client/src/app/components/volunteer-group-custom/volunteer-group-members-tab/volunteer-group-members-tab.component.ts)
- Add `openAddMemberModal()` method (opens `VolunteerGroupAddMemberModalComponent` via `NgbModal`)
- Add `removeMember()` method with confirmation dialog
- Add Output event `memberChanged` for parent to refresh counts
- Wire up reload after add/remove

#### [MODIFY] [volunteer-group-members-tab.component.html](file:///d:/source/repos/scheduler/Scheduler/Scheduler.Client/src/app/components/volunteer-group-custom/volunteer-group-members-tab/volunteer-group-members-tab.component.html)
- Add header with "Add Member" button + refresh button (matching `crew-members-tab` pattern)
- Add action buttons per member row: edit membership link, remove button
- Link member names to volunteer profile detail page (`/volunteers/{id}`) when volunteer profile exists

#### [MODIFY] [app.module.ts](file:///d:/source/repos/scheduler/Scheduler/Scheduler.Client/src/app/app.module.ts)
- Declare `VolunteerGroupAddMemberModalComponent`

## Verification Plan

### Automated Tests
- Run `ng build` from `Scheduler.Client` to confirm compilation:
  ```
  cd d:\source\repos\scheduler\Scheduler\Scheduler.Client
  npx ng build
  ```

### Manual Verification
Since this is pure UI work on custom components, the primary verification is visual via the dev server:

1. **Navigate to a Volunteer detail page** → click the "History" tab → confirm the `change-history-viewer` loads and displays version history (or shows "No history available" if new)
2. **Navigate to a Volunteer Group detail page** → click "History" tab → same verification
3. **On the Volunteer Group detail page** → click "Members" tab → confirm "Add Member" button appears → click it → confirm modal opens with Resource/Role/Sequence/Joined Date fields → submit → confirm new member appears in list
4. **On a member row** → confirm remove button is visible → click it → confirm deletion prompt → confirm member removed from list after confirmation

> [!NOTE]
> Since the dev server requires authentication infrastructure, I'll focus on ensuring a clean `ng build` compilation. Full visual testing can be done when you run the app locally.
