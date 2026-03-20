//
// fm-detail-panel.component.ts
//
// AI-Developed — This file was developed with AI assistance.
//
// Child component: Document detail panel — preview, tags, entity links,
// properties, version history, and download/delete actions.
//
import { Component, Input, Output, EventEmitter, ViewChild, ElementRef } from '@angular/core';
import { SafeResourceUrl } from '@angular/platform-browser';
import { FileManagerService, DocumentDTO, DocumentTagDTO } from '../../../services/file-manager.service';


@Component({
    selector: 'fm-detail-panel',
    templateUrl: './fm-detail-panel.component.html',
    styleUrls: ['./fm-detail-panel.component.scss']
})
export class FmDetailPanelComponent {

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

    // ─── Outputs ─────────────────────────────────────────────────────
    @Output() close = new EventEmitter<void>();
    @Output() download = new EventEmitter<DocumentDTO>();
    @Output() delete = new EventEmitter<DocumentDTO>();
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


    constructor(public fileManagerService: FileManagerService) {}


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
}
