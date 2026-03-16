# Community CMS — Phase 1 Build

## 1. CommunityDatabaseGenerator
- [x] Create project and entity definitions
- [x] Generate SQL scripts and application code

## 2. CommunityTools
- [x] Create project and run code generation

## 3. Database Setup
- [x] Create Community database
- [x] EF Core Power Tools scaffolding (camelCase properties)
- [x] Multi-tenancy support added (tenantGuid)

## 4. Community.Server — Wire Up
- [x] Program.cs with Foundation startup
- [x] Register 17 DataControllers in Program.cs
- [x] PublicContentController wired to real DB queries (9 endpoints)

## 5. Community.Client — Angular Bootstrap
- [x] Auto-generated data services (18)
- [x] Auto-generated data components (17)
- [x] Create angular.json, package.json, tsconfig files
- [x] Create src/main.ts, index.html, styles.scss
- [x] Create proxy.conf.js for dev server
- [x] npm install (815 packages)

## 6. Community.Client — Public Site Components
- [x] AppComponent (shell + routing)
- [x] HeaderComponent (navigation from API)
- [x] FooterComponent
- [x] HomePageComponent
- [x] PageViewComponent (CMS pages by slug)
- [x] PostListComponent (news listing)
- [x] PostDetailComponent
- [x] ContactComponent
- [x] PublicContentService
- [x] SeoService
- [x] Design system (styles, fonts, colors)

## 7. Verification
- [x] Client builds successfully (2.06MB bundle)
- [x] Server builds successfully (0 warnings, 0 errors)
- [ ] Start server + client and test end-to-end
- [ ] Home page renders in browser
- [ ] Page/Post views work

## 8. SSR (capstone)
- [ ] Add @angular/ssr
- [ ] Configure server-side rendering
