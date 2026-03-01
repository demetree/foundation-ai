import { Component, OnInit, OnDestroy } from '@angular/core';
import { Router } from '@angular/router';
import { Subject } from 'rxjs';
import { takeUntil, debounceTime, finalize } from 'rxjs/operators';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { CollectionService, CollectionSummary, CollectionPart, ImportedSetRecord, WishlistItem, ImportSetResult } from '../../services/collection.service';
import { AlertService, MessageSeverity } from '../../services/alert.service';
import { ConfirmationService } from '../../services/confirmation-service';
import { LDrawThumbnailService } from '../../services/ldraw-thumbnail.service';
import { RebrickableSyncService, SyncStatus, SyncTransaction } from '../../services/rebrickable-sync.service';
import { ImportSetModalComponent } from '../import-set-modal/import-set-modal.component';

type TabId = 'parts' | 'imported-sets' | 'wishlist' | 'rebrickable';

@Component({
    selector: 'app-my-collection',
    templateUrl: './my-collection.component.html',
    styleUrl: './my-collection.component.scss'
})
export class MyCollectionComponent implements OnInit, OnDestroy {

    private destroy$ = new Subject<void>();
    private searchSubject = new Subject<string>();

    // 3D thumbnail cache
    thumbnails = new Map<string, string>();

    // State
    loading = true;
    loadingParts = false;
    loadingImports = false;
    loadingWishlist = false;

    collections: CollectionSummary[] = [];
    activeCollection: CollectionSummary | null = null;
    activeTab: TabId = 'parts';

    // Parts tab
    parts: CollectionPart[] = [];
    filteredParts: CollectionPart[] = [];
    displayedParts: CollectionPart[] = [];
    searchTerm = '';
    viewMode: 'grid' | 'list' = 'grid';

    // Pagination
    pageSize = 36;
    currentPage = 1;
    totalPages = 1;

    // Imported sets tab
    importedSets: ImportedSetRecord[] = [];

    // Wishlist tab
    wishlistItems: WishlistItem[] = [];

    // Rebrickable tab
    loadingSync = false;
    syncStatus: SyncStatus | null = null;
    transactions: SyncTransaction[] = [];
    transactionsTotalCount = 0;
    transactionsPage = 1;
    transactionsPageSize = 20;
    transactionDirectionFilter: string | null = null;
    transactionSuccessFilter: boolean | null = null;
    showTokenInput = false;
    apiTokenInput = '';
    usernameInput = '';
    passwordInput = '';
    userTokenInput = '';
    selectedMode = 'RealTime';
    selectedAuthMode = 'ApiToken';
    pulling = false;
    connecting = false;
    showSyncModeEdit = false;

    // Auth mode options
    authModes = [
        { value: 'ApiToken', label: 'Login Once', icon: 'fas fa-key', desc: 'API key + username/password — encrypted token stored in database' },
        { value: 'TokenOnly', label: 'Token Only', icon: 'fas fa-shield-alt', desc: 'Paste API key + user token directly — your password never touches BMC' },
        { value: 'SessionOnly', label: 'Session Only', icon: 'fas fa-lock', desc: 'API key + username/password — nothing saved to database, re-enter each session' }
    ];

    // Integration mode options
    integrationModes = [
        { value: 'None', label: 'No Integration', icon: 'fas fa-ban', desc: 'Rebrickable sync disabled' },
        { value: 'RealTime', label: 'Real-Time Sync', icon: 'fas fa-bolt', desc: 'Push & pull on every change' },
        { value: 'PushOnly', label: 'Push Only', icon: 'fas fa-upload', desc: 'BMC → Rebrickable only' },
        { value: 'ImportOnly', label: 'Import Only', icon: 'fas fa-download', desc: 'Rebrickable → BMC only' }
    ];

    getCurrentModeOption() {
        return this.integrationModes.find(m => m.value === this.selectedMode);
    }

    constructor(
        private router: Router,
        private collectionService: CollectionService,
        private alertService: AlertService,
        private confirmationService: ConfirmationService,
        private thumbnailService: LDrawThumbnailService,
        private syncService: RebrickableSyncService,
        private modalService: NgbModal
    ) { }


    ngOnInit(): void {
        this.searchSubject.pipe(
            debounceTime(250),
            takeUntil(this.destroy$)
        ).subscribe(term => {
            this.searchTerm = term;
            this.currentPage = 1;
            this.applyFilters();
        });

        // Subscribe to 3D thumbnail render results
        this.thumbnailService.thumbnail$.pipe(
            takeUntil(this.destroy$)
        ).subscribe(result => {
            this.thumbnails.set(result.cacheKey, result.dataUrl);
        });


        this.loadCollections();
    }


    ngOnDestroy(): void {
        this.destroy$.next();
        this.destroy$.complete();
    }


    // ───────────────────── Data Loading ─────────────────────

    loadCollections(): void {
        this.loading = true;
        this.collectionService.getMyCollections().pipe(
            takeUntil(this.destroy$),
            finalize(() => this.loading = false)
        ).subscribe({
            next: (collections) => {
                this.collections = collections;
                if (collections.length > 0) {
                    this.selectCollection(collections[0]);
                }
            },
            error: () => {
                this.alertService.showMessage('Error', 'Failed to load collections', MessageSeverity.error);
            }
        });
    }


    selectCollection(collection: CollectionSummary): void {
        this.activeCollection = collection;
        this.loadTabData();
    }


    loadTabData(): void {
        if (this.activeTab === 'rebrickable') {
            this.loadRebrickableData();
            return;
        }

        if (!this.activeCollection) return;

        switch (this.activeTab) {
            case 'parts':
                this.loadParts();
                break;
            case 'imported-sets':
                this.loadImportedSets();
                break;
            case 'wishlist':
                this.loadWishlist();
                break;
        }
    }


    loadParts(): void {
        if (!this.activeCollection) return;
        this.loadingParts = true;

        this.collectionService.getCollectionParts(this.activeCollection.id).pipe(
            takeUntil(this.destroy$),
            finalize(() => this.loadingParts = false)
        ).subscribe({
            next: (parts) => {
                this.parts = parts;
                this.applyFilters();
            },
            error: () => {
                this.alertService.showMessage('Error', 'Failed to load collection parts', MessageSeverity.error);
            }
        });
    }


    loadImportedSets(): void {
        if (!this.activeCollection) return;
        this.loadingImports = true;

        this.collectionService.getImportedSets(this.activeCollection.id).pipe(
            takeUntil(this.destroy$),
            finalize(() => this.loadingImports = false)
        ).subscribe({
            next: (sets) => this.importedSets = sets,
            error: () => {
                this.alertService.showMessage('Error', 'Failed to load imported sets', MessageSeverity.error);
            }
        });
    }


    loadWishlist(): void {
        if (!this.activeCollection) return;
        this.loadingWishlist = true;

        this.collectionService.getWishlist(this.activeCollection.id).pipe(
            takeUntil(this.destroy$),
            finalize(() => this.loadingWishlist = false)
        ).subscribe({
            next: (items) => this.wishlistItems = items,
            error: () => {
                this.alertService.showMessage('Error', 'Failed to load wishlist', MessageSeverity.error);
            }
        });
    }


    // ───────────────────── Filtering & Pagination ─────────────────────

    onSearch(event: Event): void {
        const term = (event.target as HTMLInputElement).value;
        this.searchSubject.next(term);
    }


    applyFilters(): void {
        let result = [...this.parts];

        if (this.searchTerm) {
            const lower = this.searchTerm.toLowerCase();
            result = result.filter(p =>
                p.partName?.toLowerCase().includes(lower) ||
                p.ldrawPartId?.toLowerCase().includes(lower) ||
                (p.ldrawTitle && p.ldrawTitle.toLowerCase().includes(lower)) ||
                p.colourName?.toLowerCase().includes(lower) ||
                p.categoryName?.toLowerCase().includes(lower)
            );
        }

        this.filteredParts = result;
        this.totalPages = Math.max(1, Math.ceil(result.length / this.pageSize));
        if (this.currentPage > this.totalPages) this.currentPage = 1;
        this.updateDisplayedParts();
    }


    updateDisplayedParts(): void {
        const start = (this.currentPage - 1) * this.pageSize;
        this.displayedParts = this.filteredParts.slice(start, start + this.pageSize);

        // Kick off 3D thumbnail rendering (with colour overrides)
        const partsWithGeometry = this.displayedParts
            .filter(p => p.geometryOriginalFileName)
            .map(p => ({ geometryOriginalFileName: p.geometryOriginalFileName, colourHex: this.normalizeHex(p.colourHex) }));
        if (partsWithGeometry.length > 0) {
            this.thumbnailService.renderBatch(partsWithGeometry);
        }
    }


    setTab(tab: TabId): void {
        this.activeTab = tab;
        this.loadTabData();
    }


    nextPage(): void {
        if (this.currentPage < this.totalPages) {
            this.currentPage++;
            this.updateDisplayedParts();
        }
    }


    prevPage(): void {
        if (this.currentPage > 1) {
            this.currentPage--;
            this.updateDisplayedParts();
        }
    }


    goToPage(page: number): void {
        this.currentPage = page;
        this.updateDisplayedParts();
    }


    getVisiblePages(): number[] {
        const pages: number[] = [];
        const start = Math.max(1, this.currentPage - 2);
        const end = Math.min(this.totalPages, start + 4);
        for (let i = start; i <= end; i++) {
            pages.push(i);
        }
        return pages;
    }


    // ───────────────────── Actions ─────────────────────

    async removePart(part: CollectionPart): Promise<void> {
        if (!this.activeCollection) return;

        const confirmed = await this.confirmationService.confirm(
            'Remove Part',
            `Remove "${part.partName || part.ldrawPartId}" (${part.colourName}) from your collection?`
        );

        if (!confirmed) return;

        this.collectionService.removePart(this.activeCollection!.id, part.id).pipe(
            takeUntil(this.destroy$)
        ).subscribe({
            next: () => {
                this.alertService.showMessage('Removed', 'Part removed from collection', MessageSeverity.success);
                this.loadParts();
                this.loadCollections(); // refresh stats
            },
            error: () => {
                this.alertService.showMessage('Error', 'Failed to remove part', MessageSeverity.error);
            }
        });
    }


    openImportModal(): void {
        if (!this.activeCollection) return;

        const modalRef = this.modalService.open(ImportSetModalComponent, {
            centered: true,
            size: 'lg',
            backdrop: 'static',
            keyboard: true
        });

        modalRef.componentInstance.collectionId = this.activeCollection.id;

        modalRef.result.then(
            (result: ImportSetResult) => {
                // Successful import
                this.alertService.showMessage(
                    'Import Complete',
                    `Added ${result.partsAdded} new parts, updated ${result.partsUpdated} existing (${result.totalQuantityAdded} total bricks)`,
                    MessageSeverity.success
                );
                this.loadParts();
                this.loadCollections();
                this.loadImportedSets();
            },
            () => { /* dismissed */ }
        );
    }


    // ───────────────────── Rebrickable Integration ─────────────────────

    loadRebrickableData(): void {
        this.loadingSync = true;
        this.syncService.getStatus().pipe(
            takeUntil(this.destroy$),
            finalize(() => this.loadingSync = false)
        ).subscribe({
            next: (status) => {
                this.syncStatus = status;
                this.selectedMode = status.integrationMode || 'None';
                this.loadTransactions();
            },
            error: () => {
                this.syncStatus = null;
                this.loadTransactions();
            }
        });
    }


    loadTransactions(): void {
        this.syncService.getTransactions(
            this.transactionsPageSize,
            this.transactionsPage,
            this.transactionDirectionFilter || undefined,
            this.transactionSuccessFilter !== null ? this.transactionSuccessFilter : undefined
        ).pipe(
            takeUntil(this.destroy$)
        ).subscribe({
            next: (page) => {
                this.transactions = page.results;
                this.transactionsTotalCount = page.totalCount;
            },
            error: () => {
                this.transactions = [];
                this.transactionsTotalCount = 0;
            }
        });
    }


    connectToRebrickable(): void {
        if (!this.apiTokenInput.trim()) return;

        // Validate required fields based on auth mode
        if (this.selectedAuthMode === 'TokenOnly') {
            if (!this.userTokenInput.trim()) return;
        } else {
            if (!this.usernameInput.trim() || !this.passwordInput.trim()) return;
        }

        this.connecting = true;
        this.syncService.connect({
            apiToken: this.apiTokenInput.trim(),
            username: this.selectedAuthMode !== 'TokenOnly' ? this.usernameInput.trim() : undefined,
            password: this.selectedAuthMode !== 'TokenOnly' ? this.passwordInput : undefined,
            userToken: this.selectedAuthMode === 'TokenOnly' ? this.userTokenInput.trim() : undefined,
            authMode: this.selectedAuthMode,
            integrationMode: this.selectedMode
        }).pipe(
            takeUntil(this.destroy$),
            finalize(() => this.connecting = false)
        ).subscribe({
            next: () => {
                this.alertService.showMessage('Connected', 'Successfully connected to Rebrickable!', MessageSeverity.success);
                this.apiTokenInput = '';
                this.usernameInput = '';
                this.passwordInput = '';
                this.userTokenInput = '';
                this.showTokenInput = false;
                this.loadRebrickableData();
            },
            error: (err) => {
                const msg = err?.error?.error || 'Connection failed. Check your credentials.';
                this.alertService.showMessage('Connection Failed', msg, MessageSeverity.error);
            }
        });
    }


    reauthenticateRebrickable(): void {
        if (!this.apiTokenInput.trim()) return;

        this.connecting = true;
        this.syncService.reauthenticate({
            apiToken: this.apiTokenInput.trim(),
            username: this.selectedAuthMode !== 'TokenOnly' ? this.usernameInput.trim() : undefined,
            password: this.selectedAuthMode !== 'TokenOnly' ? this.passwordInput : undefined,
            userToken: this.selectedAuthMode === 'TokenOnly' ? this.userTokenInput.trim() : undefined,
            authMode: this.selectedAuthMode,
            integrationMode: this.selectedMode
        }).pipe(
            takeUntil(this.destroy$),
            finalize(() => this.connecting = false)
        ).subscribe({
            next: () => {
                this.alertService.showMessage('Re-authenticated', 'Token refreshed successfully!', MessageSeverity.success);
                this.apiTokenInput = '';
                this.usernameInput = '';
                this.passwordInput = '';
                this.userTokenInput = '';
                this.showTokenInput = false;
                this.loadRebrickableData();
            },
            error: (err) => {
                const msg = err?.error?.error || 'Re-authentication failed.';
                this.alertService.showMessage('Re-auth Failed', msg, MessageSeverity.error);
            }
        });
    }


    checkTokenHealth(): void {
        this.syncService.checkTokenHealth().pipe(
            takeUntil(this.destroy$)
        ).subscribe({
            next: (result) => {
                if (result.valid) {
                    this.alertService.showMessage('Token Valid', 'Your Rebrickable token is healthy.', MessageSeverity.success);
                } else {
                    this.alertService.showMessage('Token Invalid', result.error || 'Token validation failed.', MessageSeverity.warn);
                }
                this.loadRebrickableData();
            },
            error: () => {
                this.alertService.showMessage('Error', 'Could not check token health.', MessageSeverity.error);
            }
        });
    }


    async disconnectFromRebrickable(): Promise<void> {
        const confirmed = await this.confirmationService.confirm(
            'Disconnect Rebrickable',
            'This will remove your API token and stop sync. Your BMC data will not be affected.'
        );

        if (!confirmed) return;

        this.syncService.disconnect().pipe(
            takeUntil(this.destroy$)
        ).subscribe({
            next: () => {
                this.alertService.showMessage('Disconnected', 'Rebrickable integration disabled.', MessageSeverity.success);
                this.loadRebrickableData();
            },
            error: () => {
                this.alertService.showMessage('Error', 'Failed to disconnect.', MessageSeverity.error);
            }
        });
    }


    updateSyncSettings(): void {
        this.syncService.updateSettings({
            integrationMode: this.selectedMode
        }).pipe(
            takeUntil(this.destroy$)
        ).subscribe({
            next: () => {
                this.alertService.showMessage('Settings Updated', `Integration mode set to ${this.selectedMode}`, MessageSeverity.success);
                this.loadRebrickableData();
            },
            error: () => {
                this.alertService.showMessage('Error', 'Failed to update settings.', MessageSeverity.error);
            }
        });
    }


    triggerFullPull(): void {
        this.pulling = true;
        this.syncService.pullFull().pipe(
            takeUntil(this.destroy$),
            finalize(() => this.pulling = false)
        ).subscribe({
            next: (result) => {
                this.alertService.showMessage(
                    'Import Complete',
                    `Created ${result.totalCreated}, updated ${result.totalUpdated}` +
                    (result.errorCount > 0 ? `, ${result.errorCount} errors` : ''),
                    result.errorCount > 0 ? MessageSeverity.warn : MessageSeverity.success
                );
                this.loadRebrickableData();
                this.loadCollections();
            },
            error: () => {
                this.alertService.showMessage('Error', 'Pull failed. Check the communications log.', MessageSeverity.error);
            }
        });
    }


    setTransactionFilter(direction: string | null, success: boolean | null): void {
        this.transactionDirectionFilter = direction;
        this.transactionSuccessFilter = success;
        this.transactionsPage = 1;
        this.loadTransactions();
    }


    nextTransactionsPage(): void {
        const maxPage = Math.ceil(this.transactionsTotalCount / this.transactionsPageSize);
        if (this.transactionsPage < maxPage) {
            this.transactionsPage++;
            this.loadTransactions();
        }
    }


    prevTransactionsPage(): void {
        if (this.transactionsPage > 1) {
            this.transactionsPage--;
            this.loadTransactions();
        }
    }


    getStatusCodeClass(code: number): string {
        if (code >= 200 && code < 300) return 'status-success';
        if (code >= 400 && code < 500) return 'status-client-error';
        if (code >= 500) return 'status-server-error';
        return 'status-unknown';
    }


    getDirectionIcon(direction: string): string {
        return direction === 'Push' ? 'fas fa-arrow-up' : 'fas fa-arrow-down';
    }


    getDirectionClass(direction: string): string {
        return direction === 'Push' ? 'direction-push' : 'direction-pull';
    }


    formatRelativeTime(dateStr: string): string {
        const date = new Date(dateStr);
        const now = new Date();
        const diffMs = now.getTime() - date.getTime();
        const diffMins = Math.floor(diffMs / 60000);

        if (diffMins < 1) return 'just now';
        if (diffMins < 60) return `${diffMins}m ago`;
        const diffHours = Math.floor(diffMins / 60);
        if (diffHours < 24) return `${diffHours}h ago`;
        const diffDays = Math.floor(diffHours / 24);
        if (diffDays < 7) return `${diffDays}d ago`;
        return date.toLocaleDateString();
    }


    // ───────────────────── Utility ─────────────────────

    getQuantityAvailable(part: CollectionPart): number {
        return Math.max(0, part.quantityOwned - part.quantityUsed);
    }

    getThumbnailKey(part: CollectionPart): string {
        return LDrawThumbnailService.cacheKey(part.geometryOriginalFileName, this.normalizeHex(part.colourHex));
    }

    getColourStyle(hex: string): string {
        if (!hex) return '#999';
        return hex.startsWith('#') ? hex : `#${hex}`;
    }

    /**
     * Strip leading '#' from hex colour values returned by the API.
     * The DB stores '#A0A5A9' but the thumbnail service expects 'A0A5A9'.
     */
    private normalizeHex(hex: string | undefined | null): string | undefined {
        if (!hex) return undefined;
        return hex.startsWith('#') ? hex.substring(1) : hex;
    }

    getStatIcon(stat: string): string {
        switch (stat) {
            case 'unique': return 'fas fa-puzzle-piece';
            case 'total': return 'fas fa-cubes';
            case 'wishlist': return 'fas fa-star';
            case 'sets': return 'fas fa-box-open';
            default: return 'fas fa-info-circle';
        }
    }
}
