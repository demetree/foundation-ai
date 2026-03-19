//
// file-manager.component.ts
//
// AI-Developed — This file was significantly developed with AI assistance.
//
// Main File Manager component — an Explorer-like UI for managing
// documents and folders.  Uses FileManagerService for all API calls.
//
import { Component, OnInit, ViewChild, ElementRef, HostListener } from '@angular/core';
import { trigger, transition, style, animate } from '@angular/animations';
import { forkJoin } from 'rxjs';
import { AlertService, MessageSeverity } from '../../services/alert.service';
import { FileManagerService, FolderDTO, DocumentDTO, DocumentTagDTO, UploadOptions } from '../../services/file-manager.service';
import { ConfirmationService } from '../../services/confirmation-service';


@Component({
    selector: 'app-file-manager',
    templateUrl: './file-manager.component.html',
    styleUrls: ['./file-manager.component.scss'],
    animations: [
        trigger('slidePanel', [
            transition(':enter', [
                style({ width: 0, opacity: 0 }),
                animate('200ms ease-out', style({ width: '300px', opacity: 1 }))
            ]),
            transition(':leave', [
                animate('150ms ease-in', style({ width: 0, opacity: 0 }))
            ])
        ])
    ]
})
export class FileManagerComponent implements OnInit {

    @ViewChild('fileUploadInput') fileUploadInput!: ElementRef<HTMLInputElement>;

    // ─── State ───────────────────────────────────────────────────────
    folderTree: FolderDTO[] = [];
    allFolders: FolderDTO[] = [];
    childFolders: FolderDTO[] = [];
    documents: DocumentDTO[] = [];
    breadcrumbs: { id: number; name: string }[] = [];

    currentFolderId: number | null = null;
    selectedDocument: DocumentDTO | null = null;

    viewMode: 'grid' | 'list' = 'grid';
    sidebarCollapsed = false;
    isLoading = false;
    isDragOver = false;
    private dragEnterCount = 0;

    // Folder import progress
    importProgress: { phase: string; current: number; total: number; detail: string } | null = null;

    // Search
    searchQuery = '';
    isSearching = false;

    // Context menus
    showDocContext = false;
    showFolderContext = false;
    contextX = 0;
    contextY = 0;
    contextDocument: DocumentDTO | null = null;
    contextFolder: FolderDTO | null = null;

    // New folder dialog
    showNewFolderModal = false;
    newFolderName = '';
    newFolderParentId: number | null = null;
    editingFolder: FolderDTO | null = null;

    // Rename document dialog
    showRenameDocModal = false;
    renameDocName = '';
    renamingDocument: DocumentDTO | null = null;

    // ─── Tags ────────────────────────────────────────────────────────
    allTags: DocumentTagDTO[] = [];
    selectedDocumentTags: DocumentTagDTO[] = [];
    activeTagFilters: Set<number> = new Set();
    documentTagsMap: Map<number, DocumentTagDTO[]> = new Map();  // docId -> tags
    tagCounts: Map<number, number> = new Map();  // tagId -> count

    // Tag dropdown (detail panel)
    showTagDropdown = false;

    // Tag manager modal
    showTagManagerModal = false;
    newTagName = '';
    newTagColor = '#6366f1';
    editingTagId: number | null = null;
    editingTagName = '';
    editingTagColor = '';

    // Multi-select / bulk
    selectedDocIds: Set<number> = new Set();
    showBulkTagDropdown = false;


    constructor(
        public fileManagerService: FileManagerService,
        private alertService: AlertService,
        private confirmationService: ConfirmationService
    ) {}


    ngOnInit(): void {
        this.loadFolders();
        this.loadDocuments();
        this.loadTags();
    }


    // ─── Data Loading ────────────────────────────────────────────────

    loadFolders(): void {
        this.fileManagerService.getFolders().subscribe({
            next: (folders) => {
                this.allFolders = folders.filter(f => !f.deleted);
                this.folderTree = this.fileManagerService.buildFolderTree([...this.allFolders]);
                this.updateChildFolders();
                this.updateBreadcrumbs();
            },
            error: (err) => {
                console.error('Error loading folders', err);
                this.alertService.showMessage('Error', 'Could not load folders.', MessageSeverity.error);
            }
        });
    }

    loadDocuments(): void {
        this.isLoading = true;
        this.fileManagerService.getDocumentsInFolder(this.currentFolderId).subscribe({
            next: (docs) => {
                this.documents = docs.filter(d => !d.deleted);
                this.isLoading = false;
                this.loadDocumentTags();
                this.clearSelection();
            },
            error: (err) => {
                console.error('Error loading documents', err);
                this.alertService.showMessage('Error', 'Could not load documents.', MessageSeverity.error);
                this.isLoading = false;
            }
        });
    }

    refreshCurrentView(): void {
        this.selectedDocument = null;
        this.loadFolders();
        this.loadDocuments();
        this.loadTags();
    }


    // ─── Navigation ──────────────────────────────────────────────────

    navigateToFolder(folderId: number | null): void {
        this.currentFolderId = folderId;
        this.selectedDocument = null;
        this.isSearching = false;
        this.searchQuery = '';
        this.loadDocuments();
        this.updateChildFolders();
        this.updateBreadcrumbs();
        this.expandToFolder(folderId);
    }

    private updateChildFolders(): void {
        this.childFolders = this.allFolders.filter(f =>
            (this.currentFolderId == null
                ? f.parentDocumentFolderId == null
                : f.parentDocumentFolderId === this.currentFolderId)
        );
    }

    private updateBreadcrumbs(): void {
        this.breadcrumbs = [];
        if (this.currentFolderId == null) return;

        let folderId: number | null | undefined = this.currentFolderId;
        const crumbs: { id: number; name: string }[] = [];

        while (folderId != null) {
            const folder = this.allFolders.find(f => f.id === folderId);
            if (!folder) break;
            crumbs.unshift({ id: folder.id, name: folder.name });
            folderId = folder.parentDocumentFolderId;
        }

        this.breadcrumbs = crumbs;
    }

    /** Expands the tree path to the given folder so it's visible. */
    private expandToFolder(folderId: number | null): void {
        if (folderId == null) return;

        let id: number | null | undefined = folderId;
        while (id != null) {
            const node = this.findInTree(this.folderTree, id);
            if (node) node.expanded = true;
            const flat = this.allFolders.find(f => f.id === id);
            id = flat?.parentDocumentFolderId;
        }
    }

    private findInTree(nodes: FolderDTO[], id: number): FolderDTO | null {
        for (const n of nodes) {
            if (n.id === id) return n;
            if (n.children) {
                const found = this.findInTree(n.children, id);
                if (found) return found;
            }
        }
        return null;
    }

    toggleFolderExpand(event: Event, folder: FolderDTO): void {
        event.stopPropagation();
        folder.expanded = !folder.expanded;
    }


    // ─── Document Actions ────────────────────────────────────────────

    selectDocument(doc: DocumentDTO): void {
        const newSelection = this.selectedDocument?.id === doc.id ? null : doc;
        this.selectedDocument = newSelection;
        this.showTagDropdown = false;

        if (newSelection) {
            this.selectedDocumentTags = this.documentTagsMap.get(newSelection.id) || [];
        } else {
            this.selectedDocumentTags = [];
        }
    }

    downloadFile(doc: DocumentDTO): void {
        this.fileManagerService.downloadDocument(doc.id).subscribe({
            next: (blob) => {
                const url = window.URL.createObjectURL(blob);
                const a = document.createElement('a');
                a.href = url;
                a.download = doc.fileName;
                a.click();
                window.URL.revokeObjectURL(url);
            },
            error: (err) => {
                console.error('Download failed', err);
                this.alertService.showMessage('Error', 'Download failed.', MessageSeverity.error);
            }
        });
    }

    deleteFile(doc: DocumentDTO): void {
        this.confirmationService.confirm(
            'Delete Document',
            `Are you sure you want to delete "${doc.fileName}"?`
        ).then((confirmed: boolean) => {
            if (!confirmed) return;

            this.fileManagerService.deleteDocument(doc.id).subscribe({
                next: () => {
                    this.alertService.showMessage('Deleted', `"${doc.fileName}" has been deleted.`, MessageSeverity.success);
                    if (this.selectedDocument?.id === doc.id) this.selectedDocument = null;
                    this.loadDocuments();
                },
                error: (err) => {
                    console.error('Delete failed', err);
                    this.alertService.showMessage('Error', 'Could not delete document.', MessageSeverity.error);
                }
            });
        });
    }

    renameDocument(doc: DocumentDTO): void {
        this.showDocContext = false;
        this.renamingDocument = doc;
        this.renameDocName = doc.name;
        this.showRenameDocModal = true;
    }

    confirmRenameDoc(): void {
        if (!this.renamingDocument || !this.renameDocName?.trim()) return;

        this.fileManagerService.updateDocumentMetadata({
            ...this.renamingDocument,
            name: this.renameDocName.trim()
        }).subscribe({
            next: () => {
                this.alertService.showMessage('Renamed', 'Document renamed.', MessageSeverity.success);
                this.showRenameDocModal = false;
                this.renamingDocument = null;
                this.loadDocuments();
            },
            error: (err) => {
                console.error('Rename failed', err);
                this.alertService.showMessage('Error', 'Could not rename document.', MessageSeverity.error);
            }
        });
    }


    // ─── Upload ──────────────────────────────────────────────────────

    triggerUpload(): void {
        this.fileUploadInput.nativeElement.click();
    }

    onFilesSelected(event: Event): void {
        const input = event.target as HTMLInputElement;
        if (!input.files || input.files.length === 0) return;
        this.uploadFiles(Array.from(input.files));
        input.value = ''; // Reset so the same file can be selected again
    }

    uploadFiles(files: File[]): void {
        const options: UploadOptions = {
            folderId: this.currentFolderId
        };

        this.isLoading = true;
        this.fileManagerService.uploadDocuments(files, options).subscribe({
            next: (uploaded) => {
                const count = uploaded.length;
                this.alertService.showMessage('Uploaded',
                    `${count} file${count !== 1 ? 's' : ''} uploaded successfully.`,
                    MessageSeverity.success);
                this.loadDocuments();
            },
            error: (err) => {
                console.error('Upload failed', err);
                this.alertService.showMessage('Error', 'Upload failed.', MessageSeverity.error);
                this.isLoading = false;
            }
        });
    }

    //
    // Drag & drop — uses a counter to handle nested dragenter/dragleave events
    // from child elements.  The overlay is always in the DOM (CSS class toggle)
    // to prevent the *ngIf DOM insertion/removal flicker loop.
    //
    onDragOver(event: DragEvent): void {
        event.preventDefault();
        event.stopPropagation();
    }


    onDragEnter(event: DragEvent): void {
        event.preventDefault();
        event.stopPropagation();
        this.dragEnterCount++;
        this.isDragOver = true;
    }


    onDragLeave(event: DragEvent): void {
        event.preventDefault();
        event.stopPropagation();
        this.dragEnterCount--;

        if (this.dragEnterCount <= 0) {
            this.dragEnterCount = 0;
            this.isDragOver = false;
        }
    }


    onDrop(event: DragEvent): void {
        event.preventDefault();
        event.stopPropagation();
        this.dragEnterCount = 0;
        this.isDragOver = false;

        // Check for folder entries via the webkit API
        const items = event.dataTransfer?.items;
        if (items && items.length > 0) {
            const entries: FileSystemEntry[] = [];
            const looseFiles: File[] = [];

            for (let i = 0; i < items.length; i++) {
                const entry = items[i].webkitGetAsEntry?.();
                if (entry) {
                    if (entry.isDirectory) {
                        entries.push(entry);
                    } else {
                        // Collect loose files alongside folders
                        const file = items[i].getAsFile();
                        if (file) looseFiles.push(file);
                    }
                }
            }

            if (entries.length > 0) {
                this.importFolderEntries(entries, looseFiles);
                return;
            }

            if (looseFiles.length > 0) {
                this.uploadFiles(looseFiles);
                return;
            }
        }

        // Fallback: plain file list
        if (event.dataTransfer?.files && event.dataTransfer.files.length > 0) {
            this.uploadFiles(Array.from(event.dataTransfer.files));
        }
    }


    // ═══════════════════════════════════════════════════════════════════════
    //  FOLDER IMPORT (recursive drag-and-drop)
    // ═══════════════════════════════════════════════════════════════════════

    /**
     * Orchestrator: walks all dropped directory entries, creates folders,
     * and uploads files with a progress indicator.
     */
    private async importFolderEntries(dirEntries: FileSystemEntry[], looseFiles: File[]): Promise<void> {
        this.importProgress = { phase: 'Scanning', current: 0, total: 0, detail: 'Reading folder structure…' };

        try {
            // 1. Walk the entire tree
            const tree: { path: string; files: File[] }[] = [];
            const allPaths: Set<string> = new Set();

            for (const entry of dirEntries) {
                if (entry.isDirectory) {
                    await this.walkDirectoryTree(entry as FileSystemDirectoryEntry, entry.name, tree, allPaths);
                }
            }

            // Sort paths so parents come before children
            const sortedPaths = [...allPaths].sort((a, b) => a.split('/').length - b.split('/').length || a.localeCompare(b));

            const totalFiles = tree.reduce((sum, t) => sum + t.files.length, 0) + looseFiles.length;

            // 2. Create folders sequentially (parent-before-child)
            this.importProgress = { phase: 'Creating folders', current: 0, total: sortedPaths.length, detail: '' };
            const pathToFolderId = new Map<string, number>();

            for (let i = 0; i < sortedPaths.length; i++) {
                const folderPath = sortedPaths[i];
                const parts = folderPath.split('/');
                const folderName = parts[parts.length - 1];
                const parentPath = parts.slice(0, -1).join('/');

                // Determine parent folder ID
                let parentId: number | null = this.currentFolderId;
                if (parentPath && pathToFolderId.has(parentPath)) {
                    parentId = pathToFolderId.get(parentPath)!;
                }

                this.importProgress = {
                    phase: 'Creating folders',
                    current: i + 1,
                    total: sortedPaths.length,
                    detail: folderName
                };

                // Conflict detection: check if folder already exists
                const existing = this.allFolders.find(f =>
                    f.name === folderName &&
                    ((parentId === null && f.parentDocumentFolderId == null) ||
                     f.parentDocumentFolderId === parentId)
                );

                if (existing) {
                    pathToFolderId.set(folderPath, existing.id);
                } else {
                    const created = await this.fileManagerService.createFolder({
                        name: folderName,
                        parentDocumentFolderId: parentId,
                        sequence: 0
                    } as any).toPromise();

                    if (created) {
                        pathToFolderId.set(folderPath, created.id);
                        // Add to allFolders so subsequent conflict checks can find it
                        this.allFolders.push(created);
                    }
                }
            }

            // 3. Upload files per folder
            let filesUploaded = 0;

            // Upload loose files first (into current folder)
            if (looseFiles.length > 0) {
                this.importProgress = {
                    phase: 'Uploading files',
                    current: filesUploaded,
                    total: totalFiles,
                    detail: 'Root files'
                };
                await this.fileManagerService.uploadDocuments(looseFiles, {
                    folderId: this.currentFolderId
                }).toPromise();
                filesUploaded += looseFiles.length;
            }

            // Upload files into their respective folders
            for (const entry of tree) {
                if (entry.files.length === 0) continue;

                const folderId = pathToFolderId.get(entry.path) ?? this.currentFolderId;

                this.importProgress = {
                    phase: 'Uploading files',
                    current: filesUploaded,
                    total: totalFiles,
                    detail: entry.path.split('/').pop() || 'files'
                };

                await this.fileManagerService.uploadDocuments(entry.files, {
                    folderId: folderId
                }).toPromise();

                filesUploaded += entry.files.length;
            }

            // 4. Done
            this.importProgress = null;
            const folderCount = sortedPaths.length;
            this.alertService.showMessage('Import Complete',
                `Imported ${folderCount} folder${folderCount !== 1 ? 's' : ''} and ${totalFiles} file${totalFiles !== 1 ? 's' : ''}.`,
                MessageSeverity.success);
            this.loadFolders();
            this.loadDocuments();

        } catch (err) {
            console.error('Folder import failed', err);
            this.alertService.showMessage('Error', 'Folder import failed. Some items may have been partially imported.', MessageSeverity.error);
            this.importProgress = null;
            this.loadFolders();
            this.loadDocuments();
        }
    }


    /**
     * Recursively walks a FileSystemDirectoryEntry, collecting folder paths
     * and files.  Chrome caps readEntries() at 100 items per call, so we
     * loop until the batch is empty.
     */
    private async walkDirectoryTree(
        dirEntry: FileSystemDirectoryEntry,
        currentPath: string,
        results: { path: string; files: File[] }[],
        allPaths: Set<string>
    ): Promise<void> {
        allPaths.add(currentPath);

        const reader = dirEntry.createReader();
        const entries = await this.readAllEntries(reader);

        const files: File[] = [];

        for (const entry of entries) {
            if (entry.isFile) {
                const file = await this.fileEntryToFile(entry as FileSystemFileEntry);
                files.push(file);
            } else if (entry.isDirectory) {
                await this.walkDirectoryTree(
                    entry as FileSystemDirectoryEntry,
                    `${currentPath}/${entry.name}`,
                    results,
                    allPaths
                );
            }
        }

        results.push({ path: currentPath, files });
    }


    /** Reads all entries from a DirectoryReader (handles Chrome's 100-item cap). */
    private readAllEntries(reader: FileSystemDirectoryReader): Promise<FileSystemEntry[]> {
        return new Promise((resolve, reject) => {
            const all: FileSystemEntry[] = [];

            const readBatch = () => {
                reader.readEntries(
                    (batch) => {
                        if (batch.length === 0) {
                            resolve(all);
                        } else {
                            all.push(...batch);
                            readBatch(); // Continue reading (Chrome caps at 100)
                        }
                    },
                    (err) => reject(err)
                );
            };

            readBatch();
        });
    }


    /** Converts a FileSystemFileEntry to a File object. */
    private fileEntryToFile(fileEntry: FileSystemFileEntry): Promise<File> {
        return new Promise((resolve, reject) => {
            fileEntry.file(
                (file) => resolve(file),
                (err) => reject(err)
            );
        });
    }


    // ─── Search ──────────────────────────────────────────────────────

    onSearch(): void {
        if (!this.searchQuery?.trim()) {
            this.clearSearch();
            return;
        }

        this.isSearching = true;
        this.isLoading = true;
        this.selectedDocument = null;

        this.fileManagerService.searchDocuments(this.searchQuery.trim()).subscribe({
            next: (results) => {
                this.documents = results.filter(d => !d.deleted);
                this.childFolders = [];
                this.isLoading = false;
            },
            error: (err) => {
                console.error('Search failed', err);
                this.alertService.showMessage('Error', 'Search failed.', MessageSeverity.error);
                this.isLoading = false;
            }
        });
    }

    onSearchInputChange(): void {
        // If user clears the search field, return to folder view
        if (!this.searchQuery && this.isSearching) {
            this.clearSearch();
        }
    }

    clearSearch(): void {
        this.searchQuery = '';
        this.isSearching = false;
        this.loadDocuments();
        this.updateChildFolders();
    }


    // ─── Folder Actions ──────────────────────────────────────────────

    showNewFolderDialog(parent?: FolderDTO): void {
        this.showFolderContext = false;
        this.editingFolder = null;
        this.newFolderName = '';
        this.newFolderParentId = parent?.id ?? this.currentFolderId;
        this.showNewFolderModal = true;
    }

    confirmNewFolder(): void {
        if (!this.newFolderName?.trim()) return;

        if (this.editingFolder) {
            // Rename mode
            this.fileManagerService.updateFolder({
                ...this.editingFolder,
                name: this.newFolderName.trim()
            }).subscribe({
                next: () => {
                    this.alertService.showMessage('Renamed', 'Folder renamed.', MessageSeverity.success);
                    this.showNewFolderModal = false;
                    this.editingFolder = null;
                    this.loadFolders();
                },
                error: (err) => {
                    console.error('Rename folder failed', err);
                    this.alertService.showMessage('Error', 'Could not rename folder.', MessageSeverity.error);
                }
            });
        } else {
            // Create mode
            this.fileManagerService.createFolder({
                name: this.newFolderName.trim(),
                parentDocumentFolderId: this.newFolderParentId,
                sequence: 0
            } as any).subscribe({
                next: () => {
                    this.alertService.showMessage('Created', 'Folder created.', MessageSeverity.success);
                    this.showNewFolderModal = false;
                    this.loadFolders();
                },
                error: (err) => {
                    console.error('Create folder failed', err);
                    this.alertService.showMessage('Error', 'Could not create folder.', MessageSeverity.error);
                }
            });
        }
    }

    renameFolder(folder: FolderDTO): void {
        this.showFolderContext = false;
        this.editingFolder = folder;
        this.newFolderName = folder.name;
        this.newFolderParentId = folder.parentDocumentFolderId ?? null;
        this.showNewFolderModal = true;
    }

    deleteFolderAction(folder: FolderDTO): void {
        this.showFolderContext = false;

        this.confirmationService.confirm(
            'Delete Folder',
            `Delete "${folder.name}" and all its contents?`
        ).then((confirmed: boolean) => {
            if (!confirmed) return;

            this.fileManagerService.deleteFolder(folder.id, true).subscribe({
                next: () => {
                    this.alertService.showMessage('Deleted', `Folder "${folder.name}" deleted.`, MessageSeverity.success);
                    if (this.currentFolderId === folder.id) {
                        this.navigateToFolder(folder.parentDocumentFolderId ?? null);
                    }
                    this.loadFolders();
                },
                error: (err) => {
                    console.error('Delete folder failed', err);
                    this.alertService.showMessage('Error', 'Could not delete folder.', MessageSeverity.error);
                }
            });
        });
    }

    downloadFolderZip(folder: FolderDTO): void {
        this.showFolderContext = false;
        this.alertService.showMessage('Preparing', `Zipping "${folder.name}"…`, MessageSeverity.info);

        this.fileManagerService.downloadFolderAsZip(folder.id).subscribe({
            next: (blob) => {
                const url = window.URL.createObjectURL(blob);
                const a = document.createElement('a');
                a.href = url;
                a.download = `${folder.name}.zip`;
                a.click();
                window.URL.revokeObjectURL(url);
            },
            error: (err) => {
                console.error('Folder zip download failed', err);
                this.alertService.showMessage('Error', 'Could not download folder as zip.', MessageSeverity.error);
            }
        });
    }

    downloadSelectedAsZip(): void {
        const ids = [...this.selectedDocIds];
        if (ids.length === 0) return;

        this.alertService.showMessage('Preparing', `Zipping ${ids.length} file(s)…`, MessageSeverity.info);

        this.fileManagerService.downloadDocumentsAsZip(ids).subscribe({
            next: (blob) => {
                const url = window.URL.createObjectURL(blob);
                const a = document.createElement('a');
                a.href = url;
                a.download = 'Documents.zip';
                a.click();
                window.URL.revokeObjectURL(url);
            },
            error: (err) => {
                console.error('Bulk zip download failed', err);
                this.alertService.showMessage('Error', 'Could not download files as zip.', MessageSeverity.error);
            }
        });
    }


    // ─── Context Menus ───────────────────────────────────────────────

    onDocContextMenu(event: MouseEvent, doc: DocumentDTO): void {
        event.preventDefault();
        this.showFolderContext = false;
        this.contextDocument = doc;
        this.contextX = event.clientX;
        this.contextY = event.clientY;
        this.showDocContext = true;
    }

    onFolderContextMenu(event: MouseEvent, folder: FolderDTO | null): void {
        event.preventDefault();
        this.showDocContext = false;
        this.contextFolder = folder;
        this.contextX = event.clientX;
        this.contextY = event.clientY;
        this.showFolderContext = true;
    }

    @HostListener('document:click')
    onDocumentClick(): void {
        this.showDocContext = false;
        this.showFolderContext = false;
        this.showBulkTagDropdown = false;
    }


    // ═══════════════════════════════════════════════════════════════════════
    //  TAG OPERATIONS
    // ═══════════════════════════════════════════════════════════════════════

    loadTags(): void {
        this.fileManagerService.getTags().subscribe({
            next: (tags) => {
                this.allTags = tags.filter(t => !t.deleted);
            },
            error: (err) => console.error('Error loading tags', err)
        });
    }

    /** Loads tags for all visible documents in parallel and builds the map + counts. */
    loadDocumentTags(): void {
        if (this.documents.length === 0) {
            this.documentTagsMap.clear();
            this.computeTagCounts();
            return;
        }

        const requests: { [key: string]: any } = {};
        for (const doc of this.documents) {
            requests[doc.id.toString()] = this.fileManagerService.getTagsForDocument(doc.id);
        }

        forkJoin(requests).subscribe({
            next: (results: any) => {
                this.documentTagsMap.clear();
                for (const docId of Object.keys(results)) {
                    this.documentTagsMap.set(parseInt(docId, 10), results[docId]);
                }
                this.computeTagCounts();

                // Refresh selected document tags if one is selected
                if (this.selectedDocument) {
                    this.selectedDocumentTags = this.documentTagsMap.get(this.selectedDocument.id) || [];
                }
            },
            error: (err) => console.error('Error loading document tags', err)
        });
    }

    private computeTagCounts(): void {
        this.tagCounts.clear();
        for (const [, tags] of this.documentTagsMap) {
            for (const tag of tags) {
                this.tagCounts.set(tag.id, (this.tagCounts.get(tag.id) || 0) + 1);
            }
        }
    }

    /** Get tags for a document from the cached map. */
    getDocTags(docId: number): DocumentTagDTO[] {
        return this.documentTagsMap.get(docId) || [];
    }

    /** Get the filtered document list (applies active tag filters). */
    get filteredDocuments(): DocumentDTO[] {
        if (this.activeTagFilters.size === 0) return this.documents;
        return this.documents.filter(doc => {
            const docTags = this.documentTagsMap.get(doc.id) || [];
            return [...this.activeTagFilters].every(tagId =>
                docTags.some(t => t.id === tagId)
            );
        });
    }


    // ─── Detail Panel Tag Add/Remove ────────────────────────────────

    /** Tags not yet assigned to the selected document (for the dropdown). */
    get unassignedTags(): DocumentTagDTO[] {
        const assignedIds = new Set(this.selectedDocumentTags.map(t => t.id));
        return this.allTags.filter(t => !assignedIds.has(t.id));
    }

    addTagToSelectedDoc(tag: DocumentTagDTO): void {
        if (!this.selectedDocument) return;
        const docId = this.selectedDocument.id;

        this.fileManagerService.addTagToDocument(docId, tag.id).subscribe({
            next: () => {
                this.selectedDocumentTags = [...this.selectedDocumentTags, tag];
                const current = this.documentTagsMap.get(docId) || [];
                this.documentTagsMap.set(docId, [...current, tag]);
                this.computeTagCounts();
                this.showTagDropdown = false;
            },
            error: (err) => {
                console.error('Error adding tag', err);
                this.alertService.showMessage('Error', 'Could not add tag.', MessageSeverity.error);
            }
        });
    }

    removeTagFromSelectedDoc(tag: DocumentTagDTO): void {
        if (!this.selectedDocument) return;
        const docId = this.selectedDocument.id;

        this.fileManagerService.removeTagFromDocument(docId, tag.id).subscribe({
            next: () => {
                this.selectedDocumentTags = this.selectedDocumentTags.filter(t => t.id !== tag.id);
                const current = this.documentTagsMap.get(docId) || [];
                this.documentTagsMap.set(docId, current.filter(t => t.id !== tag.id));
                this.computeTagCounts();
            },
            error: (err) => {
                console.error('Error removing tag', err);
                this.alertService.showMessage('Error', 'Could not remove tag.', MessageSeverity.error);
            }
        });
    }

    toggleTagDropdown(event: Event): void {
        event.stopPropagation();
        this.showTagDropdown = !this.showTagDropdown;
    }


    // ─── Tag Filter Bar ──────────────────────────────────────────────

    toggleTagFilter(tag: DocumentTagDTO): void {
        if (this.activeTagFilters.has(tag.id)) {
            this.activeTagFilters.delete(tag.id);
        } else {
            this.activeTagFilters.add(tag.id);
        }
        // Force change detection by creating a new Set
        this.activeTagFilters = new Set(this.activeTagFilters);
    }

    isTagFilterActive(tagId: number): boolean {
        return this.activeTagFilters.has(tagId);
    }

    clearTagFilters(): void {
        this.activeTagFilters.clear();
        this.activeTagFilters = new Set();
    }


    // ─── Tag Management Modal ────────────────────────────────────────

    openTagManager(event?: Event): void {
        event?.stopPropagation();
        this.showTagManagerModal = true;
        this.newTagName = '';
        this.newTagColor = '#6366f1';
        this.editingTagId = null;
    }

    createNewTag(): void {
        if (!this.newTagName.trim()) return;

        this.fileManagerService.createTag({
            name: this.newTagName.trim(),
            color: this.newTagColor,
            sequence: this.allTags.length
        } as any).subscribe({
            next: (created) => {
                this.allTags = [...this.allTags, created];
                this.newTagName = '';
                this.newTagColor = '#6366f1';
                this.alertService.showMessage('Created', `Tag "${created.name}" created.`, MessageSeverity.success);
            },
            error: (err) => {
                console.error('Error creating tag', err);
                this.alertService.showMessage('Error', 'Could not create tag.', MessageSeverity.error);
            }
        });
    }

    startEditTag(tag: DocumentTagDTO, event: Event): void {
        event.stopPropagation();
        this.editingTagId = tag.id;
        this.editingTagName = tag.name;
        this.editingTagColor = tag.color || '#6366f1';
    }

    saveEditTag(tag: DocumentTagDTO): void {
        this.fileManagerService.updateTag({
            ...tag,
            name: this.editingTagName.trim(),
            color: this.editingTagColor
        }).subscribe({
            next: (updated) => {
                const idx = this.allTags.findIndex(t => t.id === updated.id);
                if (idx >= 0) this.allTags[idx] = updated;
                this.editingTagId = null;
                this.loadDocumentTags(); // Refresh cached tags
                this.alertService.showMessage('Updated', `Tag "${updated.name}" updated.`, MessageSeverity.success);
            },
            error: (err) => {
                console.error('Error updating tag', err);
                this.alertService.showMessage('Error', 'Could not update tag.', MessageSeverity.error);
            }
        });
    }

    cancelEditTag(): void {
        this.editingTagId = null;
    }

    deleteExistingTag(tag: DocumentTagDTO): void {
        this.confirmationService.confirm(
            'Delete Tag',
            `Delete tag "${tag.name}"? It will be removed from all documents.`
        ).then((confirmed: boolean) => {
            if (!confirmed) return;

            this.fileManagerService.deleteTag(tag.id).subscribe({
                next: () => {
                    this.allTags = this.allTags.filter(t => t.id !== tag.id);
                    this.activeTagFilters.delete(tag.id);
                    this.loadDocumentTags();
                    this.alertService.showMessage('Deleted', `Tag "${tag.name}" deleted.`, MessageSeverity.success);
                },
                error: (err) => {
                    console.error('Error deleting tag', err);
                    this.alertService.showMessage('Error', 'Could not delete tag.', MessageSeverity.error);
                }
            });
        });
    }


    // ─── Multi-Select / Bulk Tagging ─────────────────────────────────

    toggleDocSelection(event: Event, doc: DocumentDTO): void {
        event.stopPropagation();
        if (this.selectedDocIds.has(doc.id)) {
            this.selectedDocIds.delete(doc.id);
        } else {
            this.selectedDocIds.add(doc.id);
        }
        this.selectedDocIds = new Set(this.selectedDocIds);
    }

    isDocSelected(docId: number): boolean {
        return this.selectedDocIds.has(docId);
    }

    selectAllDocs(): void {
        if (this.selectedDocIds.size === this.filteredDocuments.length) {
            this.selectedDocIds.clear();
        } else {
            this.selectedDocIds = new Set(this.filteredDocuments.map(d => d.id));
        }
    }

    clearSelection(): void {
        this.selectedDocIds.clear();
        this.selectedDocIds = new Set();
        this.showBulkTagDropdown = false;
    }

    bulkAddTag(tag: DocumentTagDTO): void {
        const docIds = [...this.selectedDocIds];
        const requests = docIds.map(id => this.fileManagerService.addTagToDocument(id, tag.id));

        forkJoin(requests).subscribe({
            next: () => {
                this.alertService.showMessage('Tagged', `Tag "${tag.name}" added to ${docIds.length} document(s).`, MessageSeverity.success);
                this.loadDocumentTags();
                this.showBulkTagDropdown = false;
            },
            error: (err) => {
                console.error('Bulk tag failed', err);
                this.alertService.showMessage('Error', 'Could not tag all documents.', MessageSeverity.error);
            }
        });
    }

    bulkRemoveTag(tag: DocumentTagDTO): void {
        const docIds = [...this.selectedDocIds];
        const requests = docIds.map(id => this.fileManagerService.removeTagFromDocument(id, tag.id));

        forkJoin(requests).subscribe({
            next: () => {
                this.alertService.showMessage('Untagged', `Tag "${tag.name}" removed from ${docIds.length} document(s).`, MessageSeverity.success);
                this.loadDocumentTags();
                this.showBulkTagDropdown = false;
            },
            error: (err) => {
                console.error('Bulk untag failed', err);
                this.alertService.showMessage('Error', 'Could not untag all documents.', MessageSeverity.error);
            }
        });
    }

    /** Default tag colors for the color picker in tag management. */
    tagColorOptions: string[] = [
        '#6366f1', '#8b5cf6', '#ec4899', '#ef4444',
        '#f97316', '#eab308', '#22c55e', '#14b8a6',
        '#06b6d4', '#3b82f6', '#64748b', '#78716c'
    ];
}
