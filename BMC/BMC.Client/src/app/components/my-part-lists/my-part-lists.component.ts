import { Component, OnInit, OnDestroy } from '@angular/core';
import { Router } from '@angular/router';
import { Subject } from 'rxjs';
import { takeUntil, finalize, debounceTime } from 'rxjs/operators';
import { MyPartListsService, PartListSummary, PartListDetail, PartListItem } from '../../services/my-part-lists.service';
import { ConfirmationService } from '../../services/confirmation-service';

@Component({
    selector: 'app-my-part-lists',
    templateUrl: './my-part-lists.component.html',
    styleUrls: ['./my-part-lists.component.scss']
})
export class MyPartListsComponent implements OnInit, OnDestroy {
    Math = Math;  // Expose Math for template bindings
    private destroy$ = new Subject<void>();
    private itemSearchSubject = new Subject<string>();

    // ─── Views ───
    view: 'lists' | 'detail' = 'lists';

    // ─── Lists View ───
    lists: PartListSummary[] = [];
    loading = true;

    // Create form
    showCreateForm = false;
    newListName = '';
    newListBuildable = false;
    creating = false;

    // Edit list
    editingListId: number | null = null;
    editListName = '';
    editListBuildable = false;
    savingList = false;

    // ─── Detail View ───
    selectedList: PartListDetail | null = null;
    loadingDetail = false;
    itemSearchText = '';
    filteredItems: PartListItem[] = [];

    // Edit item
    editingItemId: number | null = null;
    editItemQuantity = 1;
    savingItem = false;

    // Feedback
    successMessage = '';
    errorMessage = '';
    private feedbackTimeout: any;


    constructor(
        public router: Router,
        private partListsService: MyPartListsService,
        private confirmationService: ConfirmationService
    ) { }


    ngOnInit(): void {
        this.loadLists();

        // Debounced item search
        this.itemSearchSubject.pipe(
            debounceTime(250),
            takeUntil(this.destroy$)
        ).subscribe(() => {
            this.applyItemFilter();
        });
    }


    ngOnDestroy(): void {
        this.destroy$.next();
        this.destroy$.complete();
        clearTimeout(this.feedbackTimeout);
    }


    // ═══════════════════════════════════════════════════════════════
    //  LISTS VIEW
    // ═══════════════════════════════════════════════════════════════

    loadLists(): void {
        this.loading = true;
        this.partListsService.getAll().pipe(
            takeUntil(this.destroy$),
            finalize(() => this.loading = false)
        ).subscribe({
            next: (lists) => this.lists = lists,
            error: () => this.showError('Failed to load part lists.')
        });
    }


    // ─── Create ───

    toggleCreateForm(): void {
        this.showCreateForm = !this.showCreateForm;
        if (this.showCreateForm) {
            this.newListName = '';
            this.newListBuildable = false;
        }
    }

    createList(): void {
        if (!this.newListName.trim()) return;
        this.creating = true;

        this.partListsService.create(this.newListName.trim(), this.newListBuildable).pipe(
            takeUntil(this.destroy$),
            finalize(() => this.creating = false)
        ).subscribe({
            next: (result) => {
                this.showSuccess(`Created: ${result.name}`);
                this.showCreateForm = false;
                this.loadLists();
            },
            error: () => this.showError('Failed to create list.')
        });
    }

    // ─── Edit list ───

    startEditList(list: PartListSummary, event: Event): void {
        event.stopPropagation();
        this.editingListId = list.id;
        this.editListName = list.name;
        this.editListBuildable = list.isBuildable;
    }

    cancelEditList(): void {
        this.editingListId = null;
    }

    saveEditList(list: PartListSummary): void {
        this.savingList = true;
        this.partListsService.update(list.id, this.editListName.trim(), this.editListBuildable).pipe(
            takeUntil(this.destroy$),
            finalize(() => this.savingList = false)
        ).subscribe({
            next: () => {
                this.showSuccess('List updated.');
                this.editingListId = null;
                this.loadLists();
            },
            error: () => this.showError('Failed to update list.')
        });
    }

    // ─── Delete list ───

    async deleteList(list: PartListSummary, event: Event): Promise<void> {
        event.stopPropagation();
        const confirmed = await this.confirmationService.confirm(
            'Delete Part List',
            `Delete "${list.name}" and all its ${list.itemCount} items?`
        );
        if (!confirmed) return;

        this.partListsService.delete(list.id).pipe(
            takeUntil(this.destroy$)
        ).subscribe({
            next: () => {
                this.showSuccess(`Deleted: ${list.name}`);
                this.loadLists();
            },
            error: () => this.showError('Failed to delete list.')
        });
    }


    // ─── Open detail ───

    openList(list: PartListSummary): void {
        if (this.editingListId === list.id) return;
        this.loadDetail(list.id);
    }


    // ═══════════════════════════════════════════════════════════════
    //  DETAIL VIEW
    // ═══════════════════════════════════════════════════════════════

    loadDetail(listId: number): void {
        this.loadingDetail = true;
        this.view = 'detail';
        this.itemSearchText = '';

        this.partListsService.getById(listId).pipe(
            takeUntil(this.destroy$),
            finalize(() => this.loadingDetail = false)
        ).subscribe({
            next: (detail) => {
                this.selectedList = detail;
                this.filteredItems = detail.items;
            },
            error: () => {
                this.showError('Failed to load list detail.');
                this.backToLists();
            }
        });
    }


    backToLists(): void {
        this.view = 'lists';
        this.selectedList = null;
        this.itemSearchText = '';
        this.loadLists();
    }


    // ─── Item filtering ───

    onItemSearch(event: Event): void {
        const value = (event.target as HTMLInputElement).value;
        this.itemSearchSubject.next(value);
    }

    applyItemFilter(): void {
        if (!this.selectedList) return;
        const term = this.itemSearchText.toLowerCase().trim();
        if (!term) {
            this.filteredItems = this.selectedList.items;
        } else {
            this.filteredItems = this.selectedList.items.filter(i =>
                i.partName?.toLowerCase().includes(term)
                || i.partNum?.toLowerCase().includes(term)
                || i.colourName?.toLowerCase().includes(term)
                || i.partCategory?.toLowerCase().includes(term)
            );
        }
    }


    // ─── Edit item ───

    startEditItem(item: PartListItem, event: Event): void {
        event.stopPropagation();
        this.editingItemId = item.id;
        this.editItemQuantity = item.quantity;
    }

    cancelEditItem(): void {
        this.editingItemId = null;
    }

    saveEditItem(item: PartListItem): void {
        if (!this.selectedList) return;
        this.savingItem = true;

        this.partListsService.updateItem(
            this.selectedList.id,
            item.id,
            this.editItemQuantity
        ).pipe(
            takeUntil(this.destroy$),
            finalize(() => this.savingItem = false)
        ).subscribe({
            next: () => {
                this.showSuccess('Part updated.');
                this.editingItemId = null;
                this.loadDetail(this.selectedList!.id);
            },
            error: () => this.showError('Failed to update part.')
        });
    }


    // ─── Quantity shortcut ───

    updateItemQuantity(item: PartListItem, delta: number): void {
        if (!this.selectedList) return;
        const newQty = item.quantity + delta;
        if (newQty < 1) return;

        this.partListsService.updateItem(
            this.selectedList.id,
            item.id,
            newQty
        ).pipe(
            takeUntil(this.destroy$)
        ).subscribe({
            next: () => {
                item.quantity = newQty;
            },
            error: () => this.showError('Failed to update quantity.')
        });
    }


    // ─── Remove item ───

    async removeItem(item: PartListItem, event: Event): Promise<void> {
        event.stopPropagation();
        if (!this.selectedList) return;

        const confirmed = await this.confirmationService.confirm(
            'Remove Part',
            `Remove "${item.partName}" (${item.colourName}) from this list?`
        );
        if (!confirmed) return;

        this.partListsService.removeItem(this.selectedList.id, item.id).pipe(
            takeUntil(this.destroy$)
        ).subscribe({
            next: () => {
                this.showSuccess(`${item.partName} removed.`);
                this.loadDetail(this.selectedList!.id);
            },
            error: () => this.showError('Failed to remove part.')
        });
    }


    // ═══════════════════════════════════════════════════════════════
    //  HELPERS
    // ═══════════════════════════════════════════════════════════════

    get totalParts(): number {
        return this.filteredItems.reduce((sum, i) => sum + i.quantity, 0);
    }

    get uniqueColours(): number {
        const colourIds = new Set(this.filteredItems.map(i => i.brickColourId));
        return colourIds.size;
    }


    getColourStyle(hex: string | null): { [key: string]: string } {
        if (!hex) return {};
        return { 'background-color': `#${hex}` };
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
