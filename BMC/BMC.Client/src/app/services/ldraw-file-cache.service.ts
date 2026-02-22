/**
 * LDrawFileCacheService — IndexedDB-backed persistent cache for LDraw files.
 *
 * AI-DEVELOPED — Transparently extends THREE.Cache with IndexedDB persistence.
 *
 * How it works:
 *  1. Enables THREE.Cache (used by THREE.FileLoader for in-memory caching)
 *  2. On startup, hydrates THREE.Cache from IndexedDB — so even the first load
 *     of a session is instant for previously-fetched files
 *  3. Monkey-patches Cache.add() to also persist new entries to IndexedDB
 *     (fire-and-forget, doesn't block rendering)
 *
 * Since LDraw files (.dat, .ldr) are static and immutable, they only need to
 * be fetched from the server once — ever. Subsequent loads (even across browser
 * sessions) are served instantly from IndexedDB.
 *
 * Both the catalog-part-detail interactive viewer and the ldraw-thumbnail
 * batch renderer benefit automatically, since they both use LDrawLoader which
 * internally uses FileLoader → THREE.Cache.
 */

import { Injectable, Inject } from '@angular/core';
import Dexie, { Table } from 'dexie';
import { Cache } from 'three';


/**
 * Persisted file entry in IndexedDB.
 */
interface LDrawFileCacheEntry {
    /** Cache key — matches THREE.Cache key format: 'file:<full-url>' */
    cacheKey: string;
    /** Raw text content of the LDraw file */
    content: string;
    /** Timestamp when this entry was first cached */
    cachedAt: number;
}


/**
 * Dexie database for persistent LDraw file storage.
 */
class LDrawFileCacheDatabase extends Dexie {
    files!: Table<LDrawFileCacheEntry, string>;

    constructor() {
        super('bmc-ldraw-files-v1');

        this.version(1).stores({
            files: 'cacheKey'
        });
    }
}


@Injectable({ providedIn: 'root' })
export class LDrawFileCacheService {

    private db: LDrawFileCacheDatabase;
    private loaded = false;
    private loadPromise: Promise<void> | null = null;
    private originalCacheAdd: typeof Cache.add | null = null;

    //
    // URL prefix filter — only cache LDraw API file requests, not arbitrary URLs
    //
    private apiPrefix: string;


    constructor(@Inject('BASE_URL') baseUrl: string) {
        this.db = new LDrawFileCacheDatabase();
        this.apiPrefix = `file:${baseUrl}api/ldraw/file/`;
    }


    /**
     * Initialise the cache layer. Call once before any LDraw loading begins.
     *
     * - Enables THREE.Cache
     * - Hydrates Cache from IndexedDB
     * - Installs the IndexedDB write-through hook on Cache.add
     */
    async initialise(): Promise<void> {
        if (this.loaded) return;

        if (!this.loadPromise) {
            this.loadPromise = this.doInit();
        }

        await this.loadPromise;
    }


    /**
     * Get cache statistics for debugging.
     */
    get stats(): { memory: number; persistent: number } {
        const memoryCount = Object.keys(Cache.files).length;
        return { memory: memoryCount, persistent: -1 }; // persistent count requires async
    }


    // ── Private ──────────────────────────────────────────────────────────

    private async doInit(): Promise<void> {

        //
        // 1. Enable THREE.Cache — FileLoader will now check Cache.get() before
        //    fetching, and call Cache.add() after successful fetches.
        //
        Cache.enabled = true;

        //
        // 2. Hydrate THREE.Cache from IndexedDB
        //
        try {
            const entries = await this.db.files.toArray();

            for (const entry of entries) {
                Cache.files[entry.cacheKey] = entry.content;
            }

            console.log(`[LDrawFileCache] Hydrated THREE.Cache with ${entries.length} files from IndexedDB.`);
        } catch (err) {
            console.warn('[LDrawFileCache] Failed to load from IndexedDB:', err);
        }

        //
        // 3. Monkey-patch Cache.add to persist new entries to IndexedDB.
        //    We only cache LDraw API responses (not arbitrary resources).
        //
        this.originalCacheAdd = Cache.add.bind(Cache);

        const self = this;
        Cache.add = function (key: string, file: any): void {

            //
            // Call original Cache.add (stores in memory)
            //
            if (self.originalCacheAdd) {
                self.originalCacheAdd(key, file);
            }

            //
            // Persist to IndexedDB if this is an LDraw file response (text content)
            //
            if (typeof key === 'string' && key.startsWith(self.apiPrefix) && typeof file === 'string') {
                self.db.files.put({
                    cacheKey: key,
                    content: file,
                    cachedAt: Date.now()
                }).catch(err => {
                    console.warn('[LDrawFileCache] IndexedDB write failed for', key, err);
                });
            }

        };

        this.loaded = true;
    }
}
