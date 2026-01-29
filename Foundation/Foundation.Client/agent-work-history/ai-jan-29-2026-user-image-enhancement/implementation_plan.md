# User Image Enhancement Implementation Plan

Improve user profile image functionality by adding image management to the add-edit modal for better discoverability and visually refreshing the existing image upload modal with premium styling.

---

## Proposed Changes

### Phase 1: Premium Refresh of Image Upload Modal

#### [MODIFY] [user-image-upload.component.scss](file:///d:/repos/Scheduler/Foundation/Foundation.Client/src/app/components/user-custom/user-image-upload/user-image-upload.component.scss)

Update styling to match the premium design system:

- **Header gradient**: Change from `#1565c0 → #42a5f5` to the standard premium gradient `#0d47a1 → #1976d2 → #42a5f5`
- **Drop zone glassmorphism**: Add subtle backdrop blur and gradient border for a modern glass effect
- **Hover/drag states**: Add smooth transitions and glow effects
- **Icon animation**: Add pulse animation on the upload icon for visual interest
- **Button styling**: Match the premium glass button style used elsewhere

---

#### [MODIFY] [user-image-upload.component.html](file:///d:/repos/Scheduler/Foundation/Foundation.Client/src/app/components/user-custom/user-image-upload/user-image-upload.component.html)

Minor template updates:

- Add CSS classes for new animation states
- Improve user info section styling

---

### Phase 2: Add Image Section to Add/Edit Modal

#### [MODIFY] [user-custom-add-edit.component.html](file:///d:/repos/Scheduler/Foundation/Foundation.Client/src/app/components/user-custom/user-custom-add-edit/user-custom-add-edit.component.html)

Add an image preview/upload section at the top of the **Personal Information** section:

- Display current avatar (image or initials) in edit mode
- Show "Change Photo" button that opens the image upload modal
- For new users, show placeholder with "Add Photo" option
- Compact inline design that doesn't overwhelm the form

---

#### [MODIFY] [user-custom-add-edit.component.ts](file:///d:/repos/Scheduler/Foundation/Foundation.Client/src/app/components/user-custom/user-custom-add-edit/user-custom-add-edit.component.ts)

Add TypeScript support:

- Import `NgbModal` (already imported)
- Import `UserImageUploadComponent`
- Add helper methods: `hasUserImage()`, `getUserImageUrl()`, `getUserInitials()`
- Add `openImageUpload()` method to trigger the modal
- Handle modal result to refresh the image preview

---

#### [MODIFY] [user-custom-add-edit.component.scss](file:///d:/repos/Scheduler/Foundation/Foundation.Client/src/app/components/user-custom/user-custom-add-edit/user-custom-add-edit.component.scss)

Add styles for the new image section:

- Avatar circle styling matching the detail header
- Hover effects and transitions
- Button styling for "Change Photo" action

---

## Verification Plan

### Build Verification
```powershell
cd d:\repos\Scheduler\Foundation\Foundation.Client
npm run build
```

### Manual Verification

The changes are UI-focused and best verified visually. Please test the following:

**Image Upload Modal Refresh:**
1. Navigate to a user detail page
2. Click on the user avatar/initials in the header
3. Verify the modal has the premium blue gradient header (`#0d47a1` → `#1976d2` → `#42a5f5`)
4. Verify the drop zone has a subtle glass effect with gradient border
5. Hover over the drop zone and verify smooth transition effects
6. Drag a file over and verify the enhanced drag-over state

**Image in Add/Edit Modal:**
1. Navigate to a user detail page and click "Edit User"
2. Verify there's an avatar/image section at the top of the Personal Information tab
3. If the user has an image, verify it displays correctly
4. If no image, verify initials are shown
5. Click "Change Photo" and verify the image upload modal opens
6. Upload an image, close the modal, and verify the preview updates
