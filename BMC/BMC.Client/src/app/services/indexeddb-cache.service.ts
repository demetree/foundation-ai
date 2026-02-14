import { Injectable } from '@angular/core';
import { Observable, from, of } from 'rxjs';
import { tap, switchMap } from 'rxjs/operators';
import Dexie, { Table } from 'dexie';


/**
 * Schema for a single cache entry stored in IndexedDB.
 */
export interface CacheEntry {
    /** Composite key: storeName + serialized params */
    key: string;
    /** Logical grouping, e.g. 'brick-parts', 'brick-categories' */
    storeName: string;
    /** Raw JSON data (before Revive*() hydration) */
    data: any;
    /** Timestamp when cached (Date.now()) */
    cachedAt: number;
    /** TTL in minutes — after this, entry is considered stale */
    ttlMinutes: number;
}


/**
 * Summary info about a cache store, used by getCacheInfo().
 */
export interface CacheStoreInfo {
    storeName: string;
    entryCount: number;
    oldestEntry: Date | null;
    newestEntry: Date | null;
}


/**
 * Dexie database for the BMC client-side cache.
 */
class BmcCacheDatabase extends Dexie {
    cacheEntries!: Table<CacheEntry, string>;

    constructor() {
        super('bmc-cache');

        this.version(1).stores({
            // key is the primary key; storeName is indexed for bulk operations
            cacheEntries: 'key, storeName'
        });
    }
}


/**
 * Generic IndexedDB caching service for BMC data services.
 *
 * Stores raw JSON in IndexedDB with configurable TTLs. On cache hit, returns
 * the raw data which the caller feeds through existing Revive*() pipelines.
 *
 * Usage:
 *   this.cacheService.getOrFetch<BrickPartData[]>(
 *       'brick-parts',
 *       { active: true, deleted: false },
 *       (params) => this.partService.GetBrickPartList(params),
 *       1440  // 24-hour TTL
 *   ).subscribe(parts => { ... });
 */
@Injectable()
export class IndexedDBCacheService {

    private db: BmcCacheDatabase;

    constructor() {
        this.db = new BmcCacheDatabase();
    }


    /**
     * Get data from cache if fresh, otherwise fetch from server and cache.
     *
     * @param storeName  Logical group name (e.g. 'brick-parts')
     * @param params     Query parameters — serialized into the cache key
     * @param fetchFn    Function that returns an Observable<T> from the server
     * @param ttlMinutes How long the cached data stays fresh (default: 1440 = 24h)
     * @param reviveFn   Optional function to rehydrate raw JSON on cache hits
     *                   (e.g. service.ReviveBrickPartList). Not needed for server
     *                   responses since the service already revives those.
     * @returns Observable<T> — either from cache or server
     */
    getOrFetch<T>(
        storeName: string,
        params: any,
        fetchFn: (params: any) => Observable<T>,
        ttlMinutes: number = 1440,
        reviveFn?: (raw: any) => T
    ): Observable<T> {

        const key = this.buildKey(storeName, params);

        return from(this.getCachedEntry(key)).pipe(
            switchMap(entry => {

                //
                // Cache hit: entry exists and is still fresh
                //
                if (entry && this.isFresh(entry)) {
                    const data = reviveFn ? reviveFn(entry.data) : entry.data as T;
                    return of(data);
                }

                //
                // Cache miss or stale: fetch from server and store
                //
                return fetchFn(params).pipe(
                    tap(data => {
                        //
                        // Store asynchronously — don't block the response
                        //
                        this.putEntry(key, storeName, data, ttlMinutes);
                    })
                );
            })
        );
    }


    /**
     * Invalidate a specific cache entry, or all entries for a store.
     */
    async invalidate(storeName: string, params?: any): Promise<void> {
        if (params !== undefined) {
            const key = this.buildKey(storeName, params);
            await this.db.cacheEntries.delete(key);
        } else {
            //
            // Delete all entries for this store
            //
            await this.db.cacheEntries.where('storeName').equals(storeName).delete();
        }
    }


    /**
     * Clear the entire cache database.
     */
    async clearAll(): Promise<void> {
        await this.db.cacheEntries.clear();
    }


    /**
     * Get summary info about all cache stores (for diagnostics / cache management UI).
     */
    async getCacheInfo(): Promise<CacheStoreInfo[]> {
        const allEntries = await this.db.cacheEntries.toArray();

        //
        // Group by storeName
        //
        const storeMap = new Map<string, CacheEntry[]>();
        for (const entry of allEntries) {
            if (!storeMap.has(entry.storeName)) {
                storeMap.set(entry.storeName, []);
            }
            storeMap.get(entry.storeName)!.push(entry);
        }

        const result: CacheStoreInfo[] = [];
        for (const [storeName, entries] of storeMap) {
            const timestamps = entries.map(e => e.cachedAt);
            result.push({
                storeName,
                entryCount: entries.length,
                oldestEntry: timestamps.length > 0 ? new Date(Math.min(...timestamps)) : null,
                newestEntry: timestamps.length > 0 ? new Date(Math.max(...timestamps)) : null
            });
        }

        return result;
    }


    // ─────────────────────────────────────────────────
    //  Internal helpers
    // ─────────────────────────────────────────────────

    /**
     * Build a deterministic cache key from store name + params.
     */
    private buildKey(storeName: string, params: any): string {
        if (params == null) {
            return storeName + '::_default_';
        }

        //
        // Sort keys for deterministic serialization
        //
        const sorted: Record<string, any> = {};
        for (const k of Object.keys(params).sort()) {
            if (params[k] != null) {
                sorted[k] = params[k];
            }
        }

        return storeName + '::' + JSON.stringify(sorted);
    }


    /**
     * Check if a cache entry is still within its TTL.
     */
    private isFresh(entry: CacheEntry): boolean {
        const ageMinutes = (Date.now() - entry.cachedAt) / (1000 * 60);
        return ageMinutes < entry.ttlMinutes;
    }


    /**
     * Get a cache entry from IndexedDB (or undefined if not found).
     */
    private async getCachedEntry(key: string): Promise<CacheEntry | undefined> {
        try {
            return await this.db.cacheEntries.get(key);
        } catch {
            // IndexedDB error — treat as cache miss
            return undefined;
        }
    }


    /**
     * Store a cache entry in IndexedDB.
     */
    private async putEntry(key: string, storeName: string, data: any, ttlMinutes: number): Promise<void> {
        try {
            //
            // Serialize to plain JSON first — revived objects contain BehaviorSubjects,
            // Observables, and prototype methods that IndexedDB's structured clone
            // algorithm cannot handle. JSON round-trip strips all non-serializable props.
            //
            const plainData = JSON.parse(JSON.stringify(data));

            await this.db.cacheEntries.put({
                key,
                storeName,
                data: plainData,
                cachedAt: Date.now(),
                ttlMinutes
            });
        } catch (err) {
            console.warn('[IndexedDBCache] Failed to write cache entry:', err);
        }
    }
}
