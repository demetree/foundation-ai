# BMC.Client Mobile UI Polish

Audit of the BMC.Client Angular app for mobile responsiveness. The user reported the login page sits too low on mobile and the initial zoom is wrong (requiring manual zoom-in after login).

## Audit Findings

After reviewing all core layout files and key component styles, here's what I found:

### What's Already Good
- Desktop/mobile layout branches in `app.component.html` (splits at 768px)
- Header has hamburger menu + mobile nav panel with responsive breakpoints at 900/768/600px
- Sidebar hides at 768px
- Dashboard, set-explorer, parts-catalog, and set-detail all have `@media` breakpoints
- Viewport meta tag in `index.html` is correct: `width=device-width, initial-scale=1`

### Issues Found

| # | Component | Issue | Severity |
|---|-----------|-------|----------|
| 1 | **Login page** | No mobile `@media` overrides at all — padding (48px/40px), logo (120px), and brand margin (36px) push the form too low on small screens | High |
| 2 | **App layout** | `.app-main-mobile` has `padding: 16px`, but inner components (set-explorer, parts-catalog, set-detail) add their own 24px+ padding — double-padding wastes mobile space | Medium |
| 3 | **Set-detail page** | `set-detail-page` has its own `padding: 32px 40px` which adds to the 16px from `app-main-mobile` — excessive on mobile | Medium |
| 4 | **Parts-catalog** | Filter sidebar layout uses fixed `grid-template-columns: 260px 1fr` which collapses at 900px, but the search bar `min-width: 260px` on `.search-wrapper` could cause overflow on small phones | Low |
| 5 | **Set-explorer** | Search wrapper `min-width: 260px` — same issue as parts-catalog | Low |

> [!NOTE]
> On the "zoom is wrong" issue — the viewport meta tag is correct, so this is most likely caused by the excessive padding making content overflow or appear zoomed out as the device tries to fit wide content. Fixing the double-padding and oversized login card should resolve the perceived zoom issue without needing meta tag changes.

## Proposed Changes

### Login Component

#### [MODIFY] [login.component.scss](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/components/login/login.component.scss)

Add mobile `@media` breakpoints to compact the login card on small screens:

```diff
+@media (max-width: 480px) {
+    .login-container {
+        padding: 32px 24px;
+    }
+
+    .brand-logo-hero {
+        width: 80px;
+        height: 80px;
+        margin-bottom: 12px;
+    }
+
+    .brand-title {
+        font-size: 1.4rem;
+    }
+
+    .login-brand {
+        margin-bottom: 24px;
+    }
+
+    .login-form {
+        gap: 16px;
+    }
+}
```

---

### App Layout

#### [MODIFY] [app.component.scss](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/components/../app.component.scss)

Reduce mobile main area padding so it doesn't stack excessively with component-level padding:

```diff
 .app-main-mobile {
     flex: 1;
-    padding: 16px;
+    padding: 8px;
     margin-top: var(--header-height);
     min-height: calc(100vh - var(--header-height));
 }
```

---

### Set Detail

#### [MODIFY] [set-detail.component.scss](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/components/set-detail/set-detail.component.scss)

The 600px breakpoint already reduces padding to 16px, but `tab-content` padding of 24px remains full-width. Add a mobile tweak:

```diff
 @media (max-width: 600px) {
     .set-detail-page {
         padding: 16px;
     }
     ...
+
+    .tab-content {
+        padding: 16px;
+    }
+
+    .external-links {
+        flex-wrap: wrap;
+    }
 }
```

---

### Set Explorer

#### [MODIFY] [set-explorer.component.scss](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/components/set-explorer/set-explorer.component.scss)

Fix search wrapper min-width that can cause overflow on narrow devices:

```diff
 @media (max-width: 600px) {
     .set-explorer {
         padding: 16px;
     }
+
+    .search-wrapper {
+        min-width: auto;
+    }
+
+    .year-range {
+        width: 100%;
+        justify-content: center;
+    }
     ...
 }
```

---

### Parts Catalog

#### [MODIFY] [parts-catalog.component.scss](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/components/parts-catalog/parts-catalog.component.scss)

Remove search wrapper constraints on small screens and tighten header:

```diff
 @media (max-width: 600px) {
     .parts-catalog {
         padding: 16px;
     }
+
+    .header-top {
+        flex-direction: column;
+        gap: 12px;
+    }
+
+    .header-actions {
+        width: 100%;
+        justify-content: space-between;
+    }
     ...
 }
```

## Verification Plan

### Browser Testing
- Load the app in a mobile-width browser (375px iPhone width) and verify:
  1. Login page card is fully visible and vertically centered without scrolling
  2. After login, content is at proper zoom level, no horizontal scroll
  3. Dashboard, set-explorer, parts-catalog, and set-detail pages display correctly
  4. No double-padding issues on any page
