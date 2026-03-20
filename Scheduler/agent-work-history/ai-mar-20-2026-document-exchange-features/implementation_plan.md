# Document Exchange Features — Implementation Plan

Three features to close the Dropbox sharing gap: **Shareable Links**, **Email a File**, and **Upload Portal**.

---

## Proposed Changes

### Feature 1: Shareable Links

A GUID-based public download URL with optional password, expiry, and download tracking.

---

#### [NEW] DocumentShareLink table (SQL migration)

New table in the Scheduler database:

```sql
CREATE TABLE DocumentShareLink (
    id              INT IDENTITY(1,1) PRIMARY KEY,
    tenantGuid      UNIQUEIDENTIFIER NOT NULL,
    documentId      INT NOT NULL REFERENCES Document(id),
    token           UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),  -- the public URL token
    password        NVARCHAR(256) NULL,         -- optional bcrypt hash
    expiresAt       DATETIME2 NULL,             -- NULL = never expires
    maxDownloads    INT NULL,                   -- NULL = unlimited
    downloadCount   INT NOT NULL DEFAULT 0,
    createdBy       NVARCHAR(256) NOT NULL,
    createdDate     DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    active          BIT NOT NULL DEFAULT 1,
    deleted         BIT NOT NULL DEFAULT 0,
    versionNumber   INT NOT NULL DEFAULT 1,
    objectGuid      UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID()
);
CREATE UNIQUE INDEX IX_DocumentShareLink_Token ON DocumentShareLink(token);
```

> [!NOTE]
> Password will be stored as a bcrypt hash, not plaintext. The `token` is a GUID used in the public URL (e.g., `/share/a1b2c3d4-...`).

---

#### [NEW] [DocumentShareLink.cs](file:///g:/source/repos/Scheduler/SchedulerDatabase/Database/DocumentShareLink.cs)

EF Core entity class following the existing `Document` pattern — partial class with `tenantGuid`, `versionNumber`, `objectGuid`, `active`, `deleted`.

#### [NEW] [DocumentShareLinkExtension.cs](file:///g:/source/repos/Scheduler/SchedulerDatabase/EntityExtensions/DocumentShareLinkExtension.cs)

DTO classes (`DocumentShareLinkDTO`, `DocumentShareLinkOutputDTO`), `ToDTO()`, `ToOutputDTO()`, `FromDTO()`, `Clone()` — following the established pattern from `DocumentExtension.cs`.

#### [MODIFY] [SchedulerContext.cs](file:///g:/source/repos/Scheduler/SchedulerDatabase/Database/SchedulerContext.cs)

Add `DbSet<DocumentShareLink>` and configure the entity in `OnModelCreating`.

---

#### [MODIFY] [FileManagerController.cs](file:///g:/source/repos/Scheduler/Scheduler/Scheduler.Server/Controllers/FileManagerController.cs)

Add **authenticated** share link management endpoints:

| Method | Route | Auth | Purpose |
|--------|-------|------|---------|
| `POST` | `api/FileManager/Documents/{id}/ShareLink` | Writer | Create a share link |
| `GET` | `api/FileManager/Documents/{id}/ShareLinks` | Reader | List share links for a doc |
| `DELETE` | `api/FileManager/ShareLinks/{linkId}` | Writer | Revoke a share link |

Add **unauthenticated** public download endpoint:

| Method | Route | Auth | Purpose |
|--------|-------|------|---------|
| `GET` | `api/Share/{token}` | **None** | Returns doc metadata + branded download page |
| `GET` | `api/Share/{token}/Download` | **None** | Streams the file binary |
| `POST` | `api/Share/{token}/Verify` | **None** | Verify password (if set) |

The unauthenticated endpoints verify: token exists, link is active/not deleted, not expired, download count not exceeded. If password-protected, requires a prior `Verify` call that sets a short-lived cookie/header.

---

#### [MODIFY] [file-manager.service.ts](file:///g:/source/repos/Scheduler/Scheduler/Scheduler.Client/src/app/services/file-manager.service.ts)

Add methods: `createShareLink(documentId, options)`, `getShareLinks(documentId)`, `revokeShareLink(linkId)`.

New interface:
```typescript
export interface ShareLinkDTO {
    id: number;
    documentId: number;
    token: string;
    password?: string;      // only sent on create (plaintext), never returned
    hasPassword: boolean;   // returned from server
    expiresAt?: string;
    maxDownloads?: number;
    downloadCount: number;
    createdBy: string;
    createdDate: string;
}
```

---

#### [MODIFY] [fm-detail-panel.component.html](file:///g:/source/repos/Scheduler/Scheduler/Scheduler.Client/src/app/components/file-manager/fm-detail-panel/fm-detail-panel.component.html) + `.ts`

Add a **"Share"** section between version history and action buttons:
- **"Create Share Link"** button → opens a small inline form (expiry date picker, optional password, optional max downloads)
- List of existing share links with copy-to-clipboard, revoke, and download count display
- Visual indicator if a document has active share links (badge on the file card)

---

### Feature 2: Email a File

Send a document to an email address directly from the file manager, using the existing `SendGridEmailService`.

---

#### [MODIFY] [FileManagerController.cs](file:///g:/source/repos/Scheduler/Scheduler/Scheduler.Server/Controllers/FileManagerController.cs)

New endpoint:

| Method | Route | Auth | Purpose |
|--------|-------|------|---------|
| `POST` | `api/FileManager/Documents/{id}/Email` | Writer | Send document via email |

Request body:
```json
{
    "toEmail": "client@example.com",
    "toName": "John Smith",
    "subject": "Document: Invoice Q4 2025",
    "message": "Please find the attached document.",
    "sendAsLink": false
}
```

Logic:
- **Small files (≤ 10MB)**: `SendGridEmailService.SendEmailWithAttachmentAsync` — document sent as base64 attachment
- **Large files (> 10MB)**: Auto-create a share link (7-day expiry, single-use), embed the link in the email body
- If `sendAsLink` is explicitly `true`, always create a share link regardless of file size
- HTML email template follows the `VolunteerNotificationService.BuildHtmlWrapper` pattern
- Audit event created: `"Emailed document '{fileName}' to {toEmail}"`

> [!IMPORTANT]
> When the document is linked to a Contact entity, the controller can suggest the Contact's email address. This pre-population happens client-side by reading the entity link nav properties.

---

#### [MODIFY] [file-manager.service.ts](file:///g:/source/repos/Scheduler/Scheduler/Scheduler.Client/src/app/services/file-manager.service.ts)

Add: `emailDocument(documentId, request)`.

---

#### [MODIFY] [fm-detail-panel.component.html](file:///g:/source/repos/Scheduler/Scheduler/Scheduler.Client/src/app/components/file-manager/fm-detail-panel/fm-detail-panel.component.html) + `.ts`

Add an **"Email"** button in the actions section. Clicking opens a compact modal/inline form:
- **To**: text input, pre-populated from linked Contact email when available
- **Subject**: defaults to `"Document: {fileName}"`
- **Message**: optional text area
- **Send as link** checkbox (auto-checked for files > 10MB)
- **Send** button

---

### Feature 3: Upload Portal (File Request)

Allow users to request file uploads from external people via a tokenized, unauthenticated upload page.

---

#### [NEW] DocumentUploadRequest table (SQL migration)

```sql
CREATE TABLE DocumentUploadRequest (
    id              INT IDENTITY(1,1) PRIMARY KEY,
    tenantGuid      UNIQUEIDENTIFIER NOT NULL,
    token           UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
    title           NVARCHAR(256) NOT NULL,         -- "Please upload your signed contract"
    description     NVARCHAR(2000) NULL,            -- instructions for the uploader
    targetFolderId  INT NULL REFERENCES DocumentFolder(id),
    -- Optional entity link (auto-link uploads to this entity)
    contactId       INT NULL REFERENCES Contact(id),
    clientId        INT NULL REFERENCES Client(id),
    scheduledEventId INT NULL REFERENCES ScheduledEvent(id),
    maxFiles        INT NULL DEFAULT 10,            -- NULL = unlimited
    maxFileSizeMB   INT NULL DEFAULT 50,
    allowedMimeTypes NVARCHAR(500) NULL,            -- NULL = any, or "image/*,application/pdf"
    expiresAt       DATETIME2 NULL,
    uploadCount     INT NOT NULL DEFAULT 0,
    createdBy       NVARCHAR(256) NOT NULL,
    createdDate     DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    active          BIT NOT NULL DEFAULT 1,
    deleted         BIT NOT NULL DEFAULT 0,
    versionNumber   INT NOT NULL DEFAULT 1,
    objectGuid      UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID()
);
CREATE UNIQUE INDEX IX_DocumentUploadRequest_Token ON DocumentUploadRequest(token);
```

---

#### [NEW] [DocumentUploadRequest.cs](file:///g:/source/repos/Scheduler/SchedulerDatabase/Database/DocumentUploadRequest.cs) + Extension

Entity + DTO classes following established patterns.

#### [MODIFY] [SchedulerContext.cs](file:///g:/source/repos/Scheduler/SchedulerDatabase/Database/SchedulerContext.cs)

Add `DbSet<DocumentUploadRequest>`.

---

#### [MODIFY] [FileManagerController.cs](file:///g:/source/repos/Scheduler/Scheduler/Scheduler.Server/Controllers/FileManagerController.cs)

Authenticated management endpoints:

| Method | Route | Auth | Purpose |
|--------|-------|------|---------|
| `POST` | `api/FileManager/UploadRequests` | Writer | Create an upload request |
| `GET` | `api/FileManager/UploadRequests` | Reader | List active upload requests |
| `DELETE` | `api/FileManager/UploadRequests/{id}` | Writer | Revoke a request |
| `POST` | `api/FileManager/UploadRequests/{id}/SendEmail` | Writer | Email the request link |

Unauthenticated public upload endpoint:

| Method | Route | Auth | Purpose |
|--------|-------|------|---------|
| `GET` | `api/Upload/{token}` | **None** | Returns request metadata (title, description, constraints) |
| `POST` | `api/Upload/{token}` | **None** | Accepts file upload (multipart) |

The public upload endpoint:
- Validates token, checks expiry, file count, file size, MIME types
- Creates `Document` in the target folder with entity links from the request
- Counts against storage quota
- Broadcasts SignalR notification to the tenant group
- Increments `uploadCount`

---

#### [MODIFY] [file-manager.service.ts](file:///g:/source/repos/Scheduler/Scheduler/Scheduler.Client/src/app/services/file-manager.service.ts)

Add: `createUploadRequest(options)`, `getUploadRequests()`, `revokeUploadRequest(id)`, `sendUploadRequestEmail(id, toEmail, toName)`.

---

#### [NEW] [fm-upload-request-modal](file:///g:/source/repos/Scheduler/Scheduler/Scheduler.Client/src/app/components/file-manager/fm-upload-request-modal/) component

A modal dialog for creating upload requests:
- Title, description, target folder picker
- Optional entity link selector (reuse existing entity link picker pattern)
- Constraints: max files, max file size, allowed types, expiry
- Email recipient fields (to, subject, message)
- Preview of the public upload URL

---

#### [NEW] Public upload page

A standalone, unauthenticated page at `/upload/{token}`:
- Branded with tenant name/logo (fetched from request metadata)
- Shows title and instructions
- Drag-and-drop upload zone (reuse existing drop zone CSS)
- File constraints displayed
- Success confirmation with upload count
- No navigation to the rest of the app

> [!IMPORTANT]
> This page needs its own Angular route that bypasses the auth guard. The route should be added to the app routing module with `canActivate: []` (no guards).

---

## User Review Required

> [!WARNING]
> **New database tables required**: `DocumentShareLink` and `DocumentUploadRequest` need to be added to the SchedulerDatabase and a migration run. Let me know if you prefer EF migrations or manual SQL scripts.

1. **Public URL base path**: The share links and upload portal need a public-facing base URL. Should we use the existing app domain (e.g., `https://app.example.com/share/{token}`) or a separate subdomain?
2. **Feature 3 scope**: The Upload Portal is the most complex feature. Would you prefer to tackle Features 1 & 2 first and defer the Upload Portal to a follow-up session?
3. **Email attachment size limit**: I suggested 10MB as the cutoff for attachment vs. share-link. Does that feel right for your SendGrid plan?

---

## Verification Plan

### Automated Tests
- **Build verification**: `dotnet build` across the solution to ensure all new entities, DTOs, controller endpoints, and client service methods compile cleanly
- No existing test project was found in the repo, so build verification is the primary automated gate

### Manual Verification

After implementation, verify the following in a running instance:

1. **Share Links**: Right-click a document → open detail panel → Share section → Create a link → Copy URL → Open in an incognito/unauthenticated browser → Confirm download works. Test password-protected link and expired link scenarios.
2. **Email a File**: Select a document linked to a Contact → Click Email → Verify Contact email is pre-populated → Send → Check inbox for received email with attachment. Test with a large file to verify the share-link fallback.
3. **Upload Portal**: Create an upload request for a folder → Copy the public URL → Open in incognito → Upload a file → Verify it appears in the target folder with correct entity links → Verify SignalR notification fires for logged-in users.
