import { Component, OnInit, OnDestroy } from '@angular/core';
import { Router } from '@angular/router';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import {
    SetListService,
    SetListSummary,
    SetListDetail,
    SetListItem,
    VersionHistoryEntry
} from '../../services/set-list.service';
import { CollectionService, SetSearchResult } from '../../services/collection.service';
import { AuthService } from '../../services/auth.service';


@Component({
    selector: 'app-my-set-lists',
    templateUrl: './my-set-lists.component.html',
    styleUrls: ['./my-set-lists.component.scss']
})
export class MySetListsComponent implements OnInit, OnDestroy {

    private destroy$ = new Subject<void>();

    // ───── State ─────
    loading = true;
    setLists: SetListSummary[] = [];
    selectedList: SetListDetail | null = null;
    activeView: 'lists' | 'detail' = 'lists';
    activeTab: 'items' | 'history' = 'items';

    // Create / edit
    showCreateForm = false;
    newListName = '';
    newListBuildable = false;
    editingList: SetListSummary | null = null;
    editName = '';
    editBuildable = false;

    // Add set modal
    showAddSetModal = false;
    setSearchQuery = '';
    setSearchResults: SetSearchResult[] = [];
    searching = false;
    addingSet = false;

    // History
    historyEntries: VersionHistoryEntry[] = [];
    loadingHistory = false;

    // Item operations
    savingItem = false;

    // Inline feedback
    successMessage = '';
    errorMessage = '';

    constructor(
        public router: Router,
        private setListService: SetListService,
        private collectionService: CollectionService,
        private authService: AuthService
    ) { }


    ngOnInit(): void {
        this.loadSetLists();
    }

    ngOnDestroy(): void {
        this.destroy$.next();
        this.destroy$.complete();
    }


    // ═══════════════════════════════════════════════════════════════
    // Data Loading
    // ═══════════════════════════════════════════════════════════════

    loadSetLists(): void {
        this.loading = true;
        this.setListService.getAll()
            .pipe(takeUntil(this.destroy$))
            .subscribe({
                next: (lists) => {
                    this.setLists = lists;
                    this.loading = false;
                },
                error: (err) => {
                    this.errorMessage = 'Failed to load set lists.';
                    this.loading = false;
                    console.error('Error loading set lists:', err);
                }
            });
    }

    openListDetail(list: SetListSummary): void {
        this.selectedList = null;
        this.activeView = 'detail';
        this.activeTab = 'items';
        this.historyEntries = [];

        this.setListService.getById(list.id)
            .pipe(takeUntil(this.destroy$))
            .subscribe({
                next: (detail) => {
                    this.selectedList = detail;
                },
                error: (err) => {
                    this.errorMessage = 'Failed to load set list details.';
                    console.error('Error loading set list:', err);
                }
            });
    }

    goBackToLists(): void {
        this.activeView = 'lists';
        this.selectedList = null;
        this.loadSetLists();
    }

    loadHistory(): void {
        if (!this.selectedList || this.historyEntries.length > 0) return;
        this.loadingHistory = true;
        this.setListService.getHistory(this.selectedList.id)
            .pipe(takeUntil(this.destroy$))
            .subscribe({
                next: (entries) => {
                    this.historyEntries = entries;
                    this.loadingHistory = false;
                },
                error: () => {
                    this.historyEntries = [];
                    this.loadingHistory = false;
                }
            });
    }


    // ═══════════════════════════════════════════════════════════════
    // CRUD — Set Lists
    // ═══════════════════════════════════════════════════════════════

    createList(): void {
        if (!this.newListName.trim()) return;
        this.setListService.create(this.newListName.trim(), this.newListBuildable)
            .pipe(takeUntil(this.destroy$))
            .subscribe({
                next: (result) => {
                    this.showSuccess(`Created "${result.name}"`);
                    this.showCreateForm = false;
                    this.newListName = '';
                    this.newListBuildable = false;
                    this.loadSetLists();
                },
                error: (err) => {
                    this.showError('Failed to create set list.');
                    console.error('Create error:', err);
                }
            });
    }

    startEditList(list: SetListSummary, event: Event): void {
        event.stopPropagation();
        this.editingList = list;
        this.editName = list.name;
        this.editBuildable = list.isBuildable;
    }

    cancelEditList(): void {
        this.editingList = null;
    }

    saveEditList(): void {
        if (!this.editingList || !this.editName.trim()) return;
        this.setListService.update(this.editingList.id, this.editName.trim(), this.editBuildable)
            .pipe(takeUntil(this.destroy$))
            .subscribe({
                next: () => {
                    this.showSuccess('List updated.');
                    this.editingList = null;
                    this.loadSetLists();
                },
                error: (err) => {
                    this.showError('Failed to update set list.');
                    console.error('Update error:', err);
                }
            });
    }

    deleteList(list: SetListSummary, event: Event): void {
        event.stopPropagation();
        if (!confirm(`Delete "${list.name}"? This will also remove all items in the list.`)) return;

        this.setListService.delete(list.id)
            .pipe(takeUntil(this.destroy$))
            .subscribe({
                next: () => {
                    this.showSuccess(`Deleted "${list.name}".`);
                    this.loadSetLists();
                },
                error: (err) => {
                    this.showError('Failed to delete set list.');
                    console.error('Delete error:', err);
                }
            });
    }


    // ═══════════════════════════════════════════════════════════════
    // CRUD — Set List Items
    // ═══════════════════════════════════════════════════════════════

    openAddSetModal(): void {
        this.showAddSetModal = true;
        this.setSearchQuery = '';
        this.setSearchResults = [];
    }

    closeAddSetModal(): void {
        this.showAddSetModal = false;
    }

    searchSets(): void {
        const query = this.setSearchQuery.trim();
        if (query.length < 2) {
            this.setSearchResults = [];
            return;
        }
        this.searching = true;
        this.collectionService.searchSets(query)
            .pipe(takeUntil(this.destroy$))
            .subscribe({
                next: (results) => {
                    this.setSearchResults = results;
                    this.searching = false;
                },
                error: () => {
                    this.setSearchResults = [];
                    this.searching = false;
                }
            });
    }

    addSetToList(set: SetSearchResult): void {
        if (!this.selectedList || this.addingSet) return;
        this.addingSet = true;
        this.setListService.addItem(this.selectedList.id, set.id, 1, true)
            .pipe(takeUntil(this.destroy$))
            .subscribe({
                next: () => {
                    this.showSuccess(`Added "${set.name}" to the list.`);
                    this.addingSet = false;
                    this.closeAddSetModal();
                    // Reload detail
                    if (this.selectedList) {
                        this.openListDetail({ id: this.selectedList.id } as SetListSummary);
                    }
                },
                error: (err) => {
                    this.showError('Failed to add set.');
                    this.addingSet = false;
                    console.error('Add set error:', err);
                }
            });
    }

    updateItemQuantity(item: SetListItem, delta: number): void {
        if (!this.selectedList) return;
        const newQty = Math.max(item.quantity + delta, 1);
        this.savingItem = true;
        this.setListService.updateItem(this.selectedList.id, item.id, newQty)
            .pipe(takeUntil(this.destroy$))
            .subscribe({
                next: () => {
                    item.quantity = newQty;
                    this.savingItem = false;
                },
                error: () => {
                    this.showError('Failed to update quantity.');
                    this.savingItem = false;
                }
            });
    }

    removeItem(item: SetListItem): void {
        if (!this.selectedList) return;
        if (!confirm(`Remove "${item.setName}" from this list?`)) return;

        this.setListService.removeItem(this.selectedList.id, item.id)
            .pipe(takeUntil(this.destroy$))
            .subscribe({
                next: () => {
                    this.showSuccess(`Removed "${item.setName}".`);
                    if (this.selectedList) {
                        this.selectedList.items = this.selectedList.items.filter(i => i.id !== item.id);
                    }
                },
                error: () => {
                    this.showError('Failed to remove set.');
                }
            });
    }


    // ═══════════════════════════════════════════════════════════════
    // Tab management
    // ═══════════════════════════════════════════════════════════════

    setTab(tab: 'items' | 'history'): void {
        this.activeTab = tab;
        if (tab === 'history') {
            this.loadHistory();
        }
    }


    // ═══════════════════════════════════════════════════════════════
    // Helpers
    // ═══════════════════════════════════════════════════════════════

    getTotalSets(): number {
        return this.setLists.reduce((sum, l) => sum + l.totalSets, 0);
    }

    getSyncedCount(): number {
        return this.setLists.filter(l => l.rebrickableListId != null).length;
    }

    getSyncStatusClass(list: SetListSummary | SetListDetail): string {
        if (list.pendingSyncCount > 0) return 'syncing';
        if (list.rebrickableListId) return 'synced';
        return 'local';
    }

    getSyncStatusLabel(list: SetListSummary | SetListDetail): string {
        if (list.pendingSyncCount > 0) return `${list.pendingSyncCount} pending`;
        if (list.rebrickableListId) return 'Synced';
        return 'Local only';
    }

    formatDate(dateStr: string): string {
        if (!dateStr) return '—';
        const d = new Date(dateStr);
        return d.toLocaleDateString(undefined, { year: 'numeric', month: 'short', day: 'numeric' });
    }

    private showSuccess(msg: string): void {
        this.successMessage = msg;
        this.errorMessage = '';
        setTimeout(() => this.successMessage = '', 4000);
    }

    private showError(msg: string): void {
        this.errorMessage = msg;
        this.successMessage = '';
        setTimeout(() => this.errorMessage = '', 6000);
    }
}
