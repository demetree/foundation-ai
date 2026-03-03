import { Component, OnInit, OnDestroy } from '@angular/core';
import { Subject } from 'rxjs';
import { takeUntil, finalize, debounceTime } from 'rxjs/operators';
import { MyLostPartsService, LostPart, LostPartsStats, AffectedSet } from '../../services/my-lost-parts.service';
import { ConfirmationService } from '../../services/confirmation-service';

@Component({
    selector: 'app-my-lost-parts',
    templateUrl: './my-lost-parts.component.html',
    styleUrls: ['./my-lost-parts.component.scss']
})
export class MyLostPartsComponent implements OnInit, OnDestroy {
    Math = Math;
    private destroy$ = new Subject<void>();
    private searchSubject = new Subject<string>();

    // Data
    lostParts: LostPart[] = [];
    stats: LostPartsStats | null = null;
    affectedSets: AffectedSet[] = [];
    loading = true;

    // Filters
    searchText = '';
    selectedSetId: number | null = null;
    sortBy = 'partName';

    // Edit
    editingId: number | null = null;
    editQuantity = 1;
    savingEdit = false;

    // Feedback
    successMessage = '';
    errorMessage = '';
    private feedbackTimeout: any;


    constructor(
        private lostPartsService: MyLostPartsService,
        private confirmationService: ConfirmationService
    ) { }


    ngOnInit(): void {
        this.loadAll();

        this.searchSubject.pipe(
            debounceTime(350),
            takeUntil(this.destroy$)
        ).subscribe(() => {
            this.loadLostParts();
        });
    }


    ngOnDestroy(): void {
        this.destroy$.next();
        this.destroy$.complete();
        clearTimeout(this.feedbackTimeout);
    }


    // ═══════════════════════════════════════════════════════════════
    //  DATA LOADING
    // ═══════════════════════════════════════════════════════════════

    loadAll(): void {
        this.loading = true;
        this.loadStats();
        this.loadAffectedSets();
        this.loadLostParts();
    }


    loadLostParts(): void {
        this.loading = true;
        this.lostPartsService.getAll(
            this.searchText || undefined,
            this.selectedSetId || undefined,
            this.sortBy
        ).pipe(
            takeUntil(this.destroy$),
            finalize(() => this.loading = false)
        ).subscribe({
            next: (parts) => this.lostParts = parts,
            error: () => this.showError('Failed to load lost parts.')
        });
    }


    loadStats(): void {
        this.lostPartsService.getStats().pipe(
            takeUntil(this.destroy$)
        ).subscribe({
            next: (stats) => this.stats = stats,
            error: () => { }
        });
    }


    loadAffectedSets(): void {
        this.lostPartsService.getAffectedSets().pipe(
            takeUntil(this.destroy$)
        ).subscribe({
            next: (sets) => this.affectedSets = sets,
            error: () => { }
        });
    }


    // ═══════════════════════════════════════════════════════════════
    //  FILTERS
    // ═══════════════════════════════════════════════════════════════

    onSearchInput(event: Event): void {
        this.searchSubject.next((event.target as HTMLInputElement).value);
    }

    clearSearch(): void {
        this.searchText = '';
        this.loadLostParts();
    }

    onSetFilter(event: Event): void {
        const val = (event.target as HTMLSelectElement).value;
        this.selectedSetId = val ? parseInt(val, 10) : null;
        this.loadLostParts();
    }

    onSortChange(event: Event): void {
        this.sortBy = (event.target as HTMLSelectElement).value;
        this.loadLostParts();
    }


    // ═══════════════════════════════════════════════════════════════
    //  QUANTITY CONTROLS
    // ═══════════════════════════════════════════════════════════════

    updateQuantity(part: LostPart, delta: number): void {
        const newQty = part.lostQuantity + delta;
        if (newQty < 1) return;

        this.lostPartsService.update(part.id, newQty).pipe(
            takeUntil(this.destroy$)
        ).subscribe({
            next: () => {
                part.lostQuantity = newQty;
            },
            error: () => this.showError('Failed to update quantity.')
        });
    }


    // ═══════════════════════════════════════════════════════════════
    //  EDIT
    // ═══════════════════════════════════════════════════════════════

    startEdit(part: LostPart, event: Event): void {
        event.stopPropagation();
        this.editingId = part.id;
        this.editQuantity = part.lostQuantity;
    }

    cancelEdit(): void {
        this.editingId = null;
    }

    saveEdit(part: LostPart): void {
        this.savingEdit = true;
        this.lostPartsService.update(part.id, this.editQuantity).pipe(
            takeUntil(this.destroy$),
            finalize(() => this.savingEdit = false)
        ).subscribe({
            next: () => {
                this.showSuccess('Quantity updated.');
                this.editingId = null;
                this.loadAll();
            },
            error: () => this.showError('Failed to update.')
        });
    }


    // ═══════════════════════════════════════════════════════════════
    //  REMOVE (Found it!)
    // ═══════════════════════════════════════════════════════════════

    async removePart(part: LostPart, event: Event): Promise<void> {
        event.stopPropagation();
        const confirmed = await this.confirmationService.confirm(
            'Found This Part?',
            `Mark "${part.partName}" (${part.colourName}) as found and remove from lost parts?`
        );
        if (!confirmed) return;

        this.lostPartsService.remove(part.id).pipe(
            takeUntil(this.destroy$)
        ).subscribe({
            next: () => {
                this.showSuccess(`${part.partName} found! Removed.`);
                this.loadAll();
            },
            error: () => this.showError('Failed to remove.')
        });
    }


    // ═══════════════════════════════════════════════════════════════
    //  HELPERS
    // ═══════════════════════════════════════════════════════════════

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
