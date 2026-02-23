import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';
import { SetExplorerItem } from './set-explorer-api.service';


const STORAGE_KEY = 'bmc-compare-sets';
const MAX_SETS = 4;


/**
 * Singleton service for managing the set comparison basket.
 * Persists selection in sessionStorage so it survives navigation.
 * Max 4 sets can be compared at once.
 */
@Injectable({
    providedIn: 'root'
})
export class SetComparisonService {

    private _sets: SetExplorerItem[] = [];
    private _sets$ = new BehaviorSubject<SetExplorerItem[]>([]);

    /** Observable of the current comparison set list */
    readonly sets$ = this._sets$.asObservable();

    /** Current count of sets in comparison */
    get count(): number { return this._sets.length; }

    /** Max allowed sets */
    readonly max = MAX_SETS;


    constructor() {
        this.loadFromStorage();
    }


    /** Add a set to comparison (no-op if already present or at max) */
    addSet(set: SetExplorerItem): void {
        if (this._sets.length >= MAX_SETS) return;
        if (this._sets.some(s => s.id === set.id)) return;
        this._sets = [...this._sets, set];
        this.emit();
    }

    /** Remove a set from comparison by ID */
    removeSet(id: number): void {
        this._sets = this._sets.filter(s => s.id !== id);
        this.emit();
    }

    /** Toggle a set in/out of comparison */
    toggleSet(set: SetExplorerItem): void {
        if (this.isInComparison(set.id)) {
            this.removeSet(set.id);
        } else {
            this.addSet(set);
        }
    }

    /** Check if a set is already in comparison */
    isInComparison(id: number): boolean {
        return this._sets.some(s => s.id === id);
    }

    /** Clear all sets from comparison */
    clearAll(): void {
        this._sets = [];
        this.emit();
    }


    private emit(): void {
        this._sets$.next(this._sets);
        this.saveToStorage();
    }

    private saveToStorage(): void {
        try {
            sessionStorage.setItem(STORAGE_KEY, JSON.stringify(this._sets));
        } catch { /* quota exceeded — silently ignore */ }
    }

    private loadFromStorage(): void {
        try {
            const raw = sessionStorage.getItem(STORAGE_KEY);
            if (raw) {
                this._sets = JSON.parse(raw) as SetExplorerItem[];
                this._sets$.next(this._sets);
            }
        } catch { /* corrupt data — start fresh */ }
    }
}
