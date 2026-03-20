//
// fm-sidebar.component.ts
//
// AI-Developed — This file was developed with AI assistance.
//
// Child component: File Manager sidebar — folder tree, accordion sections
// (favorites, tags, entity links), quick-nav toolbar, storage bar.
//
import { Component, Input, Output, EventEmitter } from '@angular/core';
import { FileManagerService, FolderDTO, DocumentDTO, DocumentTagDTO } from '../../../services/file-manager.service';


@Component({
    selector: 'fm-sidebar',
    templateUrl: './fm-sidebar.component.html',
    styleUrls: ['./fm-sidebar.component.scss']
})
export class FmSidebarComponent {

    // ─── Folder tree ─────────────────────────────────────────────────
    @Input() folderTree: FolderDTO[] = [];
    @Input() allFolders: FolderDTO[] = [];
    @Input() currentFolderId: number | null = null;
    @Input() isSearching = false;

    // ─── Sidebar layout ──────────────────────────────────────────────
    @Input() sidebarCollapsed = false;

    // ─── Accordion states ────────────────────────────────────────────
    @Input() sidebarFavoritesOpen = false;
    @Input() sidebarTagsOpen = false;
    @Input() sidebarLinksOpen = false;

    // ─── Favorites ───────────────────────────────────────────────────
    @Input() favoriteDocuments: DocumentDTO[] = [];

    // ─── Tags ────────────────────────────────────────────────────────
    @Input() allTags: DocumentTagDTO[] = [];
    @Input() tagCounts: Map<number, number> = new Map();
    @Input() activeTagFilters: Set<number> = new Set();

    // ─── Entity links ────────────────────────────────────────────────
    @Input() activeEntityLinkTypes: { fk: string; label: string; icon: string; color: string; count: number }[] = [];
    @Input() activeEntityLinkFilter: string | null = null;

    // ─── Quick nav ───────────────────────────────────────────────────
    @Input() trashCount = 0;
    @Input() showTrash = false;
    @Input() showActivity = false;

    // ─── Storage ─────────────────────────────────────────────────────
    @Input() storageUsage: { totalBytes: number; documentCount: number; quotaBytes: number } | null = null;
    @Input() storagePercentage = 0;
    @Input() storageBarColor = '#198754';

    // ─── Outputs ─────────────────────────────────────────────────────
    @Output() navigateToFolder = new EventEmitter<number | null>();
    @Output() toggleSidebarCollapse = new EventEmitter<void>();
    @Output() toggleFavoritesAccordion = new EventEmitter<void>();
    @Output() toggleTagsAccordion = new EventEmitter<void>();
    @Output() toggleLinksAccordion = new EventEmitter<void>();
    @Output() toggleTagFilter = new EventEmitter<DocumentTagDTO>();
    @Output() clearEntityLinkFilter = new EventEmitter<void>();
    @Output() toggleEntityLinkFilter = new EventEmitter<string>();
    @Output() toggleTrash = new EventEmitter<void>();
    @Output() toggleActivity = new EventEmitter<void>();
    @Output() openTagManager = new EventEmitter<Event>();
    @Output() showNewFolderDialog = new EventEmitter<void>();
    @Output() folderContextMenu = new EventEmitter<{ event: MouseEvent; folder: FolderDTO | null }>();
    @Output() selectFavoriteDoc = new EventEmitter<DocumentDTO>();


    constructor(public fileManagerService: FileManagerService) {}


    isTagFilterActive(tagId: number): boolean {
        return this.activeTagFilters.has(tagId);
    }


    toggleFolderExpand(event: Event, folder: FolderDTO): void {
        event.stopPropagation();
        folder.expanded = !folder.expanded;
    }


    onFolderContextMenu(event: MouseEvent, folder: FolderDTO | null): void {
        event.preventDefault();
        this.folderContextMenu.emit({ event, folder });
    }
}
