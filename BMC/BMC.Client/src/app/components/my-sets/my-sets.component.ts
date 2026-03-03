import { Component, OnInit, OnDestroy } from '@angular/core';
import { Router } from '@angular/router';
import { Subject } from 'rxjs';
import { takeUntil, debounceTime, finalize } from 'rxjs/operators';
import { MySetsService, OwnedSet, CollectionStats, ThemeOption } from '../../services/my-sets.service';
import { CollectionService, SetSearchResult } from '../../services/collection.service';
import { AlertService } from '../../services/alert.service';
import { ConfirmationService } from '../../services/confirmation-service';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';

@Component({
    selector: 'app-my-sets',
    templateUrl: './my-sets.component.html',
    styleUrls: ['./my-sets.component.scss']
})
export class MySetsComponent implements OnInit, OnDestroy {
    Math = Math;  // Expose Math for template bindings
    private destroy$ = new Subject<void>();
    private searchSubject = new Subject<string>();

    // Data
    sets: OwnedSet[] = [];
    stats: CollectionStats | null = null;
    themes: ThemeOption[] = [];

    // State
    loading = true;
    loadingStats = false;
    searchText = '';
    selectedStatus = '';
    selectedThemeId: number | null = null;
    sortBy = 'recent';
    currentPage = 1;
    pageSize = 24;
    totalCount = 0;

    // Editing
    editingSet: OwnedSet | null = null;
    editQuantity = 1;
    editRating: number | null = null;
    editNotes = '';
    editStatus = '';
    savingEdit = false;

    // Add set modal
    showAddModal = false;
    setSearchQuery = '';
    setSearchResults: SetSearchResult[] = [];
    searching = false;
    addingSet = false;

    // Feedback
    successMessage = '';
    errorMessage = '';
    private feedbackTimeout: any;

    // Sort options for the dropdown
    sortOptions = [
        { value: 'recent', label: 'Recently Added' },
        { value: 'name', label: 'Set Name' },
        { value: 'year', label: 'Year' },
        { value: 'rating', label: 'Rating' },
        { value: 'parts', label: 'Part Count' },
        { value: 'acquired', label: 'Date Acquired' },
    ];

    // Status options
    statusOptions = ['Owned', 'Built', 'Sealed', 'Wishlist', 'For Sale', 'Retired'];

    constructor(
        public router: Router,
        private mySetsService: MySetsService,
        private collectionService: CollectionService,
        private alertService: AlertService,
        private confirmationService: ConfirmationService,
        private modalService: NgbModal
    ) { }


    ngOnInit(): void {
        this.loadSets();
        this.loadStats();
        this.loadThemes();

        // Debounced search
        this.searchSubject.pipe(
            debounceTime(400),
            takeUntil(this.destroy$)
        ).subscribe(() => {
            this.currentPage = 1;
            this.loadSets();
        });
    }


    ngOnDestroy(): void {
        this.destroy$.next();
        this.destroy$.complete();
        clearTimeout(this.feedbackTimeout);
    }


    // ─────────────────────────── Data Loading ───────────────────────────

    loadSets(): void {
        this.loading = true;

        this.mySetsService.getAll({
            search: this.searchText || undefined,
            status: this.selectedStatus || undefined,
            themeId: this.selectedThemeId ?? undefined,
            sortBy: this.sortBy,
            pageSize: this.pageSize,
            pageNumber: this.currentPage
        }).pipe(
            takeUntil(this.destroy$),
            finalize(() => this.loading = false)
        ).subscribe({
            next: (page) => {
                this.sets = page.items;
                this.totalCount = page.totalCount;
            },
            error: () => {
                this.showError('Failed to load sets.');
            }
        });
    }


    loadStats(): void {
        this.loadingStats = true;
        this.mySetsService.getStats().pipe(
            takeUntil(this.destroy$),
            finalize(() => this.loadingStats = false)
        ).subscribe({
            next: (stats) => this.stats = stats,
            error: () => { /* silent */ }
        });
    }


    loadThemes(): void {
        this.mySetsService.getThemes().pipe(
            takeUntil(this.destroy$)
        ).subscribe({
            next: (themes) => this.themes = themes,
            error: () => { /* silent */ }
        });
    }


    // ─────────────────────────── Search & Filters ───────────────────────────

    onSearch(event: Event): void {
        const value = (event.target as HTMLInputElement).value;
        this.searchSubject.next(value);
    }


    onFilterChange(): void {
        this.currentPage = 1;
        this.loadSets();
    }


    onSortChange(): void {
        this.currentPage = 1;
        this.loadSets();
    }


    clearFilters(): void {
        this.searchText = '';
        this.selectedStatus = '';
        this.selectedThemeId = null;
        this.sortBy = 'recent';
        this.currentPage = 1;
        this.loadSets();
    }


    get hasFilters(): boolean {
        return !!(this.searchText || this.selectedStatus || this.selectedThemeId);
    }


    // ─────────────────────────── Pagination ───────────────────────────

    get totalPages(): number {
        return Math.ceil(this.totalCount / this.pageSize);
    }


    nextPage(): void {
        if (this.currentPage < this.totalPages) {
            this.currentPage++;
            this.loadSets();
        }
    }


    prevPage(): void {
        if (this.currentPage > 1) {
            this.currentPage--;
            this.loadSets();
        }
    }


    goToPage(page: number): void {
        this.currentPage = page;
        this.loadSets();
    }


    getVisiblePages(): number[] {
        const pages: number[] = [];
        const start = Math.max(1, this.currentPage - 2);
        const end = Math.min(this.totalPages, this.currentPage + 2);
        for (let i = start; i <= end; i++) pages.push(i);
        return pages;
    }


    // ─────────────────────────── Edit Set ───────────────────────────

    startEdit(set: OwnedSet, event: Event): void {
        event.stopPropagation();
        this.editingSet = set;
        this.editQuantity = set.quantity;
        this.editRating = set.personalRating;
        this.editNotes = set.notes || '';
        this.editStatus = set.status || 'Owned';
    }


    cancelEdit(): void {
        this.editingSet = null;
    }


    saveEdit(): void {
        if (!this.editingSet) return;
        this.savingEdit = true;

        this.mySetsService.updateSet(this.editingSet.id, {
            quantity: this.editQuantity,
            personalRating: this.editRating ?? undefined,
            notes: this.editNotes,
            status: this.editStatus
        }).pipe(
            takeUntil(this.destroy$),
            finalize(() => this.savingEdit = false)
        ).subscribe({
            next: () => {
                this.showSuccess('Set updated successfully.');
                this.editingSet = null;
                this.loadSets();
                this.loadStats();
            },
            error: () => this.showError('Failed to update set.')
        });
    }


    // ─────────────────────────── Quantity Controls ───────────────────────────

    updateQuantity(set: OwnedSet, delta: number): void {
        const newQty = set.quantity + delta;
        if (newQty < 1) return;

        this.mySetsService.updateSet(set.id, { quantity: newQty }).pipe(
            takeUntil(this.destroy$)
        ).subscribe({
            next: () => {
                set.quantity = newQty;
                this.loadStats();
            },
            error: () => this.showError('Failed to update quantity.')
        });
    }


    // ─────────────────────────── Remove Set ───────────────────────────

    async removeSet(set: OwnedSet, event: Event): Promise<void> {
        event.stopPropagation();
        const confirmed = await this.confirmationService.confirm(
            'Remove Set',
            `Remove "${set.setName}" (${set.setNumber}) from your collection?`
        );
        if (!confirmed) return;

        this.mySetsService.removeSet(set.id).pipe(
            takeUntil(this.destroy$)
        ).subscribe({
            next: () => {
                this.showSuccess(`${set.setName} removed from collection.`);
                this.loadSets();
                this.loadStats();
            },
            error: () => this.showError('Failed to remove set.')
        });
    }


    // ─────────────────────────── Add Set Modal ───────────────────────────

    openAddModal(): void {
        this.showAddModal = true;
        this.setSearchQuery = '';
        this.setSearchResults = [];
    }


    closeAddModal(): void {
        this.showAddModal = false;
    }


    searchSets(): void {
        if (this.setSearchQuery.length < 2) {
            this.setSearchResults = [];
            return;
        }

        this.searching = true;
        this.collectionService.searchSets(this.setSearchQuery).pipe(
            takeUntil(this.destroy$),
            finalize(() => this.searching = false)
        ).subscribe({
            next: (results) => this.setSearchResults = results,
            error: () => this.setSearchResults = []
        });
    }


    addSetFromSearch(result: SetSearchResult): void {
        if (this.addingSet) return;
        this.addingSet = true;

        this.mySetsService.addSet(result.id, 1, 'Owned').pipe(
            takeUntil(this.destroy$),
            finalize(() => this.addingSet = false)
        ).subscribe({
            next: (response) => {
                const verb = response.action === 'updated' ? 'Updated' : 'Added';
                this.showSuccess(`${verb}: ${result.name}`);
                this.closeAddModal();
                this.loadSets();
                this.loadStats();
            },
            error: () => this.showError('Failed to add set.')
        });
    }


    // ─────────────────────────── Star Rating ───────────────────────────

    getStars(rating: number | null): number[] {
        return [1, 2, 3, 4, 5];
    }


    setEditRating(star: number): void {
        this.editRating = this.editRating === star ? null : star;
    }


    // ─────────────────────────── Helpers ───────────────────────────

    getStatusIcon(status: string): string {
        switch (status?.toLowerCase()) {
            case 'owned': return 'fas fa-box';
            case 'built': return 'fas fa-hammer';
            case 'sealed': return 'fas fa-lock';
            case 'wishlist': return 'fas fa-heart';
            case 'for sale': return 'fas fa-tag';
            case 'retired': return 'fas fa-archive';
            default: return 'fas fa-box';
        }
    }


    getStatusClass(status: string): string {
        switch (status?.toLowerCase()) {
            case 'owned': return 'status-owned';
            case 'built': return 'status-built';
            case 'sealed': return 'status-sealed';
            case 'wishlist': return 'status-wishlist';
            case 'for sale': return 'status-forsale';
            case 'retired': return 'status-retired';
            default: return 'status-owned';
        }
    }


    navigateToSet(set: OwnedSet): void {
        this.router.navigate(['/lego/sets', set.legoSetId]);
    }


    private showSuccess(msg: string): void {
        this.successMessage = msg;
        this.errorMessage = '';
        clearTimeout(this.feedbackTimeout);
        this.feedbackTimeout = setTimeout(() => this.successMessage = '', 4000);
    }


    private showError(msg: string): void {
        this.errorMessage = msg;
        this.successMessage = '';
        clearTimeout(this.feedbackTimeout);
        this.feedbackTimeout = setTimeout(() => this.errorMessage = '', 6000);
    }
}
