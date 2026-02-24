import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';
import { map, takeUntil } from 'rxjs/operators';
import {
    UserSetOwnershipService,
    UserSetOwnershipData,
    UserSetOwnershipSubmitData
} from '../bmc-data-services/user-set-ownership.service';

/**
 * SetOwnershipCacheService
 *
 * Singleton service that loads the user's complete ownership list once and exposes
 * O(1) lookups by legoSetId. Provides reactive observables so the UI updates
 * automatically when ownership changes.
 *
 * Status values: "owned" | "wanted"
 */
@Injectable({
    providedIn: 'root'
})
export class SetOwnershipCacheService {

    private _loaded = false;
    private _loading = false;
    private _records: UserSetOwnershipData[] = [];

    // Map from legoSetId → ownership record
    private _bySetId = new Map<number, UserSetOwnershipData>();

    // Reactive sets for quick UI binding
    private _ownedIds$ = new BehaviorSubject<Set<number>>(new Set());
    private _wantedIds$ = new BehaviorSubject<Set<number>>(new Set());

    readonly ownedIds$: Observable<Set<number>> = this._ownedIds$.asObservable();
    readonly wantedIds$: Observable<Set<number>> = this._wantedIds$.asObservable();

    get ownedCount(): number { return this._ownedIds$.value.size; }
    get wantedCount(): number { return this._wantedIds$.value.size; }

    constructor(private ownershipService: UserSetOwnershipService) { }


    /**
     * Ensure the cache is loaded. Safe to call multiple times — only fetches once.
     */
    ensureLoaded(): void {
        if (this._loaded || this._loading) return;
        this._loading = true;

        this.ownershipService.GetUserSetOwnershipList({
            active: true,
            deleted: false,
            pageSize: 10000,
            pageNumber: 1
        }).subscribe({
            next: (records) => {
                this._records = records;
                this.rebuildMaps();
                this._loaded = true;
                this._loading = false;
            },
            error: () => {
                this._loading = false;
            }
        });
    }


    /**
     * Force a full refresh of the cache from the server.
     */
    refresh(): void {
        this._loaded = false;
        this._loading = false;
        this.ownershipService.ClearAllCaches();
        this.ensureLoaded();
    }


    /**
     * Synchronous O(1) check whether a set is owned.
     */
    isOwned(legoSetId: number): boolean {
        const rec = this._bySetId.get(legoSetId);
        return !!rec && rec.status === 'owned';
    }


    /**
     * Synchronous O(1) check whether a set is on the wishlist.
     */
    isWanted(legoSetId: number): boolean {
        const rec = this._bySetId.get(legoSetId);
        return !!rec && rec.status === 'wanted';
    }


    /**
     * Toggle ownership for a set. If the set is already in the given status,
     * it will be soft-deleted. If in a different status, the status is updated.
     * If no record exists, a new one is created.
     */
    async toggleOwnership(legoSetId: number, status: 'owned' | 'wanted'): Promise<void> {
        const existing = this._bySetId.get(legoSetId);

        if (existing && existing.status === status) {
            // Remove — soft-delete
            await this.ownershipService.DeleteUserSetOwnership(existing.id).toPromise();
            this._records = this._records.filter(r => r !== existing);
        } else if (existing) {
            // Change status (e.g. wanted → owned)
            const submit = existing.ConvertToSubmitData();
            submit.status = status;
            const updated = await this.ownershipService
                .PutUserSetOwnership(existing.id, submit).toPromise();
            if (updated) {
                const idx = this._records.indexOf(existing);
                if (idx >= 0) this._records[idx] = updated;
            }
        } else {
            // Create new
            const submit = new UserSetOwnershipSubmitData();
            submit.id = 0;
            submit.legoSetId = legoSetId;
            submit.status = status;
            submit.quantity = 1;
            submit.isPublic = true;
            submit.active = true;
            submit.deleted = false;
            const created = await this.ownershipService
                .PostUserSetOwnership(submit).toPromise();
            if (created) {
                this._records.push(created);
            }
        }

        this.rebuildMaps();
    }


    /**
     * Get the full set of owned legoSetIds (snapshot).
     */
    getOwnedIds(): Set<number> {
        return new Set(this._ownedIds$.value);
    }


    /**
     * Rebuild the internal lookup maps and emit new values.
     */
    private rebuildMaps(): void {
        this._bySetId.clear();
        const owned = new Set<number>();
        const wanted = new Set<number>();

        for (const r of this._records) {
            const sid = Number(r.legoSetId);
            this._bySetId.set(sid, r);
            if (r.status === 'owned') owned.add(sid);
            else if (r.status === 'wanted') wanted.add(sid);
        }

        this._ownedIds$.next(owned);
        this._wantedIds$.next(wanted);
    }
}
