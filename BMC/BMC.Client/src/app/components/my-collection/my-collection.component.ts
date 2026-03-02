import { Component, OnInit, OnDestroy } from '@angular/core';
import { Router } from '@angular/router';
import { Subject } from 'rxjs';
import { takeUntil, debounceTime, finalize } from 'rxjs/operators';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { CollectionService, CollectionSummary, CollectionPart, ImportedSetRecord, WishlistItem, ImportSetResult } from '../../services/collection.service';
import { AlertService, MessageSeverity } from '../../services/alert.service';
import { ConfirmationService } from '../../services/confirmation-service';
import { LDrawThumbnailService } from '../../services/ldraw-thumbnail.service';
import { ImportSetModalComponent } from '../import-set-modal/import-set-modal.component';

type TabId = 'parts' | 'imported-sets' | 'wishlist';

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



    constructor(
        private router: Router,
        private collectionService: CollectionService,
        private alertService: AlertService,
        private confirmationService: ConfirmationService,
        private thumbnailService: LDrawThumbnailService,
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
