# Seed MOC From Steps — Admin Testing Tool

Take an existing BMC project and publish it to MOCHub where each building step becomes a separate version. This creates realistic test data for the version diff viewer, 3D diff preview, and version management UI.

## Proposed Changes

### Server — New Seed Endpoint

#### [MODIFY] [MocHubController.cs](file:///g:/source/repos/Scheduler/BMC/BMC.Server/Controllers/MocHubController.cs)

Add `POST /api/mochub/admin/seed-from-steps` — admin-only endpoint:

**Input:** `{ projectId: int }`

**Algorithm:**
1. Auth check: `GetSecurityUserAsync` → `DoesUserHaveAdminPrivilegeSecurityCheckAsync`
2. Load `Project` by id + tenant
3. Load all `ModelStepPart` rows through `ModelSubFile → ModelBuildStep`, ordered by `ModelSubFile.isMainModel DESC, ModelBuildStep.stepNumber ASC, ModelStepPart.sequence ASC`
4. Group by step number to get a list of steps, each containing their parts
5. Create a `PublishedMoc` record:
   - Name: `"[TEST] {project.name}"` (tagged for easy identification/cleanup)
   - Slug: auto-generated with `[TEST]` prefix
   - Visibility: `"Unlisted"` (visible by direct link only, won't pollute explore page)
6. **For each step N (1..totalSteps):**
   - Build a cumulative LDraw MPD string containing all parts from steps 1 through N
   - Each part line is reconstructed from `ModelStepPart` fields (`colorCode`, `positionX/Y/Z`, `transformMatrix`, `partFileName`)
   - Create a `MocVersion` record with `versionNumber = N`, `mpdSnapshot = cumulativeMpd`, `commitMessage = "Step N: added X parts"`
   - Set diff stats (added/removed/modified) by comparing with previous step
7. Update `PublishedMoc.currentVersionNumber` to the total step count
8. Return summary: `{ publishedMocId, versionCount, totalParts }`

> [!IMPORTANT]
> The MPD is built directly from `ModelStepPart` raw data (transform matrix strings, original part filenames) rather than going through `PlacedBrick → ModelExportService`. This preserves the original per-step granularity that `PlacedBrick.buildStepNumber` may not perfectly reflect for complex models.

---

### Client — Admin UI Trigger

#### [MODIFY] [mochub-explore.component.html](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/components/mochub-explore/mochub-explore.component.html)
- Add an admin-only "🧪 Seed MOC from Steps" button in the hero section (next to Publish button)
- Button opens a simple inline form: project ID input + confirm button

#### [MODIFY] [mochub-explore.component.ts](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/components/mochub-explore/mochub-explore.component.ts)
- Add `seedProjectId`, `seeding`, `seedResult` properties
- Add `seedMocFromSteps()` method that POSTs to the new endpoint and displays the result
- Add `isAdmin` check (use existing `authService.currentUser` roles)

#### [MODIFY] [mochub-explore.component.scss](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/components/mochub-explore/mochub-explore.component.scss)
- Styles for the seed form and result message

---

## Verification Plan

### Manual Verification
1. Log in as an admin user
2. Find an existing project ID with multiple build steps
3. Use the seed button to create a test MOC
4. Navigate to the test MOC's detail page
5. Verify all versions appear in the versions tab
6. Click through version diffs — verify added/removed part counts make sense
7. Toggle the 3D diff viewer — verify color-coded bricks are rendered
8. Verify the `[TEST]` tag makes the seeded MOC identifiable
