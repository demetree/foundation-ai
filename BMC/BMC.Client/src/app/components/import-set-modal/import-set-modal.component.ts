import { Component, OnInit, OnDestroy } from '@angular/core';
import { NgbActiveModal } from '@ng-bootstrap/ng-bootstrap';
import { Subject } from 'rxjs';
import { takeUntil, debounceTime, finalize } from 'rxjs/operators';
import { CollectionService, SetSearchResult, ImportSetResult } from '../../services/collection.service';

@Component({
    selector: 'app-import-set-modal',
    templateUrl: './import-set-modal.component.html',
    styleUrl: './import-set-modal.component.scss'
})
export class ImportSetModalComponent implements OnInit, OnDestroy {

    collectionId: number;

    searchTerm = '';
    quantity = 1;
    importing = false;
    searchingForSets = false;
    setSearchResults: SetSearchResult[] = [];
    selectedSet: SetSearchResult | null = null;
    lastImportResult: ImportSetResult | null = null;

    private searchSubject = new Subject<string>();
    private destroy$ = new Subject<void>();

    constructor(
        public activeModal: NgbActiveModal,
        private collectionService: CollectionService
    ) { }


    ngOnInit(): void {
        this.searchSubject.pipe(
            debounceTime(300),
            takeUntil(this.destroy$)
        ).subscribe(term => {
            this.doSearch(term);
        });
    }


    ngOnDestroy(): void {
        this.destroy$.next();
        this.destroy$.complete();
    }


    onSearchInput(event: Event): void {
        const term = (event.target as HTMLInputElement).value;
        this.searchTerm = term;
        this.selectedSet = null;
        this.lastImportResult = null;
        if (term.length >= 2) {
            this.searchingForSets = true;
            this.searchSubject.next(term);
        } else {
            this.setSearchResults = [];
            this.searchingForSets = false;
        }
    }


    doSearch(term: string): void {
        this.collectionService.searchSets(term).pipe(
            takeUntil(this.destroy$),
            finalize(() => this.searchingForSets = false)
        ).subscribe({
            next: (results) => this.setSearchResults = results,
            error: () => this.setSearchResults = []
        });
    }


    selectSet(set: SetSearchResult): void {
        this.selectedSet = set;
        this.setSearchResults = [];
        this.searchTerm = `${set.setNumber} — ${set.name}`;
    }


    clearSelection(): void {
        this.selectedSet = null;
        this.searchTerm = '';
        this.setSearchResults = [];
    }


    importSet(): void {
        if (!this.selectedSet) return;

        this.importing = true;
        this.collectionService.importSet(this.collectionId, this.selectedSet.setNumber, this.quantity).pipe(
            takeUntil(this.destroy$),
            finalize(() => this.importing = false)
        ).subscribe({
            next: (result) => {
                this.lastImportResult = result;
                // Close after a brief moment so the user sees the result
                setTimeout(() => this.activeModal.close(result), 1200);
            },
            error: () => {
                this.lastImportResult = null;
            }
        });
    }
}
