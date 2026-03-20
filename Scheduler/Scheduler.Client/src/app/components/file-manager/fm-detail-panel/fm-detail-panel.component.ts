//
// fm-detail-panel.component.ts
//
// AI-Developed — This file was developed with AI assistance.
//
// Child component: Document detail panel — preview, tags, entity links,
// properties, version history, share links, email, and download/delete actions.
//
import { Component, Input, Output, EventEmitter, ViewChild, ElementRef, SimpleChanges, OnChanges } from '@angular/core';
import { SafeResourceUrl } from '@angular/platform-browser';
import { FileManagerService, DocumentDTO, DocumentTagDTO, ShareLinkDTO, CreateShareLinkRequest, EmailDocumentRequest } from '../../../services/file-manager.service';
import { AlertService } from '../../../services/alert.service';


@Component({
    selector: 'fm-detail-panel',
    templateUrl: './fm-detail-panel.component.html',
    styleUrls: ['./fm-detail-panel.component.scss']
})
export class FmDetailPanelComponent implements OnChanges {

    @ViewChild('versionUploadInput') versionUploadInput!: ElementRef<HTMLInputElement>;

    // ─── Document ────────────────────────────────────────────────────
    @Input() selectedDocument: DocumentDTO | null = null;
    @Input() previewBlobUrl: string | null = null;
    @Input() safePreviewBlobUrl: SafeResourceUrl | null = null;
    @Input() textPreviewContent: string | null = null;
    @Input() isFavorite = false;

    // ─── Tags ────────────────────────────────────────────────────────
    @Input() selectedDocumentTags: DocumentTagDTO[] = [];
    @Input() unassignedTags: DocumentTagDTO[] = [];
    @Input() showTagDropdown = false;

    // ─── Entity Links ────────────────────────────────────────────────
    @Input() entityLinks: { fk: string; label: string; name: string; route: any[] | null; icon: string; color: string }[] = [];
    @Input() showAddEntityLinkDropdown = false;
    @Input() linkableEntityTypes: { fk: string; nav: string; nameField: string; label: string; icon: string; route: string | null; color: string }[] = [];
    @Input() addLinkSelectedType: { fk: string; nav: string; nameField: string; label: string; icon: string; route: string | null; color: string } | null = null;
    @Input() addLinkSearchQuery = '';
    @Input() addLinkSearchResults: { id: number | bigint; name: string }[] = [];
    @Input() addLinkSearching = false;

    // ─── Version History ─────────────────────────────────────────────
    @Input() documentVersions: any[] = [];
    @Input() showVersionHistory = false;
    @Input() isAdmin = false;

    // ─── Outputs ─────────────────────────────────────────────────────
    @Output() close = new EventEmitter<void>();
    @Output() download = new EventEmitter<DocumentDTO>();
    @Output() delete = new EventEmitter<DocumentDTO>();
    @Output() editFile = new EventEmitter<DocumentDTO>();
    @Output() toggleFavorite = new EventEmitter<number>();

    // Tags
    @Output() addTag = new EventEmitter<DocumentTagDTO>();
    @Output() removeTag = new EventEmitter<DocumentTagDTO>();
    @Output() toggleTagDropdownEvent = new EventEmitter<Event>();

    // Entity links
    @Output() toggleAddEntityLink = new EventEmitter<Event>();
    @Output() selectEntityType = new EventEmitter<any>();
    @Output() searchEntities = new EventEmitter<void>();
    @Output() assignEntityLink = new EventEmitter<number | bigint>();
    @Output() cancelAddEntityLink = new EventEmitter<void>();
    @Output() removeEntityLink = new EventEmitter<{ doc: DocumentDTO; fkField: string }>();

    // Entity link search query two-way binding
    @Output() addLinkSearchQueryChange = new EventEmitter<string>();

    // Versions
    @Output() toggleVersionHistoryEvent = new EventEmitter<void>();
    @Output() uploadNewVersion = new EventEmitter<void>();
    @Output() versionFileSelected = new EventEmitter<Event>();
    @Output() rollbackVersion = new EventEmitter<{ documentId: number; versionNumber: number }>();


    // ─── Share Link State ────────────────────────────────────────────
    showSharePanel = false;
    shareLinks: ShareLinkDTO[] = [];
    shareLinksLoading = false;
    shareCreating = false;
    shareCopiedId: number | null = null;

    // Create share link form
    sharePassword = '';
    shareExpiryDays: number | null = 7;
    shareMaxDownloads: number | null = null;

    // ─── Email State ─────────────────────────────────────────────────
    showEmailForm = false;
    emailSending = false;
    emailTo = '';
    emailSubject = '';
    emailMessage = '';
    emailSendAsLink = false;


    constructor(
        public fileManagerService: FileManagerService,
        private alertService: AlertService
    ) {}


    ngOnChanges(changes: SimpleChanges): void {
        if (changes['selectedDocument']) {
            // Reset panels when document changes
            this.showSharePanel = false;
            this.showEmailForm = false;
            this.shareLinks = [];
            this.resetEmailForm();
        }
    }


    isImagePreviewable(mimeType: string): boolean {
        return /^image\/(png|jpe?g|gif|svg\+xml|webp|bmp|ico)$/i.test(mimeType || '');
    }


    isPdfPreviewable(mimeType: string): boolean {
        return (mimeType || '').toLowerCase() === 'application/pdf';
    }


    isTextPreviewable(mimeType: string): boolean {
        const m = (mimeType || '').toLowerCase();
        return m.startsWith('text/') ||
            m === 'application/json' ||
            m === 'application/xml' ||
            m === 'application/javascript' ||
            m === 'application/x-yaml';
    }


    isPreviewable(mimeType: string): boolean {
        return this.isImagePreviewable(mimeType) || this.isPdfPreviewable(mimeType) || this.isTextPreviewable(mimeType);
    }


    onSearchQueryChange(value: string): void {
        this.addLinkSearchQueryChange.emit(value);
        this.searchEntities.emit();
    }


    onUploadNewVersion(): void {
        this.versionUploadInput?.nativeElement?.click();
    }


    onVersionFileSelected(event: Event): void {
        this.versionFileSelected.emit(event);
    }


    // ─── Share Link Methods ──────────────────────────────────────────

    toggleSharePanel(): void {
        this.showSharePanel = !this.showSharePanel;
        if (this.showSharePanel && this.selectedDocument) {
            this.loadShareLinks();
        }
    }

    loadShareLinks(): void {
        if (!this.selectedDocument) return;
        this.shareLinksLoading = true;
        this.fileManagerService.getShareLinks(this.selectedDocument.id).subscribe({
            next: links => {
                this.shareLinks = links;
                this.shareLinksLoading = false;
            },
            error: () => {
                this.shareLinksLoading = false;
            }
        });
    }

    createShareLink(): void {
        if (!this.selectedDocument) return;
        this.shareCreating = true;

        const request: CreateShareLinkRequest = {};
        if (this.sharePassword.trim()) request.password = this.sharePassword;
        if (this.shareExpiryDays != null && this.shareExpiryDays > 0) {
            const expiry = new Date();
            expiry.setDate(expiry.getDate() + this.shareExpiryDays);
            request.expiresAt = expiry.toISOString();
        }
        if (this.shareMaxDownloads != null && this.shareMaxDownloads > 0) {
            request.maxDownloads = this.shareMaxDownloads;
        }

        this.fileManagerService.createShareLink(this.selectedDocument.id, request).subscribe({
            next: link => {
                this.shareLinks.unshift(link);
                this.shareCreating = false;
                this.sharePassword = '';
                this.alertService.showSuccessMessage('Share link created', null);
                // Auto-copy to clipboard
                this.copyShareLink(link);
            },
            error: () => {
                this.shareCreating = false;
            }
        });
    }

    copyShareLink(link: ShareLinkDTO): void {
        const url = this.fileManagerService.getShareDownloadUrl(link.token);
        navigator.clipboard.writeText(url).then(() => {
            this.shareCopiedId = link.id;
            setTimeout(() => this.shareCopiedId = null, 2000);
        });
    }

    revokeShareLink(link: ShareLinkDTO): void {
        this.fileManagerService.revokeShareLink(link.id).subscribe({
            next: () => {
                this.shareLinks = this.shareLinks.filter(l => l.id !== link.id);
                this.alertService.showSuccessMessage('Share link revoked', null);
            }
        });
    }

    isShareLinkExpired(link: ShareLinkDTO): boolean {
        if (!link.expiresAt) return false;
        return new Date(link.expiresAt) < new Date();
    }

    isShareLinkExhausted(link: ShareLinkDTO): boolean {
        if (link.maxDownloads == null) return false;
        return link.downloadCount >= link.maxDownloads;
    }


    // ─── Email Methods ───────────────────────────────────────────────

    toggleEmailForm(): void {
        this.showEmailForm = !this.showEmailForm;
        if (this.showEmailForm && this.selectedDocument) {
            this.emailSubject = `Document: ${this.selectedDocument.name}`;
        }
    }

    resetEmailForm(): void {
        this.emailTo = '';
        this.emailSubject = '';
        this.emailMessage = '';
        this.emailSendAsLink = false;
        this.emailSending = false;
    }

    sendEmail(): void {
        if (!this.selectedDocument || !this.emailTo.trim()) return;
        this.emailSending = true;

        const request: EmailDocumentRequest = {
            toEmail: this.emailTo.trim(),
            subject: this.emailSubject || undefined,
            message: this.emailMessage || undefined,
            sendAsLink: this.emailSendAsLink || undefined
        };

        this.fileManagerService.emailDocument(this.selectedDocument.id, request).subscribe({
            next: result => {
                this.emailSending = false;
                this.showEmailForm = false;
                const method = result.sentAsLink ? 'as a download link' : 'as an attachment';
                this.alertService.showSuccessMessage('Email sent', `Document sent ${method} to ${this.emailTo}`);
                this.resetEmailForm();
            },
            error: () => {
                this.emailSending = false;
            }
        });
    }
}
