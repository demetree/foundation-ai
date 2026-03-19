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
import { AlertService, MessageSeverity } from '../../services/alert.service';
import { FileManagerService, FolderDTO, DocumentDTO, UploadOptions } from '../../services/file-manager.service';
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


    constructor(
        public fileManagerService: FileManagerService,
        private alertService: AlertService,
        private confirmationService: ConfirmationService
    ) {}


    ngOnInit(): void {
        this.loadFolders();
        this.loadDocuments();
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
        this.selectedDocument = this.selectedDocument?.id === doc.id ? null : doc;
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

    // Drag & drop
    onDragOver(event: DragEvent): void {
        event.preventDefault();
        event.stopPropagation();
        this.isDragOver = true;
    }

    onDragLeave(event: DragEvent): void {
        event.preventDefault();
        event.stopPropagation();
        this.isDragOver = false;
    }

    onDrop(event: DragEvent): void {
        event.preventDefault();
        event.stopPropagation();
        this.isDragOver = false;

        if (event.dataTransfer?.files && event.dataTransfer.files.length > 0) {
            this.uploadFiles(Array.from(event.dataTransfer.files));
        }
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
    }
}
